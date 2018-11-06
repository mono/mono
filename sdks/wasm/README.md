# Contents
- **bcl** directory: Core libraries to be used with the runtime.
- **driver.c**, **libmonosgen-2.0.a**, **library_mono.js**: Source / Binaries for custom building the runtime. See compilation instructions down.
- **debug**, **release** directories: Pre-compiled runtimes using the above driver in release and debug configurations.
- **sample.html**, **sample.cs**: Sample code, see sample details below.


# Compiling mono

Mono requires the latest [emscripten][1] installed and built. Emscripten is *not* required if simply using the sample.
The pre-built binaries are compiled using the following command line for the debug build:

```
emcc -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s ASSERTIONS=2 --js-library library_mono.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o debug/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"
```

# Sample

See [Getting Started Guides](./docs/getting-started)


# Debugging

The debugger requires dotnet core version 2.1.301 or greater installed.

To experiment with the debugger, do the following steps:

- When calling `packager.exe` pass the `-debug` argument to it.
- Start Chrome with remote debugging enabled (IE `/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome\ Canary --remote-debugging-port=9222`)
- Run the proxy: `dotnet dbg-proxy/ProxyDriver.dll`
- Connect to the remote debugged Chrome and pick the page which is running the wasm code
- Rewrite the request URL (just the `ws` argument) to use the proxy port instead of the browser port
- Refresh the debugged page and you should be set

Beware that the debugger is in active development so bugs and missing features will be present.

# AOT development

AOT experimentation happens with the following steps:

1) from `sdks` and configure it to disable all but WASM and BCL. (See sdks/Make.config.sample)
2) from `sdks/builds` hit `make package`
3) from `sdks/wasm` hit `make build`
4) from `sdks/wasm` hit `make build-aot-sample`
4) from `sdks/wasm/aot` hit `~/.jsvu/sm aot-driver.js`

If you don't have jsvu installed, run `make toolchain` from `sdks/wasm`. It requires a recent version of node installed in your system.

Now you can experiment with the `aot-sample` and `link-sample` make targets to try the toolchain. The first invokes the AOT compiler and the second links the results.

To update the runtimes used use the following make target in `sdks/build`

`package-wasm-interp` for the interpreter-based runtime
`package-wasm-aot` for the aot compiler
`package-wasm-aot-runtime` for the wasm runtime that works with AOT'd code.


To update the aot compiler:
```
make -C sdks/builds package-wasm-aot-compiler
make -C sdks/wasm aot-sample
make -C sdks/wasm link-sample
```

To update the aot runtime:
```
make -C sdks/builds package-wasm-aot
make -C sdks/wasm aot-sample
make -C sdks/wasm link-sample
```

To build and run AOT test suites:
```
make -C sdks/wasm build-aot-<suite name>
make -C sdks/wasm check-aot-<suite name>
```

# Notes

[1]: https://github.com/kripken/emscripten

[2]: https://docs.microsoft.com/en-us/dotnet/framework/tools/developer-command-prompt-for-vs
