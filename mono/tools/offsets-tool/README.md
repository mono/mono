This is a python version of the offsets tool in tools/offsets-tool.
Unlike the c# version which depends on mono and CppSharp, this depends on python and libclang. Currently, it only runs on osx.

It depends on python and libclang. The 'clang' directory contains the python bindings to libclang, they are taken from:
https://github.com/llvm/llvm-project/commits/master/clang/bindings/python/clang
rev 8a12e40185cd0ce7031e6abab4af12e6fc923110
