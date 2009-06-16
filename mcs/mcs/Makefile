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
	*mcs.exe.config

ifeq (1.0, $(FRAMEWORK_VERSION))
COMPILER_NAME = mcs
endif

ifeq (2.0, $(FRAMEWORK_VERSION))
LOCAL_MCS_FLAGS += -d:GMCS_SOURCE
COMPILER_NAME = gmcs
endif

ifeq (2.1, $(FRAMEWORK_VERSION))
LOCAL_MCS_FLAGS += -d:GMCS_SOURCE -d:SMCS_SOURCE
COMPILER_NAME = smcs
endif

ifeq (4.0, $(FRAMEWORK_VERSION))
LOCAL_MCS_FLAGS += -d:GMCS_SOURCE
COMPILER_NAME = dmcs
endif

PROGRAM = $(topdir)/class/lib/$(PROFILE)/$(COMPILER_NAME).exe

BUILT_SOURCES = cs-parser.cs

PROGRAM_COMPILE = $(BOOT_COMPILE)

CLEAN_FILES += y.output

%-parser.cs: %-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -cvt < $(topdir)/jay/skeleton.cs $< > jay-tmp.out && mv jay-tmp.out $@


ifeq (net_2_0_bootstrap, $(PROFILE))

all-local $(STD_TARGETS:=-local):
	@:

all-local: $(PROGRAM) $(PROGRAM).config
clean-local: clean-program
clean-program:
	rm -f $(PROGRAM) $(PROGRAM).config

$(PROGRAM) $(PROGRAM).config: $(dir $(PROGRAM))/.stamp

$(PROGRAM): $(topdir)/class/lib/$(BOOTSTRAP_PROFILE)/mcs.exe
	cp $< $@

$(PROGRAM).config: gmcs.exe.config
	cp $< $@

else

ifeq (net_2_0, $(PROFILE))

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/net_2_0_bootstrap/gmcs.exe $(DEBUG_FLAGS)

clean-local: clean-net_2_0_bootstrap
clean-net_2_0_bootstrap:
	cd .. && $(MAKE) NO_DIR_CHECK=yes PROFILE=net_2_0_bootstrap clean

bootstrap_libs = mscorlib.dll System.dll System.Xml.dll Mono.CompilerServices.SymbolWriter.dll
bootstrap_libfiles = $(bootstrap_libs:%=$(topdir)/class/lib/net_2_0_bootstrap/%)

$(bootstrap_libfiles): bootstrap-libs
	@:
.PHONY: bootstrap-libs
bootstrap-libs:
	cd .. && $(MAKE) NO_DIR_CHECK=yes PROFILE=net_2_0_bootstrap all

$(PROGRAM): $(bootstrap_libfiles)
endif

KEEP_OUTPUT_FILE_COPY = yes

include ../build/executable.make
endif

csproj-local:
	config_file=`basename $(PROGRAM) .exe`-$(PROFILE).input; \
	echo $(thisdir):$$config_file >> $(topdir)/../mono/msvc/scripts/order; \
	(echo $(is_boot); \
	echo $(BOOTSTRAP_MCS);	\
	echo $(USE_MCS_FLAGS) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS); \
	echo $(PROGRAM); \
	echo $(BUILT_SOURCES); \
	echo $(PROGRAM); \
	echo $(response)) > $(topdir)/../mono/msvc/scripts/inputs/$$config_file

#
# Below this line we have local targets used for testing and development
#

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

do-gettext:
	xgettext --keyword='Report.Error:3' --keyword='Report.Error:2' --keyword='Report.Warning:3' --keyword='Report.Warning:2' -o mcs.po --language='C#' `cat gmcs.exe.sources | grep -v /`

profile : $(PROGRAM)
	$(RUNTIME) $(RUNTIME_FLAGS) --profile=$(PROFILER) $(PROGRAM) $(USE_MCS_FLAGS) -target:exe -out:mcs2.exe $(BUILT_SOURCES) @$(response)

#
# quick hack target, to quickly develop the gmcs compiler
# Update manually.
qh:
	MONO_PATH="../class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) ../class/lib/net_2_0/gmcs.exe  /codepage:65001 -d:GMCS_SOURCE  -d:NET_1_1 -d:NET_2_0 -debug -target:exe -out:gmcs.exe cs-parser.cs  @gmcs.exe.sources
	@ cp $(COMPILER_NAME).exe* $(topdir)/class/lib/$(PROFILE)/

pa: cs-parser.cs
	MONO_PATH="../class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) ../class/lib/net_2_0/gmcs.exe  /codepage:65001 -d:GMCS_SOURCE  -d:NET_1_1 -d:NET_2_0 -debug -target:exe -out:foo.exe cs-parser.cs foo.cs -main:X  @gmcs.exe.sources

q: cs-parser.cs qh
	echo 'System.Console.WriteLine ("Hello");' | mono csharp.exe
	echo -e 'using System;\nConsole.WriteLine ("hello");' | mono csharp.exe
	echo -e '"foo" == "bar";' | mono csharp.exe
	echo -e 'var a = 1;\na + 2;' | mono csharp.exe
	echo -e 'int j;\nj = 1;' | mono csharp.exe
	echo -e 'var a = new int[]{1,2,3};\nfrom x in a select x;' | mono csharp.exe
	echo -e 'var a = from f in System.IO.Directory.GetFiles ("/tmp") where f == "passwd" select f;' | mono csharp.exe


