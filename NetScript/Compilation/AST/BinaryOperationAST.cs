namespace NetScript.Compilation.AST
{
    /// <summary>
    /// Base AST class of binary operation
    /// </summary>
    public abstract class BinaryOperationAST : ASTBase
    {
        public ASTBase A { get; set; }
        public ASTBase B { get; set; }

        public BinaryOperationAST(ASTBase a, ASTBase b)
        {
            A = a;
            B = b;
        }

        protected void CompileAB(Bytecode bc, BinaryWriter writer, CompilerArgs args)
        {
            A.ReturnOnly().Compile(writer, args);
            B.ReturnOnly().Compile(writer, args);
            writer.Write(bc);
        }
    }

    public class SingleBytecodeBinOpAST : BinaryOperationAST
    {
        public Bytecode Byte { get; set; }
        public string Operator { get; set; }

        public SingleBytecodeBinOpAST(ASTBase a, ASTBase b, Bytecode bc, string op) : base(a, b)
        {
            Byte = bc;
            Operator = op;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Byte, writer, args);

        public override string ToString() => $"({A} {Operator} {B})";
    }

    public class AndAST : BinaryOperationAST
    {
        public AndAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} && {B})";

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            A.ReturnOnly().Compile(writer, args);
            writer.Write(Bytecode.And);
            SizePosition sizePos = new(writer);
            B.ReturnOnly().Compile(writer, args);
            sizePos.SaveSize();
        }
    }

    public class OrAST : BinaryOperationAST
    {
        public OrAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} || {B})";

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            A.Compile(writer, args);
            writer.Write(Bytecode.Or);
            SizePosition sizePos = new(writer);
            B.Compile(writer, args);
            sizePos.SaveSize();
        }
    }

    public class NullCoalescingAST : BinaryOperationAST
    {
        public NullCoalescingAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} ?? {B})";

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            A.ReturnOnly().Compile(writer, args);
            writer.Write(Bytecode.NullCoalescing);
            SizePosition sizePos = new(writer);
            B.ReturnOnly().Compile(writer, args);
            sizePos.SaveSize();
        }
    }

    public class IsNotTypeAST : BinaryOperationAST
    {
        public IsNotTypeAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} is not {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            A.ReturnOnly().Compile(writer, args);
            B.ReturnOnly().Compile(writer, args);
            writer.Write(Bytecode.IsType);
            writer.Write(Bytecode.Not);
        }
    }
}
