## Extending the compiler

### Custom import resolvers

The import resolver's job is to load scripts identified by name. It is used to find both the main script (passed to the `sunCompiler.Compile` method) and scripts referenced by `import` statements. The resolver used by a compiler is the one passed through the `resolver` parameter. In order to create a custom import resolver, simply inherit from the abstract class `sunImportResolver`.

There are three primary methods in the import-resolver API:

|Method|Description|
|------|-----------|
|`Enter`|Called when the compiler begins compiling a script. The script about to be compiled is passed through the `file` parameter.|
|`Exit`|Similar to `Enter`, but is called when the compiler finishes compiling a script.|
|`Resolve`|This is where the actual import resolver logic is implemented. The name of the script is passed through the `name` parameter. The method initializes a new `sunScriptFile` instance and passes it to the `file` out parameter.|

#### Import results

The enum `sunImportResult` defines the result of the import:

|Value|Description|
|-----|-----------|
|`Loaded`|Script successfully loaded and is ready to be compiled.|
|`Missing`|Script could not be found. This is flagged as a compiler error.|
|`Skipped`|Script was found, but is not flagged for compilation. Instead, the compiler will silently ignore the import.|
|`FailedToLoad`|Script was found, but initializing the `sunScriptFile` instance failed. This is flagged as a compiler error.|

#### Script files

The `sunScriptFile` class has various properties:

|Property|Description|
|--------|-----------|
|`Name`|The name of the script. May be the original name passed to `sunImportResolver.Resolve`, or some other value. Does _not_ have to be unique.|
|`Stream`|The read-enabled stream containing the script's contents. This is used by the compiler to parse the script.|
|`Id`|An unsigned, 64-bit number used to uniquely identify this script. Every script successfully resolved by an import resolver **must** have a unique value for this property.|

### Custom binary formats

In the API, the `sunBinary` class manages writing the compiled binary. It receives instructions from the compiler to write commands, symbols, and data strings to the binary. The default implementation uses the standard SPC binary format used by Super Mario Sunshine. By inheriting from the `sunBinary` class, you may create your own output formats.

Almost all the methods in the base `sunBinary` class are either abstract or virtual. Thus, there is quite a bit of overhead in creating a custom binary formatter.

|Method|Description|
|------|-----------|
|`Open`|Called when the compiler begins compiling. Useful for initialization.|
|`Close`|Called when the compiler finishes compiling. Useful for finalization.|

Writing a binary is split into several sections:

#### .text

The .text section contains all the executable instructions that the interpreter will then run. To signal when the compiler begins and ends writing .text, there are the following virtual methods:

|Method|Description|
|------|-----------|
|`BeginText`|Called when the compiler begins writing the .text section.|
|`EndText`|Called when the compiler finishes writing the .text section.|

There is also a method for each of the standard commands defined by `TSpcInterp`. Their job is to simply write the specified command, with the following exceptions:

- It is the responsibility of the `WriteINT` method to redirect to `WriteINT0` or `WriteINT1`, if that functionality is desired.
`WriteINT0` and `WriteINT1` are **never** called by the compiler directly.

The API defines several methods and properties for navigating the .text section:

|Name|Description|
|----|-----------|
|`Offset`|Specifies the current offset into the .text section, as an unsigned, 32-bit integer.|
|`Goto`|Seeks to the specified offset into the .text section.|
|`Keep`|Pushes the current offset into the .text section onto the stack.|
|`Back`|Pops the element off the top of the stack and seeks to that offset in the .text section.|

#### .data

The .data section contains a table of all the strings used in scripts. These entries are referenced by the `str` instruction. Data entries are automatically interned by _ssc_, so your implementation need not check for duplicate entries.

The API has the following methods for writing the .data section:

|Method|Description|
|------|-----------|
|`BeginData`|Called when the compiler begins writing the .data section.|
|`WriteData`|Writes a data entry to the section, specified by the `data` parameter.|
|`EndData`|Called when the compiler finishes writing the .data section.|

#### .sym

The .sym section contains a table of all the symbols (imported and exported) used by the scripts. If the _clean-symbols_ option is specified when building _ssc_, then only symbols which are detected as being used will be written to the symbol table.

The API has the following methods for writing the .sym section:

|Method|Description|
|------|-----------|
|`BeginSymbol`|Called when the compiler begins writing the .sym section.|
|`WriteSymbol`|Writes a symbol entry to the section. The `type`, `name`, and `data` parameters specify the properties of the symbol being written. This method must manually implement string interning, if that is a requirement.|
|`EndSymbol`|Called when the compiler finishes writing the .sym section.|
