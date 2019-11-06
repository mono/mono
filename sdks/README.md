This project provides build scripts and sample apps for Mono targeting its supported platforms. Supported are Android, iOS and WebAssembly.

# Build instructions

## Dependencies

- automake 1.16.1

  if you have already built before using a previous version of automake you may need to clean the repo.

  ```bash
  git clean -xffd
  ```

  The previous should be sufficient but if that does not work then try hard resetting

  ```bash
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

For Android, you need to copy `sdks/Make.config.sample` to `sdks/Make.config` and set

```
ENABLE_ANDROID = 1
```

The `make` targets are as follow:

```bash
# Android
make -C sdks/builds provision-android && make -C sdks/android accept-android-license
make -C sdks/builds provision-mxe
make -C sdks/builds archive-android NINJA= IGNORE_PROVISION_ANDROID=1 IGNORE_PROVISION_MXE=1

# iOS
make -C sdks/builds archive-ios NINJA=

# WebAssembly
make -C sdks/builds provision-wasm
make -C sdks/builds archive-wasm  NINJA=

# Mac
make -C sdks/builds archive-mac [upcoming]
```

# WebAssembly

Go to the `wasm` directory for building and testing WebAssembly.  For more information view the [WebAssembly readme](./wasm/README.md)


# Dependencies

| Project     | Dependencies        |
| ----------- | ------------------- |
| Android     | Android SDK and NDK |
| iOS         | Xcode               |
| WebAssembly |                     |

See `sdks/versions.mk` for specific version numbers, and `sdks/paths.mk` for where they should be installed. These dependencies will not be installed as part of the build process, and will be expected to be present; an error will be triggered if it's not the case. If you need an additional version, please do contact us or submit a pull-request against [mono/mono](https://github.com/mono/mono).
