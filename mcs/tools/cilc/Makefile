thisdir = tools/cilc
SUBDIRS = 
include ../../build/rules.make

PROGRAM = cilc.exe

# fixme: make clean doesn't know to rm -rf generated.
# and we can't extend the make clean target.

CLEAN_FILES = generated Demo.dll Demo.pdb

EXTRA_DISTFILES = demo.c README TODO Test.cs

include ../../build/executable.make

test-local: Demo.dll

run-test-local: run-cilc-test

run-cilc-test: cilc.exe Demo.dll
	rm -rf generated
	$(RUNTIME) cilc.exe Demo.dll generated
	$(MAKE) -C generated
	$(CCOMPILE) -Wall `pkg-config --cflags --libs glib-2.0 mono` -Igenerated -Lgenerated -ldemo demo.c -o generated/demo

Demo.dll: Test.cs
	$(CSCOMPILE) /target:library /out:$@ $^
