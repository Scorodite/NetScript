using System;
using System.Collections.Generic;
using System.IO;
using NetScript.Compiler;
using NetScript.Compiler.AST;
using NetScript.Compiler.Tokens;
using NetScript.Interpreter;
using NetScript.Core;

namespace NetScript.Tester
{
    /// <summary>
    /// A program created to demonstrate how bytecode works
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("File path: ");
            string path = Console.ReadLine();

            TestBytecode(path);
        }

        private static object ReadConst(BinaryReader reader)
        {
            Bytecode bc = reader.ReadBytecode();

            return bc switch
            {
                Bytecode.ConstByte => reader.ReadByte(),
                Bytecode.ConstSbyte => reader.ReadSByte(),
                Bytecode.ConstShort => reader.ReadInt16(),
                Bytecode.ConstUShort => reader.ReadUInt16(),
                Bytecode.ConstInt => reader.ReadInt32(),
                Bytecode.ConstUInt => reader.ReadUInt32(),
                Bytecode.ConstLong => reader.ReadInt64(),
                Bytecode.ConstULong => reader.ReadUInt64(),
                Bytecode.ConstFloat => reader.ReadSingle(),
                Bytecode.ConstDecimal => reader.ReadDecimal(),
                Bytecode.ConstDouble => reader.ReadDouble(),
                Bytecode.ConstChar => reader.ReadChar(),
                Bytecode.ConstString => reader.ReadString(),
                _ => throw new Exception($"Unknown constant {bc}"),
            };
        }

        private static void TestBytecode(string path)
        {
            NetScriptCompiler comp = new();
            MemoryStream memory = new();

            string code = File.ReadAllText(path);
            ASTBase[] asts = comp.GetASTs(Lexer.LexString(code, comp.Expressions));
            Console.WriteLine(string.Join(";\n", asts as object[]) + "\n\n");

            comp.Compile(code, memory);
            memory.Position = 0;
            BinaryReader reader = new(memory);

            foreach (Bytecode bc in Enum.GetValues(typeof(Bytecode)))
            {
                if (!bc.ToString().StartsWith("Const"))
                {
                    Console.Write($"{(int)bc} = {bc}   ");
                }
            }

            string[] names = new string[reader.ReadInt32()];
            Console.WriteLine("\nNames:");
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = reader.ReadString();
                Console.WriteLine($"\t{i} = {names[i]}");
            }

            Console.WriteLine("DLLs:");
            int dlls = reader.ReadInt32();
            for (int i = 0; i < dlls; i++)
            {
                Console.WriteLine($"\t{reader.ReadString()}");
            }

            int importsLen = reader.ReadInt32();
            Dictionary<string, string> imports = new(importsLen);
            Console.WriteLine("Imports:");
            for (int i = 0; i < importsLen; i++)
            {
                int nameID = reader.ReadInt32();
                imports[names[nameID]] = reader.ReadString();
                Console.WriteLine($"\t{names[nameID]} = {imports[names[nameID]]}");
            }

            object[] consts = new object[reader.ReadInt32()];
            Console.WriteLine("Constants:");
            for (int i = 0; i < consts.Length; i++)
            {
                consts[i] = ReadConst(reader);
                Console.WriteLine($"\t{i} = {consts[i]}");
            }
            Console.WriteLine();
            while (memory.Position < memory.Length)
            {
                Console.Write($"{memory.ReadByte()} ");
            }
        }

        private static void TestInterpreter(string path)
        {
            MemoryStream memory = new();
            NetScriptCompiler comp = new();
            comp.Compile(path, memory);
            memory.Position = 0;
            VariableCollection vars = InterpreterNS.Interpret(memory);

            if (vars.TryGetValue("main", out var mainObj) && mainObj is InvokableFunction main)
            {
                Console.WriteLine(main.Invoke(null, null));
            }
        }
    }
}
