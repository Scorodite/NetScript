using NetScript.Core;
using System.IO;

namespace NetScript.Compiler.AST
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

    public class RevAST : UnaryOperationAST
    {
        public RevAST(ASTBase a) : base(a) { }
        public override string ToString() => $"-{A}";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileA(Bytecode.Rev, writer, args);
    }

    public class BinRevAST : UnaryOperationAST
    {
        public BinRevAST(ASTBase a) : base(a) { }
        public override string ToString() => $"~{A}";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileA(Bytecode.BinRev, writer, args);
    }

    public class NotAST : UnaryOperationAST
    {
        public NotAST(ASTBase a) : base(a) { }
        public override string ToString() => $"!{A}";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileA(Bytecode.Not, writer, args);
    }

    public class GetTypeAST : UnaryOperationAST
    {
        public GetTypeAST(ASTBase a) : base(a) { }
        public override string ToString() => $"typeof {A}";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileA(Bytecode.GetTypeObj, writer, args);
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

    public class DefaultAST : UnaryOperationAST
    {
        public DefaultAST(ASTBase a) : base(a) { }
        public override string ToString() => $"default {A}";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileA(Bytecode.Default, writer, args);
    }
}
