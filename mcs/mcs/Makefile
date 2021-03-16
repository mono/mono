# To produce a debugging parser, use the version that says "-cvt"
JAY_FLAGS=-c
# JAY_FLAGS=-cvt

thisdir := mcs
SUBDIRS := 
include ../build/rules.make

PROGRAM = mcs.exe

EXTRA_DISTFILES = \
	cs-parser.jay		\
	mcs.exe.sources

LIB_REFS = System.Core System.Xml System
LOCAL_MCS_FLAGS += -d:STATIC,NO_SYMBOL_WRITER,NO_AUTHENTICODE

ifndef NO_THREAD_ABORT
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_THREAD_ABORT
endif

ifndef NO_PROCESS_START
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_PROCESS_START
endif

LOCAL_MCS_FLAGS += $(REFERENCE_SOURCES_FLAGS)

BUILT_SOURCES = cs-parser.cs

CLEAN_FILES += y.output

%-parser.cs: %-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay $(JAY_FLAGS) -o jay-tmp.out $< < $(topdir)/jay/skeleton.cs && mv jay-tmp.out $@

KEEP_OUTPUT_FILE_COPY = yes

include ../build/executable.make

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
	$(TIME) $(RUNTIME) $(RUNTIME_FLAGS) $(PROGRAM) $(USE_MCS_FLAGS) $(MCS_REFERENCES) -target:exe -out:$@ $(BUILT_SOURCES) $(EXTRA_SOURCES) @$(response)

mcs3.exe: mcs2.exe
	$(TIME) $(RUNTIME) $(RUNTIME_FLAGS) ./mcs2.exe $(USE_MCS_FLAGS) $(MCS_REFERENCES) -target:exe -out:$@ $(BUILT_SOURCES) $(EXTRA_SOURCES) @$(response)

wc:
	wc -l $(BUILT_SOURCES) `cat $(sourcefile)`

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

debug-parser:
	rm cs-parser.cs
	$(MAKE) JAY_FLAGS=-cvt

#
# quick hack target, to quickly develop the gmcs compiler
# Update manually.

q: cs-parser.cs qh
	echo 'System.Console.WriteLine ("Hello");' | mono csharp.exe
	echo -e 'using System;\nConsole.WriteLine ("hello");' | mono csharp.exe
	echo -e '"foo" == "bar";' | mono csharp.exe
	echo -e 'var a = 1;\na + 2;' | mono csharp.exe
	echo -e 'int j;\nj = 1;' | mono csharp.exe
	echo -e 'var a = new int[]{1,2,3};\nfrom x in a select x;' | mono csharp.exe
	echo -e 'var a = from f in System.IO.Directory.GetFiles ("/tmp") where f == "passwd" select f;' | mono csharp.exe


