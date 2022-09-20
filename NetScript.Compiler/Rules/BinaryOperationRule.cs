using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetScript.Compiler.AST;
using NetScript.Compiler.Tokens;

namespace NetScript.Compiler.Rules
{
    /// <summary>
    /// Parsing rule of binary operations
    /// </summary>
    public class BinaryOperationRule
    {
        public TokenType Operator { get; }
        public Type ASTBinOp { get; }
        public bool Reverse { get; set; }

        public BinaryOperationRule(TokenType op, Type astbinop) : this(op, astbinop, false) { }

        public BinaryOperationRule(TokenType op, Type astbinop, bool rev)
        {
            if (!astbinop.IsSubclassOf(typeof(BinaryOperationAST)))
            {
                throw new ArgumentException($"{nameof(astbinop)} must be subclass of {nameof(BinaryOperationAST)}");
            }
            Operator = op;
            ASTBinOp = astbinop;
            Reverse = rev;
        }

        protected BinaryOperationRule(TokenType op)
        {
            Operator = op;
        }

        public virtual bool IsRight(IList<Token> tokens, int index)
        {
            return tokens[index].Type == Operator;
        }

        public virtual ASTBase GetAST(IList<Token> tcoll, Compiler compiler, int index)
        {
            List<Token> list = tcoll is List<Token> tcolllist ? tcolllist : new(tcoll);
            ASTBase a = compiler.GetAST(list.GetRange(0, index));
            ASTBase b = compiler.GetAST(list.GetRange(index + 1, list.Count - index - 1));

            BinaryOperationAST op = (BinaryOperationAST)Activator.CreateInstance(ASTBinOp, new object[] { a, b });
            op.Index = list[index].Index;
            return op;
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
