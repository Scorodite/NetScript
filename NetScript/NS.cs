using System;
using System.IO;
using NetScript.Compiler;
using NetScript.Interpreter;

namespace NetScript
{
    public static class NS
    {
        public static NetScriptCompiler Compiler => _Compiler.Value;
        private static readonly Lazy<NetScriptCompiler> _Compiler = new(true);

        public static VariableCollection Run(string code)
        {
            using MemoryStream memory = new();
            Compiler.Compile(code, memory);
            memory.Position = 0;
            return InterpreterNS.Interpret(memory);
        }

        public static VariableCollection Run(Stream s)
        {
            return InterpreterNS.Interpret(s);
        }

        public static void Compile(string code, Stream s)
        {
            Compiler.Compile(code, s);
        }
    }
}
