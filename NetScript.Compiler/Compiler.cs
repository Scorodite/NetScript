using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NetScript.Compiler.AST;
using NetScript.Compiler.Tokens;
using NetScript.Compiler.Rules;
using NetScript.Core;
using System.Text.RegularExpressions;

namespace NetScript.Compiler
{
    public abstract class Compiler
    {
        public abstract ICollection<(Regex, TokenType)> Expressions { get; }
        public abstract ICollection<ParsingRule> Rules { get; }
        public abstract ICollection<ICollection<BinaryOperationRule>> BinaryRules { get; }
        public abstract ICollection<UnaryOperationRule> UnaryRules { get; }

        public abstract void Compile(string code, Stream output);
        public abstract ASTBase[] GetASTs(List<Token> tokens);
        public abstract ASTBase GetAST(List<Token> tokens);

        public virtual bool IsBracket(TokenType type) =>
            IsOpeningBracket(type) || IsClosingBracket(type);

        public virtual bool IsOpeningBracket(TokenType type) => type switch
        {
            TokenType.OpeningGroup or TokenType.OpeningQuote or TokenType.OpeningIndex or TokenType.OpeningGeneric => true,
            _ => false,
        };

        public virtual bool IsClosingBracket(TokenType type) => type switch
        {
            TokenType.ClosingGroup or TokenType.ClosingQuote or TokenType.ClosingIndex or TokenType.ClosingGeneric => true,
            _ => false,
        };

        public virtual TokenType GetGroupRelative(TokenType type) => type switch
        {
            TokenType.OpeningGroup => TokenType.ClosingGroup,
            TokenType.ClosingGroup => TokenType.OpeningGroup,
            TokenType.OpeningQuote => TokenType.ClosingQuote,
            TokenType.ClosingQuote => TokenType.OpeningQuote,
            TokenType.OpeningIndex => TokenType.ClosingIndex,
            TokenType.ClosingIndex => TokenType.OpeningIndex,
            TokenType.OpeningGeneric => TokenType.ClosingGeneric,
            TokenType.ClosingGeneric => TokenType.OpeningGeneric,
            _ => throw new Exception("No relative group found"),
        };

        public int FindToken(List<Token> tokens, TokenType type, int begin, int end = -1)
        {
            Stack<Token> depth = new();
            if (end == -1)
            {
                end = tokens.Count;
            }
            for (int i = begin; i < end; i++)
            {
                Token token = tokens[i];
                if (token.Type == type && depth.Count == 0)
                {
                    return i;
                }
                else if (IsOpeningBracket(token.Type))
                {
                    depth.Push(token);
                }
                else if (IsClosingBracket(token.Type))
                {
                    if (depth.Count == 0)
                    {
                        throw new CompilerException($"Group was never closed", token.Index);
                    }

                    if (token.Type != GetGroupRelative(depth.Pop().Type))
                    {
                        throw new CompilerException($"Group type mismatch", token.Index);
                    }
                }
            }
            return -1;
        }

        public int FindTokenRev(List<Token> tokens, TokenType type, int begin, int end = -1)
        {
            Stack<Token> depth = new();
            if (end == -1)
            {
                end = 0;
            }
            for (int i = begin; i >= end; i--)
            {
                Token token = tokens[i];
                if (token.Type == type && depth.Count == 0)
                {
                    return i;
                }
                else if (IsClosingBracket(token.Type))
                {
                    depth.Push(token);
                }
                else if (IsOpeningBracket(token.Type))
                {
                    if (depth.Count == 0)
                    {
                        throw new CompilerException($"Group was never closed", token.Index);
                    }

                    if (token.Type != GetGroupRelative(depth.Pop().Type))
                    {
                        throw new CompilerException($"Group type mismatch", token.Index);
                    }
                }
            }
            return -1;
        }
    }
}
