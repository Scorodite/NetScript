using System;
using System.IO;
using System.Linq;
using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NetScript.Core;

namespace NetScript.Interpreter
{
    public static class InterpreterNS
    {
        public static VariableCollection Interpret(Stream stream)
        {
            Package p = new(stream);

            Execute(p);
            return p.Last().Variables;
        }

        public static void Execute(Package p)
        {
            while (true)
            {
                Bytecode bc = p.Reader.ReadBytecode();

                switch (bc)
                {
                    case Bytecode.ClearStack: p.Stack.Clear(); break;
                    case Bytecode.PushConst: p.Stack.Push(p.Constants[p.Reader.ReadInt32()]); break;
                    case Bytecode.PushNull: p.Stack.Push(null); break;
                    case Bytecode.PushTrue: p.Stack.Push(true); break;
                    case Bytecode.PushFalse: p.Stack.Push(false); break;
                    case Bytecode.NewVariable:
                        {
                            object val = p.Stack.Pop();
                            string name = p.Names[p.Reader.ReadInt32()];
                            p.Variables.Add(name, val);
                            p.Stack.Push(val);
                        }
                        break;
                    case Bytecode.SetVariable:
                        {
                            object val = p.Stack.Pop();
                            string name = p.Names[p.Reader.ReadInt32()];
                            p.Variables.Set(name, val);
                            p.Stack.Push(val);
                        }
                        break;
                    case Bytecode.GetVariable:
                        {
                            string name = p.Names[p.Reader.ReadInt32()];
                            p.Stack.Push(p.Variables.Get(name));
                        }
                        break;
                    case Bytecode.GetField:
                        {
                            string name = p.Names[p.Reader.ReadInt32()];
                            object obj = p.Stack.Pop();
                            Type t;
                            if (obj is Type objT)
                            {
                                t = objT;
                                obj = null;
                            }
                            else
                            {
                                t = obj.GetType();
                            }

                            if (t.GetField(name) is FieldInfo field && field.IsStatic == obj is null)
                            {
                                p.Stack.Push(field.GetValue(obj));
                            }
                            else if (t.GetProperty(name) is PropertyInfo prop && prop.GetMethod is not null &&
                                prop.GetMethod.IsStatic == obj is null && prop.GetMethod.IsPublic)
                            {
                                p.Stack.Push(prop.GetValue(obj));
                            }
                            else if (obj is ExpandoObject expando)
                            {
                                p.Stack.Push(((IDictionary<string, object>)obj)[name]);
                            }
                            else if (t.GetMethods().ToList().FindAll(f => f.Name == name && f.IsStatic == obj is null) is List<MethodInfo> methods &&
                                     methods.Count > 0)
                            {
                                p.Stack.Push(new MethodWithObject(obj, methods.ToArray()));
                            }
                            else
                            {
                                throw new Exception($"Can not find field {name} at {t.FullName}");
                            }
                        }
                        break;
                    case Bytecode.SetField:
                        {
                            string name = p.Names[p.Reader.ReadInt32()];
                            object val = p.Stack.Pop();
                            object obj = p.Stack.Pop();
                            Type t;
                            if (obj is Type objT)
                            {
                                t = objT;
                                obj = null;
                            }
                            else
                            {
                                t = obj.GetType();
                            }

                            if (t.GetField(name) is FieldInfo field && field.IsStatic == obj is null)
                            {
                                field.SetValue(obj, val);
                            }
                            else if (t.GetProperty(name) is PropertyInfo prop && prop.GetMethod is not null &&
                                prop.GetMethod.IsStatic == obj is null && prop.GetMethod.IsPublic)
                            {
                                prop.SetValue(obj, val);
                            }
                            else if (obj is ExpandoObject expando)
                            {
                                ((IDictionary<string, object>)obj)[name] = val;
                            }
                            else
                            {
                                throw new Exception($"Can not find field {name} at {t.FullName}");
                            }
                            p.Stack.Push(obj);
                        }
                        break;
                    case Bytecode.GetIndex:
                        {
                            object[] args = new object[p.Reader.ReadByte()];
                            Type[] sign = new Type[args.Length];
                            for (int i = args.Length - 1; i > -1; i--)
                            {
                                args[i] = p.Stack.Pop();
                                sign[i] = args[i]?.GetType() ?? typeof(void);
                            }
                            object obj = p.Stack.Pop();
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
                        break;
                    case Bytecode.SetIndex:
                        {
                            object val = p.Stack.Pop();
                            object[] args = new object[p.Reader.ReadByte()];
                            Type[] sign = new Type[args.Length];
                            for (int i = args.Length - 1; i > -1; i--)
                            {
                                args[i] = p.Stack.Pop();
                                sign[i] = args[i]?.GetType() ?? typeof(void);
                            }
                            object obj = p.Stack.Pop();
                            bool success = false;
                            foreach (PropertyInfo indexer in from prop in obj.GetType().GetProperties()
                                                             where (val is null && !prop.PropertyType.IsValueType ||
                                                                prop.PropertyType == val.GetType() ||
                                                                val.GetType().IsSubclassOf(prop.PropertyType)) &&
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
                        break;
                    case Bytecode.Invoke:
                        {
                            object[] args = new object[p.Reader.ReadByte()];
                            for (int i = args.Length - 1; i > -1; i--)
                            {
                                args[i] = p.Stack.Pop();
                            }
                            object obj = p.Stack.Pop();
                            switch (obj)
                            {
                                case Type t:
                                    p.Stack.Push(Activator.CreateInstance(t, args));
                                    break;
                                case MethodWithObject mwo:
                                    p.Stack.Push(mwo.Invoke(args));
                                    break;
                                case Function func:
                                    if (func.GetContext(args, null) is Context context)
                                    {
                                        p.Add(context);
                                    }
                                    else
                                    {
                                        p.Stack.Push(null);
                                    }
                                    break;
                                default:
                                    throw new Exception($"Can not invoke {obj?.GetType() ?? typeof(void)}");
                            }
                        }
                        break;
                    case Bytecode.Generic:
                        {
                            Type[] args = new Type[p.Reader.ReadByte()];
                            for (int i = args.Length - 1; i > -1; i--)
                            {
                                object arg = p.Stack.Pop();
                                if (arg is Type t)
                                {
                                    args[i] = t;
                                }
                                else
                                {
                                    throw new Exception($"Generic must recieve System.Type, not {arg?.GetType() ?? typeof(void)}");
                                }
                            }
                            object obj = p.Stack.Pop();
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
                        break;
                    case Bytecode.And:
                        {
                            int bcLen = p.Reader.ReadInt32();
                            object first = p.Stack.Pop();
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
                        break;
                    case Bytecode.Or:
                        {
                            int bcLen = p.Reader.ReadInt32();
                            object first = p.Stack.Pop();
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
                        break;
                    case Bytecode.NullCoalescing:
                        {
                            int bcLen = p.Reader.ReadInt32();
                            object first = p.Stack.Pop();
                            if (first is not null)
                            {
                                p.Stream.Position += bcLen;
                                p.Stack.Push(first);
                            }
                        }
                        break;
                    case Bytecode.If:
                        {
                            int trueSkip = p.Reader.ReadInt32();
                            int falseSkip = p.Reader.ReadInt32();
                            object cond = p.Stack.Pop();
                            if (cond is bool cbool)
                            {
                                if (cbool)
                                {
                                    p.CreateSubcontext(p.Stream, p.Stream.Position, p.Stream.Position + falseSkip, p.Stream.Position + trueSkip);
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
                        break;
                    case Bytecode.IfElse:
                        {
                            int sizeTrue = p.Reader.ReadInt32();
                            int sizeFalse = p.Reader.ReadInt32();
                            object cond = p.Stack.Pop();
                            if (cond is bool cbool)
                            {
                                if (cbool)
                                {
                                    p.CreateSubcontext(p.Stream, p.Stream.Position, p.Stream.Position + sizeTrue,
                                        p.Stream.Position + sizeTrue + sizeFalse);
                                }
                                else
                                {
                                    p.Stream.Position += sizeTrue;
                                    p.CreateSubcontext(p.Stream, p.Stream.Position, p.Stream.Position + sizeFalse);
                                }
                            }
                            else
                            {
                                throw new Exception($"If expression must recieve System.Boolean");
                            }
                        }
                        break;
                    case Bytecode.Loop:
                        {
                            int size = p.Reader.ReadInt32();
                            p.CreateSubcontext(p.Stream, p.Stream.Position, p.Stream.Position + size, ContextType.Loop);
                        }
                        break;
                    case Bytecode.Break:
                        {
                            p.Break();
                        }
                        break;
                    case Bytecode.Continue:
                        {
                            p.Continue();
                        }
                        break;
                    case Bytecode.Return:
                        {
                            p.Return();
                        }
                        break;
                    case Bytecode.CreateFunction:
                        {
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

                            int size = p.Reader.ReadInt32();
                            byte[] funcBc = new byte[size];
                            p.Stream.Read(funcBc);
                            p.Stack.Push(new CustomFunction(generics, args, funcBc, p, p.Variables));
                        }
                        break;
                    case Bytecode.PushName:
                        {
                            p.Stack.Push(p.Names[p.Reader.ReadInt32()]);
                        }
                        break;
                    #region Operators
                    case Bytecode.Sum: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a + b); } break;
                    case Bytecode.Sub: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a - b); } break;
                    case Bytecode.Mul: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a * b); } break;
                    case Bytecode.Div: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a / b); } break;
                    case Bytecode.Mod: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a % b); } break;
                    case Bytecode.BinAnd: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a & b); } break;
                    case Bytecode.BinOr: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a | b); } break;
                    case Bytecode.BinXor: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a ^ b); } break;
                    case Bytecode.Equal: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a == b); } break;
                    case Bytecode.NotEqual: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a != b); } break;
                    case Bytecode.Greater: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a > b); } break;
                    case Bytecode.Less: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a < b); } break;
                    case Bytecode.GreaterOrEqual: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a >= b); } break;
                    case Bytecode.LessOrEqual: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a <= b); } break;
                    case Bytecode.Range: { dynamic b = p.Stack.Pop(); dynamic a = p.Stack.Pop(); p.Stack.Push(a..b); } break;
                    case Bytecode.Rev: { dynamic a = p.Stack.Pop(); p.Stack.Push(-a); } break;
                    case Bytecode.BinRev: { dynamic a = p.Stack.Pop(); p.Stack.Push(~a); } break;
                    case Bytecode.Not: { dynamic a = p.Stack.Pop(); p.Stack.Push(!a); } break;
                    case Bytecode.GetTypeObj: { object a = p.Stack.Pop(); p.Stack.Push(a?.GetType() ?? typeof(void)); } break;
                    case Bytecode.Default:
                        {
                            object a = p.Stack.Pop();

                            if (a is Type t)
                            {
                                p.Stack.Push(t.IsValueType ? Activator.CreateInstance(t) : null);
                            }
                            else
                            {
                                throw new Exception($"Default statement must recieve System.Type, not {a?.GetType() ?? typeof(void)}");
                            }
                        }
                        break;
                    case Bytecode.Convert:
                        {
                            object b = p.Stack.Pop();
                            object a = p.Stack.Pop();

                            if (b is Type type)
                            {
                                p.Stack.Push(Convert.ChangeType(a, type));
                            }
                            else
                            {
                                throw new Exception($"Convert statement must recieve System.Type, not {b?.GetType() ?? typeof(void)}");
                            }
                        }
                        break;
                    case Bytecode.IsType:
                        {
                            object b = p.Stack.Pop();
                            object a = p.Stack.Pop();

                            if (b is Type type)
                            {
                                p.Stack.Push(a is null ? type == typeof(void) : (a.GetType() == type || a.GetType().IsSubclassOf(type)));
                            }
                            else
                            {
                                throw new Exception($"Is statement must recieve System.Type, not {b?.GetType() ?? typeof(void)}");
                            }
                        }
                        break;
                    #endregion
                    default:
                        throw new Exception($"Unresolved byte at {p.Stream.Position - 1}");
                }

                if (p.IsEnd)
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
                        Context curr = p.Current;
                        p.RemoveLast();
                        p.Stack.Push(curr.Return);
                    }
                }
            }
        }

        public static bool CompareSigns(Type[] a, Type[] b)
        {
            if (a.Length == b.Length)
            {
                for (int i = 0; i < a.Length; i++)
                {
                     if (!(a[i] == typeof(void) && b[i].IsValueType) && !a[i].IsSubclassOf(b[i]) && a[i] != b[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
