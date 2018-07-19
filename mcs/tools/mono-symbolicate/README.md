Mono Symbolicate Tool - README

Usage
-----------------------------

```
mono-symbolicate <msym dir> <input file>
mono-symbolicate store-symbols <msym dir> [<dir>]+
```

Description
-----------------------------

`mono-symbolicate` is a tool that converts a stack trace with `<filename unknown>:0`
into one with file names and line numbers.

The output of calling this tool will be the provided stacktracesfile where
`<filename unknown>:0` parts are replaced by a file name and a line number.

The tool uses the AOTID and MVID metadata contained at the end of the stacktraces,
to retrieve the symbols mapping code into line numbers from a provided symbol directory.

When `mono-symbolicate` is called with a symbol directory and a file containing a stacktrace:
``` mono-symbolicate <msym dir> <input file> ```
The tool writes into stdout the file contents while adding file location to stack frames when
it is possible to symbolicate with the symbols available on the symbol directory.

## Symbol directory
The symbol directory contains subfolder named as a MVID or AOTID.
 - MVID subfolders contain .dll/.exe and .mdb files.
 - AOTID subfolder contain .msym files.

Managed assemblies can be added by calling `mono-symbolicate` with the command `store-symbols`:
```
mono-symbolicate store-symbols <msym dir> [<dir>]+
```

.msym are generated and stored automatically in an AOTID subfolder at the same time the assembly
is compiled ahead of time, by running a command such as:
```
mono --aot=msym-dir=<msym dir>
```

## Practical example

If you do not have `mono-symbolicate` installed on your system you can set it to the built one by doing:
```
alias mono="MONO_PATH=../../class/lib/net_4_x ../../../runtime/mono-wrapper"
alias mono-symbolicate="mono ../../class/lib/net_4_x/mono-symbolicate.exe"
```

For the example we will use the csharp file `Test/StackTraceDumper.cs` which contains a program that
outputs a larger number of stacktraces.

The first step is to compile our program with debug data.
```
mkdir example
mcs -debug -out:example/Program.exe Test/StackTraceDumper.cs
```

Next we need to create the symbol directory and store all the symbols we might need while symbolicating.
```
mkdir example/msym-dir
mono-symbolicate store-symbols example/msym-dir example
```

After running `mono-symbolicate store-symbols` command the directory `example/msym-dir` should have a subdirectory
named as a minified GUID containing the Program.exe and Program.mdb.

If we want to symbolicate the stacktraces containing BCL stack frames we need to add those symbols too.
```
mono-symbolicate store-symbols example/msym-dir ../../class/lib/net_4_x
```

We delete the mdb file so on our next step the generated stack trace won't have line numbers.
```
rm example/*.mdb
```

Now we can run our program and dump its output to `example/out`.
```
mono example/Program.exe > example/out
```

Doing `cat example/out` shows us a large number of stacktraces without any line number.

We can finally run `mono-symbolicate` to replace all the `<filename unknown>:0` with actually useful file names and line numbers.
```
mono-symbolicate example/msym-dir example/out
```
The previous command should display the same as `cat example/out` but this time with file names and line numbers.

