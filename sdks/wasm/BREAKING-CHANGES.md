# Breaking Changes

View [CHANGELOG.md](CHANGELOG.md) for more detail. This file is only a high level overview of breaking changes.

## 2019-07-23

[#15802](https://github.com/mono/mono/pull/15802) - Since `runtime.js` now takes care of loading the correct runtime modules, when `<EnableAutoRuntimeLoading>` is `True`, referencing `<script defer src="mono.js"></script>` from the html will now throw an error.**

- **[sdk] Add new property `IncludeMonoWasmThreads`**
   - Determines whether to include support for threads or not.
      - "None" - Do not include runtime for threads. - (Default)
      - "ThreadsOnly" - Only include runtime for threads support.
      - "All" - Include runtimes for both thread and non thread support.

- **[sdk] Add new property `EnableAutoRuntimeLoading`**
   - Whether to enable support for automatically loading the correct wasm (threaded or non-threaded) runtime.
      - "True" - The `runtime.js` will attempt to load the correct runtime. 
      - "False" - The `runtime.js` will NOT attempt to load the correct runtime. 

   - `runtime.js` file now supports loading the correct `mono.js` when `<EnableAutoRuntimeLoading>` is `True`.  If threads are supported it will load the threads `mono.js` module instead of the non threads runtime modules.
      - If both runtimes are included then a new directory `threads` will be created to load the threads runtime.

- To load the correct runtime if not using the `EnableAutoRuntimeLoading` the following can be used as an example.

```
    <script>
        var firstScript = document.getElementsByTagName('script')[0],
            js = document.createElement('script');
        js.src = "mono.js";
        if (typeof SharedArrayBuffer !== 'undefined')
            js.src = "threads/mono.js";
        firstScript.parentNode.insertBefore(js, firstScript);
    </script>

```

## 2019-07-01

- [#15504](https://github.com/mono/mono/pull/15504)
- [#15389](https://github.com/mono/mono/pull/15389)

> Recent changes have occurred to the directory structure for `Available WebAssembly Runtime Builds` (see diagrams in README).  All builds can now be found in one directory `sdk/wasm/builds`.  This was necessary to start supporting more options and functionality.
