using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NetScript.Interpretation
{
    public abstract class Function
    {
        public abstract string Name { get; }

        public abstract FunctionResult GetResult(object?[]? args, Type[]? generics);

        public override string ToString()
        {
            return Name;
        }
		
		#region ToAction
		public static Action ToAction(InvokableFunction i) => () => i.Invoke(null, null);
            
		public static Action<T1> ToAction<T1>(InvokableFunction func) =>
			(a) => func.Invoke(new object?[] { a }, null);

		public static Action<T1, T2> ToAction<T1, T2>(InvokableFunction func) =>
			(a, b) => func.Invoke(new object?[] { a, b }, null);

		public static Action<T1, T2, T3> ToAction<T1, T2, T3>(InvokableFunction func) =>
			(a, b, c) => func.Invoke(new object?[] { a, b, c }, null);

		public static Action<T1, T2, T3, T4> ToAction<T1, T2, T3, T4>(InvokableFunction func) =>
			(a, b, c, d) => func.Invoke(new object?[] { a, b, c, d }, null);

		public static Action<T1, T2, T3, T4, T5> ToAction<T1, T2, T3, T4, T5>(InvokableFunction func) =>
			(a, b, c, d, e) => func.Invoke(new object?[] { a, b, c, d, e }, null);

		public static Action<T1, T2, T3, T4, T5, T6> ToAction<T1, T2, T3, T4, T5, T6>(InvokableFunction func) =>
			(a, b, c, d, e, f) => func.Invoke(new object?[] { a, b, c, d, e, f }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7> ToAction<T1, T2, T3, T4, T5, T6, T7>(InvokableFunction func) =>
			(a, b, c, d, e, f, g) => func.Invoke(new object?[] { a, b, c, d, e, f, g }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7, T8> ToAction<T1, T2, T3, T4, T5, T6, T7, T8>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h) => func.Invoke(new object?[] { a, b, c, d, e, f, g, h }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> ToAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i) => func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ToAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j) => func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ToAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k) => func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ToAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l) => func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ToAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l, m) => func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l, m }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ToAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l, m, n) => func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ToAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) => func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o }, null);

		public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ToAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p }, null);
		#endregion

		#region ToFunc
        public static Func<TResult> ToFunc<TResult>(InvokableFunction i) => () => (TResult)i.Invoke(null, null);

		public static Func<T1, TResult> ToFunc<T1, TResult>(InvokableFunction func) =>
		  (a) => (TResult)func.Invoke(new object?[] { a }, null);

		public static Func<T1, T2, TResult> ToFunc<T1, T2, TResult>(InvokableFunction func) =>
			(a, b) => (TResult)func.Invoke(new object?[] { a, b }, null);

		public static Func<T1, T2, T3, TResult> ToFunc<T1, T2, T3, TResult>(InvokableFunction func) =>
			(a, b, c) => (TResult)func.Invoke(new object?[] { a, b, c }, null);

		public static Func<T1, T2, T3, T4, TResult> ToFunc<T1, T2, T3, T4, TResult>(InvokableFunction func) =>
			(a, b, c, d) => (TResult)func.Invoke(new object?[] { a, b, c, d }, null);

		public static Func<T1, T2, T3, T4, T5, TResult> ToFunc<T1, T2, T3, T4, T5, TResult>(InvokableFunction func) =>
			(a, b, c, d, e) => (TResult)func.Invoke(new object?[] { a, b, c, d, e }, null);

		public static Func<T1, T2, T3, T4, T5, T6, TResult> ToFunc<T1, T2, T3, T4, T5, T6, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g, h }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l, m) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l, m }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l, m, n) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o }, null);

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(InvokableFunction func) =>
			(a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) => (TResult)func.Invoke(new object?[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p }, null);
		#endregion

        public static Delegate ToDelegate<T>(InvokableFunction func) where T : Delegate
        {
            Type[] sign = typeof(T).GetMethod(nameof(Action.Invoke)).GetParameters().Select(p => p.ParameterType).ToArray();
            Type ret = typeof(T).GetMethod(nameof(Action.Invoke)).ReturnType;
            if (ret == typeof(void))
            {
                MethodInfo method = (from m in typeof(Function).GetMethods()
                                     where m.Name == nameof(ToAction) && m.GetGenericArguments().Length == sign.Length
                                     select m)
                                     .First()
                                     .MakeGenericMethod(sign);
                Delegate action = (Delegate)method.Invoke(null, new object[] { func });
                return Delegate.CreateDelegate(typeof(T), action.Target, action.Method);
            }
            else
            {
                Type[] invokeGenerics = new Type[sign.Length + 1];
                sign.CopyTo(invokeGenerics, 0);
                invokeGenerics[^1] = ret;
                MethodInfo method = (from m in typeof(Function).GetMethods()
                                     where m.Name == nameof(ToFunc) && m.GetGenericArguments().Length == invokeGenerics.Length
                                     select m)
                                     .First()
                                     .MakeGenericMethod(invokeGenerics);
                Delegate function = (Delegate)method.Invoke(null, new object[] { func });
                return Delegate.CreateDelegate(typeof(T), function.Target, function.Method);
            }
        }
    }

    public abstract class InvokableFunction : Function
    {
        public abstract object? Invoke(object?[]? args, Type[]? generics);
    }

    public class CustomFunction : InvokableFunction
    {
        public override string Name { get; }
        public string[] Generics { get; }
        public string[] Args { get; }
        public byte[] Code { get; }
        public VariableCollection Variables { get; }
        
        private Runtime Package { get; }

        public CustomFunction(string name, string[] generics, string[] args, byte[] funcBc, Runtime pkg, VariableCollection vars)
        {
            Name = name;
            Generics = generics;
            Args = args;
            Code = funcBc;
            Package = pkg;
            Variables = vars;
        }

        public override FunctionResult GetResult(object?[]? args, Type[]? generics)
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
            return new(new Context(Name, new ByteArrayReader(Code)) { Variables = vars, Parent = Package.Current, Type = ContextType.Function });
        }

        public override object? Invoke(object?[]? args, Type[]? generics)
        {
            Context cont = (Context)GetResult(args, generics).Value;
            Runtime clone = Package.Clone();
            clone.Add(cont);
            Interpreter.Execute(clone);
            return cont.Return;
        }
    }

    public class GenericFunction : InvokableFunction
    {
        public override string Name { get; }
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
            Name = $"{Base.Name}<[{string.Join(", ", Generics as object[])}]>";
        }

        public override FunctionResult GetResult(object?[]? args, Type[]? generics)
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

        public override object? Invoke(object?[]? args, Type[]? generics)
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