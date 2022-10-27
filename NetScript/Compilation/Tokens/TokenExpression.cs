using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetScript.Compilation.Tokens
{
    public class TokenExpression
    {
        public Regex Expression { get; set; }
        public TokenType Type { get; set; }

        public TokenExpression(Regex expression, TokenType type)
        {
            Expression = expression;
            Type = type;
        }

        public TokenExpression(string expression, TokenType type)
        {
            Expression = new(expression);
            Type = type;
        }
    }
}
