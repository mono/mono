* install `brew install llvm`
* build mono
* run `Mono.Compiler` tests:
```
DYLD_FALLBACK_LIBRARY_PATH=/usr/local/opt/llvm/lib make run-test -C mcs/class/Mono.Compiler V=1
```
