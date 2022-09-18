using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScript.Compiler.Tokens
{
    public enum TokenType
    {
        Sum, Sub, Mul, Div, Mod, BinAnd, BinOr, BinXor,
        And, Or,
        Equal, NotEqual, Greater, GreaterOrEqual, Less, LessOrEqual,
        IsType, IsNotType, NullCoalescing, Convert, GetType, GetName, Default,
        BinRev, Not,
        Range,
        Assign, Field, Import,
        Function, If, Else, While, For, Loop, In, Return, Break, Continue, Output,
        String, Char,
        Byte, Sbyte, Short, Ushort, Int, Uint, Long, Ulong, Bool, Null,
        Float, Double, Decimal,
        OpeningQuote, ClosingQuote,
        OpeningGroup, ClosingGroup,
        OpeningIndex, ClosingIndex,
        OpeningGeneric, ClosingGeneric,
        EOL, Skip, Name, Sep,
        NewVariable,
        LoadDll,
        Try,
        Catch,
    }
}
