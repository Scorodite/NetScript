using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScript.Compiler
{
    public class CompilerException : Exception
    {
        public int Index { get; }

        public CompilerException(string message) : this(message, -1) { }
 
        public CompilerException(string message, int index) : base(message)
        {
            Index = index;
        }
    }
}
