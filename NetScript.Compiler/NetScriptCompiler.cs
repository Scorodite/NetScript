using NetScript.Compiler.Tokens;
using NetScript.Compiler.Rules;
using NetScript.Compiler.AST;
using NetScript.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;

namespace NetScript.Compiler
{
    public class NetScriptCompiler : Compiler
    {
        public override List<(Regex, TokenType)> Expressions { get; }
        public override List<SpecialConstructionRule> SpecialRules { get; }
        public override List<ParsingRule> Rules { get; }
        public override List<ICollection<BinaryOperationRule>> BinaryRules { get; }
        public override List<UnaryOperationRule> UnaryRules { get; }

        public NetScriptCompiler()
        {
            Expressions = new();
            SpecialRules = new();
            Rules = new();
            BinaryRules = new();
            UnaryRules = new();

            BuildExpressions();
            BuildSpecialRules();
            BuildRules();
            BuildBinaryRules();
            BuildUnaryRules();
        }

        protected virtual void BuildExpressions()
        {
            Expressions.Add((new(@"//.+|/\*(.|\n)+?\*/|\s+"), TokenType.Skip));
            Expressions.Add((new(@";"), TokenType.EOL));
            Expressions.Add((new(@"\d+[Bb]\b"), TokenType.Byte));
            Expressions.Add((new(@"\d+[Ss][Bb]\b"), TokenType.Sbyte));
            Expressions.Add((new(@"\d+[Ss]\b"), TokenType.Short));
            Expressions.Add((new(@"\d+[Uu][Ss]\b"), TokenType.Ushort));
            Expressions.Add((new(@"\d+[Uu][Ii]\b"), TokenType.Uint));
            Expressions.Add((new(@"\d+[Ll]\b"), TokenType.Long));
            Expressions.Add((new(@"\d+[Uu][Ll]\b"), TokenType.Ulong));
            Expressions.Add((new(@"\d+(\.\d+)?[Ff]\b"), TokenType.Float));
            Expressions.Add((new(@"\d+(\.\d+)?[Dd][Ee]\b"), TokenType.Decimal));
            Expressions.Add((new(@"\d+\.\d+[Dd]?\b|\d+[Dd]\b"), TokenType.Double));
            Expressions.Add((new(@"\d+\b"), TokenType.Int));
            Expressions.Add((new(@"(true|false)\b"), TokenType.Bool));
            Expressions.Add((new(@"null\b"), TokenType.Null));
            Expressions.Add((new(@"""(\\.|[^""\\\n])*"""), TokenType.String));
            Expressions.Add((new(@"'(\\.|[^'\\\n])*'"), TokenType.Char));
            Expressions.Add((new(@"<\["), TokenType.OpeningGeneric));
            Expressions.Add((new(@"\]>"), TokenType.ClosingGeneric));
            Expressions.Add((new(@"\{"), TokenType.OpeningQuote));
            Expressions.Add((new(@"\}"), TokenType.ClosingQuote));
            Expressions.Add((new(@"\("), TokenType.OpeningGroup));
            Expressions.Add((new(@"\)"), TokenType.ClosingGroup));
            Expressions.Add((new(@"\["), TokenType.OpeningIndex));
            Expressions.Add((new(@"\]"), TokenType.ClosingIndex));
            Expressions.Add((new(@"\+"), TokenType.Sum));
            Expressions.Add((new(@"-"), TokenType.Sub));
            Expressions.Add((new(@"\*"), TokenType.Mul));
            Expressions.Add((new(@"/"), TokenType.Div));
            Expressions.Add((new(@"%"), TokenType.Mod));
            Expressions.Add((new(@"&&"), TokenType.And));
            Expressions.Add((new(@"\|\|"), TokenType.Or));
            Expressions.Add((new(@"&"), TokenType.BinAnd));
            Expressions.Add((new(@"\|"), TokenType.BinOr));
            Expressions.Add((new(@"\^"), TokenType.BinXor));
            Expressions.Add((new(@"=="), TokenType.Equal));
            Expressions.Add((new(@"!="), TokenType.NotEqual));
            Expressions.Add((new(@"<="), TokenType.LessOrEqual));
            Expressions.Add((new(@">="), TokenType.GreaterOrEqual));
            Expressions.Add((new(@">"), TokenType.Greater));
            Expressions.Add((new(@"<"), TokenType.Less));
            Expressions.Add((new(@"is\s+not\b"), TokenType.IsNotType));
            Expressions.Add((new(@"is\b"), TokenType.IsType));
            Expressions.Add((new(@"\?\?"), TokenType.NullCoalescing));
            Expressions.Add((new(@"to\b"), TokenType.Convert));
            Expressions.Add((new(@"typeof\b"), TokenType.GetType));
            Expressions.Add((new(@"nameof\b"), TokenType.GetName));
            Expressions.Add((new(@"default\b"), TokenType.Default));
            Expressions.Add((new(@"~"), TokenType.BinRev));
            Expressions.Add((new(@"!"), TokenType.Not));
            Expressions.Add((new(@"\.\."), TokenType.Range));
            Expressions.Add((new(@"="), TokenType.Assign));
            Expressions.Add((new(@"\."), TokenType.Field));
            Expressions.Add((new(@","), TokenType.Sep));
            Expressions.Add((new(@":"), TokenType.Clarification));
            Expressions.Add((new(@"import\s+\w+((\.\w+)*\.\*|(\.\w+)+(`\d+)?)"), TokenType.Import));
            Expressions.Add((new(@"var\b"), TokenType.NewVariable));
            Expressions.Add((new(@"func\b"), TokenType.Function));
            Expressions.Add((new(@"if\b"), TokenType.If));
            Expressions.Add((new(@"else\b"), TokenType.Else));
            Expressions.Add((new(@"while\b"), TokenType.While));
            Expressions.Add((new(@"for\b"), TokenType.For));
            Expressions.Add((new(@"in\b"), TokenType.In));
            Expressions.Add((new(@"return\b"), TokenType.Return));
            Expressions.Add((new(@"break\b"), TokenType.Break));
            Expressions.Add((new(@"continue\b"), TokenType.Continue));
            Expressions.Add((new(@"output\b"), TokenType.Output));
            Expressions.Add((new(@"throw\b"), TokenType.Throw));
            Expressions.Add((new(@"loop\b"), TokenType.Loop));
            Expressions.Add((new(@"try\b"), TokenType.Try));
            Expressions.Add((new(@"catch\b"), TokenType.Catch));
            Expressions.Add((new(@"loaddll\b"), TokenType.LoadDll));
            Expressions.Add((new(@"[A-Za-zА-Яа-я_][A-Za-zА-Яа-я_0-9]*"), TokenType.Name));
        }

