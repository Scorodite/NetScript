using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScript.Interpretation
{
    public static class Extensions
    {
        public static object PopNotNull(this System.Collections.Stack s)
        {
            if (s.Pop() is object res)
            {
                return res;
            }
            throw new NullReferenceException();
        }
    }
}
