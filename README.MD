# NetScript
NetScript is C# embedded programming language that can operate with .NET classes and objects.
It contains bytecode compiler and interpreter
## Usage
```
using NetScript;

namespace Test
{
	class Program
	{
		static void Main()
		{
			NS.Run("Console.WriteLine(\"Hello world!\")");
		}
	}
}
```
## Syntax

### Constants
Avariable constants:
- null: `null`
- bool: `true` `false`
- byte: `0b` `255B`
- sbyte: `0sb` `-123SB` `127SB`
- short: `0s` `123S`
- ushort: `0us` `123US`
- int: `123`
- uint: `0ui` `123UI`
- long: `0l` `123L`
- ulong: `0ul` `123UL`
- float: `0f` `1.123F`
- decimal: `0de` `1.123DE`
- double: `0d` `1.123D` `2.345`
- char: `'c'` `'\n'`
- string: `"Sample string"`
### Variables
```
var variable = "Variable value";
Console.WriteLine(variable); // Variable value
```
### Operators
- C# binary operators: `+` `-` `*` `/` `%` `&&` `||` `??` `&` `|` `^` `==` `!=` `>` `<` `>=` `<=` `..` `is`
- C# unary operators: `-` `~` `!`
- `typeof` unary operator that outputs type of object
```
var a = 1sb;
Console.WriteLine(typeof a); // System.SByte
```
- `nameof` unary operator that outputs string of provided name 
```
Console.WriteLine(nameof a); // a
Console.WriteLine(typeof nameof a); // System.String 

// nameof operator requires only names
Console.WriteLine(nameof object()); // Compilation error
```
- `default` unary operator that outputs default value of provided type 
```
Console.WriteLine(default object); // null
Console.WriteLine(default int); // 0
var t = typeof 0;
Console.WriteLine(default t); // 0

// default operator requires only System.Type objects 
Console.WriteLine(default 123); // System.Exception: Default statement must recieve System.Type, not System.Int32
```
- `to` binary operator that converts objects in another types
```
var a = 1;
var b = a to byte;
Console.WriteLine(typeof a); // System.Int32
Console.WriteLine(typeof b); // System.Byte
```
### Creating object from C# classes
```
var obj = object();
var exp = expando();

Console.WriteLine(obj); // System.Object
Console.WriteLine(exp); // System.Dynamic.ExpandoObject
```
### Getting and setting fields, properties, invoking methods
```
var obj = expando();
obj.val1 = "Value 1";
obj.val2 = "Value 1";

Console.WriteLine(obj.val1);
Console.WriteLine(obj.val2);
```
### Using generics
```
var listOfStringsType = List<[string]>;
var listOfStrings = listOfStringsType();
Console.WriteLine(listOfStrings); // System.Collections.Generic.List`1[System.String]

// or

var listOfStrings = List<[string]>();
Console.WriteLine(listOfStrings); // System.Collections.Generic.List`1[System.String]
```
### Importing C# classes
```
import System.EventArgs;
// It is necessary to specify generics count
import System.Collections.Generic.Stack`1;

Console.WriteLine(EventArgs()); // System.EventArgs
Console.WriteLine(Stack<[string]>()); // System.Collections.Generic.Stack`1[System.String]

// You can import all types from specific namespace
import System.Collections.Generic.*;

Console.WriteLine(SortedDictionary<[string, int]>); // System.Collections.Generic.SortedDictionary`2[System.String,System.Int32]
```
NetScript has preimported classes:
- `System.Object` as `object`
- `System.Dynamic.ExpandoObject` as `expando`
- `System.Boolean` as `bool`
- `System.Byte` as `byte`
- `System.Int16` as `short`
- `System.Int32` as `int`
- `System.Int64` as `long`
- `System.SByte` as `sbyte`
- `System.UInt16` as `ushort`
- `System.UInt32` as `uint`
- `System.UInt64` as `ulong`
- `System.Single` as `float`
- `System.Decimal` as `decimal`
- `System.Double` as `double`
- `System.Char` as `char`
- `System.String` as `string`
- `System.Collections.Generic.List<>` as `List`
- `System.Collections.Generic.Dictionary<,>` as `Dictionary`
- `System.Console` as `Console`
- `System.Math` as `Math`
- `NetScript.Interpreter.Range` as `Range`
- `NetScript.Interpreter.Function` as `Function`
### Importing .NET dlls
```
loaddll "System.Collections.NonGeneric.dll";

import System.Collections.Stack;
Console.WriteLine(Stack());
// or
loaddll "CustomLib.dll";
import CustomLib.*;
```
### Functions
```
// Empty function
func empty

