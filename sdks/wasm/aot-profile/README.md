# Introduction

This is a sample for generating AOT profile data.

* Link the profiler into the app by using the --profile= argument of packager.exe
* Start the profiler by calling
```
mono_wasm_load_profiler_aot ("aot:write-at-method=HelloWorld:StopProfile,send-to-method=HelloWorld:Dump");
```
from the wasm module.
* In the app, call ```HelloWorld:StopProfile``` to stop profiling.
* The aot profiler will call ```HelloWorld:Dump``` with the data.
* The Dump () function stores the data into the ```AotProfileData.data``` JS property.
* The puppeteer script in ```gen-profile.js``` starts a web server, visits the page, reads
the data out of the JS property, and saves it to disk.
