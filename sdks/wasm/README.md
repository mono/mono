# Contents
- bcl Directory. Core libraries to be used with the runtime.
- driver.c, libmonosgen-2.0.a, library_mono.js: Source / Binaries for custom building the runtime. See compilation instructions down.
- debug, release: Pre-compiled runtimes using the above driver in release and debug configurations.
- sample.html / sample.cs: Sample code, see sample details down there.


# Compiling mono

Mono requires the latest emscripten installed and built[1]. The pre-built binaries are compiled using the following command line for the debug build:

```
	emcc -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s ASSERTIONS=2 --js-library library_mono.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o debug/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"

```

# Sample

To setup the sample, first you need to compile & setup a directory to serve binaries:

```
mkdir managed
cp bcl/mscorlib.dll managed/
csc /nostdlib /target:library /r:managed/mscorlib.dll /out:managed/sample.dll sample.cs 
```

Second, pick a pre-built runtime from one of the directories and copy all files to the the root SDK directory:

```
cp debug/* .
```

Once that's done, Start a web server from the SDK directory (where sample.html is):

```
python -m SimpleHTTPServer
```

Unfortunately, the above http server doesn't give  wasm binaries the right mime type, which disables WebAssembly stream compilation.
The included server.py script solves this and can be used instead.

Go to `locahost:8000/sample.html` and it should work.

[1] https://github.com/kripken/emscripten
