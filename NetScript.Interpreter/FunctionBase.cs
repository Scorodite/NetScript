using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NetScript.Interpreter
{
    public abstract class Function
    {
        public static readonly Function ToAction = new ToActionFunction();
        public static readonly Function ToFunc = new ToFuncFunction();
        public static readonly Function ToDelegate = new ToDelegateFunction();

        public abstract FunctionResult GetResult(object[] args, Type[] generics);

        private class ToActionFunction : Function
        {
            public override FunctionResult GetResult(object[] args, Type[] generics)
            {
                if (args.Length != 1 || args[0] is not InvokableFunction invokable)
                {
                    throw new Exception($"{nameof(Function)}.{nameof(ToAction)} requires {nameof(InvokableFunction)} as argument");
                }
                MethodInfo convert = typeof(ToActionFunction).GetMethods()
                    .First(mi => mi.Name == nameof(Convert) && mi.GetGenericArguments().Length == (generics?.Length ?? 0))
                    .MakeGenericMethod(generics);
                Delegate res = convert.Invoke(null, new object[] { invokable }) as Delegate;
                return new(res);
            }

            public static Action Convert(InvokableFunction i) => () => i.Invoke(null, null);
            
            public static Action<T1> Convert<T1>(InvokableFunction func) =>
                (a) => func.Invoke(new object[] { a }, null);

            public static Action<T1, T2> Convert<T1, T2>(InvokableFunction func) =>
                (a, b) => func.Invoke(new object[] { a, b }, null);

            public static Action<T1, T2, T3> Convert<T1, T2, T3>(InvokableFunction func) =>
                (a, b, c) => func.Invoke(new object[] { a, b, c }, null);

            public static Action<T1, T2, T3, T4> Convert<T1, T2, T3, T4>(InvokableFunction func) =>
                (a, b, c, d) => func.Invoke(new object[] { a, b, c, d }, null);

            public static Action<T1, T2, T3, T4, T5> Convert<T1, T2, T3, T4, T5>(InvokableFunction func) =>
                (a, b, c, d, e) => func.Invoke(new object[] { a, b, c, d, e }, null);

            public static Action<T1, T2, T3, T4, T5, T6> Convert<T1, T2, T3, T4, T5, T6>(InvokableFunction func) =>
                (a, b, c, d, e, f) => func.Invoke(new object[] { a, b, c, d, e, f }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7> Convert<T1, T2, T3, T4, T5, T6, T7>(InvokableFunction func) =>
                (a, b, c, d, e, f, g) => func.Invoke(new object[] { a, b, c, d, e, f, g }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7, T8> Convert<T1, T2, T3, T4, T5, T6, T7, T8>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h) => func.Invoke(new object[] { a, b, c, d, e, f, g, h }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i) => func.Invoke(new object[] { a, b, c, d, e, f, g, h, i }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j) => func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k) => func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l) => func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l, m) => func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l, m }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) => func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o }, null);

            public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p }, null);
        }

        private class ToFuncFunction : Function
        {
            public override FunctionResult GetResult(object[] args, Type[] generics)
            {
                if (args.Length != 1 || args[0] is not InvokableFunction invokable)
                {
                    throw new Exception($"{nameof(Function)}.{nameof(ToFunc)} requires {nameof(InvokableFunction)} as argument");
                }
                MethodInfo convert = typeof(ToFuncFunction).GetMethods()
                    .First(mi => mi.Name == nameof(Convert) && mi.GetGenericArguments().Length == (generics?.Length ?? 0))
                    .MakeGenericMethod(generics);
                Delegate res = convert.Invoke(null, new object[] { invokable }) as Delegate;
                return new(res);
            }

            public static Func<TResult> Convert<TResult>(InvokableFunction i) => () => (TResult)i.Invoke(null, null);

            public static Func<T1, TResult> Convert<T1, TResult>(InvokableFunction func) =>
              (a) => (TResult)func.Invoke(new object[] { a }, null);

            public static Func<T1, T2, TResult> Convert<T1, T2, TResult>(InvokableFunction func) =>
                (a, b) => (TResult)func.Invoke(new object[] { a, b }, null);

            public static Func<T1, T2, T3, TResult> Convert<T1, T2, T3, TResult>(InvokableFunction func) =>
                (a, b, c) => (TResult)func.Invoke(new object[] { a, b, c }, null);

            public static Func<T1, T2, T3, T4, TResult> Convert<T1, T2, T3, T4, TResult>(InvokableFunction func) =>
                (a, b, c, d) => (TResult)func.Invoke(new object[] { a, b, c, d }, null);

            public static Func<T1, T2, T3, T4, T5, TResult> Convert<T1, T2, T3, T4, T5, TResult>(InvokableFunction func) =>
                (a, b, c, d, e) => (TResult)func.Invoke(new object[] { a, b, c, d, e }, null);

            public static Func<T1, T2, T3, T4, T5, T6, TResult> Convert<T1, T2, T3, T4, T5, T6, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g, h }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g, h, i }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l, m) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l, m }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l, m, n) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o }, null);

            public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(InvokableFunction func) =>
                (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => (TResult)func.Invoke(new object[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p }, null);
        }

        private class ToDelegateFunction : Function
        {
            public override FunctionResult GetResult(object[] args, Type[] generics)
            {
                if (generics is null || args is null || generics.Length != 1 || args.Length != 1 ||
                    (args.First() is not InvokableFunction && args.First() is not Delegate) ||
                    !generics.First().IsSubclassOf(typeof(Delegate)))
                {
                    throw new Exception($"{nameof(Function)}.{nameof(ToDelegate)} requires {nameof(InvokableFunction)} or {nameof(Delegate)} as argument and {nameof(Delegate)} as generic");
                }
                Type target = generics.First();
                if (args.First() is Delegate deleg)
                {
                    return new(Delegate.CreateDelegate(target, deleg.Method));
                }
                else if (args.First() is InvokableFunction invokable)
                {
                    Type[] sign = target.GetMethod(nameof(Action.Invoke)).GetParameters().Select(p => p.ParameterType).ToArray();
                    Type ret = target.GetMethod(nameof(Action.Invoke)).ReturnType;
                    if (ret == typeof(void))
                    {
                        MethodInfo method = typeof(ToActionFunction).GetMethods()
                            .First(m => m.Name == nameof(ToActionFunction.Convert) && m.GetGenericArguments().Length == sign.Length)
                            .MakeGenericMethod(sign);
                        Delegate action = (Delegate)method.Invoke(null, new object[] { invokable });
                        return new(Delegate.CreateDelegate(target, action.Target, action.Method));
                    }
                    else
                    {
                        Type[] invokeGenerics = new Type[sign.Length + 1];
                        sign.CopyTo(invokeGenerics, 0);
                        invokeGenerics[^1] = ret;
                        MethodInfo method =
                            typeof(ToFuncFunction).GetMethods()
                            .First(m => m.Name == nameof(ToFuncFunction.Convert) && m.GetGenericArguments().Length == sign.Length)
                            .MakeGenericMethod(invokeGenerics);
                        Delegate func = (Delegate)method.Invoke(null, new object[] { invokable });
                        return new(Delegate.CreateDelegate(target, func.Target, func.Method));
                    }
                }
                else
                {
                    return new(null as object);
                }
            }
        }
    }

    public abstract class InvokableFunction : Function
    {
        public abstract object Invoke(object[] args, Type[] generics);
    }

    public class CustomFunction : InvokableFunction
    {
        public string[] Generics { get; }
        public string[] Args { get; }
        public byte[] Code { get; }
        public VariableCollection Variables { get; }
        
        private Runtime Package { get; }

        public CustomFunction(string[] generics, string[] args, byte[] funcBc, Runtime pkg, VariableCollection vars)
        {
            Generics = generics;
            Args = args;
            Code = funcBc;
            Package = pkg;
            Variables = vars;
        }

        public override FunctionResult GetResult(object[] args, Type[] generics)
        {
            if ((generics is null || generics.Length == 0) != (Generics.Length == 0))
            {
                throw new Exception($"This function requires {Generics.Length} generics, not {generics?.Length ?? 0}");
            }
            if ((args is null || args.Length == 0) != (Args.Length == 0))
            {
                throw new Exception($"This function requires {Args.Length} arguments, not {args?.Length ?? 0}");
            }
            VariableCollection vars = new() { Parent = Variables };
            if (args is not null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    vars.Add(Args[i], args[i]);
                }
            }
            if (generics is not null)
            {
                for (int i = 0; i < generics.Length; i++)
                {
                    vars.Add(Generics[i], generics[i]);
                }
            }
            return new(new Context(new ByteArrayReader(Code)) { Variables = vars, Parent = Package.Current, Type = ContextType.Function });
        }

        public override object Invoke(object[] args, Type[] generics)
        {
            Context cont = GetResult(args, generics).Value as Context;
            Runtime clone = Package.Clone();
            clone.Add(cont);
            InterpreterNS.Execute(clone);
            return cont.Return;
        }
    }

    public class GenericFunction : InvokableFunction
    {
        public Function Base { get; }
        public Type[] Generics { get; }

        public GenericFunction(Function baseFunc, Type[] gens)
        {
            if (Base is GenericFunction)
            {
                throw new Exception("Double generics are not allowed");
            }
            Base = baseFunc;
            Generics = gens;
        }

        public override FunctionResult GetResult(object[] args, Type[] generics)
        {
            if (generics?.Length > 0)
            {
                throw new Exception("Double generics are not allowed");
            }
            else
            {
                return Base.GetResult(args, Generics);
            }
        }

        public override object Invoke(object[] args, Type[] generics)
        {
            if (generics is not null && generics.Length != 0)
            {
                throw new Exception("Double generics are not allowed");
            }
            if (Base is CustomFunction custom)
            {
                return custom.Invoke(args, Generics);
            }
            else
            {
                throw new Exception($"Can only invoke {nameof(CustomFunction)}");
            }
        }
    }

    public struct FunctionResult
    {
        public bool IsContext;
        public object Value;

        public FunctionResult(Context ctxt)
        {
            IsContext = true;
            Value = ctxt;
        }

        public FunctionResult(object obj)
        {
            IsContext = false;
            Value = obj;
        }
    }
}