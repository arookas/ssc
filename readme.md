# ssc

## Summary

_ssc_ is a basic, work-in-progress compiler for SunScript. It supports all of the byte-code functionality and provides some basic compile-time functionality as well. The compiler compiles to the SPC binary format (.sb files) used by Super Mario Sunshine.

This program utilizes the [Grammatica](http://grammatica.percederberg.net/) library to generate a LL parser using a grammar syntax file.

## Usage

To use _ssc_, fire it up via the command prompt and pass it the single SunScript file to be compiled as the argument. If a compiler or syntax error occurs, details will be printed in the output; otherwise, a compiled .sb file with the same name will be created in the same folder as the input.

## Language

SunScript is the name of the language parsed natively by _ssc_. It closely resembles both JavaScript and the native byte-code used by Super Mario Sunshine's SPC interpreter. A SunScript file uses the extension ".sun" for convenience. Below is a brief summary of the language's syntax.

### Comments

Both single-line comments and multi-line comments are supported. A single line comment begins with `//` and ends at the next new-line character.  A multi-line comment begins with `/*` and ends with `*/`.

### Variables

A variable is created by use of the `var` keyword. A new variable may be either declared or defined. A definition assigns a default value explicitly to the variable, while a declaration assigns the interpreter's default value:

```
var a; // declares a variable with default value
var b = 33; // defines a variable by explicitly giving starting value
var c += b; // compound operators are also available (but not very useful for definitions)
```

There are three primitive types in SunScript, as shown in the following table. All variables are dynamically typed and can change at any time by simply assigning it an expression of another type.

|Type  |       Example|
|:-----|-------------:|
|int   |`132`, `-0xff`|
|float |`15.64`, `9.0`|
|string|  `"foo\nbar"`|

SunScript also has the two boolean keywords `true` and `false` (currently defined as the integers one and zero, respectively).

---

To get the type of any variable or expression, use the ``typeof`` statement:

```
var a = 1.1 * 8;
var type_a = typeof(a);
var type_b = typeof(1.1 * 8); // type_a == type_b
```

To explicitly cast to the integer and floating-point types, use the `int` and `float` casting statements:

```
const PI = 3.14;
var piIsExactlyThree = int(PI); // *gasp*
```

#### Constants

Read-only variables may be created via the `const` keyword. Only constant definitions are allowed; constant declarations are **not** supported.

To save space in the output binary, constants are not stored in the symbol table, nor are they compiled in the text section. Instead, they are simply evaluated each time they are actually used (similar to macros in CPP):

```
const PI = 3.14;
const TWOPI = 2 * pi;
const R = 300.0;

var circ = R * TWOPI; // this actually compiles to 'var circ = r * (2 * (3.14));'
```

_**Note:** You may have function calls or any other valid expression assigned to a constant._

### Functions

There are two types of callables in SunScript: _builtins_ and _functions_. A builtin may only be declared (must not have a body), while a function may only be defined (must have a body). Callables may have any number of parameters.

To define a function, use the `function` keyword. For builtins, use the `builtin` keyword:

```
builtin getSystemFlag(flag);
builtin setSystemFlag(flag, value);

function setOnSystemFlag(flag) { setSystemFlag(flag, true); }
function setOffSystemFlag(flag) { setSystemFlag(flag, false); }
```

A callable may have any number of parameters. Each parameter is dynamically typed. A builtin may have a variadic signature by specifying an ellipsis keyword `...` as the final parameter (variadic functions are **not** supported).

A callable's return value is also dynamic. Use a `return` statement to override the interpreter's default return value for a given code path.

---

Functions and builtins may be called either as standalone statements or in expressions. To call a function or builtin, simply pass its name, followed by any arguments, each separated by a comma:

```
appearReadyGo(); // calls the function 'appearReadyGo' with no arguments
insertTimer(1, 0); // calls the function 'insertTimer' with two arguments, '1' and '0'
```

_**Note:** You cannot call a function or builtin in code preceding its definition or declaration, respectively. All callable declarations and definitions must also be in the global scope._

### Operators

The following table describes the operators supported by SunScript, as well as their precedence and associativity.

|Precedence|Symbol|Name|Associativity|
|:--------:|:-----|:---|:-----------:|
|-1|`=`<br>`+=` `-=` `*=` `/=` `%=` `&=` `|=`|assignment<br>compound assignment|right|
|0|`||`|logical-OR|left|
|1|`&&`|logical-AND|left|
|2|`|`|bitwise-OR|left|
|3|`&`|bitwise-AND|left|
|4|`==` `!=`|equality and inequality|left|
|5|`<` `>` `<=` `>=`|comparison|left|
|6|`<<` `>>`|bitwise-shift|left|
|7|`+` `-`|addition and subtraction|left|
|8|`*` `/` `%`|multiplication, division, and modulo|left|
|9|`!`<br>`-`|logical-NOT<br>negation|right|
|10|`[?:]`|ternary conditional|right|

### Flow Control

SunScript has support for `while`, `do`, and `for` loops, as well as the `exit`, `break`, `continue`, and `return` statements:

```
function checkTime(time) {
    if (time < 30) {
        startMiss();
        exit;
    }
}
for (;;) {
    checkTime();
    if (stop) {
        break;
    }
}
```

#### Named Loops

Loops may be named. To name a loop, simply prefix the loop with a label. `break` and `continue` statements may be passed the name of the loop which they affect:

```
outer_loop:
for (var a; a < 4; a += 1) {
    for (var b; b < 4; b += 1) {
        if (b == 2) break outer_loop;
    }
}
```

### Importing

You may split a script amongst several files. Doing this requires the use of the `import` statement. Simply pass the name of the SunScript file to import:

```
import "constants.sun"; // looks for 'constants.sun' in current directory
import "C:/math.sun"; // looks for 'math.sun' in the root C:/ drive only
```

_**Note:** It is recommended to use forward slashes as a path delimiter as a backslash will be interpreted as an escape._

The `import` statement supports both relative and absolute path names. If the path is relative, then the file will be looked in two places: first, the path to the file currently being compiled; otherwise, if not there, the path of the compiler executable. Otherwise, if the path is absolute, only that path is searched as-is.

If the file cannot be found, a compiler error will occur.

_**Note:** In order to prevent recursive inclusion, _ssc_ keeps track of all imported files and will ignore `import` statements whose files have already been compiled before._