// Function with arguments but without body
func argsOnly(a, b, c)

// Function with generics only
// Generics works the same as arguments but allows only System.Type
func genericsOnly<[T1, T2, T3]>

//Function without arguments but with body
func bodyOnly {
	Console.WriteLine("Hello world");
}

// Function with arguments and generics
func genericsAndArgs<[T1, T2]>(a, b)

// Function with arguments and body
func argsAndBody(a, b) {
	Console.WriteLine(a + b);
}

// Function with generics and body 
func genericsAndBody<[T1, T2]> {
	Console.WriteLine("T1 = " + T1 + "\nT2 = " + T2);
}

// Function with arguments, generics and body 
func all<[T1]>(a) {
	Console.WriteLine("a = " + a + "\nT1 = " + T1);
}

// Unnamed function is created the same as function, but without indication name
// Unnamed functions require ; in the end of expression
var unnamed = func;

var unnamedFunc = func<[T]>(a) { Console.WriteLine(a is T); };

//Functions can be invoked as in C# 
all<[string]>(123);
// Output:
// a = 123
// T1 = System.String
```
### Converting NetScript functions into C# delegates
```
import System.EventArgs;
import System.EventHandler;

func nsfunc<[T]>(a, b) {
    Console.WriteLine("a = " + a + "\nb = " + b + "\nT = " + T);
    return a + b;
}

// In ToAction you must insert arguments types as generics
var csact = Function.ToAction<[string, string]>(nsfunc<[int]>);

// In ToAction you must insert arguments types and return type as generics
var csfunc = Function.ToFunc<[byte, byte, int]>(nsfunc<[string]>);

// In ToDelegate you must insert only delegate type as generic
var csev = Function.ToDelegate<[EventHandler]>(nsfunc<[sbyte]>);

Console.WriteLine(csact);
Console.WriteLine(csfunc);
Console.WriteLine(csev);
// Console:
// System.Action`2[System.String,System.String]
// System.Func`3[System.Byte,System.Byte,System.Int32]
// System.EventHandler

var funcOutput = csfunc(1b, 3b);
// Console:
//a = 1
//b = 3
//T = System.String

Console.WriteLine(funcOutput); // 4
Console.WriteLine(typeof funcOutput); // System.Int32
```
### Adding and removing events
```
import System.Windows.Forms.Button;

var button = Button();

func onClick(sender, ev) {
    Console.WriteLine(sender.ToString() + " was clicked");
}

button.Click = onClick;

// To be able to remove event you must store onClick delegate
import System.EventHandler;

var onClickDelegate = Function.ToDelegate<[EventHandler]>(onClick);

button.Click = onClickDelegate;

button.Click.Remove(onClickDelegate);
```
### Lists
```
var objectList = ["Sample string", 1, null];

var integerList = [int : 1, 2, 3, 4];

var emptyObjectList = [];

var emptyIntegerList = [int:];

Console.WriteLine(objectList); // System.Collections.Generic.List`1[System.Object]
Console.WriteLine(objectList.Count); // 3
Console.WriteLine(integerList); // System.Collections.Generic.List`1[System.Int32]
Console.WriteLine(integerList.Count); // 4
Console.WriteLine(emptyObjectList); // System.Collections.Generic.List`1[System.Object]
Console.WriteLine(emptyObjectList.Count); // 0
Console.WriteLine(emptyIntegerList); // System.Collections.Generic.List`1[System.Int32]
Console.WriteLine(emptyIntegerList.Count); // 0

Console.WriteLine(objectList[0]); // Sample string
```
### Arrays
```
var objectArray = {"Sample string", -1, null};

var integerArray = {int : 1, 2, 3, 4};

var emptyObjectArray = {};

var emptyIntegerArray = {int:};

Console.WriteLine(objectArray); // System.Object[]
Console.WriteLine(objectArray.Length); // 3
Console.WriteLine(integerArray); // System.Int32[]
Console.WriteLine(integerArray.Length); // 4
Console.WriteLine(emptyObjectArray); // System.Object[]
Console.WriteLine(emptyObjectArray.Length); // 0
Console.WriteLine(emptyIntegerArray); // System.Int32[]
Console.WriteLine(emptyIntegerArray.Length); // 0

Console.WriteLine(objectArray is object[]); // True
Console.WriteLine(integerArray is int[]); // True

Console.WriteLine(objectArray[1]); // -1
```
### Multidimensional arrays
```
var objectArray2d = { 2 :
	{'A', 2, null},
	{object(), 'B', 32B}
};
var integerArray2d = { int 2 :
	{1, 2, 3},
	{4, 5, 6}
};

