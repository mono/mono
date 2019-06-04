.PHONY: all linux osx clean

all:

linux:
	gcc -fPIC -shared -o libmono-profiler-sample.so sample.c `pkg-config --cflags mono-2`

osx:
	clang -undefined suppress -flat_namespace -o mono-profiler-sample.dylib sample.c `pkg-config --cflags mono-2`

clean:
	$(RM) -f libmono-profiler-sample.so
	$(RM) -f libmono-profiler-sample.dylib
