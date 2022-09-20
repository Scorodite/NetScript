using System;

namespace NetScript.Interpreter
{
    /// <summary>
    /// Exception that is generated if "throw" expression throws object that is not inherited from Exception
    /// </summary>
    class NotThrowableException : Exception
    {
        public object Object { get; }

        public NotThrowableException(object obj) : base(obj.ToString())
        {
            Object = obj;
        }
    }
}