var objectArray3d = {3 :
	{
		{'A', 1},
		{'B', 2b},
		{'C', 3sb}
	},
	{
		{'D', 4l},
		{'E', 5f},
		{'F', 6de}
	}
};

var integerArray3d = { int 3 :
	{
		{1, 2},
		{3, 4},
		{5, 6}
	},
	{
		{7, 8},
		{9, 10},
		{11, 12}
	}
};

Console.WriteLine(objectArray2d);
Console.WriteLine(integerArray2d);
Console.WriteLine(objectArray3d);
Console.WriteLine(integerArray3d);

Console.WriteLine(objectArray2d[1, 0]);
Console.WriteLine(objectArray3d[0, 1, 0]);
```
### Constructor
```
var exp = expando().{
	.name = "Example expando name";
	.age = 54;
};

Console.WriteLine(exp); // System.Dynamic.ExpandoObject
Console.WriteLine(exp.name); // Example expando name
Console.WriteLine(exp.age); // 54

// It is not nessesary to use newly created object in constructor
var exp2 = expando();
exp2.{
	.name = "Second expando name";
	.age = 55;
};
Console.WriteLine(exp2.name);
Console.WriteLine(exp2.age);

// Also you can get variable of object rightly after constructor
Console.WriteLine(expando().{
	.name = "Third expando name";
}.name);

// You can use another expressions in constructors
import System.Random;

Console.WriteLine(expando().{
	.name = "Second expando name";
	// Constructor object can be accessed by using "."
	Console.WriteLine(.); // System.Dynamic.ExpandoObject
	var r = Random();
	.age = r.Next(18, 99);
}.age); // 33
```
### If
```
var a = int.Parse(Console.ReadLine()); // 54

if a == 0 || a == 1 {
	Console.WriteLine("a == 0 || a == 1");
}
else if a == 2 {
	Console.WriteLine("a == 2");
}
else {
	Console.WriteLine("a < 0 || a > 2");
}
// Console:
// a < 0 || a > 2
```
### While
```
var i = 0;
while i < 100 {
	Console.WriteLine(i = i + 1);
}
// Console:
// 1
// ...
// 100
```
### For
```
var list = [string : "El1", "El2", "El3"];

for i in list {
	Console.WriteLine(i);
}
// Console:
// El1
// El2
// El3
```
### Loop, break and continue
```
var i = 0;

loop {
	i = i + 1;
	if i > 100 {
		break;
	}
	Console.WriteLine(i);
}
// Console:
// 1
// ...
// 100

var i = 0;
loop {
	i = i + 1;
	if i > 100 {
		break;
	}
	else if i % 2 == 0 {
		continue;
	}
	Console.WriteLine(i);
}
// Console:
// 1
// 3
// 5
// ...
// 97
// 99
```
### Try, catch and throw
```
import System.Exception;

try {
    Console.WriteLine(15 / 0);
} catch exceptionVariable {
    Console.WriteLine(typeof exceptionVariable); // System.DivisionByZeroException
}

func div(a, b) {
    if b == 0 {
        throw Exception("Can not divide by zero");
    }
    return a / b;
}

try {
    div(1, 0)
} catch ex {
    Console.WriteLine(ex.Message); // Can not divide by zero
}

// You can throw System.String and it will be converted in System.Exception
try {
    throw "Test";
} catch ex {
    Console.WriteLine(typeof ex); // System.Exception
    Console.WriteLine(ex.Message); // Test
}

// If you will throw not System.Exception or System.String,
// you will get NetScript.Interpreter.NotThrowableException
try {
    throw expando();
} catch ex {
    Console.WriteLine(typeof ex); // NetScript.Interpreter.NotThrowableException
    Console.WriteLine(ex.Object); // System.Dynamic.ExpandoObject
}
```
### Output
```
//You can exit from current context using "output" statement

if true {
	Console.WriteLine(1);
	output;
	Console.WriteLine(2);
}
// Console:
// 1

// You can use constructions like if, for, while, loop, try & catch as arguments

Console.WriteLine(if true { output "Yes" } else { output "No" }); // Yes

// In loops you can use "break" instead of "output"
Console.WriteLine(for i in Range(10) {
	if i > 5 {
		break i;
	}
}); // 6

// In try catch expression catch context by default will output catched exception
Console.WriteLine(typeof try {output 1 / 0} catch ex {}); // System.DivideByZeroException

// You can use output in constructors 
Console.WriteLine(expando().{
    .name = "Expando name";
    output .name;
}); // Expando name
```
## License

This project is licensed under [GPLv3](LICENSE).