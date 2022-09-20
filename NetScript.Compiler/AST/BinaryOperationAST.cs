using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetScript.Core;
using System.IO;

namespace NetScript.Compiler.AST
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

    public class SumAST : BinaryOperationAST
    {
        public SumAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} + {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Sum, writer, args);
    }

    public class SubAST : BinaryOperationAST
    {
        public SubAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} - {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Sub, writer, args);
    }

    public class MulAST : BinaryOperationAST
    {
        public MulAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} * {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Mul, writer, args);
    }

    public class DivAST : BinaryOperationAST
    {
        public DivAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} / {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Div, writer, args);
    }

    public class ModAST : BinaryOperationAST
    {
        public ModAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} % {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Mod, writer, args);
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

    public class BinAndAST : BinaryOperationAST
    {
        public BinAndAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} & {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.BinAnd, writer, args);
    }

    public class BinOrAST : BinaryOperationAST
    {
        public BinOrAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} | {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.BinOr, writer, args);
    }

    public class BinXorAST : BinaryOperationAST
    {
        public BinXorAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} ^ {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.BinXor, writer, args);
    }

    public class EqualAST : BinaryOperationAST
    {
        public EqualAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} == {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Equal, writer, args);
    }

    public class NotEqualAST : BinaryOperationAST
    {
        public NotEqualAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} != {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.NotEqual, writer, args);
    }

    public class GreaterAST : BinaryOperationAST
    {
        public GreaterAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} > {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Greater, writer, args);
    }

    public class LessAST : BinaryOperationAST
    {
        public LessAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} < {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Less, writer, args);
    }

    public class GreaterOrEqualAST : BinaryOperationAST
    {
        public GreaterOrEqualAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} >= {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.GreaterOrEqual, writer, args);
    }

    public class LessOrEqualAST : BinaryOperationAST
    {
        public LessOrEqualAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} <= {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.LessOrEqual, writer, args);
    }

    public class IsTypeAST : BinaryOperationAST
    {
        public IsTypeAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} is {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.IsType, writer, args);
    }

    public class IsNotTypeAST : BinaryOperationAST
    {
        public IsNotTypeAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} is {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            A.ReturnOnly().Compile(writer, args);
            B.ReturnOnly().Compile(writer, args);
            writer.Write(Bytecode.IsType);
            writer.Write(Bytecode.Not);
        }
    }

    public class RangeAST : BinaryOperationAST
    {
        public RangeAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} .. {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Range, writer, args);
    }

    public class ConvertAST : BinaryOperationAST
    {
        public ConvertAST(ASTBase a, ASTBase b) : base(a, b) { }
        public override string ToString() => $"({A} to {B})";
        public override void Compile(BinaryWriter writer, CompilerArgs args) =>
            CompileAB(Bytecode.Convert, writer, args);
    }
}
