using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetScript.Compilation.AST;
using NetScript.Compilation.Tokens;

namespace NetScript.Compilation.Rules
{
    /// <summary>
    /// Parsing rule of binary operations
    /// </summary>
    public abstract class BinaryOperationRule
    {
        public TokenType Operator { get; }
        public bool Reverse { get; set; }

        protected BinaryOperationRule(TokenType op)
        {
            Operator = op;
        }

        public virtual bool IsRight(IList<Token> tokens, int index)
        {
            return tokens[index].Type == Operator;
        }

        public abstract ASTBase GetAST(IList<Token> tcoll, Compiler compiler, int index);
    }

    public class SingleByteBinOpRule : BinaryOperationRule
    {
        public Bytecode Byte { get; }

        public SingleByteBinOpRule(TokenType op, Bytecode bc) : this(op, bc, false) { }

        public SingleByteBinOpRule(TokenType op, Bytecode bc, bool rev) : base(op)
        {
            Byte = bc;
            Reverse = rev;
        }

        public override ASTBase GetAST(IList<Token> tcoll, Compiler compiler, int index)
        {
            List<Token> list = tcoll is List<Token> tcolllist ? tcolllist : new(tcoll);
            ASTBase a = compiler.GetAST(list.GetRange(0, index));
            ASTBase b = compiler.GetAST(list.GetRange(index + 1, list.Count - index - 1));

            return new SingleBytecodeBinOpAST(a, b, Byte, tcoll[index].Value) { Index = list[index].Index };
        }
    }

    public class CustomBinaryOperationRule : BinaryOperationRule
    {
        public Func<ASTBase, ASTBase, int, ASTBase> Func { get; }

        public CustomBinaryOperationRule(TokenType op, Func<ASTBase, ASTBase, int, ASTBase> func) : base(op)
        {
            Func = func;
        }

        public override ASTBase GetAST(IList<Token> tcoll, Compiler compiler, int index)
        {
            List<Token> list = tcoll is List<Token> tcolllist ? tcolllist : new(tcoll);
            ASTBase a = compiler.GetAST(list.GetRange(0, index));
            ASTBase b = compiler.GetAST(list.GetRange(index + 1, list.Count - index - 1));

            return Func(a, b, list[index].Index);
        }
    }

    /// <summary>
    /// Rule of assign operation
    /// </summary>
    public class AssignOperationRule : BinaryOperationRule
    {
        public AssignOperationRule(TokenType op) : base(op)
        {
            Reverse = true;
        }

        public override ASTBase GetAST(IList<Token> tcoll, Compiler compiler, int index)
        {
            List<Token> list = tcoll is List<Token> tcolllist ? tcolllist : new(tcoll);
            ASTBase a = compiler.GetAST(list.GetRange(0, index));
            ASTBase b = compiler.GetAST(list.GetRange(index + 1, list.Count - index - 1));
            int pos = list[index].Index;
            switch (a)
            {
                case GetVariableAST ast:
                    return new SetVariableAST(ast.Name, b) { Index = pos };
                case GetIndexAST ast:
                    return new SetIndexAST(ast.Obj, ast.Args, b) { Index = pos };
                case GetFieldAST ast:
                    return new SetFieldAST(ast.Obj, ast.Name, b) { Index = pos };
                case NewVariableAST ast:
                    ast.Value = b;
                    ast.Index = pos;
                    return ast;
                default:
                    throw new CompilerException($"Can not assign {a}", list.First().Index);
            }
        }
    }
}
