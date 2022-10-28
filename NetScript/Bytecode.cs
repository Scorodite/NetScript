using System.IO;

namespace NetScript
{
    public enum Bytecode : byte
    {
        ClearStack,
        PopStack,
        PushConst,
        PushName,
        PushNull,
        PushTrue,
        PushFalse,
        PushContextValue,

        NewVariable,
        GetVariable,
        SetVariable,

        GetField,
        SetField,

        GetIndex,
        SetIndex,

        Invoke,
        Generic,

        CreateFunction,
        If,
        IfElse,
        Loop,
        AddSubcontext,
        Constructor,
        TryCatch,

        Return,
        Break,
        Continue,
        Output,
        Throw,

        And,
        Or,
        NullCoalescing,

        Sum, Sub, Mul, Div, Mod,
        ShiftLeft, ShiftRight,
        BinAnd, BinOr, BinXor,
        Equal, NotEqual, Greater, Less,
        GreaterOrEqual, LessOrEqual, Range, Convert, IsType,
        
        Rev, BinRev, Not, GetTypeObj, Default,

        CreateList, CreateArray, GetArrayType,

        ConstByte, ConstSbyte, ConstShort, ConstUShort, ConstInt, ConstUInt,
        ConstLong, ConstULong, ConstFloat, ConstDecimal, ConstDouble,
        ConstChar, ConstString,
    }

    public static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, Bytecode bc)
        {
            writer.Write((byte)bc);
        }
        public static Bytecode ReadBytecode(this BinaryReader reader)
        {
            return (Bytecode)reader.ReadByte();
        }
    }
}
