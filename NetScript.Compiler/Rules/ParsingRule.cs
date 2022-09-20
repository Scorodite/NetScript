using NetScript.Compiler.AST;
using NetScript.Compiler.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetScript.Compiler.Rules
{
    public abstract class ParsingRule
    {
        public abstract bool IsRight(List<Token> tokens, Compiler compiler);
        public abstract ASTBase GetAST(List<Token> tokens, Compiler compiler);

        public static string[] GetNameSequence(List<Token> tokens, TokenType opening, TokenType closing, ref int i)
        {
            List<string> res = new();
            if (i < tokens.Count - 1 && tokens[i].Type == opening /*&& tokens[i + 1].Type != closing*/)
            {
                i++;
                for (; ; )
                {
                    if (tokens[i].Type == TokenType.Name)
                    {
                        res.Add(tokens[i++].Value);
                        if (tokens[i].Type == closing)
                        {
                            i++;
                            break;
                        }
                        else if (tokens[i++].Type != TokenType.Sep)
                        {
                            throw new CompilerException("Unknown construction", tokens[i - 1].Index);
                        }
                    }
                    else if (tokens[i].Type == closing)
                    {
                        i++;
                        break;
                    }
                    else
                    {
                        throw new CompilerException("Unknown construction", tokens[i].Index);
                    }
                }
            }
            return res.ToArray();
        }
    }

    public class ConstantRule : ParsingRule
    {
        public TokenType Type { get; set; }
        public Func<Token, object> Func { get; set; }
        
        public ConstantRule(TokenType type, Func<Token, object> func)
        {
            Type = type;
            Func = func;
        }

        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count == 1 && tokens.First().Type == Type;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            return new ConstantAST(Func(tokens.First())) { Index = tokens.First().Index };
        }
    }

    public class SingleTokenRule : ParsingRule
    {
        public TokenType Type { get; set; }
        public Func<Token, ASTBase> Func { get; set; }
        
        public SingleTokenRule(TokenType type, Func<Token, ASTBase> func)
        {
            Type = type;
            Func = func;
        }

        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count == 1 && tokens.First().Type == Type;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            ASTBase res = Func(tokens.First());
            res.Index = tokens.First().Index;
            return res;
        }
    }

    public class NewVariableRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count == 2 && tokens.First().Type == TokenType.NewVariable && tokens.Last().Type == TokenType.Name;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            return new NewVariableAST(tokens.Last().Value) { Index = tokens.First().Index };
        }
    }

    public abstract class InvokeLikeRule : ParsingRule
    {
        public TokenType OpenArgs { get; set; }
        public TokenType CloseArgs { get; set; }

        public InvokeLikeRule(TokenType openArgs, TokenType closeArgs)
        {
            OpenArgs = openArgs;
            CloseArgs = closeArgs;
        }

        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Last().Type == CloseArgs && compiler.FindTokenRev(tokens, OpenArgs, tokens.Count - 2) > 0;
        }

        public (ASTBase, ASTBase[]) GetObjAndArgs(List<Token> tokens, Compiler compiler)
        {
            int openPos = compiler.FindTokenRev(tokens, OpenArgs, tokens.Count - 2);
            if (openPos == tokens.Count - 2)
            {
                return (compiler.GetAST(tokens.GetRange(0, tokens.Count - 2)), Array.Empty<ASTBase>());
            }
            else
            {
                ASTBase obj = compiler.GetAST(tokens.GetRange(0, openPos));
                int lastSep = openPos;
                List<ASTBase> args = new();
                for (;;)
                {
                    int sepPos = compiler.FindToken(tokens, TokenType.Sep, lastSep + 1, tokens.Count - 1);
                    if (sepPos >= 0)
                    {
                        args.Add(compiler.GetAST(tokens.GetRange(lastSep + 1, sepPos - lastSep - 1)));
                    }
                    else
                    {
                        List<Token> arg = tokens.GetRange(lastSep + 1, tokens.Count - lastSep - 2);
                        if (arg.Count > 0)
                        {
                            args.Add(compiler.GetAST(arg));
                        }
                        return (obj, args.ToArray());
                    }
                    lastSep = sepPos;
                }
            }
        }
    }

    public class InvokeRule : InvokeLikeRule
    {
        public InvokeRule() : base(TokenType.OpeningGroup, TokenType.ClosingGroup) { }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            (ASTBase obj, ASTBase[] args) = GetObjAndArgs(tokens, compiler);
            return new InvokeAST(obj, args) { Index = tokens.First().Index };
        }
    }

    public class GetIndexRule : InvokeLikeRule
    {
        public GetIndexRule() : base(TokenType.OpeningIndex, TokenType.ClosingIndex) { }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            (ASTBase obj, ASTBase[] args) = GetObjAndArgs(tokens, compiler);
            return new GetIndexAST(obj, args) { Index = tokens.First().Index };
        }
    }

    public class GenericRule : InvokeLikeRule
    {
        public GenericRule() : base(TokenType.OpeningGeneric, TokenType.ClosingGeneric) { }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            (ASTBase obj, ASTBase[] args) = GetObjAndArgs(tokens, compiler);
            return new GenericAST(obj, args) { Index = tokens.First().Index };
        }
    }

    public class GetFieldRule : ParsingRule
    {
        public GetFieldRule() { }

        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count >= 2 && tokens.Last().Type == TokenType.Name && tokens[tokens.Count - 2].Type == TokenType.Field;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            string name = tokens.Last().Value;
            if (tokens.Count == 2)
            {
                return new GetFieldAST(new GetContextValueAST() { Index = tokens.First().Index }, name) { Index = tokens.Last().Index };
            }
            ASTBase obj = compiler.GetAST(tokens.GetRange(0, tokens.Count - 2));

            return new GetFieldAST(obj, name) { Index = tokens.Last().Index };
        }
    }

    public abstract class ListLikeRule : ParsingRule
    {
        protected TokenType OpenArgs { get; set; }
        protected TokenType CloseArgs { get; set; }
        protected TokenType SepArgs { get; set; }
        protected bool AllowRank { get; set; }

        public ListLikeRule(TokenType openArgs, TokenType closeArgs, TokenType sepArgs, bool allowRank)
        {
            OpenArgs = openArgs;
            CloseArgs = closeArgs;
            SepArgs = sepArgs;
            AllowRank = allowRank;
        }

        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Last().Type == CloseArgs && compiler.FindTokenRev(tokens, OpenArgs, tokens.Count - 2) == 0;
        }

        public (ASTBase, ASTBase[], int) GetTypeAndItems(List<Token> tokens, Compiler compiler)
        {
            int clarPos = compiler.FindToken(tokens, TokenType.Clarification, 1, tokens.Count - 1);
            int rank = -1;
            int lastSep;
            ASTBase type;
            if (clarPos > 0)
            {
                lastSep = clarPos;
                if (AllowRank && tokens[clarPos - 1].Type == TokenType.Int)
                {
                    rank = int.Parse(tokens[clarPos - 1].Value);
                    if (rank < 1)
                    {
                        throw new CompilerException("Rank can not be lower than one", tokens[clarPos - 1].Index);
                    }
                    clarPos--;
                }
                type = clarPos <= 1 ?
                    null :
                    compiler.GetAST(tokens.GetRange(1, clarPos - 1));
            }
            else
            {
                lastSep = 0;
                type = null;
            }
            List<ASTBase> args = new();
            for (;;)
            {
                int sepPos = compiler.FindToken(tokens, SepArgs, lastSep + 1, tokens.Count - 1);
                if (sepPos >= 0)
                {
                    args.Add(compiler.GetAST(tokens.GetRange(lastSep + 1, sepPos - lastSep - 1)));
                }
                else
                {
                    List<Token> arg = tokens.GetRange(lastSep + 1, tokens.Count - lastSep - 2);
                    if (arg.Count > 0)
                    {
                        args.Add(compiler.GetAST(arg));
                    }
                    return (type, args.ToArray(), rank);
                }
                lastSep = sepPos;
            }
        }
    }

    public class UnnamedFunctionRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.Function;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            int i = 1;
            if (tokens.Count == 1)
            {
                return new CreateFunctionAST(null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ASTBase>()) { Index = tokens.First().Index };
            }

            string[] generics = GetNameSequence(tokens, TokenType.OpeningGeneric, TokenType.ClosingGeneric, ref i);
            string[] args = GetNameSequence(tokens, TokenType.OpeningGroup, TokenType.ClosingGroup, ref i);
            ASTBase[] asts;

            if (i < tokens.Count - 1 && tokens[i].Type == TokenType.OpeningQuote)
            {
                int quoteClose = compiler.FindToken(tokens, TokenType.ClosingQuote, ++i);
                if (quoteClose == tokens.Count - 1)
                {
                    asts = compiler.GetASTs(tokens.GetRange(i, quoteClose - i));
                }
                else
                {
                    throw new CompilerException("Unknown construction", tokens.First().Index);
                }
            }
            else
            {
                asts = Array.Empty<ASTBase>();
            }
            return new CreateFunctionAST(null, args, generics, asts) { Index = tokens.First().Index };
        }

    }

    public class LoadDllRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count == 2 && tokens[0].Type == TokenType.LoadDll && tokens[1].Type == TokenType.String;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            return new LoadDllAST(System.Text.RegularExpressions.Regex.Unescape(tokens.Last().Value[1..^1])) { Index = tokens.First().Index };
        }
    }

    public class ConstructorRule : ParsingRule
    {
        public ConstructorRule() { }

        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            int openQuote = compiler.FindTokenRev(tokens, TokenType.OpeningQuote, tokens.Count - 2);
            return tokens.Last().Type == TokenType.ClosingQuote && openQuote > 2 && tokens[openQuote - 1].Type == TokenType.Field;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            int openPos = compiler.FindTokenRev(tokens, TokenType.OpeningQuote, tokens.Count - 2);
            ASTBase obj = compiler.GetAST(tokens.GetRange(0, openPos - 1));
            ASTBase[] asts = compiler.GetASTs(tokens.GetRange(openPos + 1, tokens.Count - openPos - 2));
            return new ConstructorAST(obj, asts);
        }
    }

    public class ListRule : ListLikeRule
    {
        public ListRule() : base(TokenType.OpeningIndex, TokenType.ClosingIndex, TokenType.Sep, false) { }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            (ASTBase type, ASTBase[] items, int _) = GetTypeAndItems(tokens, compiler);

            return new ListAST(type ?? ASTBase.TypeObject, items);
        }
    }

    public class ArrayRule : ListLikeRule
    {
        public ArrayRule() : base(TokenType.OpeningQuote, TokenType.ClosingQuote, TokenType.Sep, true) { }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            (ASTBase type, ASTBase[] items, int rank) = GetTypeAndItems(tokens, compiler);

            return new ArrayAST(type, items, rank > 0 ? rank : 1);
        }
    }
}
