# Introduction

This is a sample for generating coverage profile data.

* Link the profiler into the app by using the --profile= argument of packager.exe
* Start the profiler by calling
```
mono_wasm_init_coverage_profiler (null);
```
from the wasm module.
* In the app, call ```Runtime:StopProfile``` to stop profiling.
* The profiler will call ```Runtime.DumpCoverageProfileData``` with the data.
* The Dump () function stores the data into the ```Module.coverage_profile_data``` JS property.
* The puppeteer script in ```gen-profile.js``` starts a web server, visits the page, reads
the data out of the JS property, and saves it to disk.
