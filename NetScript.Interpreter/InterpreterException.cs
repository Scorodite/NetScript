using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScript.Interpreter
{
    public class InterpreterException : Exception
    {
        public InterpreterException(string msg, Exception ex) : base(msg, ex) { }

        public override string ToString()
        {
            return Message;
        }
    }
}
