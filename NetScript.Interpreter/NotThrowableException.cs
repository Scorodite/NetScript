using System;

namespace NetScript.Interpreter
{
    class NotThrowableException : Exception
    {
        public object Object { get; }

        public NotThrowableException(object obj) : base(obj.ToString())
        {
            Object = obj;
        }
    }
}