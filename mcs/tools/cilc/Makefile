RUNTIME=mono
CSC = $(RUNTIME) ../../mcs/mcs.exe
CSCFLAGS=/nologo /debug+ /debug:full /out:$@

all: cilc.exe

test: cilc.exe Test.dll
	rm -rf generated
	mkdir generated
	mono cilc.exe Test.dll generated
	$(MAKE) -C generated
	gcc -Wall `pkg-config --cflags --libs glib-2.0 mono` -Igenerated -Lgenerated -ldemo demo.c -o generated/demo
	cp Test.dll generated

cilc.exe: cilc.cs
	$(CSC) $(CSCFLAGS) cilc.cs

Test.dll: Test.cs
	mcs /out:Test.dll --target library Test.cs

clean:
	rm -rf generated cilc.exe Test.dll
