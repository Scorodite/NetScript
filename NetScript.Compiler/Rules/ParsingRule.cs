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
        public TokenType OpenArgs { get; set; }
        public TokenType CloseArgs { get; set; }
        public TokenType SepArgs { get; set; }

        public ListLikeRule(TokenType openArgs, TokenType closeArgs, TokenType sepArgs)
        {
            OpenArgs = openArgs;
            CloseArgs = closeArgs;
            SepArgs = sepArgs;
        }

        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Last().Type == CloseArgs && compiler.FindTokenRev(tokens, OpenArgs, tokens.Count - 2) == 0;
        }

        public (ASTBase, ASTBase[]) GetTypeAndItems(List<Token> tokens, Compiler compiler)
        {
            int clarPos = compiler.FindToken(tokens, TokenType.Clarification, 1, tokens.Count - 1);
            ASTBase type;
            if (clarPos > 0)
            {
                type = compiler.GetAST(tokens.GetRange(1, clarPos - 1));
            }
            else
            {
                clarPos = 0;
                type = null;
            }

            int lastSep = clarPos;
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
                    return (type, args.ToArray());
                }
                lastSep = sepPos;
            }
        }
    }

    public class FunctionRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.Function;
        }

        public override ASTBase GetAST(List<Token> tlist, Compiler compiler)
        {
            int i = 1;
            List<Token> tokens = tlist is List<Token> tl ? tl : new(tlist);
            if (tokens.Count == 1)
            {
                return new CreateFunctionAST(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ASTBase>()) { Index = tokens.First().Index };
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
            return new CreateFunctionAST(args, generics, asts) { Index = tokens.First().Index };
        }

        public static string[] GetNameSequence(List<Token> tokens, TokenType opening, TokenType closing, ref int i)
        {
            List<string> res = new();
            if (i < tokens.Count - 1 && tokens[i].Type == opening /*&& tokens[i + 1].Type != closing*/)
            {
                i++;
                for (;;)
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

    public class IfRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.If;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {

            int firstBracketBegin = compiler.FindToken(tokens, TokenType.OpeningQuote, 0);
            if (firstBracketBegin < 2 || tokens[firstBracketBegin - 1].Type != TokenType.ClosingGroup)
            {
                throw new CompilerException("Unknown construction", tokens.First().Index);
            }
            int firstBracketEnd = compiler.FindToken(tokens, TokenType.ClosingQuote, firstBracketBegin + 1);
            ASTBase firstTrueCondition = compiler.GetAST(tokens.GetRange(1, firstBracketBegin - 1));
            ASTBase[] firstTrueAST = compiler.GetASTs(tokens.GetRange(firstBracketBegin + 1, firstBracketEnd - firstBracketBegin - 1));
            int currPos = firstBracketEnd + 1;
            List<(ASTBase[], ASTBase)> asts = new() { (firstTrueAST, firstTrueCondition) };

            for (;;)
            {
                if (currPos == tokens.Count)
                {
                    return new IfAST(asts.ToArray()) { Index = tokens.First().Index };
                }
                else if (tokens[currPos].Type == TokenType.Else)
                {
                    if (tokens[currPos + 1].Type == TokenType.If && tokens[currPos + 2].Type == TokenType.OpeningGroup) // ELSE IF
                    {
                        int closingCond = compiler.FindToken(tokens, TokenType.ClosingGroup, currPos + 3);
                        if (tokens[closingCond + 1].Type == TokenType.OpeningQuote)
                        {
                            int closingQuote = compiler.FindToken(tokens, TokenType.ClosingQuote, closingCond + 2);
                            ASTBase condition = compiler.GetAST(tokens.GetRange(currPos + 2, closingCond - currPos - 1));
                            ASTBase[] ifASTs = compiler.GetASTs(tokens.GetRange(closingCond + 2, closingQuote - closingCond - 2));
                            currPos = closingQuote + 1;
                            asts.Add((ifASTs, condition));
                            continue;
                        }

                    }
                    else if (tokens[currPos + 1].Type == TokenType.OpeningQuote) // ELSE
                    {
                        int closingQuote = compiler.FindToken(tokens, TokenType.ClosingQuote, currPos + 2);
                        if (closingQuote == tokens.Count - 1)
                        {
                            ASTBase[] elseASTs = compiler.GetASTs(tokens.GetRange(currPos + 2, closingQuote - currPos - 2));
                            return new IfAST(asts.ToArray(), elseASTs) { Index = tokens.First().Index };
                        }
                    }
                }
                throw new CompilerException("Unknown construction", tokens[currPos].Index);
            }
        }
    }

    public class WhileRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.While;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            int bracketBegin = compiler.FindToken(tokens, TokenType.OpeningQuote, 0);
            if (bracketBegin < 2 || tokens[bracketBegin - 1].Type != TokenType.ClosingGroup)
            {
                throw new CompilerException("Unknown construction", tokens.First().Index);
            }
            ASTBase condition = compiler.GetAST(tokens.GetRange(1, bracketBegin - 1));
            ASTBase[] asts = compiler.GetASTs(tokens.GetRange(bracketBegin + 1, tokens.Count - bracketBegin - 2));
            return new WhileAST(condition, asts) { Index = tokens.First().Index };
        }
    }

    public class ForRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.For;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            int bracketBegin = compiler.FindToken(tokens, TokenType.OpeningQuote, 0);
            if (bracketBegin < 2 || tokens[bracketBegin - 1].Type != TokenType.ClosingGroup ||
                tokens[1].Type != TokenType.Name || tokens[2].Type != TokenType.In)
            {
                throw new CompilerException("Unknown construction", tokens.First().Index);
            }
            string name = tokens[1].Value;
            ASTBase enumerable = compiler.GetAST(tokens.GetRange(3, bracketBegin - 3));
            ASTBase[] asts = compiler.GetASTs(tokens.GetRange(bracketBegin + 1, tokens.Count - bracketBegin - 2));
            return new ForAST(name, enumerable, asts) { Index = tokens.First().Index };
        }
    }

    public class LoopRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.Loop;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            if (tokens[1].Type != TokenType.OpeningQuote || tokens.Last().Type != TokenType.ClosingQuote)
            {
                throw new CompilerException("Unknown construction", tokens.First().Index);
            }
            ASTBase[] asts = compiler.GetASTs(tokens.GetRange(2, tokens.Count - 3));
            return new LoopAST(asts) { Index = tokens.First().Index };
        }
    }

    public class BreakRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.Break;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            if (tokens.Count == 1)
            {
                return new BreakAST(new ConstantAST(null) { Index = tokens.First().Index }) { Index = tokens.First().Index };
            }
            if (tokens[1].Type != TokenType.OpeningGroup || tokens.Last().Type != TokenType.ClosingGroup)
            {
                throw new CompilerException("If break expression returns value, it must be in brackets", tokens.First().Index);
            }
            ASTBase ast = compiler.GetAST(tokens.GetRange(1, tokens.Count - 1));
            return new BreakAST(ast) { Index = tokens.First().Index };
        }
    }

    public class ReturnRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.Return;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            if (tokens.Count == 1)
            {
                return new ReturnAST(new ConstantAST(null) { Index = tokens.First().Index }) { Index = tokens.First().Index };
            }
            if (tokens[1].Type != TokenType.OpeningGroup || tokens.Last().Type != TokenType.ClosingGroup)
            {
                throw new CompilerException("If return expression returns value, it must be in brackets", tokens.First().Index);
            }
            ASTBase ast = compiler.GetAST(tokens.GetRange(1, tokens.Count - 1));
            return new ReturnAST(ast) { Index = tokens.First().Index };
        }
    }
    
    public class OutputRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.Output;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            if (tokens.Count == 1)
            {
                return new ReturnAST(new ConstantAST(null) { Index = tokens.First().Index }) { Index = tokens.First().Index };
            }
            if (tokens[1].Type != TokenType.OpeningGroup || tokens.Last().Type != TokenType.ClosingGroup)
            {
                throw new CompilerException("If output expression returns value, it must be in brackets", tokens.First().Index);
            }
            ASTBase ast = compiler.GetAST(tokens.GetRange(1, tokens.Count - 1));
            return new OutputAST(ast) { Index = tokens.First().Index };
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

    public class TryCatchRule : ParsingRule
    {
        public override bool IsRight(List<Token> tokens, Compiler compiler)
        {
            return tokens.Count > 0 && tokens.First().Type == TokenType.Try && compiler.FindToken(tokens, TokenType.Catch, 1) > 0;
        }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            if (tokens[1].Type == TokenType.OpeningQuote)
            {
                int tryEnd = compiler.FindToken(tokens, TokenType.ClosingQuote, 2);
                if (tryEnd > 0 && tokens[tryEnd + 1].Type == TokenType.Catch &&
                    tokens[tryEnd + 2].Type == TokenType.Name &&
                    tokens[tryEnd + 3].Type == TokenType.OpeningQuote)
                {
                    string exceptionVar = tokens[tryEnd + 2].Value;
                    int catchEnd = compiler.FindToken(tokens, TokenType.ClosingQuote, tryEnd + 4);
                    if (catchEnd == tokens.Count - 1)
                    {
                        return new TryCatchAST(exceptionVar,
                            compiler.GetASTs(tokens.GetRange(2, tryEnd - 2)),
                            compiler.GetASTs(tokens.GetRange(tryEnd + 4, catchEnd - tryEnd - 4)));
                    }
                }
            }
            throw new CompilerException("Unknown construction", tokens.First().Index);
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
        public ListRule() : base(TokenType.OpeningIndex, TokenType.ClosingIndex, TokenType.Sep) { }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            (ASTBase type, ASTBase[] items) = GetTypeAndItems(tokens, compiler);

            return new ListAST(type ?? new GetVariableAST("object"), items);
        }
    }

    public class ArrayRule : ListLikeRule
    {
        public ArrayRule() : base(TokenType.OpeningQuote, TokenType.ClosingQuote, TokenType.Sep) { }

        public override ASTBase GetAST(List<Token> tokens, Compiler compiler)
        {
            (ASTBase type, ASTBase[] items) = GetTypeAndItems(tokens, compiler);

            return new ArrayAST(type ?? new GetVariableAST("object"), items);
        }
    }
}
