using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NetScript.Compiler.Tokens
{
    /// <summary>
    /// Lexer. It converts string in tokens
    /// </summary>
    public static class Lexer
    {
        public static List<Token> LexString(string code, IEnumerable<(Regex re, TokenType type)> expressions)
        {
            List<Token> result = new();

            for (int i = 0; i < code.Length;)
            {
                bool isMatch = false;
                foreach ((Regex re, TokenType type) in expressions)
                {
                    Match m = re.Match(code, i);

                    if (m.Success && m.Index == i)
                    {
                        isMatch = true;
                        if (type != TokenType.Skip)
                        {
                            result.Add(new(m.Value, type, i));
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
