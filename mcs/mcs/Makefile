thisdir := mcs
SUBDIRS := 
include ../build/rules.make

BUILT_SOURCES = cs-parser.cs

EXTRA_DISTFILES = \
	compiler.csproj		\
	compiler.doc		\
	compiler.sln		\
	cs-parser.jay		\
	NOTES			\
	TODO

PROGRAM = $(topdir)/class/lib/$(PROFILE)/mcs.exe
PROGRAM_COMPILE = $(BOOT_COMPILE)
PROGRAM_INSTALL_DIR = $(prefix)/lib/mono/1.0

CLEAN_FILES = y.output

cs-parser.cs: cs-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $< > jay-tmp.out && mv jay-tmp.out $@

include ../build/executable.make

# Testing targets

TIME = time

# This used to be called test, but that conflicts with the global
# recursive target.

btest: mcs2.exe mcs3.exe
	ls -l mcs2.exe mcs3.exe

mcs2.exe: $(PROGRAM)
	$(TIME) $(RUNTIME) $(PROGRAM) $(USE_MCS_FLAGS) /target:exe /out:$@ $(BUILT_SOURCES) @$(response)

mcs3.exe: mcs2.exe
	$(TIME) $(RUNTIME) ./mcs2.exe $(USE_MCS_FLAGS) /target:exe /out:$@ $(BUILT_SOURCES) @$(response)

wc:
	wc -l $(BUILT_SOURCES) `cat $(sourcefile)`

ctest: 
	rm -f mcs2.exe mcs3.exe
	$(MAKE) USE_MCS_FLAGS= btest

# we need this because bash tries to use its own crappy timer
FRIENDLY_TIME = $(shell which time) -f'%U seconds'

do-time : $(PROGRAM)
	@ echo -n "Run 1:   "
	@ rm -f mcs2.exe
	@ $(MAKE) TIME="$(FRIENDLY_TIME)" mcs2.exe > /dev/null || (echo FAILED; exit 1)
	@ echo -n "Run 2:   "
	@ rm -f mcs3.exe
	@ $(MAKE) TIME="$(FRIENDLY_TIME)" mcs3.exe > /dev/null || (echo FAILED; exit 1)
	@ $(MAKE) do-corlib

do-corlib:
	@ echo -n "corlib:  "
	@ rm -f ../class/lib/mscorlib.dll
	@ cd ../class/corlib ; $(MAKE) BOOTSTRAP_MCS='$(FRIENDLY_TIME) mono $$(topdir)/class/lib/$(PROFILE)/mcs.exe' > /dev/null || (echo FAILED; exit 1)

PROFILER=default

profile : $(PROGRAM)
	$(RUNTIME) --profile=$(PROFILER) $(PROGRAM) $(USE_MCS_FLAGS) /target:exe /out:mcs2.exe $(BUILT_SOURCES) @$(response)

