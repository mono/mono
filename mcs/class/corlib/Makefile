thisdir = class/corlib
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/corlib

LIBRARY = corlib.dll
LIBRARY_NAME = mscorlib.dll
LIB_MCS_FLAGS = $(corlib_flags) $(RESOURCE_FILES:%=-resource:%)
LIBRARY_USE_INTERMEDIATE_FILE = yes

LIBRARY_COMPILE = $(BOOT_COMPILE)
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

AOT_IN_PROFILES = net_2_0 net_4_0

RESOURCE_FILES = \
	resources/collation.core.bin \
	resources/collation.tailoring.bin \
	resources/collation.cjkCHS.bin \
	resources/collation.cjkCHT.bin \
	resources/collation.cjkJA.bin \
	resources/collation.cjkKO.bin \
	resources/collation.cjkKOlv2.bin

corlib_flags = -unsafe -nostdlib
LOCAL_MCS_FLAGS = -nowarn:612,618 -d:INSIDE_CORLIB -d:LIBC

# System.IO/DirectoryInfoTest.cs needs Mono.Posix
TEST_MCS_FLAGS = -debug+ -debug:full -nowarn:168,219,618,672 -unsafe -r:$(topdir)/class/lib/$(PROFILE)/Mono.Posix.dll -define:MONO_DATACONVERTER_STATIC_METHODS

EXTRA_DISTFILES = \
	Test/ms_run_test.sh			\
	Test/resources/MyResources.resources	\
	Test/resources/MyResources.de.resources	\
	Test/resources/Empty.resources		\
	Test/resources/AFile.txt		\
	Test/resources/StreamTest.resources	\
	Test/resources/StreamTest.ja.resources	\
	Test/resources/bug81759.resources	\
	Test/resources/415628.bin		\
	Test/resources/Fergie.GED		\
	$(RESOURCE_FILES)

include $(topdir)/build/library.make

ifdef FIXME_CORLIB_CMP
# corlib_cmp
cmplib = $(topdir)/class/lib/$(PROFILE)/corlib_cmp.dll
cmppdb = $(cmplib:.dll=.pdb)
cmp_response = $(depsdir)/$(PROFILE)_corlib_cmp.dll.response
cmp_makefrag = $(depsdir)/$(PROFILE)_corlib_cmp.dll.makefrag
cmp_flags = -r:$(PLATFORM_CORLIB) $(corlib_flags)

EXTRA_DISTFILES += corlib_cmp.dll.excludes
CLEAN_FILES += $(cmplib) $(cmp_response) $(cmp_makefrag) $(cmppdb)

$(cmplib): $(cmp_makefrag) $(cmp_response)
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(cmp_flags) -target:library -out:$@ @$(cmp_response)

$(cmp_response): $(sourcefile) corlib_cmp.dll.excludes
	@echo Creating $@ ...
	@sort $(sourcefile) corlib_cmp.dll.excludes | uniq -u | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@

$(cmp_makefrag): $(cmp_response)
	@echo Creating $@ ...
	@sed 's,^,$(cmplib): ,' $< >$@

# Since we make corlib_cmp on demand, there isn't a real need
# to have full dep tracking for it. Also, the generation of this
# file is busted on Win32 ('sort' seems to mess up line endings),
# leading to a broken build.
#
# -include $(cmp_makefrag)

$(cmp_response) $(cmp_makefrag): Makefile $(depsdir)/.stamp
endif
