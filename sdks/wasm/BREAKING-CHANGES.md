# Breaking Changes

View [CHANGELOG.md](CHANGELOG.md) for more detail. This file is only a high level overview of breaking changes.

## 2019-07-23

[#15802](https://github.com/mono/mono/pull/15802) - Since `runtime.js` now takes care of loading the correct runtime modules referencing `<script defer src="mono.js"></script>` from the html will now throw error.**

- **[sdk] Add new property `EnableMonoWasmThreads`**
   - Determines whether to enable support for threads or not.
      - "None" - Do not enable runtime for threads. - (Default)
      - "ThreadsOnly" - Only enable runtime for threads support.
      - "All" - Enable runtimes for both thread and non thread support.

   - `runtime.js` file now supports loading the correct `mono.js`.  If threads are supported it will load the threads `mono.js` module instead of the non threads runtime modules.
      - If both runtimes are included then a new directory `threads` will be created to load the threads runtime.


## 2019-07-01

- [#15504](https://github.com/mono/mono/pull/15504)
- [#15389](https://github.com/mono/mono/pull/15389)

> Recent changes have occurred to the directory structure for `Available WebAssembly Runtime Builds` (see diagrams in README).  All builds can now be found in one directory `sdk/wasm/builds`.  This was necessary to start supporting more options and functionality.
