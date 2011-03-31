thisdir = class/corlib
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/corlib

LIBRARY = corlib.dll
LIBRARY_NAME = mscorlib.dll
LIB_MCS_FLAGS = $(RESOURCE_FILES:%=-resource:%)
LIBRARY_USE_INTERMEDIATE_FILE = yes

ifeq (basic, $(PROFILE))

else

ifeq (2, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += --runtime:v2
else
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += --runtime:v4
else
$(error Unknown framework version)
endif
endif

endif

LIBRARY_COMPILE = $(BOOT_COMPILE)
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

RESOURCE_FILES = \
	resources/collation.core.bin \
	resources/collation.tailoring.bin \
	resources/collation.cjkCHS.bin \
	resources/collation.cjkCHT.bin \
	resources/collation.cjkJA.bin \
	resources/collation.cjkKO.bin \
	resources/collation.cjkKOlv2.bin

LOCAL_MCS_FLAGS = -unsafe -nostdlib -nowarn:612,618 -d:INSIDE_CORLIB -d:LIBC
DEFAULT_REFERENCES =

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
corlib_flags = -unsafe -nostdlib
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

vts = VersionTolerantSerialization
vtsdir = Test/System.Runtime.Serialization.Formatters.Binary/$(vts)
vtslibs = \
	$(vtsdir)/$(vts)TestLib/1.0/Address.dll \
	$(vtsdir)/$(vts)TestLib/2.0/Address.dll \
	$(vtsdir)/$(vts)TestLib/3.0/Address.dll \
	$(vtsdir)/$(vts)TestLib/4.0/Address.dll \
	$(vtsdir)/$(vts)TestLib/5.0/Address.dll \
	$(vtsdir)/$(vts)TestLib/6.0/Address.dll

test-vts: $(vtslibs) $(vtsdir)/BinarySerializationOverVersions.exe

$(vtsdir)/$(vts)TestLib/%/Address.dll: $(vtsdir)/$(vts)TestLib/%/Address.cs
	$(Q_MCS) $(MCS) -target:library -warn:0  $^

$(vtsdir)/BinarySerializationOverVersions.exe: $(vtsdir)/BinarySerializationOverVersions.cs $(vtsdir)/$(vts)TestLib/1.0/Address.dll
	$(Q_MCS) $(MCS) $(test_nunit_ref) -warn:0  \
		-r:$(vtsdir)/$(vts)TestLib/1.0/Address.dll \
		$(vtsdir)/BinarySerializationOverVersions.cs
	@cp $(vtsdir)/$(vts)TestLib/1.0/Address.dll $(vtsdir)

run-test-vts: test-vts
	@echo Running vts tests...
	$(TEST_RUNTIME) $(RUNTIME_FLAGS) $(TEST_HARNESS) -noshadow \
		$(vtsdir)/BinarySerializationOverVersions.exe 
test: test-vts
run-test: run-test-vts

EXTRA_DISTFILES += \
	$(vtsdir)/$(vts)TestLib/1.0/Address.cs \
	$(vtsdir)/$(vts)TestLib/2.0/Address.cs \
	$(vtsdir)/$(vts)TestLib/3.0/Address.cs \
	$(vtsdir)/$(vts)TestLib/4.0/Address.cs \
	$(vtsdir)/$(vts)TestLib/5.0/Address.cs \
	$(vtsdir)/BinarySerializationOverVersions.cs
