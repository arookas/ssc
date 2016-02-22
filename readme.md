# ssc

## Summary

_ssc_ is a basic, work-in-progress compiler for SunScript. It supports all of the byte-code functionality of Super Mario Sunshine's SPC interpreter.
The compiler compiles to the SPC binary format (.sb files) used by Super Mario Sunshine.

This program utilizes the [Grammatica](http://grammatica.percederberg.net/) library to generate an LL(k) parser using a grammar syntax file.

## Usage

To use _ssc_, create an instance of the `sunCompiler` class.
Use the `sunCompiler.Compile` method or any of its overloads to compile a script:

|Parameter|Description|
|---------|-----------|
|`name`|The name of the main script to compile. This is passed to the import resolver.|
|`output`|The output stream into which the compiled binary file will be written.|
|`binary`|The custom binary formatter to use for output. If not specified, the default SPC binary format will be used.|
|`resolver`|An instance of the import resolver to use. If not specified, `sunImportResolver.Default` will be used.|

_ssc_ by default resolves imports by loading files on disk (see [language.md](language.md) for more information).
To use a custom import resolver, create a new class inheriting from `sunImportResolver` and pass an instance of it to the `sunCompiler.Compile` method.

The result of compilation will be returned in a `sunCompilerResults` instance.
Use the various properties on this type to gather the information of the compilation:

|Property|Description|
|--------|-----------|
|`Success`|Whether the script was compiled successfully. If not, the `Error` property will be non-`null`.|
|`Error`|The fatal error which occured during compilation. If compilation was successful, this will be `null`.|
|`CompileTime`|The time it took to compile, measured as a `TimeSpan` instance.|
|`Data`|An array containing the data-table entries.|
|`Symbols`|An array containing the symbol-table entries.|

If the error is of the type `sunSourceException`, you can cast and retrieve the script name, line, and column of the error.

---

The repository comes with a [ST3 syntax definition file](sunscript.sublime-syntax):

![SunScript ST3 syntax preview](http://i.imgur.com/9VtTpks.png)

## Compiling

### Dependencies

There are several dependencies when building _ssc_.
All dependencies must be placed in the _lib-dir_ folder (see below), unless stated otherwise.

|Dependency|Notes|
|----------|-----|
|[arookas library](https://github.com/arookas/arookas)|Preferably, the library should be built in the same configuration as _ssc_ (e.g. debug, release).|
|[Grammatica 1.6](http://grammatica.percederberg.net/)|Already included in the repository.|

_**Note:** A Java runtime compatible with JDK 1.5 is required for generating the Grammatica parser classes during compilation.
For more information, see Grammatica's official [installation documentation](http://grammatica.percederberg.net/doc/release/install.html)._

### premake

This repository contains a [premake5](https://premake.github.io/) [configuration file](premake5.lua). 
The script generates a solution with the following projects:

 - **ssc**, the base _ssc_ API library
 - **frontend**, a basic command-line frontend
 - **sbdump**, a tool which dumps disassembly and other information on compiled SPC binaries

After setting up the dependencies, simply run the script through premake5 and build the resulting solution.
There are also options which are able to be configured via the premake5 command line:

|Option|Description|
|------|-----------|
|lib-dir|Sets the path to the dependencies. Defaults to the _lib/_ folder.|
|clean-functions|Compiles only used functions.|
|clean-symbols|Cleans symbol table of unused symbols.|

## Language

For more information on the SunScript language and its syntax, see [language.md](language.md).

## Extending the compiler

_ssc_ is designed to be extensible. For more information on extending _ssc_, see [extensions.md](extensions.md).
