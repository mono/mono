thisdir = tools/mkbundle
SUBDIRS = 
include ../../build/rules.make

PROGRAM = mkbundle.exe

OTHER_RES = template.c template_z.c template_main.c template_common.inc bundle-mono-api.inc

RESOURCE_FILES = $(OTHER_RES)

LOCAL_MCS_FLAGS= $(OTHER_RES:%=-resource:%)

LOCAL_MCS_FLAGS += -d:STATIC,NO_SYMBOL_WRITER,NO_AUTHENTICODE
LIB_REFS = System.Xml System System.Core System.IO.Compression.FileSystem

EXTRA_DISTFILES = $(RESOURCE_FILES)

include ../../build/executable.make

$(the_lib): $(RESOURCE_FILES)

test-simple: simple.exe
	mono --debug $(the_lib) --simple simple.exe -o foo && ./foo
	mono --debug $(the_lib) --cross default simple.exe -o foo && ./foo
	mono --debug $(the_lib) --sdk `dirname \`which mono\``/.. simple.exe -o foo && ./foo
	-rm DEMO.zip
	$(topdir)/../scripts/mono-package-runtime `dirname \`which mono\``/.. DEMO
	mkdir -p ~/.mono/targets/DEMO
	unzip -d ~/.mono/targets/DEMO DEMO.zip
	mono --debug $(the_lib) --cross DEMO simple.exe -o foo | grep "Assembly.*mscorlib.dll"
	./foo

simple.exe: Makefile
	echo 'class X { static void Main () { System.Console.WriteLine ("OK");}}' > simple.cs && mcs simple.cs
