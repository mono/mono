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

For this we'll use the included sample web page in the sdk.

### Step 1

Create a new directory for your application:

```
mkdir my-app
cd my-app
```

### Step 2

Copy the sample code from the SDK. We'll assume that the $WASM_SDK variable points to it.

```
cp $WASM_SDK/sample.html .
cp $WASM_SDK/sample.cs .
```

### Step 3

Compile and package your application.

```
csc /target:library sample.cs
mono $WASM_SDK/packager.exe sample.dll
```

The package command will generate a `runtime.js` file that will properly load the runtime and call `App.Init` after that.
It will copy the required files such as the runtime and class libraries.

### Step 4

Launch the provided webserver:

```
python $WASM_SDK/server.py
```

Go to `http://localhost:8000/sample.html`


# Debugging

To experiment with the debugger, do the following steps:

- When calling `packager.exe` pass the `-debug` argument to it.
- Start Chrome with remote debugging enabled (IE `/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome\ Canary --remote-debugging-port=9222`)
- Download and run the debugger proxy: https://github.com/kumpera/ws-proxy
- Connect to the remote debugged Chrome and pick the page which is running the wasm code
- Rewrite the request URL (just the `ws` argument) to use the proxy port instead of the browser port
- Refresh the debugged page and you should be set

Beware that the debugger is in active development so bugs and missing features will be present.

# Notes

[1]: https://github.com/kripken/emscripten

[2]: https://docs.microsoft.com/en-us/dotnet/framework/tools/developer-command-prompt-for-vs
