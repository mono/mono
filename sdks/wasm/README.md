## Contents
- **bcl** directory: Core libraries to be used with the runtime.
- **driver.c**, **libmonosgen-2.0.a**, **library_mono.js**: Source / Binaries for custom building the runtime. See compilation instructions down.
- **debug**, **release** directories: Pre-compiled runtimes using the above driver in release and debug configurations.
- **sample.html**, **sample.cs**: Sample code, see sample details below.


## Compiling mono

Mono requires the latest [emscripten][1] installed and built. Emscripten is *not* required if simply using the sample.
The pre-built binaries are compiled using the following command line for the debug build:

```
emcc -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s ASSERTIONS=2 --js-library library_mono.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o debug/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"
```

## Sample

### Step 1
To setup the sample, first you need to compile & setup a directory to serve binaries:

**Linux / Mac**
```
mkdir managed
cp bcl/mscorlib.dll managed/
csc /nostdlib /target:library /r:managed/mscorlib.dll /out:managed/sample.dll sample.cs 
```
**Windows**

The **csc** executable is a build tool that is installed with Visual Studio. For the commands below, using a command window opened using a [*Developer Command Prompt for Visual Studio*][2] is convenient.
```
md managed
copy bcl\mscorlib.dll managed
csc /nostdlib /target:library /r:managed/mscorlib.dll /out:managed/sample.dll sample.cs 
```

### Step 2
Pick a pre-built runtime from one of the directories and copy all files to the root SDK directory. Allow file overwrites if necessary:

**Linux / Mac**
```
cp debug/* .
```
**Windows**
```
copy debug\* .
```

### Step 3
Start a web server from the SDK directory (where sample.html is):

```
python -m SimpleHTTPServer
```

Unfortunately, the above http server does not give wasm binaries the right mime type, which disables WebAssembly stream compilation.
The included server.py script solves this and can be used instead:
```
python server.py
```

### Step 4
From within a browser, go to `locahost:8000/sample.html` to see the sample app, which will show a text box (allowing C# code to be entered) and **Run** button when successfully built.


[1]: https://github.com/kripken/emscripten

[2]: https://docs.microsoft.com/en-us/dotnet/framework/tools/developer-command-prompt-for-vs
