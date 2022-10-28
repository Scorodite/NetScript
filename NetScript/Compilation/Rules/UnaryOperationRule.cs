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
    /// Parsing rule of unary operations
    /// </summary>
    public abstract class UnaryOperationRule
    {
        public TokenType Operator { get; }
        //public Type ASTUnOp { get; }

        protected UnaryOperationRule(TokenType op)
        {
            Operator = op;
        }

        public bool IsRight(IList<Token> tokens)
        {
            return tokens.FirstOrDefault() is Token t && t.Type == Operator;
        }

        public abstract ASTBase GetAST(IList<Token> tokens, Compiler compiler);
    }

    public class CustomUnaryOperationRule : UnaryOperationRule
    {
        public Func<ASTBase, int, ASTBase> Func { get; }

        public CustomUnaryOperationRule(TokenType op, Func<ASTBase, int, ASTBase> func) : base(op)
        {
            Func = func;
        }

        public override ASTBase GetAST(IList<Token> tokens, Compiler compiler)
        {
            List<Token> list = new(tokens);
            int pos = list.First().Index;
            list.RemoveAt(0);
            ASTBase a = compiler.GetAST(list);

            return Func(a, pos);
        }
    }

    public class SingleByteUnOpRule : UnaryOperationRule
    {
        public Bytecode Byte { get; }

        public SingleByteUnOpRule(TokenType op, Bytecode bc) : base(op)
        {
            Byte = bc;
        }

        public override ASTBase GetAST(IList<Token> tokens, Compiler compiler)
        {
            List<Token> list = new(tokens);
            int pos = list.First().Index;
            list.RemoveAt(0);
            ASTBase a = compiler.GetAST(list);

            return new SingleBytecodeUnOpAST(a, Byte, list.First().Value) { Index = pos };
        }
    }
}
