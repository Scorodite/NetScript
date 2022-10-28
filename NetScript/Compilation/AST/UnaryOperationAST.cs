using System.IO;

namespace NetScript.Compilation.AST
{
    /// <summary>
    /// Base AST class of unary operation
    /// </summary>
    public abstract class UnaryOperationAST : ASTBase
    {
        public ASTBase A { get; set; }

        public UnaryOperationAST(ASTBase a)
        {
            A = a;
        }

        protected void CompileA(Bytecode bc, BinaryWriter writer, CompilerArgs args)
        {
            A.ReturnOnly().Compile(writer, args);
            writer.Write(bc);
        }
    }

    public class SingleBytecodeUnOpAST : UnaryOperationAST
    {
        public Bytecode Byte { get; set; }
        public string Operator { get; set; }

        public SingleBytecodeUnOpAST(ASTBase a, Bytecode bc, string op) : base(a)
        {
            Byte = bc;
            Operator = op;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileA(Byte, writer, args);

        public override string ToString() => $"{Operator} {A}";
    }

    public class GetNameAST : UnaryOperationAST
    {
        public GetNameAST(ASTBase a) : base(a)
        {
            if (a is not GetVariableAST)
            {
                throw new CompilerException("nameof construction requires name", Index);
            }
        }
        public override string ToString() => $"nameof {A}";
        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            writer.Write(Bytecode.PushName);
            writer.Write(args.GetNameID((A as GetVariableAST)?.Name ?? string.Empty));
        }
    }
}