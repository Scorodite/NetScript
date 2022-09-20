using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetScript.Compiler.AST;
using NetScript.Compiler.Tokens;

namespace NetScript.Compiler.Rules
{
    public abstract class SpecialConstructionRule
    {
        public abstract bool IsRight(List<Token> tokens, int index, Compiler compiler);
        public abstract ASTBase GetAST(List<Token> tokens, ref int i, Compiler compiler);
    }

    public class IfRule : SpecialConstructionRule
    {
        public override bool IsRight(List<Token> tokens, int index, Compiler compiler)
        {
            return tokens[index].Type == TokenType.If;
        }

        public override ASTBase GetAST(List<Token> tokens, ref int i, Compiler compiler)
        {
            int firstBracketBegin = compiler.FindToken(tokens, TokenType.OpeningQuote, i);

            if (firstBracketBegin <= i + 1)
            {
                throw new CompilerException("If expression must have condition", tokens.First().Index);
            }

            int firstBracketEnd = compiler.FindToken(tokens, TokenType.ClosingQuote, firstBracketBegin + 1);
            ASTBase firstTrueCondition = compiler.GetAST(tokens.GetRange(i + 1, firstBracketBegin - i - 1));
            ASTBase[] firstTrueAST = compiler.GetASTs(tokens.GetRange(firstBracketBegin + 1, firstBracketEnd - firstBracketBegin - 1));
            int currPos = firstBracketEnd + 1;
            List<(ASTBase[], ASTBase)> asts = new() { (firstTrueAST, firstTrueCondition) };

            for (; ; )
            {
                if (currPos >= tokens.Count || tokens[currPos].Type != TokenType.Else)
                {
                    i = currPos;
                    return new IfAST(asts.ToArray()) { Index = tokens.First().Index };
                }
                else if (tokens[currPos].Type == TokenType.Else)
                {
                    if (tokens[currPos + 1].Type == TokenType.If) // ELSE IF
                    {
                        int openingQuote = compiler.FindToken(tokens, TokenType.OpeningQuote, currPos + 3);
                        if (openingQuote > 0)
                        {
                            int closingQuote = compiler.FindToken(tokens, TokenType.ClosingQuote, openingQuote + 1);
                            ASTBase condition = compiler.GetAST(tokens.GetRange(currPos + 2, openingQuote - currPos - 2));
                            ASTBase[] ifASTs = compiler.GetASTs(tokens.GetRange(openingQuote + 1, closingQuote - openingQuote - 1));
                            currPos = closingQuote + 1;
                            asts.Add((ifASTs, condition));
                            continue;
                        }

                    }
                    else if (tokens[currPos + 1].Type == TokenType.OpeningQuote) // ELSE
                    {
                        int closingQuote = compiler.FindToken(tokens, TokenType.ClosingQuote, currPos + 2);
                        if (closingQuote > 0)
                        {
                            i = closingQuote + 1;
                            ASTBase[] elseASTs = compiler.GetASTs(tokens.GetRange(currPos + 2, closingQuote - currPos - 2));
                            return new IfAST(asts.ToArray(), elseASTs) { Index = tokens.First().Index };
                        }
                    }
                }
                throw new CompilerException("Unknown construction", tokens[currPos].Index);
            }
        }
    }

    public abstract class ReturnLikeRule : SpecialConstructionRule
    {
        protected TokenType Type { get; set; }

        public ReturnLikeRule(TokenType type)
        {
            Type = type;
        }

        public override bool IsRight(List<Token> tokens, int index, Compiler compiler)
        {
            return tokens[index].Type == Type;
        }

        public override ASTBase GetAST(List<Token> tokens, ref int i, Compiler compiler)
        {
            int eolPos = compiler.FindToken(tokens, TokenType.EOL, i);
            if (eolPos < 0)
            {
                eolPos = tokens.Count;
            }
            ASTBase val = compiler.GetAST(tokens.GetRange(i + 1, eolPos - i - 1));
            i = eolPos + 1;
            return GetAST(val);
        }

        protected abstract ASTBase GetAST(ASTBase val);
    }

    public class ReturnRule : ReturnLikeRule
    {
        public ReturnRule() : base(TokenType.Return) { }

        protected override ASTBase GetAST(ASTBase val) =>
            new ReturnAST(val.NullIfEmpty());
    }

    public class BreakRule : ReturnLikeRule
    {
        public BreakRule() : base(TokenType.Break) { }

        protected override ASTBase GetAST(ASTBase val) =>
            new BreakAST(val.NullIfEmpty());
    }

    public class OutputRule : ReturnLikeRule
    {
        public OutputRule() : base(TokenType.Output) { }

        protected override ASTBase GetAST(ASTBase val) =>
            new OutputAST(val.NullIfEmpty());
    }

    public class ThrowRule : ReturnLikeRule
    {
        public ThrowRule() : base(TokenType.Throw) { }

        protected override ASTBase GetAST(ASTBase val)
        {
            if (val is EmptyAST)
            {
                throw new CompilerException("Throw statement requires value");
            }
            return new ThrowAST(val.NullIfEmpty());
        }
    }

    public class LoopRule : SpecialConstructionRule
    {
        public override bool IsRight(List<Token> tokens, int index, Compiler compiler)
        {
            return tokens[index].Type == TokenType.Loop;
        }

