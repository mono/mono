thisdir = tools/csharplib
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -d:GMCS_SOURCE -d:NET_1_1 -d:NET_2_0 -r:System -r:System.Xml

csharplib.dll.sources: ../../mcs/gmcs.exe.sources Makefile
	cat ../../mcs/gmcs.exe.sources  | sed -e 's,^,../../mcs/,' -e 's,mcs/../,,' > csharplib.dll.sources
	echo "../../mcs/cs-parser.cs" >> csharplib.dll.sources
	echo sign.cs >> csharplib.dll.sources

LIBRARY_NEEDS_POSTPROCESSING = yes

LIBRARY = csharplib.dll

include ../../build/library.make

MONO_PATH=MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

$(the_lib): $(the_libdir)/.stamp Makefile csharplib.dll.sources $(build_lib)
	$(MKINSTALLDIRS) fixup
	$(MONO_PATH):../tuner $(RUNTIME) ../linker/monolinker.exe -d fixup/$(PROFILE) -o out -l none -c skip -b true -x link.xml -s Mono.Tuner.AdjustVisibility,Mono.Tuner:OutputStep -s Mono.Tuner.PrintStatus,Mono.Tuner:OutputStep  -i compiler.info
	$(SN) $(SNFLAGS) out/csharplib.dll $(topdir)/class/mono.snk
	cp out/csharplib.dll $(the_lib)
