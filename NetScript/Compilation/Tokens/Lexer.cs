using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NetScript.Compilation.Tokens
{
    /// <summary>
    /// Lexer. It converts string in tokens
    /// </summary>
    public static class Lexer
    {
        public static List<Token> LexString(string code, IEnumerable<TokenExpression> expressions)
        {
            List<Token> result = new();

            for (int i = 0; i < code.Length;)
            {
                bool isMatch = false;
                foreach (TokenExpression expr in expressions)
                {
                    Match m = expr.Expression.Match(code, i);

                    if (m.Success && m.Index == i)
                    {
                        isMatch = true;
                        if (expr.Type != TokenType.Skip)
                        {
                            result.Add(new(m.Value, expr.Type, i));
                        }
                        i += m.Length;
                        break;
                    }
                }
                if (!isMatch)
                {
                    throw new CompilerException($"Failed to match token", i);
                }
            }

            return result;
        }
    }
}