        protected virtual void BuildSpecialRules()
        {
            SpecialRules.Add(new IfRule());
            SpecialRules.Add(new LoopRule());
            SpecialRules.Add(new ReturnRule());
            SpecialRules.Add(new BreakRule());
            SpecialRules.Add(new OutputRule());
            SpecialRules.Add(new ThrowRule());
            SpecialRules.Add(new WhileRule());
            SpecialRules.Add(new ForRule());
            SpecialRules.Add(new TryCatchRule());
            SpecialRules.Add(new FuncRule());
        }

        protected virtual void BuildRules()
        {
            Rules.Add(new ConstantRule(TokenType.Null, t => null));
            Rules.Add(new ConstantRule(TokenType.Bool, t => t.Value == "true"));
            Rules.Add(new ConstantRule(TokenType.Byte, t => byte.Parse(t.Value[..^1])));
            Rules.Add(new ConstantRule(TokenType.Sbyte, t => sbyte.Parse(t.Value[..^2])));
            Rules.Add(new ConstantRule(TokenType.Short, t => short.Parse(t.Value[..^1])));
            Rules.Add(new ConstantRule(TokenType.Ushort, t => ushort.Parse(t.Value[..^2])));
            Rules.Add(new ConstantRule(TokenType.Int, t => int.Parse(t.Value)));
            Rules.Add(new ConstantRule(TokenType.Uint, t => uint.Parse(t.Value[..^2])));
            Rules.Add(new ConstantRule(TokenType.Long, t => long.Parse(t.Value[..^1])));
            Rules.Add(new ConstantRule(TokenType.Ulong, t => ulong.Parse(t.Value[..^2])));
            Rules.Add(new ConstantRule(TokenType.Float, t => float.Parse(t.Value[..^1], CultureInfo.InvariantCulture)));
            Rules.Add(new ConstantRule(TokenType.Double, t =>
                double.Parse(t.Value.ToLower().EndsWith('d') ? t.Value[..^1] : t.Value, CultureInfo.InvariantCulture)));
            Rules.Add(new ConstantRule(TokenType.Decimal, t => decimal.Parse(t.Value[..^2], CultureInfo.InvariantCulture)));
            Rules.Add(new ConstantRule(TokenType.String, t => Regex.Unescape(t.Value[1..^1])));
            Rules.Add(new ConstantRule(TokenType.Char, t => char.Parse(Regex.Unescape(t.Value[1..^1]))));
            Rules.Add(new SingleTokenRule(TokenType.Continue, t => new ContinueAST()));
            Rules.Add(new SingleTokenRule(TokenType.Name, t => new GetVariableAST(t.Value)));
            Rules.Add(new SingleTokenRule(TokenType.Import, t => new ImportAST(t.Value["import".Length..].Trim())));
            Rules.Add(new SingleTokenRule(TokenType.Field, t => new GetContextValueAST()));
            Rules.Add(new GetFieldRule());
            Rules.Add(new NewVariableRule());
            Rules.Add(new UnnamedFunctionRule());
            Rules.Add(new ArrayRule());
            Rules.Add(new ListRule());
            Rules.Add(new InvokeRule());
            Rules.Add(new GetIndexRule());
            Rules.Add(new GenericRule());
            Rules.Add(new LoadDllRule());
            Rules.Add(new ConstructorRule());
        }

