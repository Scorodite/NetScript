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
        public virtual bool ReturnsValue => true;

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

        public ASTBase ReturnOnly()
        {
            if (ReturnsValue)
            {
                return this;
            }
            else
            {
                throw new CompilerException("Required statement that returns value", Index);
            }
        }

        public static readonly GetVariableAST TypeObject = new("object");

        public ASTBase NullIfEmpty() =>
            this is EmptyAST ? new ConstantAST(null) : this;
    }

    public class EmptyAST : ASTBase
    {
        public override bool ReturnsValue => false;
        public static readonly EmptyAST Instance = new();

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            throw new CompilerException("Empty statement error");
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
        public override bool ReturnsValue => false;

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
        public override bool ReturnsValue => false;

        public BreakAST(ASTBase value)
        {
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Value.ReturnOnly().Compile(writer, args);
            writer.Write(Bytecode.Break);
        }

        public override string ToString()
        {
            return $"break {(Value is not ConstantAST con || con.Obj is not null ? Value.ToString() : string.Empty)}";
        }
    }

    public class ReturnAST : ASTBase
    {
        public ASTBase Value { get; set; }
        public override bool ReturnsValue => false;

        public ReturnAST(ASTBase value)
        {
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Value.ReturnOnly().Compile(writer, args);
            writer.Write(Bytecode.Return);
        }

        public override string ToString()
        {
            return $"return {(Value is not ConstantAST con || con.Obj is not null ? Value.ToString() : string.Empty)}";
        }
    }

    public class ThrowAST : ASTBase
    {
        public ASTBase Value { get; set; }
        public override bool ReturnsValue => false;

        public ThrowAST(ASTBase value)
        {
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Value.ReturnOnly().Compile(writer, args);
            writer.Write(Bytecode.Throw);
        }

        public override string ToString()
        {
            return $"throw {Value}";
        }
    }

    public class OutputAST : ASTBase
    {
        public ASTBase Value { get; set; }
        public override bool ReturnsValue => false;

        public OutputAST(ASTBase value)
        {
            Value = value;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Value.ReturnOnly().Compile(writer, args);
            writer.Write(Bytecode.Output);
        }

        public override string ToString()
        {
            return $"output {(Value is not ConstantAST con || con.Obj is not null ? Value.ToString() : string.Empty)}";
        }
    }

    public class ContinueAST : ASTBase
    {
        public ContinueAST() { }
        public override bool ReturnsValue => false;

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
            Value.ReturnOnly().Compile(writer, args);
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
            Value.ReturnOnly().Compile(writer, args);
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
        public override bool ReturnsValue => false;

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
            Obj.ReturnOnly().Compile(writer, args);

            foreach (ASTBase arg in Args)
            {
                arg.ReturnOnly().Compile(writer, args);
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
            Obj.ReturnOnly().Compile(writer, args);

            if (Args.Length > 0)
            {
                if (Args.FirstOrDefault(a => a is not EmptyAST) is null)
                {
                    writer.Write(Bytecode.GetArrayType);
                    writer.Write((byte)(Args.Length + 1));
                }
                else
                {
                    foreach (ASTBase arg in Args)
                    {
                        if (arg is EmptyAST)
                        {
                            throw new CompilerException("Tried to get empty index", Index);
                        }
                        arg.ReturnOnly().Compile(writer, args);
                    }

                    writer.Write(Bytecode.GetIndex);
                    writer.Write((byte)Args.Length);
                }
            }
            else
            {
                writer.Write(Bytecode.GetArrayType);
                writer.Write((byte)1);
            }
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
            Obj.ReturnOnly().Compile(writer, args);

            foreach (ASTBase arg in Args)
            {
                arg.ReturnOnly().Compile(writer, args);
            }

            Value.ReturnOnly().Compile(writer, args);

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
            Obj.ReturnOnly().Compile(writer, args);

            foreach (ASTBase arg in Args)
            {
                arg.ReturnOnly().Compile(writer, args);
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
            Obj.ReturnOnly().Compile(writer, args);
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
            Obj.ReturnOnly().Compile(writer, args);
            Value.ReturnOnly().Compile(writer, args);
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
        public string Name { get; set; }
        public string[] Args { get; set; }
        public string[] Generics { get; set; }
        public ASTBase[] ASTs { get; set; }

        public CreateFunctionAST(string name, string[] args, string[] generics, ASTBase[] asts)
        {
            Name = name;
            Args = args;
            Generics = generics;
            ASTs = asts;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            writer.Write(Bytecode.CreateFunction);
            writer.Write(string.IsNullOrWhiteSpace(Name) ? args.UnnamedID : args.GetNameID(Name));

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

            writer.Write(args.NextCodeSector());
            SizePosition sizePos = new(writer);
            Compiler.CompileAll(ASTs, writer, args);
            sizePos.SaveSize();

            if (!string.IsNullOrWhiteSpace(Name))
            {
                writer.Write(Bytecode.NewVariable);
                writer.Write(args.GetNameID(Name));
            }
        }

        public override string ToString()
        {
            return $"func {Name} {(Generics.Length > 0 ? $"<[{string.Join(", ", Generics as object[])}]>" : string.Empty)}" +
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
                cond.ReturnOnly().Compile(writer, args);
                writer.Write(Bytecode.If);
                trueSkips.Add(new(writer));
                SizePosition falseSkip = new(writer);
                trueSkips.Last().Begin = writer.BaseStream.Position;

                Compiler.CompileAll(asts, writer, args);

                falseSkip.SaveSize();
            }

            if (ElseASTs?.Length > 0)
            {
                (ASTBase[] asts, ASTBase cond) = IfASTs.Last();
                cond.ReturnOnly().Compile(writer, args);
                writer.Write(Bytecode.IfElse);
                SizePosition trueLen = new(writer);
                SizePosition elseLen = new(writer);
                trueLen.Begin = writer.BaseStream.Position;

                Compiler.CompileAll(asts, writer, args);

                trueLen.SaveSize();
                elseLen.Begin = writer.BaseStream.Position;

                Compiler.CompileAll(ElseASTs, writer, args);

                elseLen.SaveSize();
            }
            else
            {
                (ASTBase[] asts, ASTBase cond) = IfASTs.Last();
                cond.ReturnOnly().Compile(writer, args);
                writer.Write(Bytecode.If);
                trueSkips.Add(new(writer));
                SizePosition falseSkip = new(writer);
                trueSkips.Last().Begin = writer.BaseStream.Position;

                Compiler.CompileAll(asts, writer, args);

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

            Compiler.CompileAll(ASTs, writer, args);
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
                            Enumerable.ReturnOnly(),
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

            Compiler.CompileAll(ASTs, writer, args);

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
            Compiler.CompileAll(ASTs, writer, args);
            sizePos.SaveSize();
        }

        public override string ToString()
        {
            return $"loop {{ {string.Join("; ", ASTs as object[])} }}";
        }
    }

    public class TryCatchAST : ASTBase
    {
        public string ExceptionVariable { get; set; }
        public ASTBase[] TryASTs { get; set; }
        public ASTBase[] CatchASTs { get; set; }

        public TryCatchAST(string excVariable, ASTBase[] tryASTs, ASTBase[] catchASTs)
        {
            ExceptionVariable = excVariable;
            TryASTs = tryASTs;
            CatchASTs = catchASTs;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            writer.Write(Bytecode.TryCatch);
            writer.Write(args.GetNameID(ExceptionVariable));

            SizePosition tryBlockSize = new(writer);
            SizePosition catchBlockSize = new(writer);
            tryBlockSize.Begin += sizeof(int);

            Compiler.CompileAll(TryASTs, writer, args);

            tryBlockSize.SaveSize();
            catchBlockSize.Begin = writer.BaseStream.Position;

            Compiler.CompileAll(CatchASTs, writer, args);

            catchBlockSize.SaveSize();
        }

        public override string ToString()
        {
            return $"try {{ {string.Join("; ", TryASTs as object[])} }} catch {ExceptionVariable} {{ {string.Join("; ", CatchASTs as object[])} }}";
        }
    }

    public class GetContextValueAST : ASTBase
    {
        public GetContextValueAST() { }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            writer.Write(Bytecode.PushContextValue);
        }

        public override string ToString()
        {
            return ".";
        }
    }

    public class ConstructorAST : ASTBase
    {
        public ASTBase Obj { get; set; }
        public ASTBase[] ASTs { get; set; }

        public ConstructorAST(ASTBase obj, ASTBase[] asts)
        {
            Obj = obj;
            ASTs = asts;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            Obj.ReturnOnly().Compile(writer, args);

            writer.Write(Bytecode.Constructor);
            SizePosition size = new(writer);

            Compiler.CompileAll(ASTs, writer, args);

            size.SaveSize();
        }

        public override string ToString()
        {
            return $"{Obj}.{{ {string.Join("; ", ASTs as object[])} }}";
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

    public class ListAST : ASTBase
    {
        public ASTBase Type { get; set; }
        public ASTBase[] Items { get; set; }

        public ListAST(ASTBase type, ASTBase[] items)
        {
            Type = type;
            Items = items;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            foreach (ASTBase item in Items)
            {
                (item ?? TypeObject).ReturnOnly().Compile(writer, args);
            }

            Type.ReturnOnly().Compile(writer, args);

            writer.Write(Bytecode.CreateList);
            writer.Write((short)Items.Length);
        }

        public override string ToString()
        {
            return $"[ {Type} : {string.Join(", ", Items as object[])} ]";
        }
    }

    public class ArrayAST : ASTBase
    {
        public ASTBase Type { get; set; }
        public ASTBase[] Items { get; set; }
        public int Rank { get; set; }

        public ArrayAST(ASTBase type, ASTBase[] items, int rank)
        {
            Type = type;
            Items = items;
            Rank = rank;
        }

        public override void Compile(BinaryWriter writer, CompilerArgs args)
        {
            if (Rank <= 1)
            {
                foreach (ASTBase item in Items)
                {
                    item.ReturnOnly().Compile(writer, args);
                }

                (Type ?? TypeObject).ReturnOnly().Compile(writer, args);

                writer.Write(Bytecode.CreateArray);
                writer.Write((byte)1);
                writer.Write((short)Items.Length);
            }
            else
            {
                int[] lenghts = GetLenghts(Rank);

                CompileChildren(0, lenghts, writer, args);

                (Type ?? TypeObject).ReturnOnly().Compile(writer, args);

                writer.Write(Bytecode.CreateArray);
                writer.Write((byte)Rank);

                foreach (int len in lenghts)
                {
                    writer.Write((short)len);
                }
            }
        }

        protected int[] GetLenghts(int depth)
        {
            if (depth <= 1)
            {
                return new int[1] { Items.Length };
            }
            else if (Items.Length > 0 && Items.First() is ArrayAST subarr && subarr.Type is null && subarr.Rank <= 1)
            {
                int[] res = new int[depth];
                res[0] = Items.Length;
                int[] sub = subarr.GetLenghts(depth - 1);
                sub.CopyTo(res, 1);
                return res;
            }
            else
            {
                throw new CompilerException("Multidimensional array must contain arrays without type and rank clarification");
            }
        }
        
        protected void CompileChildren(int currRank, int[] lenghts, BinaryWriter writer, CompilerArgs args)
        {
            int len = lenghts[currRank];
            if (Items.Length != len)
            {
                throw new CompilerException("Subarray of multidimensional array must contain same items count");
            }

            if (currRank == lenghts.Length - 1)
            {
                foreach (ASTBase item in Items)
                {
                    item.ReturnOnly().Compile(writer, args);
                }
            }
            else
            {
                foreach (ASTBase item in Items)
                {
                    if (item is ArrayAST subarr && subarr.Type is null && subarr.Rank <= 1)
                    {
                        subarr.CompileChildren(currRank + 1, lenghts, writer, args);
                    }
                    else
                    {
                        throw new CompilerException("Subarray of multidimensional array must contain arrays without type and rank clarification");
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{{ {Type} {(Rank > 1 ? Rank : null)}{(Type is not null || Rank > 1 ? " : " : null)}{string.Join(", ", Items as object[])} }}";
        }
    }
}
