thisdir = tools/csharplib
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -d:GMCS_SOURCE -d:NET_1_1 -d:NET_2_0 -r:System -r:System.Xml

Mono.CSharp.dll.sources: ../../mcs/gmcs.exe.sources Makefile
	cat ../../mcs/gmcs.exe.sources | grep -v AssemblyInfo.cs  | sed -e 's,^,../../mcs/,' -e 's,mcs/../,,' > Mono.CSharp.dll.sources
	echo "../../mcs/cs-parser.cs" >> Mono.CSharp.dll.sources
	echo sign.cs >> Mono.CSharp.dll.sources

LIBRARY_NEEDS_POSTPROCESSING = yes

LIBRARY = Mono.CSharp.dll

include ../../build/library.make

MONO_PATH=MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

$(the_lib): $(the_libdir)/.stamp Makefile Mono.CSharp.dll.sources $(build_lib)
	$(MKINSTALLDIRS) fixup
	$(MONO_PATH):../tuner $(RUNTIME) ../linker/monolinker.exe -d fixup/$(PROFILE) -o out -l none -c skip -b true -x link.xml -s Mono.Tuner.AdjustVisibility,Mono.Tuner:OutputStep -s Mono.Tuner.PrintStatus,Mono.Tuner:OutputStep  -i compiler.info
	$(SN) $(SNFLAGS) out/Mono.CSharp.dll $(topdir)/class/mono.snk
	cp out/Mono.CSharp.dll $(the_lib)
