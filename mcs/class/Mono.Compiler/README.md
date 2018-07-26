* install `brew install llvm`
* (ensure that the `data/config.in` mapping for `libLLVM` &mdash; which points at `/usr/local/opt/llvm/lib/libLLVM.dylib` &mdash; corresponds to the LLVM dylib that was installed by Homebrew)
* build mono
* run `Mono.Compiler` tests:
```
make run-test -C mcs/class/Mono.Compiler V=1
```
