using System;
using System.IO;
using NetScript.Compilation;
using NetScript.Interpretation;

namespace NetScript
{
    public static class NS
    {
        public static VariableCollection Run(string[] files)
        {
            using MemoryStream memory = new();
            Compiler.Instance.Compile(files, memory);
            memory.Position = 0;
            return Interpreter.Interpret(memory);
        }

        public static VariableCollection Run(string code)
        {
            using MemoryStream memory = new();
            Compiler.Instance.Compile(code, memory);
            memory.Position = 0;
            return Interpreter.Interpret(memory);
        }

        public static VariableCollection Run(Stream s)
        {
            return Interpreter.Interpret(s);
        }

        public static void Compile(string code, Stream s)
        {
            Compiler.Instance.Compile(code, s);
        }
    }
}
