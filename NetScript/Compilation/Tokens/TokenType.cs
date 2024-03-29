﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScript.Compilation.Tokens
{
    public enum TokenType
    {
        Sum, Sub, Mul, Div, Mod, BinAnd, BinOr, BinXor,
        And, Or,
        Equal, NotEqual, Greater, GreaterOrEqual, Less, LessOrEqual,
        ShiftLeft, ShiftRight,
        IsType, IsNotType, NullCoalescing, Convert, GetType, GetName, Default,
        BinRev, Not,
        Range,
        Assign, Field, Import,
        Function, If, Else, While, For, Loop, In,
        Return, Break, Continue, Output, Throw,
        String, Char,
        Byte, Sbyte, Short, Ushort, Int, Uint, Long, Ulong, Bool, Null,
        Float, Double, Decimal,
        OpeningQuote, ClosingQuote,
        OpeningGroup, ClosingGroup,
        OpeningIndex, ClosingIndex,
        OpeningGeneric, ClosingGeneric,
        EOL, Skip, Name, Sep, Clarification,
        NewVariable,
        LoadDll,
        Try,
        Catch,
    }
}
