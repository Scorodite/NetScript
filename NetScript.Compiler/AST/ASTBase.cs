using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NetScript.Core;
using System.Reflection;

namespace NetScript.Compiler.AST
{
    public abstract class ASTBase
    {
        public int Index { get; set; }

        public abstract void Compile(BinaryWriter writer, CompilerArgs args);

        protected static string RandomString(int count)
        {
            Random r = new();
            StringBuilder builder = new();

            for (int i = 0; i < count; i++)
            {
                builder.Append(Convert.ToChar((int)Math.Floor(26 * r.NextDouble() + 65)));
            }

            return builder.ToString();
        }
    }

    public class ConstantAST : ASTBase
    {
        public object Obj { get; set; }

        public ConstantAST(object obj)
        {
            Obj = obj;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            switch (Obj)
            {
                case null:
                    writer.Write(Bytecode.PushNull);
                    break;
                case true:
                    writer.Write(Bytecode.PushTrue);
                    break;
                case false:
                    writer.Write(Bytecode.PushFalse);
                    break;
                default:
                    writer.Write(Bytecode.PushConst);
                    writer.Write(args.GetConstant(Obj));
                    break;
            }
        }

        public override string ToString()
        {
            return Obj?.ToString() ?? "null";
        }
    }

    public class LoadDllAST : ASTBase
    {
        public string Path { get; }

        public LoadDllAST(string path)
        {
            Path = path;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            args.LoadDLL(Path);
        }

        public override string ToString()
        {
            return $"loaddll {Path}";
        }
    }

    public class BreakAST : ASTBase
    {
        public ASTBase Value { get; set; }

        public BreakAST(ASTBase value)
        {
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Value.Compile(writer, args);
            writer.Write(Bytecode.Break);
        }

        public override string ToString()
        {
            return $"break{(Value is not ConstantAST con || con.Obj is not null ? Value.ToString() : string.Empty)}";
        }
    }

    public class ReturnAST : ASTBase
    {
        public ASTBase Value { get; set; }

        public ReturnAST(ASTBase value)
        {
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Value.Compile(writer, args);
            writer.Write(Bytecode.Return);
        }

        public override string ToString()
        {
            return $"return{(Value is not ConstantAST con || con.Obj is not null ? Value.ToString() : string.Empty)}";
        }
    }

    public class ContinueAST : ASTBase
    {
        public ContinueAST() { }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            writer.Write(Bytecode.Continue);
        }

