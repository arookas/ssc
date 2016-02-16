## Language

SunScript is the name of the language parsed natively by _ssc_. It closely resembles both JavaScript and the native byte-code used by Super Mario Sunshine's SPC interpreter. A SunScript file uses the extension ".sun" for convenience. Below is a brief summary of the language's syntax.

### Comments

Both single-line comments and multi-line comments are supported. A single line comment begins with `//` and ends at the next new-line character.  A multi-line comment begins with `/*` and ends with `*/`.

### Variables

A variable is created by use of the `var` keyword and may initially be either _declared_ or _defined_.
A declaration leaves the variable with an undefined initial value, while a definition assigns a value to the variable explicitly:

```javascript
var a;
var b = 33;
```

---

There are three primitive types in SunScript, as shown in the following table.
All variables are dynamically typed and can change at any time by simply assigning it an expression of another type.

|Type  |       Example|
|:-----|-------------:|
|int   |`132`, `-0xff`|
|float |`15.64`, `9.0`|
|string|  `"foo\nbar"`|

SunScript also has the two boolean keywords `true` and `false` (currently defined as the integers one and zero, respectively).

---

To get the type of any variable or expression, use the `typeof` statement:

```javascript
var a = 1.1 * 8;
var b = typeof(a);
var c = typeof(1.1 * 8); // you may also put expressions as the argument
```

Constants for the possible return values are defined in the [standard include library](stdlib/common.sun).

---

To explicitly cast to the integer and floating-point types, use the `int` and `float` casting statements:

```javascript
const PI = 3.14;
var piIsExactlyThree = int(PI); // *gasp*
```

#### Constants

Read-only variables may be created via the `const` keyword.
Only constant definitions are allowed; constant declarations are **not** supported.

To save space in the output binary, constants are not stored in the symbol table, nor are they compiled in the text section.
Instead, they are simply evaluated each time they are actually used (similar to macros in CPP):

```javascript
const PI = 3.14;
const TWOPI = 2 * PI;
const R = 300.0;

var circ = R * TWOPI; // this actually compiles to 'var circ = 300.0 * (2 * (3.14));'
```

_**Note:** You may assign only expressions which are evaluated to be constant._

### Callables

There are two types of callables in SunScript: _builtins_ and _functions_.
A builtin may only be declared (must not have a body), while a function may only be defined (must have a body).
Callables may have any number of parameters.

To define a function, use the `function` keyword.
For builtins, use the `builtin` keyword:

```javascript
builtin getSystemFlag(flag);
builtin setSystemFlag(flag, value);

function setOnSystemFlag(flag) { setSystemFlag(flag, true); }
function setOffSystemFlag(flag) { setSystemFlag(flag, false); }
```

A callable may have any number of parameters.
Each parameter is dynamically typed.
A builtin may have a variadic signature by specifying an ellipsis keyword `...` as the final parameter (variadic functions are **not** supported):

```javascript
builtin print(...); // variadic builtin

print("Hello, ", "world!"); // this is legal
print("I have ", 3, " arguments."); // so is this
```

A callable's return value is also dynamic.
Use a `return` statement to override the interpreter's default return value for a given code path.

---

Functions and builtins may be called either as standalone statements or in expressions.
To call a function or builtin, simply pass its name, followed by any arguments, each separated by a comma:

```javascript
appearReadyGo(); // calls the function 'appearReadyGo' with no arguments
insertTimer(1, 0); // calls the function 'insertTimer' with two arguments, '1' and '0'
```

_**Note:** You cannot call a function or builtin in code preceding its definition or declaration, respectively._

### Modifiers

A declared symbol may be assigned compile-time modifiers.
The modifiers follow their respective keywords in their declaration/definition:

```javascript
var foo; // global symbol (can be resolved from other scripts)
var local bar; // global symbol (can be resolved only from the current script)

function local const getBaz() {
    return 132;
}

const local baz = getBaz(); // getBaz is marked as constant
```

_ssc_ supports the following modifiers:

|Modifier|Description|
|--------|-----------|
|`local`|Resolves only within the current script file. Applies to only script-scope symbols.|
|`const`|Marks the symbol as constant. Allows for the symbol's use in `const` assignments.|

The following matrix details which symbol types support which modifiers:

|type|`local`|`const`|
|----|:-----:|:-----:|
|variable|✓| |
|constant|✓| |
|function|✓|✓|
|builtin| |✓|

### Operators

Operators and their precedence in SunScript are largely borrowed from C++.
The following table describes the operators supported by SunScript, as well as their precedence and associativity:

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

```javascript
function checkTime(time) {
    if (time < 30) {
        startMiss();
        exit;
    }
}
var i = 0;
while (i < 100) {
    checkTime(i);
    if (stop) {
        break;
    }
}
```

#### Named Loops

Loops may be named.
To name a loop, simply prefix the loop with a label.
`break` and `continue` statements may be passed the name of the loop which they affect:

```javascript
outer_loop:
for (var a; a < 4; ++a) {
    for (var b; b < 4; ++b) {
        if (b == 2) break outer_loop;
    }
}
```

### Importing

You may split a script amongst several files.
Doing this requires the use of the `import` statement.
Simply pass the name of the SunScript file to import:

```javascript
import "constants.sun";
import "C:/math.sun";
```

_**Note:** It is recommended to avoid using backslashes, as they may be interpreted as an escape._

Importing files is managed by the `sunImportResolver` instance passed to the compiler.
The resolver can be the default (`sunImportResolver.Default`) or a completely custom one.
If an import resolver fails to find a file, a compiler error will occur.

---

The default import resolver, `sunImportResolver.Default`, imports scripts by searching and loading files from disk. Where the resolver looks for the script depends on whether the given name is _absolute_ or _relative_:

- If the name is _relative_, then the resolver will first append the name to the directory of the current file. If the script then is not found there, it will then look for the script in the compiler's executable directory.
- If the name is _absolute_, then the resolver will simply look for the script only in the path specified.

The resolver also keeps track of all files that have been imported and will skip over files whose contents have already been compiled (in order to prevent recursion).
