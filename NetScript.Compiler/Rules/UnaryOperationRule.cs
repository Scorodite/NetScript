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
    /// Parsing rule of unary operations
    /// </summary>
    public class UnaryOperationRule
    {
        public TokenType Operator { get; }
        public Type ASTUnOp { get; }

        public UnaryOperationRule(TokenType op, Type astunop)
        {
            if (!astunop.IsSubclassOf(typeof(UnaryOperationAST)))
            {
                throw new ArgumentException($"{nameof(astunop)} must be subclass of {nameof(UnaryOperationAST)}");
            }
            Operator = op;
            ASTUnOp = astunop;
        }

        public bool IsRight(IList<Token> tokens)
        {
            return tokens.FirstOrDefault() is Token t && t.Type == Operator;
        }

        public ASTBase GetAST(IList<Token> tokens, Compiler compiler)
        {
            List<Token> list = new(tokens);
            int pos = list.First().Index;
            list.RemoveAt(0);
            ASTBase a = compiler.GetAST(list);

            UnaryOperationAST op = (UnaryOperationAST)Activator.CreateInstance(ASTUnOp, new object[] { a });
            op.Index = pos;
            return op;
        }
    }
}
