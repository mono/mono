#
# This makefile needs work, currently we overwrite the value of `bmcs.exe',
# the reason is that we need this to be compiled with gmcs, so it uses the
# .NET 2.0 profile.
#
thisdir := bmcs
SUBDIRS := 
include ../build/rules.make

#uncomment to enable some debug stuff
#DEBUG_FLAGS=/define:MCS_DEBUG

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/net_2_0_bootstrap/mcs.exe $(DEBUG_FLAGS)

BUILT_SOURCES = mb-parser.cs

EXTRA_DISTFILES = mb-parser.jay

PROGRAM_COMPILE = $(BOOT_COMPILE)

CLEAN_FILES = y.output

PROGRAM = bmcs.exe

mb-parser.cs: mb-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $< > jay-tmp.out && mv jay-tmp.out $@

include ../build/executable.make

bmcs.exe: mb-parser.cs
	gmcs    -d:NET_1_1 -d:ONLY_1_1 -debug /target:exe /out:bmcs.exe mb-parser.cs  @bmcs.exe.sources -nowarn:219 -nowarn:162

install-as-mbas: all
	cp ../class/lib/default/bmcs.exe $(prefix)/lib/mono/1.0/mbas.exe

winstall: all
	cp ../class/lib/default/bmcs.exe  /cygdrive/c/WINDOWS/Microsoft.NET/Framework/v1.1.4322/

winstall-as-mbas: all
	cp ../class/lib/default/bmcs.exe  /cygdrive/c/WINDOWS/Microsoft.NET/Framework/v1.1.4322/
	cp ../class/lib/default/bmcs.exe  /cygdrive/c/WINDOWS/Microsoft.NET/Framework/v1.1.4322/mbas.exe
