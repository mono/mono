
all: objectinspector.dll libmono-profiler-size.so sample.exe
	LD_LIBRARY_PATH=. mono --profile=size sample.exe

sample.exe: objectinspector.dll sample.cs
	mcs -r:objectinspector.dll sample.cs 

objectinspector.dll: objectinspector.cs
	mcs -target:library objectinspector.cs

libmono-profiler-size.so: size.c
	gcc -Wall -g -shared -o libmono-profiler-size.so size.c `pkg-config --cflags --libs mono`
