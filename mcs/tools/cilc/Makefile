thisdir = tools/cilc
SUBDIRS = 
include ../../build/rules.make

PROGRAM = cilc.exe

# fixme: make clean doesn't know to rm -rf generated.
# and we can't extend the make clean target.

CLEAN_FILES = generated Demo.dll Demo.pdb

OTHER_RES = res-Makefile

RESOURCE_FILES = $(OTHER_RES)

EXTRA_DISTFILES = demo.c README TODO Test.cs $(RESOURCE_FILES)

include ../../build/executable.make

LOCAL_MCS_FLAGS=-r:Mono.Posix.dll $(OTHER_RES:%=-resource:%)

cilc.exe: $(RESOURCE_FILES)

test-local: Demo.dll

run-test-local: run-cilc-test

run-cilc-test: cilc.exe Demo.dll
	rm -rf generated
	$(RUNTIME) cilc.exe Demo.dll generated
	$(MAKE) -C generated
	$(CCOMPILE) -Wall `pkg-config --cflags --libs glib-2.0 mono` -Igenerated -Lgenerated -ldemo demo.c -o generated/demo

Demo.dll: Test.cs
	$(CSCOMPILE) /target:library /out:$@ $^
