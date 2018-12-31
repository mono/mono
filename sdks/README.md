This project provides build scripts and sample apps for Mono targeting its supported platforms. Supported are Android, iOS and WebAssembly.

# Build instructions

## Dependencies

 - automake 1.16.1

   if you have already built before using a previous version of automake you may need to clean the repo.

   ```
      git clean -xffd
   ```

   The previous should be sufficient but if that does not work then try hard resetting

   ```
   git reset --hard && git clean -xffd && git submodule foreach --recursive git reset --hard && git submodule foreach --recursive git clean -xffd && git submodule update --init --recursive
   ```

- [ninja build system](https://ninja-build.org)

  - Getting Ninja

    You can [download the Ninja binary](https://github.com/ninja-build/ninja/releases) or [find
it in your system's package manager](https://github.com/ninja-build/ninja/wiki/Pre-built-Ninja-packages)


## Setup

Copy `Make.config.sample` to `Make.config` and edit the new file to disable building the target you're not interested in.
Unless you have a very particular need, the BCL build should be left enabled as it's needed by all test runners.

## Building for XA/XI/XM and WebAssembly

To build Mono for Android, iOS or WebAssembly, the build scripts can be found in `sdks/builds/`. 

The `make` targets are as follow:

```
# Android
mono$ make -C sdks/builds provision-android && make -C sdks/android accept-android-license
mono$ make -C sdks/builds provision-mxe
mono$ make -C sdks/builds archive-android NINJA= IGNORE_PROVISION_ANDROID=1 IGNORE_PROVISION_MXE=1

# iOS
mono$ make -C sdks/builds archive-ios NINJA=

# WebAssembly
mono$ make -C sdks/builds provision-wasm
mono$ make -C sdks/builds archive-wasm  NINJA=

# Mac
mono$ make -C sdks/builds archive-mac [upcoming]
```

# Testing instructions

## WebAssembly

First, ensure the `runtime`, `AOT` and `bcl` have been built and packaged in the `builds` directory:

```
mono$ make -C sdks/builds package-wasm-runtime package-wasm-cross package-wasm-bcl
````

Go to the `wasm` directory for building and testing WebAssembly. Right now the following targets are available:

- mono: Encompasses all the previous steps
- build: Build the test runner and test suites
- run-all-mini: Run mini test suite
- run-all-corlib: Run corlib test suite
- run-all-system: Run System test suite
- run-all-system-core: Run System.Core test suite
- run-all-binding: Run bindings test suite
- run-browser-tests: Run tests that require a browser environment
- build-aot-all: build all AOT samples and tests
- run-aot-all: run all AOT samples and tests
- clean: cleans the wasm directory

For bcl or runtime changes, you must manually run the corresponding build/package steps in `builds`.
For test suite changes, it's enough to just rerun the local target.

# Dependencies

| Project     | Dependencies        |
| ----------- | ------------------- |
| Android     | Android SDK and NDK |
| iOS         | Xcode               |
| WebAssembly |                     |

See `sdks/versions.mk` for specific version numbers, and `sdks/paths.mk` for where they should be installed. These dependencies will not be installed as part of the build process, and will be expected to be present; an error will be triggered if it's not the case. If you need an additional version, please do contact us or submit a pull-request against [mono/mono](https://github.com/mono/mono).
