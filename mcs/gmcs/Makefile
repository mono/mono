thisdir := gmcs
SUBDIRS := 
include ../build/rules.make

#uncomment to enable some debug stuff
#DEBUG_FLAGS=/define:MCS_DEBUG

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/net_2_0_bootstrap/mcs.exe $(DEBUG_FLAGS)

BUILT_SOURCES = cs-parser.cs

EXTRA_DISTFILES = cs-parser.jay

PROGRAM = gmcs.exe
PROGRAM_COMPILE = $(BOOT_COMPILE)

CLEAN_FILES = y.output

cs-parser.cs: cs-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $< > jay-tmp.out && mv jay-tmp.out $@

include ../build/executable.make

clean-local: clean-net_2_0_bootstrap
clean-net_2_0_bootstrap:
	cd .. && $(MAKE) PROFILE=net_2_0_bootstrap clean

bootstrap_libs = mscorlib.dll System.dll System.Xml.dll Mono.CompilerServices.SymbolWriter.dll
bootstrap_libfiles = $(bootstrap_libs:%=$(topdir)/class/lib/net_2_0_bootstrap/%)

$(bootstrap_libfiles): bootstrap-libs
	@:
.PHONY: bootstrap-libs
bootstrap-libs:
	cd .. && $(MAKE) PROFILE=net_2_0_bootstrap all

gmcs.exe: $(bootstrap_libfiles)

gmcs2.exe: $(PROGRAM)
	MONO_PATH="../class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" time $(PROGRAM) -2 /target:exe /debug /out:$@ $^ $(DEBUG_FLAGS) 

gmcs3.exe: gmcs2.exe
	MONO_PATH="../class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" time $(RUNTIME) ./gmcs2.exe $(USE_MCS_FLAGS) $(DEBUG_FLAGS) -2 /target:exe /debug /out:$@ $^

gmcs4.exe: gmcs3.exe
	MONO_PATH="../class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" time $(RUNTIME) ./gmcs3.exe $(USE_MCS_FLAGS) $(DEBUG_FLAGS) -2 /target:exe /debug /out:$@ $^


# Testing targets

TIME = 

# This used to be called test, but that conflicts with the global
# recursive target.

btest: gmcs2.exe gmcs3.exe gmcs4.exe
	ls -l gmcs3.exe gmcs4.exe

wc:
	wc -l $(all_sources)

ctest: 
	rm -f gmcs3.exe mcs4.exe
	make btest USE_MCS_FLAGS= 

# we need this because bash tries to use its own crappy timer
FRIENDLY_TIME = $(shell which time) -f'%U seconds'

do-time : mcs.exe
	@ echo -n "Run 1:   "
	 $(FRIENDLY_TIME) $(RUNTIME) ./mcs.exe $(USE_MCS_FLAGS) /target:exe /out:mcs2.exe $(all_sources) > /dev/null || (echo FAILED; exit 1)
	@ echo -n "Run 2:   "
	@ $(FRIENDLY_TIME) $(RUNTIME) ./mcs2.exe $(USE_MCS_FLAGS) /target:exe /out:mcs3.exe $(all_sources) > /dev/null || (echo FAILED; exit 1)
	@ echo -n "corlib:  "
	@ rm -f ../class/lib/mscorlib.dll
	@ (cd ../class/corlib ; make BOOTSTRAP_MCS="$(FRIENDLY_TIME) mono ../../mcs/mcs.exe" > /dev/null ) || (echo FAILED; exit 1)

do-corlib:
	@ echo -n "corlib:  "
	@ rm -f ../class/lib/mscorlib.dll
	@ (cd ../class/corlib ; make BOOTSTRAP_MCS="$(FRIENDLY_TIME) mono ../../mcs/mcs.exe" > /dev/null ) || (echo FAILED; exit 1)

PROFILER=default

profile : gmcs2.exe
	-rm -f gmcs3.exe
	make gmcs3.exe RUNTIME="$(RUNTIME) --profile=$(PROFILER)"

