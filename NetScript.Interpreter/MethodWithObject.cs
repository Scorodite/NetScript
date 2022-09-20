using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScript.Interpreter
{
    /// <summary>
    /// MethodInfos with their owner
    /// </summary>
    public class MethodWithObject
    {
        public object Obj { get; }
        public MethodInfo[] Methods { get; }

        public MethodWithObject(object obj, MethodInfo[] methods)
        {
            Obj = obj;
            Methods = methods;
        }

        public object Invoke(object[] args)
        {
            Type[] sign = new Type[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                sign[i] = args[i]?.GetType() ?? typeof(void);
            }

            foreach (MethodInfo method in Methods)
            {
                Type[] methodSign = method.GetParameters().Select(m => m.ParameterType).ToArray();
                if (InterpreterNS.CompareSigns(sign, methodSign))
                {
                    return method.Invoke(Obj, args);
                }
            }

            throw new Exception($"Can not invoke {Methods.First().Name} with {string.Join(", ", sign as object[])}");
        }

        public MethodWithObject ToGeneric(Type[] args)
        {
            List<MethodInfo> res = new();

            foreach (MethodInfo method in from m in Methods where
                                          m.IsGenericMethodDefinition &&
                                          m.GetGenericArguments().Length == args.Length
                                          select m)
            {
                Type[] gens = method.GetGenericArguments();
                bool success = true;
                for (int i = 0; i < args.Length; i++)
                {
                    Type genBase = gens[i].BaseType;
                    if (args[i] != genBase && !args[i].IsSubclassOf(genBase))
                    {
                        success = false;
                        break;
                    }
                }
                if (success)
                {
                    res.Add(method.MakeGenericMethod(args));
                }
            }
            if (res.Count > 0)
            {
                return new(Obj, res.ToArray());
            }
            else
            {
                throw new Exception($"Failed to make generic {Methods.First().Name}");
            }
        }
    }
}
