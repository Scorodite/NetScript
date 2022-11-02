using System;
using System.IO;
using System.Linq;
using System.Dynamic;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace NetScript.Interpretation
{
    /// <summary>
    /// Class of NetScript bytecode interpreter
    /// </summary>
    public static class Interpreter
    {
        public static VariableCollection Interpret(Stream stream) =>
            Interpret(stream, CancellationToken.None);

        public static VariableCollection Interpret(Stream stream, CancellationToken token)
        {
            Runtime p = new(stream);

            Execute(p, token);
            
            return p.Last().Variables;
        }

        public static void Execute(Runtime p) =>
            Execute(p, CancellationToken.None);

        public static void Execute(Runtime p, CancellationToken token)
        {
            while (p.Count > 0)
            {
                try
                {
                    ExecuteUnsafe(p, token);
                    break;
                }
                catch (Exception _ex)
                {
                    Exception ex = _ex is TargetInvocationException && _ex.InnerException is Exception inner?
                        inner :
                        _ex;
                    if (!p.TryHandle(ex is InterpreterException && ex.InnerException is Exception inner2 ? inner2 : ex))
                    {
                        throw CreateTraceback(p, ex);
                    }
                }
            }

            token.ThrowIfCancellationRequested();
        }

        private static Exception CreateTraceback(Runtime p, Exception ex)
        {
            StringBuilder traceback = new();
            
            for (int i = 0; i < p.Count; i++)
            {
                traceback.Append(new string(' ', i * 2));
                traceback.Append(p[i]?.Name ?? "Unknown");
                traceback.Append('\n');
            }
            string newlines = "\n" + new string(' ', p.Count * 2);
            traceback.Append(new string(' ', p.Count * 2));
            traceback.Append(string.Join(newlines, ex.ToString().Split('\n')));

            return new InterpreterException(
                traceback.ToString(), ex is InterpreterException && ex.InnerException is Exception inner ? inner : ex);
        }

        private static void ExecuteUnsafe(Runtime p, CancellationToken token)
        {
            for (;;)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                while (p.IsEnd)
                {
                    if (!p.ContextAvariable)
                    {
                        return;
                    }
                    else if (p.Current.Type == ContextType.Loop)
                    {
                        p.Stream.Position = p.Current.Begin;
                    }
                    else
                    {
                        if (p.Count == 1)
                        {
                            return;
                        }
                        p.RemoveLast();
                    }
                }

                Bytecode bc = p.Reader.ReadBytecode();

                switch (bc)
                {
                    case Bytecode.ClearStack: p.Stack.Clear(); break;
                    case Bytecode.PopStack: p.Stack.Pop(); break;
                    case Bytecode.PushConst: p.Stack.Push(p.Constants[p.Reader.ReadInt32()]); break;
                    case Bytecode.PushNull: p.Stack.Push(null); break;
                    case Bytecode.PushTrue: p.Stack.Push(true); break;
                    case Bytecode.PushFalse: p.Stack.Push(false); break;
                    case Bytecode.NewVariable: BC_NewVariable(p); break;
                    case Bytecode.SetVariable: BC_SetVariable(p); break;
                    case Bytecode.GetVariable: p.Stack.Push(p.Variables.Get(p.Names[p.Reader.ReadInt32()])); break;
                    case Bytecode.GetField: BC_GetField(p); break;
                    case Bytecode.SetField: BC_SetField(p); break;
                    case Bytecode.GetIndex: BC_GetIndex(p); break;
                    case Bytecode.SetIndex: BC_SetIndex(p); break;
                    case Bytecode.Invoke: BC_Invoke(p); break;
                    case Bytecode.Generic: BC_Generic(p); break;
                    case Bytecode.And: BC_And(p); break;
                    case Bytecode.Or: BC_Or(p); break;
                    case Bytecode.NullCoalescing: BC_NullCoalescing(p); break;
                    case Bytecode.If: BC_If(p); break;
                    case Bytecode.IfElse: BC_IfElse(p); break;
                    case Bytecode.Loop: BC_Loop(p); break;
                    case Bytecode.Break: p.Break(); break;
                    case Bytecode.Continue: p.Continue(); break;
                    case Bytecode.Return: p.Return(); break;
                    case Bytecode.Output: BC_Output(p); break;
                    case Bytecode.CreateFunction: BC_CreateFunction(p); break;
                    case Bytecode.PushName: p.Stack.Push(p.Names[p.Reader.ReadInt32()]); break;
                    case Bytecode.TryCatch: BC_TryCatch(p); break;
                    case Bytecode.PushContextValue: p.Stack.Push(p.Current.Return); break;
                    case Bytecode.Constructor: BC_Constructor(p); break;
                    case Bytecode.CreateList: BC_CreateList(p); break;
                    case Bytecode.CreateArray: BC_CreateArray(p); break;
                    case Bytecode.GetArrayType: BC_GetArrayType(p); break;
                    case Bytecode.Throw: BC_Throw(p); break;
                    case Bytecode.AddSubcontext:
                        throw new NotImplementedException();
                    #region Operators
                    case Bytecode.Sum: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a + b); } break;
                    case Bytecode.Sub: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a - b); } break;
                    case Bytecode.Mul: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a * b); } break;
                    case Bytecode.Div: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a / b); } break;
                    case Bytecode.Mod: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a % b); } break;
                    case Bytecode.ShiftLeft: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a << b); } break;
                    case Bytecode.ShiftRight: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a >> b); } break;
                    case Bytecode.BinAnd: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a & b); } break;
                    case Bytecode.BinOr: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a | b); } break;
                    case Bytecode.BinXor: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a ^ b); } break;
                    case Bytecode.Equal: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a == b); } break;
                    case Bytecode.NotEqual: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a != b); } break;
                    case Bytecode.Greater: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a > b); } break;
                    case Bytecode.Less: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a < b); } break;
                    case Bytecode.GreaterOrEqual: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a >= b); } break;
                    case Bytecode.LessOrEqual: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a <= b); } break;
                    case Bytecode.Range: { dynamic? b = p.Stack.Pop(); dynamic? a = p.Stack.Pop(); p.Stack.Push(a..b); } break;
                    case Bytecode.Rev: { dynamic? a = p.Stack.Pop(); p.Stack.Push(-a); } break;
                    case Bytecode.BinRev: { dynamic? a = p.Stack.Pop(); p.Stack.Push(~a); } break;
                    case Bytecode.Not: { dynamic? a = p.Stack.Pop(); p.Stack.Push(!a); } break;
                    case Bytecode.GetTypeObj: { object? a = p.Stack.Pop(); p.Stack.Push(a?.GetType() ?? typeof(void)); } break;
                    case Bytecode.Default: BC_Default(p); break;
                    case Bytecode.Convert: BC_Convert(p); break;
                    case Bytecode.IsType: BC_IsType(p); break;
                    #endregion
                    default:
                        throw new Exception($"Unresolved byte at {p.Stream.Position - 1}");
                }
            }
        }

        private static void BC_NewVariable(Runtime p)
        {
            object? val = p.Stack.Pop();
            p.Variables.Add(p.Names[p.Reader.ReadInt32()], val);
            p.Stack.Push(val);
        }

        private static void BC_SetVariable(Runtime p)
        {
            object? val = p.Stack.Pop();
            p.Variables.Set(p.Names[p.Reader.ReadInt32()], val);
            p.Stack.Push(val);
        }

        private static void BC_GetField(Runtime p)
        {
            string name = p.Names[p.Reader.ReadInt32()];
            object? obj = p.Stack.Pop();
            Type t;
            if (obj is Type objT)
            {
                t = objT;
                obj = null;
            }
            else if (obj is not null)
            {
                t = obj.GetType();
            }
            else
            {
                NullReferenceException nre = new();
                if (!p.TryHandle(nre))
                {
                    throw nre;
                }
                return;
            }

            if (obj is ExpandoObject expando)
            {
                p.Stack.Push(((IDictionary<string, object>)obj)[name]);
            }
            else if (t.GetField(name) is FieldInfo field && field.IsStatic == obj is null)
            {
                p.Stack.Push(field.GetValue(obj));
            }
            else if (t.GetProperty(name) is PropertyInfo prop && prop.GetMethod is not null &&
                prop.GetMethod.IsStatic == obj is null && prop.GetMethod.IsPublic)
            {
                p.Stack.Push(prop.GetValue(obj));
            }
            else if (t.GetEvent(name) is EventInfo ev)
            {
                p.Stack.Push(new EventWithObject(obj, ev));
            }
            else if ((from f in t.GetMethods()
                      where f.Name == name && f.IsStatic == obj is null
                      select f).ToArray() is MethodInfo[] methods &&
                     methods.Length > 0)
            {
                p.Stack.Push(new MethodWithObject(obj, methods));
            }
            else
            {
                throw new Exception($"Can not find field {name} at {t.FullName}");
            }
        }

        private static void BC_SetField(Runtime p)
        {
            string name = p.Names[p.Reader.ReadInt32()];
            object? val = p.Stack.Pop();
            object? obj = p.Stack.Pop();
            Type t;
            if (obj is Type objT)
            {
                t = objT;
                obj = null;
            }
            else if (obj is not null)
            {
                t = obj.GetType();
            }
            else
            {
                NullReferenceException nre = new();
                if (!p.TryHandle(nre))
                {
                    throw nre;
                }
                return;
            }

            if (obj is ExpandoObject expando)
            {
                ((IDictionary<string, object?>)obj)[name] = val;
            }
            else if (t.GetField(name) is FieldInfo field && field.IsStatic == obj is null)
            {
                field.SetValue(obj, val);
            }
            else if (t.GetProperty(name) is PropertyInfo prop && prop.GetMethod is not null &&
                prop.GetMethod.IsStatic == obj is null && prop.GetMethod.IsPublic)
            {
                prop.SetValue(obj, val);
            }
            else if (t.GetEvent(name) is EventInfo ev && ev.AddMethod is not null &&
                ev.AddMethod.IsStatic == obj is null)
            {
                ev.AddMethod.Invoke(obj, new object?[] {
                    val is InvokableFunction ?
                        AssertNull(typeof(Function)
                        .GetMethod(nameof(Function.ToDelegate)))
                        .MakeGenericMethod(new[] { ev.AddMethod.GetParameters().First().ParameterType })
                        .Invoke(null, new[] { val }) :
                        val
                });
            }
            else
            {
                throw new Exception($"Can not find field {name} at {t.FullName} to set");
            }
            p.Stack.Push(obj);
        }

        private static void BC_GetIndex(Runtime p)
        {
            object?[] args = new object[p.Reader.ReadByte()];
            Type[] sign = new Type[args.Length];
            for (int i = args.Length - 1; i > -1; i--)
            {
                args[i] = p.Stack.Pop();
                sign[i] = args[i]?.GetType() ?? typeof(void);
            }
            object obj = p.Stack.PopNotNull();
            if (obj is Array arr)
            {
                p.Stack.Push(arr.GetValue((from arg in args select (int)arg).ToArray()));
            }
            else
            {
                bool success = false;
                foreach (PropertyInfo indexer in from prop in obj.GetType().GetProperties()
                                                 where prop.GetIndexParameters().Length > 0
                                                 select prop)
                {
                    Type[] indexerSign = indexer.GetIndexParameters().Select(pi => pi.ParameterType).ToArray();
                    if (CompareSigns(sign, indexerSign))
                    {
                        p.Stack.Push(indexer.GetValue(obj, args));
                        success = true;
                        break;
                    }
                }
                if (!success)
                {
                    throw new Exception($"Can not indexate {obj.GetType().FullName} with {string.Join(", ", sign as object[])}");
                }
            }
        }

        private static void BC_SetIndex(Runtime p)
        {
            object? val = p.Stack.Pop();
            object?[] args = new object[p.Reader.ReadByte()];
            Type[] sign = new Type[args.Length];
            for (int i = args.Length - 1; i > -1; i--)
            {
                args[i] = p.Stack.Pop();
                sign[i] = args[i]?.GetType() ?? typeof(void);
            }
            object obj = AssertNull(p.Stack.Pop());
            bool success = false;
            foreach (PropertyInfo indexer in
                from prop in obj.GetType().GetProperties()
                where (val is null && !prop.PropertyType.IsValueType ||
                prop.PropertyType == val?.GetType() ||
                (val?.GetType().IsSubclassOf(prop.PropertyType) ?? false)) &&
                prop.GetIndexParameters().Length > 0
                select prop)
            {
                Type[] indexerSign = indexer.GetIndexParameters().Select(pi => pi.ParameterType).ToArray();
                if (CompareSigns(sign, indexerSign))
                {
                    indexer.SetValue(obj, val, args);
                    p.Stack.Push(val);
                    success = true;
                    break;
                }
            }
            if (!success)
            {
                throw new Exception($"Can not indexate {obj.GetType().FullName} with {string.Join(", ", sign as object[])}");
            }
        }

        private static void BC_Invoke(Runtime p)
        {
            object?[] args = new object[p.Reader.ReadByte()];
            for (int i = args.Length - 1; i > -1; i--)
            {
                args[i] = p.Stack.Pop();
            }
            object? obj = p.Stack.Pop();
            switch (obj)
            {
                case Type t:
                    p.Stack.Push(Activator.CreateInstance(t, args));
                    break;
                case MethodWithObject mwo:
                    p.Stack.Push(mwo.Invoke(args));
                    break;
                case EventWithObject ewo:
                    p.Stack.Push(ewo.Invoke(args));
                    break;
                case Delegate deleg:
                    p.Stack.Push(deleg.DynamicInvoke(args));
                    break;
                case Function func:
                    FunctionResult fres = func.GetResult(args, null);
                    if (fres.IsContext)
                    {
                        p.Add((Context)fres.Value);
                    }
                    else
                    {
                        p.Stack.Push(fres.Value);
                    }
                    break;
                default:
                    throw new Exception($"Can not invoke {obj?.GetType() ?? typeof(void)}");
            }
        }

        private static void BC_Generic(Runtime p)
        {
            Type[] args = new Type[p.Reader.ReadByte()];
            for (int i = args.Length - 1; i > -1; i--)
            {
                object? arg = p.Stack.Pop();
                if (arg is Type t)
                {
                    args[i] = t;
                }
                else
                {
                    throw new Exception($"Generic must recieve System.Type, not {arg?.GetType() ?? typeof(void)}");
                }
            }
            object? obj = p.Stack.Pop();
            switch (obj)
            {
                case Type t:
                    p.Stack.Push(t.MakeGenericType(args));
                    break;
                case MethodWithObject mwo:
                    p.Stack.Push(mwo.ToGeneric(args));
                    break;
                case Function func:
                    p.Stack.Push(new GenericFunction(func, args));
                    break;
                default:
                    throw new Exception($"Can make generic {obj?.GetType() ?? typeof(void)}");
            }
        }

        private static void BC_And(Runtime p)
        {
            int bcLen = p.Reader.ReadInt32();
            object? first = p.Stack.Pop();
            if (first is bool fb)
            {
                if (!fb)
                {
                    p.Stream.Position += bcLen;
                    p.Stack.Push(false);
                }
            }
            else
            {
                throw new Exception("And statement must recieve System.Bool");
            }
        }

        private static void BC_Or(Runtime p)
        {
            int bcLen = p.Reader.ReadInt32();
            object? first = p.Stack.Pop();
            if (first is bool fb)
            {
                if (fb)
                {
                    p.Stream.Position += bcLen;
                    p.Stack.Push(true);
                }
            }
            else
            {
                throw new Exception("And statement must recieve System.Bool");
            }
        }

        private static void BC_NullCoalescing(Runtime p)
        {
            int bcLen = p.Reader.ReadInt32();
            object? first = p.Stack.Pop();
            if (first is not null)
            {
                p.Stream.Position += bcLen;
                p.Stack.Push(first);
            }
        }

        private static void BC_If(Runtime p)
        {
            int trueSkip = p.Reader.ReadInt32();
            int falseSkip = p.Reader.ReadInt32();
            object? cond = p.Stack.Pop();
            if (cond is bool cbool)
            {
                if (cbool)
                {
                    p.CreateSubcontext("If", p.Stream, p.Stream.Position, p.Stream.Position + falseSkip, p.Stream.Position + trueSkip);
                }
                else
                {
                    p.Stream.Position += falseSkip;
                }
            }
            else
            {
                throw new Exception($"If expression must recieve System.Boolean");
            }
        }

        private static void BC_IfElse(Runtime p)
        {
            int sizeTrue = p.Reader.ReadInt32();
            int sizeFalse = p.Reader.ReadInt32();
            object? cond = p.Stack.Pop();
            if (cond is bool cbool)
            {
                if (cbool)
                {
                    p.CreateSubcontext("If", p.Stream, p.Stream.Position, p.Stream.Position + sizeTrue,
                        p.Stream.Position + sizeTrue + sizeFalse);
                }
                else
                {
                    p.Stream.Position += sizeTrue;
                    p.CreateSubcontext("Else", p.Stream, p.Stream.Position, p.Stream.Position + sizeFalse);
                }
            }
            else
            {
                throw new Exception($"If expression must recieve System.Boolean");
            }
        }

        private static void BC_Loop(Runtime p)
        {
            int size = p.Reader.ReadInt32();
            p.CreateSubcontext("Loop", p.Stream, p.Stream.Position, p.Stream.Position + size, ContextType.Loop);
        }

        private static void BC_Output(Runtime p)
        {
            object? ret = p.Stack.Pop();
            p.Current.Return = ret;
            p.RemoveLast();
        }

        private static void BC_CreateFunction(Runtime p)
        {
            string name = p.Names[p.Reader.ReadInt32()];

            string[] generics = new string[p.Reader.ReadInt32()];
            for (int i = 0; i < generics.Length; i++)
            {
                generics[i] = p.Names[p.Reader.ReadInt32()];
            }

            string[] args = new string[p.Reader.ReadInt32()];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = p.Names[p.Reader.ReadInt32()];
            }

            int codeSectorID = p.Reader.ReadUInt16();
            int size = p.Reader.ReadInt32();
            if (p.CodeSectors.TryGetValue(codeSectorID, out var funcBc))
            {
                p.Stream.Position += size;
            }
            else
            {
                funcBc = new byte[size];
                p.Stream.Read(funcBc);
                p.CodeSectors.Add(codeSectorID, funcBc);
            }
            p.Stack.Push(new CustomFunction(name, generics, args, funcBc, p, p.Variables));
        }

        private static void BC_TryCatch(Runtime p)
        {
            string exName = p.Names[p.Reader.ReadInt32()];
            int tryLen = p.Reader.ReadInt32();
            int catchLen = p.Reader.ReadInt32();
            p.CreateSubcontext("Try", p.Stream, p.Stream.Position, p.Stream.Position + tryLen, p.Stream.Position + tryLen + catchLen, ContextType.TryCatch);
            p.Current.ReservedVariable = exName;
        }

        private static void BC_Constructor(Runtime p)
        {
            int size = p.Reader.ReadInt32();
            object? obj = p.Stack.Pop();
            p.CreateSubcontext("Constructor " + (obj?.GetType() ?? typeof(void)), p.Stream, p.Stream.Position, p.Stream.Position + size);
            p.Current.Return = obj;
        }

        private static void BC_CreateList(Runtime p)
        {
            int count = p.Reader.ReadInt16();
            object? typeUndef = p.Stack.Pop();
            if (typeUndef is not Type type)
            {
                throw new ArgumentException($"List must recieve System.Type, not {typeUndef?.GetType() ?? typeof(void)}");
            }

            Array list = Array.CreateInstance(type, count);
            for (int i = list.Length - 1; i >= 0; i--)
            {
                object? item = p.Stack.Pop();
                if ((item is null && !type.IsValueType) || item?.GetType() == type || (item?.GetType().IsSubclassOf(type) ?? false) || item is null && !type.IsValueType)
                {
                    list.SetValue(item, i);
                }
                else
                {
                    throw new Exception($"Tried to create list of type {type} with {item} ({item?.GetType() ?? typeof(void)})");
                }
            }
            p.Stack.Push(Activator.CreateInstance(typeof(List<>).MakeGenericType(type), new object[] { list }));
        }

        private static void BC_CreateArray(Runtime p)
        {
            object? typeUndef = p.Stack.Pop();
            if (typeUndef is not Type type)
            {
                throw new ArgumentException($"Array must recieve System.Type, not {typeUndef?.GetType() ?? typeof(void)}");
            }

            int rank = p.Reader.ReadByte();
            if (rank == 1)
            {
                int len = p.Reader.ReadInt16();
                Array list = Array.CreateInstance(type, len);
                for (int i = len - 1; i >= 0; i--)
                {
                    list.SetValue(p.Stack.Pop(), i);
                }
                p.Stack.Push(list);
            }
            else
            {
                int[] lens = new int[rank];
                for (int i = 0; i < rank; i++)
                {
                    lens[i] = p.Reader.ReadInt16();
                }
                CreateArray(lens, type, p);
            }
        }

        private static void BC_GetArrayType(Runtime p)
        {
            int rank = p.Reader.ReadByte();
            object? obj = p.Stack.Pop();

            if (obj is not Type type)
            {
                throw new ArgumentException($"Array must recieve System.Type, not {obj?.GetType() ?? typeof(void)}");
            }

            p.Stack.Push(rank > 1 ? type.MakeArrayType(rank) : type.MakeArrayType());
        }

        private static void BC_Throw(Runtime p)
        {
            object? obj = p.Stack.Pop();
            Exception exception;

            if (obj is Exception ex)
            {
                exception = ex;
            }
            else if (obj is string msg)
            {
                exception = new Exception(msg);
            }
            else
            {
                exception = new NotThrowableException(obj);
            }

            if (!p.TryHandle(exception))
            {
                throw exception;
            }
        }

        private static void BC_Default(Runtime p)
        {
            object? a = p.Stack.Pop();

            if (a is Type t)
            {
                p.Stack.Push(t.IsValueType ? Activator.CreateInstance(t) : null);
            }
            else
            {
                throw new Exception($"Default statement must recieve System.Type, not {a?.GetType() ?? typeof(void)}");
            }
        }

        private static void BC_Convert(Runtime p)
        {
            object? b = p.Stack.Pop();
            object? a = p.Stack.Pop();

            if (b is Type type)
            {
                p.Stack.Push(Convert.ChangeType(a, type));
            }
            else
            {
                throw new Exception($"Convert statement must recieve System.Type, not {b?.GetType() ?? typeof(void)}");
            }
        }

        private static void BC_IsType(Runtime p)
        {
            object? b = p.Stack.Pop();
            object? a = p.Stack.Pop();

            if (b is Type type)
            {
                p.Stack.Push(a is null ? type == typeof(void) : (a.GetType() == type || a.GetType().IsSubclassOf(type)));
            }
            else if (b is null)
            {
                p.Stack.Push(a is null);
            }
            else
            {
                throw new Exception($"Is statement must recieve System.Type, not {b?.GetType() ?? typeof(void)}");
            }
        }

        private static void CreateArray(int[] lens, Type t, Runtime p)
        {
            Array arr = Array.CreateInstance(t, lens);
            int[] curr = lens.Select(i => i - 1)/*.Reverse()*/.ToArray();

            do
            {
                arr.SetValue(p.Stack.Pop(), curr);
            }
            while (MoveArrayPos(curr, lens));

            p.Stack.Push(arr);
        }

        private static bool MoveArrayPos(int[] pos, int[] lens)
        {
            pos[^1]--;

            for (int i = pos.Length - 1; i > 0; i--)
            {
                if (pos[i] < 0)
                {
                    pos[i - 1]--;
                    pos[i] = lens[i] - 1;
                }
            }

            return pos[0] > -1;
        }

        /// <summary>
        /// Returns true if types A are equal or inherited from types B
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool CompareSigns(Type[] a, Type[] b)
        {
            if (a.Length == b.Length)
            {
                for (int i = 0; i < a.Length; i++)
                {
                     if (!(a[i] == typeof(void) && b[i].IsValueType) && !b[i].IsAssignableFrom(a[i]) && a[i] != b[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static T AssertNull<T>(T? v)
        {
            if (v is null)
            {
                throw new NullReferenceException();
            }
            return v;
        }
    }
}
