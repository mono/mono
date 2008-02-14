thisdir := mcs
SUBDIRS := 
include ../build/rules.make

EXTRA_DISTFILES = \
	*mcs.csproj		\
	compiler.doc		\
	*mcs.sln		\
	*cs-parser.jay		\
	*.sources		\
	generic.cs		\
	NOTES			\
	TODO			\
	gmcs.exe.config

GMCS_PROFILE := $(filter net_2_0_bootstrap net_2_0 net_2_1, $(PROFILE))

ifdef GMCS_PROFILE
	LOCAL_MCS_FLAGS += -d:GMCS_SOURCE

	ifeq (net_2_1, $(PROFILE))
		INTERNAL_SMCS = $(INTERNAL_GMCS)
		LOCAL_MCS_FLAGS += -nowarn:618 -noconfig -r:mscorlib -r:System.dll
		PROGRAM = $(topdir)/class/lib/$(PROFILE)/smcs.exe
	else
		BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/net_2_0_bootstrap/gmcs.exe $(DEBUG_FLAGS)
		PROGRAM = $(topdir)/class/lib/$(PROFILE)/gmcs.exe
	endif	
else
	PROGRAM = $(topdir)/class/lib/$(PROFILE)/mcs.exe
endif

BUILT_SOURCES = cs-parser.cs

PROGRAM_COMPILE = $(BOOT_COMPILE)

CLEAN_FILES += y.output

%-parser.cs: %-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $< > jay-tmp.out && mv jay-tmp.out $@


ifeq (net_2_0_bootstrap, $(PROFILE))

all-local $(STD_TARGETS:=-local):
	@:

all-local: $(PROGRAM) $(PROGRAM).config
clean-local: clean-program
clean-program:
	rm -f $(PROGRAM) $(PROGRAM).config

progdir = $(dir $(PROGRAM))

$(progdir)/.stamp:
	$(MKINSTALLDIRS) $(@D)
	touch $@

$(PROGRAM) $(PROGRAM).config: $(progdir)/.stamp

$(PROGRAM): $(topdir)/class/lib/default/mcs.exe
	cp $< $@

$(PROGRAM).config: gmcs.exe.config
	cp $< $@

else

ifeq (net_2_0, $(PROFILE))

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

$(PROGRAM): $(bootstrap_libfiles)
endif

KEEP_OUTPUT_FILE_COPY = yes

include ../build/executable.make
endif

# Testing targets

TIME = time

# This used to be called test, but that conflicts with the global
# recursive target.

btest: mcs2.exe mcs3.exe
	ls -l mcs2.exe mcs3.exe

mcs2.exe: $(PROGRAM)
	$(TIME) $(RUNTIME) $(RUNTIME_FLAGS) $(PROGRAM) $(USE_MCS_FLAGS) -target:exe -out:$@ $(BUILT_SOURCES) @$(response)

mcs3.exe: mcs2.exe
	$(TIME) $(RUNTIME) $(RUNTIME_FLAGS) ./mcs2.exe $(USE_MCS_FLAGS) -target:exe -out:$@ $(BUILT_SOURCES) @$(response)

wc:
	wc -l $(BUILT_SOURCES) `cat $(sourcefile)`

ctest: 
	rm -f mcs2.exe mcs3.exe
	$(MAKE) USE_MCS_FLAGS="-d:NET_1_1 -d:ONLY_1_1" btest

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
	$(RUNTIME) $(RUNTIME_FLAGS) --profile=$(PROFILER) $(PROGRAM) $(USE_MCS_FLAGS) -target:exe -out:mcs2.exe $(BUILT_SOURCES) @$(response)

qh:
	MONO_PATH="../class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) ../class/lib/net_2_0_bootstrap/gmcs.exe  /codepage:65001 -d:GMCS_SOURCE  -d:NET_1_1 -d:NET_2_0 -debug -target:exe -out:gmcs.exe cs-parser.cs  @gmcs.exe.sources
	