        public override string ToString()
        {
            return "continue";
        }
    }

    public class NewVariableAST : ASTBase
    {
        public string Name { get; set; }
        public ASTBase Value { get; set; }

        public NewVariableAST(string name)
        {
            Name = name;
            Value = new ConstantAST(null);
        }

        public NewVariableAST(string name, ASTBase value)
        {
            Name = name;
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Value.Compile(writer, args);
            writer.Write(Bytecode.NewVariable);
            writer.Write(args.GetNameID(Name));
        }

        public override string ToString()
        {
            return $"var {Name} = {Value}";
        }
    }

    public class GetVariableAST : ASTBase
    {
        public string Name { get; set; }

        public GetVariableAST(string name)
        {
            Name = name;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            writer.Write(Bytecode.GetVariable);
            writer.Write(args.GetNameID(Name, true));
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class SetVariableAST : ASTBase
    {
        public string Name { get; set; }
        public ASTBase Value { get; set; }

        public SetVariableAST(string name, ASTBase value)
        {
            Name = name;
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Value.Compile(writer, args);
            writer.Write(Bytecode.SetVariable);
            writer.Write(args.GetNameID(Name));
        }

        public override string ToString()
        {
            return $"{Name} = {Value}";
        }
    }

    public class ImportAST : ASTBase
    {
        public string Type { get; set; }

        public ImportAST(string type)
        {
            Type = type;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            string name = Type[(Type.LastIndexOf('.')+1)..];
            string ns = Type[..Type.LastIndexOf('.')];
            if (Type.EndsWith('*'))
            {
                int i = 0;
                foreach (Assembly asm in args.Assemblies)
                {
                    foreach (Type t in from asmt in asm.GetTypes() where asmt.Namespace == ns select asmt)
                    {
                        i++;
                        args.AddImport(t.Name, t);
                    }
                }
                if (i == 0)
                {
                    throw new CompilerException($"Can not import namespace {ns}", Index);
                }
            }
            else
            {
                args.AddImport(name, GetType(name, ns, args.Assemblies));
            }
        }

        private Type GetType(string name, string ns, IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly asm in assemblies)
            {
                if (asm.GetTypes().FirstOrDefault(t => t.Namespace == ns && t.Name == name) is Type t)
                {
                    return t;
                }
            }
            throw new CompilerException($"Can not import {ns}.{name}", Index);
        }

        public override string ToString()
        {
            return $"import {Type}";
        }
    }

    public class InvokeAST : ASTBase
    {
        public ASTBase Obj { get; set; }
        public ASTBase[] Args { get; set; }

        public InvokeAST(ASTBase obj)
        {
            Obj = obj;
            Args = Array.Empty<ASTBase>();
        }

        public InvokeAST(ASTBase obj, ASTBase[] args)
        {
            Obj = obj;
            Args = args;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Obj.Compile(writer, args);

            foreach (ASTBase arg in Args)
            {
                arg.Compile(writer, args);
            }

            writer.Write(Bytecode.Invoke);
            writer.Write((byte)Args.Length);
        }

        public override string ToString()
        {
            return $"{Obj}({string.Join(", ", Args as object[])})";
        }
    }

    public class GetIndexAST : ASTBase
    {
        public ASTBase Obj { get; set; }
        public ASTBase[] Args { get; set; }

        public GetIndexAST(ASTBase obj, ASTBase[] args)
        {
            Obj = obj;
            Args = args;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Obj.Compile(writer, args);

            foreach (ASTBase arg in Args)
            {
                arg.Compile(writer, args);
            }

            writer.Write(Bytecode.GetIndex);
            writer.Write((byte)Args.Length);
        }

        public override string ToString()
        {
            return $"{Obj}[{string.Join(", ", Args as object[])}]";
        }
    }

    public class SetIndexAST : ASTBase
    {
        public ASTBase Obj { get; set; }
        public ASTBase[] Args { get; set; }
        public ASTBase Value { get; set; }

        public SetIndexAST(ASTBase obj, ASTBase[] args, ASTBase value)
        {
            Obj = obj;
            Args = args;
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Obj.Compile(writer, args);

            foreach (ASTBase arg in Args)
            {
                arg.Compile(writer, args);
            }

            Value.Compile(writer, args);

            writer.Write(Bytecode.SetIndex);
            writer.Write((byte)Args.Length);
        }

        public override string ToString()
        {
            return $"{Obj}[{string.Join(", ", Args as object[])}] = {Value}";
        }
    }

    public class GenericAST : ASTBase
    {
        public ASTBase Obj { get; set; }
        public ASTBase[] Args { get; set; }

        public GenericAST(ASTBase obj, ASTBase[] args)
        {
            Obj = obj;
            Args = args;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Obj.Compile(writer, args);

            foreach (ASTBase arg in Args)
            {
                arg.Compile(writer, args);
            }

            writer.Write(Bytecode.Generic);
            writer.Write((byte)Args.Length);
        }

        public override string ToString()
        {
            return $"{Obj}<[{string.Join(", ", Args as object[])}]>";
        }
    }

    public class GetFieldAST : ASTBase
    {
        public ASTBase Obj { get; set; }
        public string Name { get; set; }

        public GetFieldAST(ASTBase obj, string name)
        {
            Obj = obj;
            Name = name;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Obj.Compile(writer, args);
            writer.Write(Bytecode.GetField);
            writer.Write(args.GetNameID(Name));
        }

        public override string ToString()
        {
            return $"{Obj}.{Name}";
        }
    }

    public class SetFieldAST : ASTBase
    {
        public ASTBase Obj { get; set; }
        public string Name { get; set; }
        public ASTBase Value { get; set; }

        public SetFieldAST(ASTBase obj, string name, ASTBase value)
        {
            Obj = obj;
            Name = name;
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Obj.Compile(writer, args);
            Value.Compile(writer, args);
            writer.Write(Bytecode.SetField);
            writer.Write(args.GetNameID(Name));
        }

        public override string ToString()
        {
            return $"{Obj}.{Name} = {Value}";
        }
    }

    public class CreateFunctionAST : ASTBase
    {
        public string[] Args { get; set; }
        public string[] Generics { get; set; }
        public ASTBase[] ASTs { get; set; }

        public CreateFunctionAST(string[] args, string[] generics, ASTBase[] asts)
        {
            Args = args;
            Generics = generics;
            ASTs = asts;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            writer.Write(Bytecode.CreateFunction);
            writer.Write(Generics.Length);
            foreach (string gen in Generics)
            {
                writer.Write(args.GetNameID(gen));
            }
            writer.Write(Args.Length);
            foreach (string arg in Args)
            {
                writer.Write(args.GetNameID(arg));
            }
            SizePosition sizePos = new(writer);
            foreach (ASTBase ast in ASTs)
            {
                ast.Compile(writer, args);
                writer.Write(Bytecode.ClearStack);
            }
            sizePos.SaveSize();
        }

        public override string ToString()
        {
            return $"func{(Generics.Length > 0 ? $"<[{string.Join(", ", Generics as object[])}]>" : string.Empty)}" +
                       $"{(Args.Length > 0 ? $"({string.Join(", ", Args as object[])})" : string.Empty)}" +
                       $"{(ASTs.Length > 0 ? $"{{ {string.Join("; ", ASTs as object[])} }}" : string.Empty)}";
        }
    }

    public class IfAST : ASTBase
    {
        public (ASTBase[], ASTBase)[] IfASTs { get; set; }
        public ASTBase[] ElseASTs { get; set; }

        public IfAST((ASTBase[], ASTBase)[] ifASTs)
        {
            IfASTs = ifASTs;
        }

        public IfAST((ASTBase[], ASTBase)[] ifASTs, ASTBase[] elseASTs)
        {
            IfASTs = ifASTs;
            ElseASTs = elseASTs;
        }

        // ([condition] [If] [trueSkip (Длина всего что осталось)] [falseSkip (Длина истинных AST)] [asts]){count} [PushNull (Для того если ничего не подошло)]
        // ([condition] [If] [trueSkip (Длина всего что осталось)] [falseSkip (Длина истинных AST)] [asts]){count - 1} [condition] [IfElse] [trueLen] [falseLen] [astsTrue] [astsFalse]
        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            List<SizePosition> trueSkips = new();

            for (int i = 0; i < IfASTs.Length - 1; i++)
            {
                (ASTBase[] asts, ASTBase cond) = IfASTs[i];
                cond.Compile(writer, args);
                writer.Write(Bytecode.If);
                trueSkips.Add(new(writer));
                SizePosition falseSkip = new(writer);
                trueSkips.Last().Begin = writer.BaseStream.Position;

                foreach (ASTBase ast in asts)
                {
                    ast.Compile(writer, args);
                    writer.Write(Bytecode.ClearStack);
                }

                falseSkip.SaveSize();
            }

            if (ElseASTs?.Length > 0)
            {
                (ASTBase[] asts, ASTBase cond) = IfASTs.Last();
                cond.Compile(writer, args);
                writer.Write(Bytecode.IfElse);
                SizePosition trueLen = new(writer);
                SizePosition elseLen = new(writer);
                trueLen.Begin = writer.BaseStream.Position;

                foreach (ASTBase ast in asts)
                {
                    ast.Compile(writer, args);
                    writer.Write(Bytecode.ClearStack);
                }

                trueLen.SaveSize();
                elseLen.Begin = writer.BaseStream.Position;

                foreach (ASTBase ast in ElseASTs)
                {
                    ast.Compile(writer, args);
                    writer.Write(Bytecode.ClearStack);
                }

                elseLen.SaveSize();
            }
            else
            {
                (ASTBase[] asts, ASTBase cond) = IfASTs.Last();
                cond.Compile(writer, args);
                writer.Write(Bytecode.If);
                trueSkips.Add(new(writer));
                SizePosition falseSkip = new(writer);
                trueSkips.Last().Begin = writer.BaseStream.Position;

                foreach (ASTBase ast in asts)
                {
                    ast.Compile(writer, args);
                    writer.Write(Bytecode.ClearStack);
                }

                falseSkip.SaveSize();

                writer.Write(Bytecode.PushNull);
            }

            foreach (SizePosition trueSkip in trueSkips)
            {
                trueSkip.SaveSize();
            }
        }

        public override string ToString()
        {
            return $"if ({IfASTs.First().Item2}) {{ {string.Join("; ", IfASTs.First().Item1 as object[])}}}" +
                   $"{(IfASTs.Length > 1 ? string.Join(' ', IfASTs.Skip(1).ToList().ConvertAll(a => $" else if ({a.Item2}) {{ {string.Join("; ", a.Item1 as object[])}}}")) : string.Empty)}" +
                   $"{(ElseASTs?.Length > 0 ? $" else {{ {string.Join("; ", ElseASTs as object[])} }}" : string.Empty)}";
        }
    }

    public class WhileAST : ASTBase
    {
        public ASTBase Condition { get; set; }
        public ASTBase[] ASTs { get; set; }

        public WhileAST(ASTBase condition, ASTBase[] asts)
        {
            Condition = condition;
            ASTs = asts;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            writer.Write(Bytecode.Loop);
            SizePosition sizePos = new(writer);
            new IfAST(new[] { (new ASTBase[] { new BreakAST(new ConstantAST(null)) }, new NotAST(Condition) as ASTBase) }).Compile(writer, args);
            writer.Write(Bytecode.ClearStack);
            foreach (ASTBase ast in ASTs)
            {
                ast.Compile(writer, args);
                writer.Write(Bytecode.ClearStack);
            }
            sizePos.SaveSize();

        }

        public override string ToString()
        {
            return $"while ({Condition}) {{ {string.Join("; ", ASTs as object[])} }}";
        }
    }

    public class ForAST : ASTBase
    {
        public string Name { get; set; }
        public ASTBase Enumerable { get; set; }
        public ASTBase[] ASTs { get; set; }

        public ForAST(string name, ASTBase enumerable, ASTBase[] asts)
        {
            Name = name;
            Enumerable = enumerable;
            ASTs = asts;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            NewVariableAST enumVar = new($"__{RandomString(5)}", new InvokeAST(
                    new GetFieldAST(
                            Enumerable,
                            nameof(IEnumerable<object>.GetEnumerator)
                    )));
            NewVariableAST itemVar = new(Name, new GetFieldAST(
                    new GetVariableAST(enumVar.Name),
                    nameof(IEnumerator<object>.Current)
                ));
            IfAST nextEnumVar = new(new[] {
                (new ASTBase[] { new BreakAST(new ConstantAST(null)) },
                new NotAST(new InvokeAST(new GetFieldAST(
                    new GetVariableAST(enumVar.Name),
                    nameof(IEnumerator<object>.MoveNext)
                ))) as ASTBase)
            });

            enumVar.Compile(writer, args);
            writer.Write(Bytecode.ClearStack);
            writer.Write(Bytecode.Loop);
            SizePosition sizePos = new(writer);
            nextEnumVar.Compile(writer, args);
            writer.Write(Bytecode.ClearStack);
            itemVar.Compile(writer, args);
            writer.Write(Bytecode.ClearStack);
            foreach (ASTBase ast in ASTs)
            {
                ast.Compile(writer, args);
                writer.Write(Bytecode.ClearStack);
            }
            sizePos.SaveSize();
        }

        public override string ToString()
        {
            return $"for {Name} in ({Enumerable}) {{ {string.Join("; ", ASTs as object[])} }}";
        }
    }

    public class LoopAST : ASTBase
    {
        public ASTBase[] ASTs { get; set; }

        public LoopAST(ASTBase[] asts)
        {
            ASTs = asts;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            writer.Write(Bytecode.Loop);
            SizePosition sizePos = new(writer);
            foreach (ASTBase ast in ASTs)
            {
                ast.Compile(writer, args);
                writer.Write(Bytecode.ClearStack);
            }
            sizePos.SaveSize();
        }

        public override string ToString()
        {
            return $"loop {{ {string.Join("; ", ASTs as object[])} }}";
        }
    }

    public class SizePosition
    {
        public BinaryWriter Writer;
        public long SizePos;
        public long Begin;
        public int Size => (int)(Writer.BaseStream.Position - Begin);

        public SizePosition(BinaryWriter writer)
        {
            SizePos = writer.BaseStream.Position;
            Writer = writer;
            Writer.Write(0);
            Begin = writer.BaseStream.Position;
        }

        public int SaveSize()
        {
            long prev = Writer.BaseStream.Position;
            int size = Size;
            Writer.BaseStream.Position = SizePos;
            Writer.Write(size);
            Writer.BaseStream.Position = prev;
            return size;
        }
    }
}
