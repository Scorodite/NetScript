using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScript.Compilation.Tokens
{
    public class Token
    {
        public string Value { get; set; }
        public TokenType Type { get; set; }
        public int Index { get; set; }

        public Token(string value, TokenType type, int index)
        {
            Value = value;
            Type = type;
            Index = index;
        }

        public override string ToString()
        {
            return $"[{Type}:{Value}]";
        }
    }
}
