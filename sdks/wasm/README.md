# Contents
- **sdk/out/wasm-bcl/wasm** directory: Core libraries to be used with the runtime.
- **driver.c**, **libmonosgen-2.0.a**, **library_mono.js**, **binding_support.js**: Source / Binaries for custom building the runtime. See compilation instructions down.
- **debug**, **release** directories: Pre-compiled runtimes using the above driver in release and debug configurations.
- **sample.html**, **sample.cs**: Sample code, see sample details below.


# Requirements

Mono requires the latest [emscripten][1] installed and built. Emscripten is *not* required if simply using the sample.

The pre-built binaries are compiled using the following command line for the debug build:

``` bash
emcc -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s ALIASING_FUNCTION_POINTERS=0 -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']" -s EMULATED_FUNCTION_POINTERS=1 -g4 -Os -s ASSERTIONS=1 --js-library library_mono.js --js-library binding_support.js --js-library dotnet_support.js driver.o mono/sdks/out/wasm-runtime-release/lib/{libmono-ee-interp.a,libmono-native.a,libmonosgen-2.0.a,libmono-ilgen.a,libmono-icall-table.a} -o debug/mono.js

```

# Compiling mono


## Commands for compiling mono

``` bash

make -C sdks/builds provision-wasm
make -C sdks/builds archive-wasm  NINJA=
make -C sdks/builds package-wasm-runtime package-wasm-cross package-wasm-bcl

```

# Sample

See [Getting Started Guides](./docs/getting-started)


# Testing instructions

## WebAssembly

First, ensure the `runtime`, `AOT` and `bcl` have been built and packaged in the `sdks/out` directory:

```bash
make -C sdks/builds package-wasm-runtime package-wasm-cross package-wasm-bcl
```

Build the test runner and test suites

```bash
make -C sdks/wasm build
```


Right now the following targets are available:

- mono: Executes the previous `package-wasm-*` step above.
- build: Build the test runner and test suites
- run-all-mini: Run mini test suite
- run-all-corlib: Run corlib test suite
- run-all-system: Run System test suite
- run-all-system-core: Run System.Core test suite
- run-all-binding: Run bindings test suite
- run-browser-tests: Run tests that require a browser environment
- build-aot-all: Build all AOT samples and tests
- run-aot-all: Run all AOT samples and tests
- build-aot-sample: Build hello world AOT sample
- run-aot-sample: Run hello world AOT sample
- build-interp-sample: Build hello world AOT interpreter sample
- run-interp-sample: Run hello world AOT interpreter sample
- build-aot-bindings-sample: Build sample using bindings
- build-aot-bindings-interp-sample: Build sample using bindings
- clean: cleans the wasm directory

For bcl or runtime changes, you must manually run the corresponding build/package steps in `builds`.
For test suite changes, it's enough to just rerun the local target.


# Debugging

The debugger requires dotnet core version 2.1.301 or greater installed.

To experiment with the debugger, do the following steps:

- When calling `packager.exe` pass the `-debug` argument to it.
- Start Chrome with remote debugging enabled (IE `/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome --remote-debugging-port=9222`)
- Run the proxy: `dotnet run -p ProxyDriver/ProxyDriver.csproj`
- Connect to the proxy by visiting http://localhost:9300/ and select the tab you wish to debug from the list of tabs
- Refresh the debugged page and you should be set

Beware that the debugger is in active development so bugs and missing features will be present.

# WebAssembly packager.exe

Read usage information about the utility see [WebAssembly packager.exe](./docs/packager.md)

# AOT support

AOT support is enabled by passing --aot to the packager.

This depends on building the cross compiler which can be done using:

``` bash
make -C sdks/wasm cross
```

If you don't have jsvu installed, run `make toolchain` from `sdks/wasm`. It requires a recent version of node installed in your system.

Run `make run-aot-sample` to run an aot-ed hello world sample.

To build and run AOT test suites:

``` bash
make -C sdks/wasm build-aot-<suite name>
make -C sdks/wasm check-aot-<suite name>
```

## AOT Bindings sample

To build the `sample` that uses bindings and http.

``` bash
make -C sdks/wasm build-aot-bindings-sample
```

This will build the `sample` in the `wasm/bin/aot-bindings-sample` ready to be served for browser consumption.

# Notes

[1]: https://github.com/kripken/emscripten

[2]: https://docs.microsoft.com/en-us/dotnet/framework/tools/developer-command-prompt-for-vs
