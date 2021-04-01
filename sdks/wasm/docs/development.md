# NETCORE Support

Currently, only local development is supported on osx.

* Get a working ```dotnet/runtime``` repo.
* Build the 'Libs' subset in that repo.
* Add the following to ```sdks/Make.config```:
```
ENABLE_WASM_NETCORE=1
ENABLE_WASM_NETCORE_LOCAL_BUILD=1
DOTNET_REPO_PATH=<path to your dotnet/runtime repo>
```
* ```make runtime-netcore``` will build the runtime and corelib for wasm and copy the result into ```out/wasm-runtime-netcore-release```
where the rest of the build will pick it up similarly to the non-netcore case.
* See the ```run-interp-netcore-hello``` make target for a sample.
