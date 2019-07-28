# Change Log

All notable changes to this project will be documented in this file.

## 2019-07-23

### Features

* [[wasm][sdk] Threading support for the SDK #15802](https://github.com/mono/mono/pull/15802)
  * Add `mono_wasm_has_threading_support` function that can be called from JavaScript code.
      - Tests whether the currently executing code was compiled with pthreads support enabled. If this function returns true, then the currently executing code was compiled with -s USE_PTHREADS=1 (and the current browser supports multithreading).

      example:

      ```
	   console.log("threading support: " + MONO.mono_wasm_has_threading_support());
      ```

   * [sdk] Add new property `EnableMonoWasmThreads`
      - Determines whether to enable support for threads or not.
         - "None" - Do not enable runtime for threads. - (Default)
         - "ThreadsOnly" - Only enable runtime for threads support.
         - "All" - Enable runtimes for both thread and non thread support.
      - `runtime.js` file now supports loading the correct `mono.js`.  If threads are supported it will load the threads `mono.js` module instead of the non threads runtime modules.
      - If both runtimes are included then a new directory `threads` will be created to load the threads runtime.
      - See [BREAKiNG-CHANGES.md](BREAKING-CHANGES.md)

   * Modify the template to add `<EnableMonoWasmThreads>None</EnableMonoWasmThreads>` by default.



## 2019-07-01

### Features

* [[wasm] Threading support. #15389](https://github.com/mono/mono/pull/15389)
