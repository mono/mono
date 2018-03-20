This project provides build scripts and sample apps for Mono targeting its supported platforms. Supported are Android, iOS and WebAssembly.

# Build instructions

## Setup

Copy `Make.config.sample` to `Make.config` and edit the new file to disable building the target you're not interested in.
Unless you have a very particular need, the BCL build should be left enabled as it's needed by all test runners.

## Targets

To build Mono for Android, iOS or WebAssembly, the build scripts can be found in `sdks/builds/`. There are 5 predefined targets that can be invoked:

 - toolchain: setup the toolchain so we can cross-compile the runtime for the appropriate platform
 - configure: configure the build of Mono
 - build: build Mono
 - package: provide a package of the built Mono, the result can be found in `sdks/builds/out/`
 - clean: clean the Mono build directory

The `make` targets are as follow:

```
# Android
make -C builds {toolchain,configure,build,package,clean}-android-{armeabi,armeabi-v7a,arm64-v8a,x86,x86_64}

# iOS
make -C builds {toolchain,configure,build,package,clean}-ios-{target{32,64},sim{32,64},cross{32,64}}

# WebAssembly
make -C builds {toolchain,configure,build,package,clean}-wasm-interp
```

# Testing instructions

## WebAssembly

First, ensure you built&packaged bcl and wasm-inter in the `builds` directory:

```
make build-wasm-interp package-wasm-interp
make build-bcl package-bcl
````

Go to the `wasm` directory for building and testing WebAssembly. Right now the following targets are available:

- build: Build the test runner and test suites
- run-mini: Run mini test suite
- run-corlib: Run corlib test suite
- run-system: Run System test suite
- run-system-core: Run System.Core test suite


For bcl or runtime changes, you must manually run the corresponding build/package steps in `builds`.
For test suite changes, it's enough to just rerun the local target.

# Dependencies

| Project     | Dependencies        |
| ----------- | ------------------- |
| Android     | Android SDK and NDK |
| iOS         | Xcode               |
| WebAssembly |                     |

See `sdks/versions.mk` for specific version numbers, and `sdks/paths.mk` for where they should be installed. These dependencies will not be installed as part of the build process, and will be expected to be present; an error will be triggered if it's not the case. If you need an additional version, please do contact us or submit a pull-request against [mono/mono](https://github.com/mono/mono).
