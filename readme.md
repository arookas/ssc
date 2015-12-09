# ssc

## Summary

_ssc_ is a basic, work-in-progress compiler for SunScript. It supports all of the byte-code functionality and provides some basic compile-time functionality as well. The compiler compiles to the SPC binary format (.sb files) used by Super Mario Sunshine.

This program utilizes the [Grammatica](http://grammatica.percederberg.net/) library to generate a LL parser using a grammar syntax file.

## Usage

To use _ssc_, fire it up via the command prompt and pass it the single SunScript file to be compiled as the argument. If a compiler or syntax error occurs, details will be printed in the output; otherwise, a compiled .sb file with the same name will be created in the same folder as the input.

## Language







For more information on the SunScript language and its syntax, see language.md.