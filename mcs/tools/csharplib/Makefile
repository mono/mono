thisdir = tools/csharplib
SUBDIRS = 
include ../../build/rules.make

LINKER = $(topdir)/class/lib/$(PROFILE)/monolinker.exe

LOCAL_MCS_FLAGS = -d:GMCS_SOURCE -r:System -r:System.Xml

ifeq (net_4_0, $(PROFILE))

LOCAL_MCS_FLAGS += -r:System.Core.dll

endif

Mono.CSharp.dll.sources: ../../mcs/gmcs.exe.sources Makefile
	cat ../../mcs/gmcs.exe.sources | grep -v AssemblyInfo.cs  | sed -e 's,^,../../mcs/,' -e 's,mcs/../,,' > Mono.CSharp.dll.sources
	echo "../../mcs/cs-parser.cs" >> Mono.CSharp.dll.sources
	echo sign.cs >> Mono.CSharp.dll.sources

LIBRARY_NEEDS_POSTPROCESSING = yes

LIBRARY = Mono.CSharp.dll

docs = \
	en/ns-Mono.CSharp.xml				\
	en/index.xml					\
	en/Mono.CSharp/Evaluator.xml			\
	en/Mono.CSharp/Evaluator+NoValueSet.xml		\
	en/Mono.CSharp/InteractiveBase.xml		\
	en/Mono.CSharp/Report.xml			\
	en/Mono.CSharp/CompiledMethod.xml		\
	en/Mono.CSharp/InteractiveBase+Simple.xml

EXTRA_DISTFILES = compiler.info link.xml $(docs)

include ../../build/library.make

MONO_PATH=MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_1_1$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

$(the_lib): $(the_libdir)/.stamp Makefile Mono.CSharp.dll.sources $(build_lib)
	$(MKINSTALLDIRS) fixup
	$(MONO_PATH) $(RUNTIME) $(LINKER) -d fixup/$(PROFILE) -o out -l none -c skip -b true -x link.xml -s Mono.Tuner.AdjustVisibility,Mono.Tuner:OutputStep -s Mono.Tuner.PrintStatus,Mono.Tuner:OutputStep  -i compiler.info
	$(SN) $(SNFLAGS) -R out/Mono.CSharp.dll $(topdir)/class/mono.snk
	cp out/Mono.CSharp.dll $(the_lib)
