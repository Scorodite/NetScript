using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using NetScript.Core;

namespace NetScript.Interpreter
{
    public class Runtime : IList<Context>
    {
        public Context Current => Content.LastOrDefault();
        public bool ContextAvariable => Content.Count != 0;
        public Stream Stream => Current?.Stream;
        public BinaryReader Reader => Current?.Reader;
        public VariableCollection Variables => Current?.Variables;
        public Stack Stack => Current?.Stack;

        public bool IsEnd =>
            Current is null ||
            (Current.End == -1 ?
                Stream.Position >= Stream.Length :
                Stream.Position >= Current.End);

        public string[] Names { get; private set; }
        public object[] Constants { get; private set; }

        private List<Context> Content { get; set; }

        #region IList
        public Context this[int index] { get => Content[index]; set => Content[index] = value; }

        public int Count => Content.Count;

        public bool IsReadOnly => false;

        public void Add(Context item)
        {
            Content.Add(item);
        }

        public void Clear()
        {
            Content.Clear();
        }

        public bool Contains(Context item)
        {
            return Content.Contains(item);
        }

        public void CopyTo(Context[] array, int arrayIndex)
        {
            Content.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Context> GetEnumerator()
        {
            return Content.GetEnumerator();
        }

        public int IndexOf(Context item)
        {
            return Content.IndexOf(item);
        }

        public void Insert(int index, Context item)
        {
            Content.Insert(index, item);
        }

        public bool Remove(Context item)
        {
            return Content.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Content.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Content).GetEnumerator();
        }
        #endregion

        private Runtime() { }

        public Runtime(Stream root)
        {
            Content = new() { new(root) };

            Names = new string[Reader.ReadInt32()];
            for (int i = 0; i < Names.Length; i++)
            {
                Names[i] = Reader.ReadString();
            }

            int dlls = Reader.ReadInt32();
            for (int i = 0; i < dlls; i++)
            {
                Assembly.LoadFrom(Reader.ReadString());
            }

            int imports = Reader.ReadInt32();
            for (int i = 0; i < imports; i++)
            {
                int nameID = Reader.ReadInt32();
                Type t = Type.GetType(Reader.ReadString());
                if (t is null)
                {
                    throw new Exception($"Failed to import {t}");
                }
                Variables.Add(Names[nameID], t, false);
            }

            Constants = new object[Reader.ReadInt32()];
            for (int i = 0; i < Constants.Length; i++)
            {
                Constants[i] = ReadConst(Reader);
            }
        }

        public void CreateSubcontext(Stream stream)
        {
            Content.Add(new(stream, Content.Last()));
        }

        public void CreateSubcontext(Stream stream, long begin, long end)
        {
            Content.Add(new(stream, Current, begin, end));
        }

        public void CreateSubcontext(Stream stream, long begin, long end, ContextType type)
        {
            Content.Add(new(stream, Current, begin, end) { Type = type });
        }

        public void CreateSubcontext(Stream stream, long begin, long end, long moveAfter)
        {
            Content.Add(new(stream, Current, begin, end, moveAfter));
        }

        public void CreateSubcontext(Stream stream, long begin, long end, long moveAfter, ContextType type)
        {
            Content.Add(new(stream, Current, begin, end, moveAfter) { Type = type });
        }

        public void RemoveLast()
        {
            object ret = Current.Return;
            long end = Current.End;
            long moveAfter = Current.MoveAfter;
            Stream prev = Stream;
            Content.RemoveAt(Content.Count - 1);
            if (moveAfter > -1)
            {
                Stream.Position = moveAfter;
            }
            else if (Stream == prev)
            {
                Stream.Position = end;
            }

            Current.Stack.Push(ret);
        }

        public void Break()
        {
            for (int i = Content.Count - 1; i >= 0; i--)
            {
                if (Content[i].Type == ContextType.Function)
                {
                    break;
                }
                else if (Content[i].Type == ContextType.Loop)
                {
                    Context loop = Content[i];
                    long after = loop.MoveAfter == -1 ? loop.End : loop.MoveAfter;
                    object ret = Current.Stack.Count > 0 ? Current.Stack.Peek() : null;
                    Content.RemoveRange(i, Content.Count - i);
                    Stream.Position = after;
                    Stack.Push(ret);
                    return;
                }
            }
            throw new Exception("Unhandled break");
        }

        public void Return()
        {
            for (int i = Content.Count - 1; i >= 0; i--)
            {
                if (Content[i].Type == ContextType.Function)
                {
                    Context retCont = Content[i];
                    long after = retCont.MoveAfter == -1 ? retCont.End : retCont.MoveAfter;
                    object ret = Current.Stack.Count > 0 ? Current.Stack.Peek() : null;
                    retCont.Return = ret;
                    Content.RemoveRange(i, Content.Count - i);
                    if (after != -1)
                    {
                        Stream.Position = after;
                    }
                    Stack?.Push(ret);
                    return;
                }
            }
            throw new Exception("Unhandled return");
        }

        public void Continue()
        {
            for (int i = Content.Count - 1; i >= 0; i--)
            {
                if (Content[i].Type == ContextType.Function)
                {
                    break;
                }
                else if (Content[i].Type == ContextType.Loop)
                {
                    Content.RemoveRange(i + 1, Content.Count - i - 1);
                    Current.Stream.Position = Current.Begin;
                    return;
                }
            }
        }

        private static object ReadConst(BinaryReader reader)
        {
            Bytecode bc = reader.ReadBytecode();
            return bc switch
            {
                Bytecode.ConstByte => reader.ReadByte(),
                Bytecode.ConstSbyte => reader.ReadSByte(),
                Bytecode.ConstShort => reader.ReadInt16(),
                Bytecode.ConstUShort => reader.ReadUInt16(),
                Bytecode.ConstInt => reader.ReadInt32(),
                Bytecode.ConstUInt => reader.ReadUInt32(),
                Bytecode.ConstLong => reader.ReadInt64(),
                Bytecode.ConstULong => reader.ReadUInt64(),
                Bytecode.ConstFloat => reader.ReadSingle(),
                Bytecode.ConstDecimal => reader.ReadDecimal(),
                Bytecode.ConstDouble => reader.ReadDouble(),
                Bytecode.ConstChar => reader.ReadChar(),
                Bytecode.ConstString => reader.ReadString(),
                _ => throw new Exception($"Unknown constant {bc}"),
            };
        }

        public Runtime Clone()
        {
            return new Runtime()
            {
                Names = Names,
                Constants = Constants,
                Content = new(),
            };
        }

        public bool TryHandle(Exception ex)
        {
            for (int i = Content.Count - 1; i >= 0; i--)
            {
                if (Content[i].Type == ContextType.TryCatch)
                {
                    Context tryCont = Content[i];
                    Content.RemoveRange(i, Content.Count - i);
                    CreateSubcontext(tryCont.Stream, tryCont.End, tryCont.MoveAfter);
                    tryCont.Stream.Position = tryCont.End;
                    Current.Return = ex;
                    Current.Variables.Add(tryCont.ReservedVariable, ex);
                    return true;
                }
            }
            return false;
        }
    }
}
