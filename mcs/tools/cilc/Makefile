thisdir = tools/cilc
SUBDIRS = 
include ../../build/rules.make

PROGRAM = cilc.exe

# fixme: make clean doesn't know to rm -rf generated.
# and we can't extend the make clean target.

CLEAN_FILES = generated Test.dll Test.pdb

EXTRA_DISTFILES = demo.c README TODO Test.cs

include ../../build/executable.make

test-local: Test.dll

run-test-local: run-cilc-test

run-cilc-test: cilc.exe Test.dll
	rm -rf generated
	mkdir generated
	$(RUNTIME) cilc.exe Test.dll generated
	$(MAKE) -C generated
	$(CCOMPILE) -Wall `pkg-config --cflags --libs glib-2.0 mono` -Igenerated -Lgenerated -ldemo demo.c -o generated/demo
	cp Test.dll generated

Test.dll: Test.cs
	$(CSCOMPILE) /target:library /out:$@ $^