        protected virtual void BuildUnaryRules()
        {
            UnaryRules.Add(new(TokenType.Sub, typeof(RevAST)));
            UnaryRules.Add(new(TokenType.BinRev, typeof(BinRevAST)));
            UnaryRules.Add(new(TokenType.Not, typeof(NotAST)));
            UnaryRules.Add(new(TokenType.GetType, typeof(GetTypeAST)));
            UnaryRules.Add(new(TokenType.GetName, typeof(GetNameAST)));
            UnaryRules.Add(new(TokenType.Default, typeof(DefaultAST)));
        }

        protected virtual void BuildBinaryRules()
        {
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new AssignOperationRule(TokenType.Assign),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.Convert, typeof(ConvertAST)), 
                new(TokenType.Range, typeof(RangeAST)), 
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.NullCoalescing, typeof(NullCoalescingAST)),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.Or, typeof(OrAST)),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.And, typeof(AndAST)),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.BinOr, typeof(BinOrAST)),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.BinXor, typeof(BinXorAST)),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.BinAnd, typeof(BinAndAST)),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.NotEqual, typeof(NotEqualAST)),
                new(TokenType.Equal, typeof(EqualAST)),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.IsType, typeof(IsTypeAST)),
                new(TokenType.IsNotType, typeof(IsNotTypeAST)),
                new(TokenType.Less, typeof(LessAST)),
                new(TokenType.Greater, typeof(GreaterAST)),
                new(TokenType.LessOrEqual, typeof(LessOrEqualAST)),
                new(TokenType.GreaterOrEqual, typeof(GreaterOrEqualAST)),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.Sum, typeof(SumAST)),
                new(TokenType.Sub, typeof(SubAST)),
            });
            BinaryRules.Add(new List<BinaryOperationRule>()
            {
                new(TokenType.Mul, typeof(MulAST)),
                new(TokenType.Div, typeof(DivAST)),
                new(TokenType.Mod, typeof(ModAST)),
            });
        }

        public override void Compile(string code, Stream output)
        {
            using MemoryStream memory = new();
            using BinaryWriter writer = new(memory);

            List<Token> tokens = Lexer.LexString(code, Expressions);
            ASTBase[] asts = GetASTs(tokens);
            CompilerArgs args = new();

            CompileAll(asts, writer, args);

            BinaryWriter outWriter = new(output);

            outWriter.Write(args.Names.Count);
            foreach (string name in args.Names)
            {
                outWriter.Write(name);
            }

            outWriter.Write(args.Dlls.Count);
            foreach (string dll in args.Dlls)
            {
                outWriter.Write(dll);
            }

            outWriter.Write(args.Imports.Count);
            foreach ((string name, Type t) in args.Imports)
            {
                outWriter.Write(args.GetNameID(name));
                outWriter.Write(Type.GetType(t.FullName) is null ? t.AssemblyQualifiedName : t.FullName);
            }

            outWriter.Write(args.Constants.Count);
            foreach (object con in args.Constants)
            {
                WriteConstant(con, outWriter);
            }

            memory.Position = 0;
            memory.CopyTo(output);
        }

        protected virtual void WriteConstant(object con, BinaryWriter writer)
        {
            switch (con)
            {
                case byte v:
                    writer.Write(Bytecode.ConstByte);
                    writer.Write(v);
                    break;
                case sbyte v:
                    writer.Write(Bytecode.ConstSbyte);
                    writer.Write(v);
                    break;
                case short v:
                    writer.Write(Bytecode.ConstShort);
                    writer.Write(v);
                    break;
                case ushort v:
                    writer.Write(Bytecode.ConstUShort);
                    writer.Write(v);
                    break;
                case int v:
                    writer.Write(Bytecode.ConstInt);
                    writer.Write(v);
                    break;
                case uint v:
                    writer.Write(Bytecode.ConstUInt);
                    writer.Write(v);
                    break;
                case long v:
                    writer.Write(Bytecode.ConstLong);
                    writer.Write(v);
                    break;
                case ulong v:
                    writer.Write(Bytecode.ConstULong);
                    writer.Write(v);
                    break;
                case float v:
                    writer.Write(Bytecode.ConstFloat);
                    writer.Write(v);
                    break;
                case decimal v:
                    writer.Write(Bytecode.ConstDecimal);
                    writer.Write(v);
                    break;
                case double v:
                    writer.Write(Bytecode.ConstDouble);
                    writer.Write(v);
                    break;
                case char v:
                    writer.Write(Bytecode.ConstChar);
                    writer.Write(v);
                    break;
                case string v:
                    writer.Write(Bytecode.ConstString);
                    writer.Write(v);
                    break;
                default:
                    throw new CompilerException($"Can not save {con.GetType().FullName} in constants");
            }
        }

        public override ASTBase[] GetASTs(List<Token> tokens)
        {
            List<ASTBase> res = new();

            for (int i = 0; i < tokens.Count; )
            {
                if (tokens[i].Type == TokenType.EOL)
                {
                    i++;
                }
                else if (TryGetSpecialAST(tokens, ref i, out var spec))
                {
                    res.Add(spec);
                }
                else
                {
                    int eolPos = FindToken(tokens, TokenType.EOL, i);
                    if (eolPos == -1) eolPos = tokens.Count;
                    List<Token> astTokens = tokens.GetRange(i, eolPos - i);
                    res.Add(GetAST(astTokens));
                    i = eolPos + 1;
                }
            }

            return res.ToArray();
        }

        public override ASTBase GetAST(List<Token> tokens)
        {
            if (tokens.Count == 0)
            {
                return EmptyAST.Instance;
            }
            if (TryUnwrapGroup(tokens, out var unwraped))
            {
                return GetAST(unwraped);
            }
            {
                int i = 0;
                if (TryGetSpecialAST(tokens, ref i, out var res) && i == tokens.Count)
                {
                    return res;
                }
            }
            if (TryGetBinaryAST(tokens, out var binop))
            {
                return binop;
            }
            if (TryGetUnaryAST(tokens, out var unop))
            {
                return unop;
            }
            if (TryGetNotCountingAST(tokens, out var ncast))
            {
                return ncast;
            }
            throw new CompilerException("Unknown construction", tokens.First().Index);
        }

        protected virtual List<List<Token>> SplitTokens(IEnumerable<Token> tokens)
        {
            List<List<Token>> res = new() { new() };
            Stack<Token> depth = new();

            foreach (Token token in tokens)
            {
                if (IsOpeningBracket(token.Type))
                {
                    depth.Push(token);
                }
                else if (IsClosingBracket(token.Type))
                {
                    if (depth.Count > 0)
                    {
                        Token opening = depth.Pop();
                        if (GetGroupRelative(token.Type) != opening.Type)
                        {
                            throw new CompilerException("Bracket was never closed", opening.Index);
                        }
                    }
                    else
                    {
                        throw new CompilerException("Bracket was never open", token.Index);
                    }
                }
                else if (depth.Count == 0 && token.Type == TokenType.EOL)
                {
                    if (res.Count > 0)
                    {
                        res.Add(new());
                    }
                    continue;
                }
                res.Last().Add(token);
            }

            if (res.Last().Count == 0)
            {
                res.RemoveAt(res.Count - 1);
            }

            return res;
        }

        protected virtual bool TryGetSpecialAST(List<Token> tokens, ref int i, out ASTBase res)
        {
            foreach (SpecialConstructionRule scr in SpecialRules)
            {
                if (scr.IsRight(tokens, i, this))
                {
                    res = scr.GetAST(tokens, ref i, this);
                    return true;
                }
            }
            res = null;
            return false;
        }

        protected virtual bool TryUnwrapGroup(List<Token> tokens, out List<Token> res)
        {
            res = new List<Token>(tokens);

            while (res.First().Type == TokenType.OpeningGroup &&
                res.Last().Type == TokenType.ClosingGroup)
            {
                if (FindToken(res, TokenType.ClosingGroup, 1) == res.Count - 1)
                {
                    res.RemoveAt(0);
                    res.RemoveAt(res.Count - 1);
                }
                else
                {
                    break;
                }
            }

            return res.Count != tokens.Count;
        }

        protected virtual bool TryGetBinaryAST(IList<Token> tokens, out ASTBase res)
        {
            foreach (IEnumerable<BinaryOperationRule> bors in BinaryRules)
            {
                Stack<Token> depth = new();
                if (bors.First().Reverse)
                {
                    for (int i = 0; i < tokens.Count; i++)
                    {
                        if (GetBinaryAST(tokens, depth, bors, i, true) is ASTBase ast)
                        {
                            res = ast;
                            return true;
                        }
                    }
                }
                else
                {
                    for (int i = tokens.Count - 1; i >= 0; i--)
                    {
                        if (GetBinaryAST(tokens, depth, bors, i, false) is ASTBase ast)
                        {
                            res = ast;
                            return true;
                        }
                    }
                }
            }
            res = default;
            return false;
        }

        protected virtual ASTBase GetBinaryAST(IList<Token> tokens, Stack<Token> depth, IEnumerable<BinaryOperationRule> bors, int i, bool rev)
        {
            if (IsClosingBracket(tokens[i].Type) && !rev || IsOpeningBracket(tokens[i].Type) && rev)
            {
                depth.Push(tokens[i]);
                return null;
            }
            else if (IsOpeningBracket(tokens[i].Type) && !rev || IsClosingBracket(tokens[i].Type) && rev)
            {
                if (depth.Count > 0)
                {
                    if (GetGroupRelative(depth.Pop().Type) != tokens[i].Type)
                    {
                        throw new CompilerException("Group was never closed", tokens[i].Index);
                    }
                }
                else
                {
                    throw new CompilerException("Group was never closed", tokens[i].Index);
                }
            }
            else if (depth.Count == 0 && i > 0 && i < tokens.Count - 1)
            {
                foreach (BinaryOperationRule bor in bors)
                {
                    if (bor.IsRight(tokens, i))
                    {
                        if (UnaryRules.Find(uor => uor.Operator == tokens[i - 1].Type) is null &&
                            BinaryRules.Find(uors => uors.FirstOrDefault(uor => uor.Operator == tokens[i - 1].Type) is not null) is null)
                        {
                            return bor.GetAST(tokens, this, i);
                        }
                    }
                }
            }
            return null;
        }

        protected virtual bool TryGetUnaryAST(IList<Token> tokens, out ASTBase res)
        {
            foreach (UnaryOperationRule uor in UnaryRules)
            {
                if (uor.IsRight(tokens))
                {
                    res = uor.GetAST(tokens, this);
                    return true;
                }
            }
            res = default;
            return false;
        }

        protected virtual bool TryGetNotCountingAST(List<Token> tokens, out ASTBase res)
        {
            foreach (ParsingRule rule in Rules)
            {
                if (rule.IsRight(tokens, this))
                {
                    res = rule.GetAST(tokens, this);
                    return true;
                }
            }
            res = default;
            return false;
        }
    }
}
