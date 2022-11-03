using System;
using System.IO;
using NetScript.Compilation;
using NetScript.Interpretation;

namespace NetScript
{
    public static class NS
    {
        public static VariableCollection Run(string[] codes) =>
            Run(codes, CancellationToken.None);

        public static VariableCollection Run(string code) =>
            Run(code, CancellationToken.None);

        public static VariableCollection Run(string[] codes, CancellationToken token)
        {
            using MemoryStream memory = new();
            Compiler.Instance.Compile(codes, memory);
            memory.Position = 0;
            return Interpreter.Interpret(memory, token);
        }

        public static VariableCollection Run(string code, CancellationToken token)
        {
            using MemoryStream memory = new();
            Compiler.Instance.Compile(code, memory);
            memory.Position = 0;
            return Interpreter.Interpret(memory, token);
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
