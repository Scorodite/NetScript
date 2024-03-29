﻿using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace NetScript.Interpretation
{
    public class Context
    {
        public Stream Stream { get; }
        public BinaryReader Reader { get; }
        public Stack Stack { get; }
        public VariableCollection Variables { get; set; }
        public Context? Parent { get; set; }
        public string? ReservedVariable { get; set; }

        public ContextType Type { get; set; }

        public string Name { get; set; }
        public long Begin { get; set; }
        public long End { get; set; }
        public long MoveAfter { get; set; }

        public object? Return { get; set; }

        public Context(string name, Stream stream)
        {
            Name = name;
            Stream = stream;
            Reader = new(Stream);
            Stack = new();
            Variables = new();

            Begin = -1;
            End = -1;
            MoveAfter = -1;
        }

        public Context(string name, Stream stream, Context parent) : this(name, stream)
        {
            Parent = parent;
            Variables.Parent = parent.Variables;
        }

        public Context(string name, Stream stream, Context parent, long begin, long end) : this(name, stream, parent)
        {
            Begin = begin;
            End = end;
        }

        public Context(string name, Stream stream, Context parent, long begin, long end, long moveAfter) : this(name, stream, parent)
        {
            Begin = begin;
            End = end;
            MoveAfter = moveAfter;
        }
    }

    public enum ContextType
    {
        Default,
        Function,
        Loop,
        TryCatch,
    }
}
