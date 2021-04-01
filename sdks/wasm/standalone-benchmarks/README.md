# WASM standalone benchmarks

This folder contains stand-alone benchmarks from various sources. Each folder contains one or more .cs files for the benchmark, and if the benchmark was ported from JavaScript the source benchmark is in the folder as well in JS form.

# Running benchmarks

All commands should be run from within the standalone-benchmarks directory.

## .NET Core
```dotnet run --project benchmarkname``` to build & run a benchmark using .NET Core. 

## Mono
Use ```./run-benchmark-mono.sh benchmarkname``` to automatically build, package and run the benchmark, or manually:

```dotnet build benchmarkname``` to build the debug-mode assembly for the benchmark, then:

```mono benchmarkname/bin/Debug/netcoreapp2.1/benchmarkname.dll``` to run the benchmark in Mono.

Use ```./run-benchmark-mono-aot.sh benchmarkname``` to run using full AOT,
 and use ```./run-benchmark-mono-interpreter.sh benchmarkname``` to run in interpreter-only mode.

## node.js
Use ```./run-benchmark-wasm.sh benchmarkname``` to automatically build, package and run the benchmark, or manually:

```dotnet build benchmarkname``` to build the debug-mode assembly for the benchmark, then:

```mono ../packager.exe --out=wasm/benchmarkname benchmarkname/bin/Debug/netcoreapp2.1/benchmarkname.dll``` to generate the WASM output, then finally:

```node test-runner.js benchmarkname wasm/benchmarkname``` to run the benchmark.