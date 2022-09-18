using System;

namespace NetScript.Interpreter
{
    public abstract class Function
    {
        public abstract Context GetContext(object[] args, Type[] generics);
    }

    public class CustomFunction : Function
    {
        public string[] Generics { get; }
        public string[] Args { get; }
        public byte[] Code { get; }
        public VariableCollection Variables { get; }
        
        private Package Package { get; }

        public CustomFunction(string[] generics, string[] args, byte[] funcBc, Package pkg, VariableCollection vars)
        {
            Generics = generics;
            Args = args;
            Code = funcBc;
            Package = pkg;
            Variables = vars;
        }

        public override Context GetContext(object[] args, Type[] generics)
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
            return new Context(new ByteArrayReader(Code)) { Variables = vars, Parent = Package.Current, Type = ContextType.Function };
        }

        public object Invoke(object[] args, Type[] generics)
        {
            Context cont = GetContext(args, generics);
            Package clone = Package.Clone();
            clone.Add(cont);
            InterpreterNS.Execute(clone);
            return cont.Return;
        }
    }

    public class GenericFunction : Function
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

        public override Context GetContext(object[] args, Type[] generics)
        {
            if (generics?.Length > 0)
            {
                throw new Exception("Double generics are not allowed");
            }
            else
            {
                return Base.GetContext(args, Generics);
            }
        }

        public object Invoke(object[] args)
        {
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
}