        public override ASTBase GetAST(List<Token> tokens, ref int i, Compiler compiler)
        {
            if (i < tokens.Count - 2 && tokens[i + 1].Type == TokenType.OpeningQuote)
            {
                int closingQuote = compiler.FindToken(tokens, TokenType.ClosingQuote, i + 2);
                if (closingQuote > 0)
                {
                    LoopAST res = new(compiler.GetASTs(tokens.GetRange(i + 2, closingQuote - i - 2)));
                    i = closingQuote + 1;
                    return res;
                }
            }
            throw new CompilerException("Unknown construction", tokens[i].Index);
        }
    }

    public class WhileRule : SpecialConstructionRule
    {
        public override bool IsRight(List<Token> tokens, int index, Compiler compiler)
        {
            return tokens[index].Type == TokenType.While;
        }

        public override ASTBase GetAST(List<Token> tokens, ref int i, Compiler compiler)
        {
            int openingQuote = compiler.FindToken(tokens, TokenType.OpeningQuote, i);
            if (openingQuote > 0)
            {
                int closingQuote = compiler.FindToken(tokens, TokenType.ClosingQuote, openingQuote + 1);
                if (closingQuote > 0)
                {
                    ASTBase condition = compiler.GetAST(tokens.GetRange(i + 1, openingQuote - i - 1));
                    ASTBase[] asts = compiler.GetASTs(tokens.GetRange(openingQuote + 1, closingQuote - openingQuote - 1));
                    i = closingQuote + 1;
                    return new WhileAST(condition, asts);
                }
            }
            throw new CompilerException("Unknown construction", tokens[i].Index);
        }
    }

    public class ForRule : SpecialConstructionRule
    {
        public override bool IsRight(List<Token> tokens, int index, Compiler compiler)
        {
            return index < tokens.Count - 5 && tokens[index].Type == TokenType.For &&
                tokens[index + 1].Type == TokenType.Name && tokens[index + 2].Type == TokenType.In;
        }

        public override ASTBase GetAST(List<Token> tokens, ref int i, Compiler compiler)
        {
            string name = tokens[i + 1].Value;
            int openingQuote = compiler.FindToken(tokens, TokenType.OpeningQuote, i);
            if (openingQuote > 0)
            {
                int closingQuote = compiler.FindToken(tokens, TokenType.ClosingQuote, openingQuote + 1);
                if (closingQuote > 0)
                {
                    ASTBase condition = compiler.GetAST(tokens.GetRange(i + 3, openingQuote - i - 3));
                    ASTBase[] asts = compiler.GetASTs(tokens.GetRange(openingQuote + 1, closingQuote - openingQuote - 1));
                    i = closingQuote + 1;
                    return new ForAST(name, condition, asts);
                }
            }
            throw new CompilerException("Unknown construction", tokens[i].Index);
        }
    }

    public class FuncRule : SpecialConstructionRule
    {
        public override bool IsRight(List<Token> tokens, int index, Compiler compiler)
        {
            return index < tokens.Count - 2 && tokens[index].Type == TokenType.Function &&
                tokens[index + 1].Type == TokenType.Name;
        }

        public override ASTBase GetAST(List<Token> tokens, ref int i_, Compiler compiler)
        {
            string name = tokens[i_ + 1].Value;

            int i = i_ + 2;

            string[] generics = ParsingRule.GetNameSequence(tokens, TokenType.OpeningGeneric, TokenType.ClosingGeneric, ref i);
            string[] args = ParsingRule.GetNameSequence(tokens, TokenType.OpeningGroup, TokenType.ClosingGroup, ref i);
            ASTBase[] asts;

            if (tokens[i].Type == TokenType.OpeningQuote)
            {
                int closingQuote = compiler.FindToken(tokens, TokenType.ClosingQuote, ++i);
                asts = compiler.GetASTs(tokens.GetRange(i, closingQuote - i));
                i = closingQuote + 1;
            }
            else
            {
                asts = Array.Empty<ASTBase>();
            }
            i_ = i;
            return new CreateFunctionAST(name, args, generics, asts) { Index = tokens.First().Index };
        }
    }

    public class TryCatchRule : SpecialConstructionRule
    {
        public override bool IsRight(List<Token> tokens, int index, Compiler compiler)
        {
            return index < tokens.Count - 7 && tokens[index].Type == TokenType.Try;
        }

        public override ASTBase GetAST(List<Token> tokens, ref int i, Compiler compiler)
        {
            if (tokens[i + 1].Type == TokenType.OpeningQuote)
            {
                int tryEnd = compiler.FindToken(tokens, TokenType.ClosingQuote, i + 2);
                if (tryEnd > 0 && tryEnd < tokens.Count - 3 && tokens[tryEnd + 1].Type == TokenType.Catch &&
                    tokens[tryEnd + 2].Type == TokenType.Name &&
                    tokens[tryEnd + 3].Type == TokenType.OpeningQuote)
                {
                    string exceptionVar = tokens[tryEnd + 2].Value;
                    int catchEnd = compiler.FindToken(tokens, TokenType.ClosingQuote, tryEnd + 4);
                    if (catchEnd > 0)
                    {
                        TryCatchAST res = new(exceptionVar,
                            compiler.GetASTs(tokens.GetRange(i + 2, tryEnd - i - 2)),
                            compiler.GetASTs(tokens.GetRange(tryEnd + 4, catchEnd - tryEnd - 4)));
                        i = catchEnd + 1;
                        return res;
                    }
                }
            }
            throw new CompilerException("Unknown construction", tokens.First().Index);
        }
    }
}
