all: cilc.exe

test: cilc.exe Test.dll
	rm -rf generated
	mkdir generated
	mono cilc.exe Test.dll
	gcc -Wall -fpic -shared `pkg-config --cflags --libs glib-2.0 mono` -lpthread generated/*.c -o generated/libtest.so
	gcc -Wall `pkg-config --cflags --libs glib-2.0 mono` -Igenerated -Lgenerated -ltest demo.c -o generated/demo
	cp Test.dll generated

cilc.exe: cilc.cs
	mcs /out:cilc.exe cilc.cs

Test.dll: Test.cs
	mcs /out:Test.dll --target library Test.cs

clean:
	rm -rf generated cilc.exe Test.dll
