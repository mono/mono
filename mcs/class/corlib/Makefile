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
TEST_MCS_FLAGS = -debug -nowarn:168,219,618,672 -unsafe -r:Mono.Posix.dll -r:System.Core.dll \
				 -define:MONO_DATACONVERTER_STATIC_METHODS $(TEST_RESX_RESOURCES:%=-resource:%)

EXTRA_DISTFILES = \
	Test/resources/MyResources.resources	\
	Test/resources/MyResources.de.resources	\
	Test/resources/Empty.resources		\
	Test/resources/AFile.txt		\
	Test/resources/StreamTest.resources	\
	Test/resources/StreamTest.ja.resources	\
	Test/resources/bug81759.resources	\
	Test/resources/415628.bin		\
	Test/resources/Fergie.GED		\
	Test/resources/culture-*.cs		\
	$(RESOURCE_FILES) \
	$(TEST_RESOURCES:.resources=.resx)

TEST_RESX_RESOURCES = \
	Test/resources/Resources.resources

TEST_RESX_RESOURCES_SATELITE = \
	Test/resources/Resources.es-ES.resources \
	Test/resources/Resources.nn-NO.resources \

CLEAN_FILES += $(TEST_RESX_RESOURCES)

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

$(TEST_RESX_RESOURCES) $(TEST_RESX_RESOURCES_SATELITE): %.resources: %.resx
	$(RESGEN) $< || cp $@.prebuilt $@

TEST_RESOURCES = $(TEST_RESX_RESOURCES) $(TEST_RESX_RESOURCES_SATELITE)

$(test_lib): $(TEST_RESOURCES) satellite-assemblies

satellite-assemblies:
	@mkdir -p es-ES
	$(CSCOMPILE) -target:library -r:$(corlib) Test/resources/culture-es-ES.cs -resource:Test/resources/Resources.es-ES.resources -out:es-ES/$(patsubst %.dll,%.Resources.dll,$(test_lib))
	@mkdir -p nn-NO
	$(CSCOMPILE) -target:library -r:$(corlib) Test/resources/culture-nn-NO.cs -resource:Test/resources/Resources.nn-NO.resources -out:nn-NO/$(patsubst %.dll,%.Resources.dll,$(test_lib))

vtsdir = Test/System.Runtime.Serialization.Formatters.Binary/VersionTolerantSerialization
vtslibs = \
	$(vtsdir)/$(PROFILE)_TestLib/1.0/Address.dll \
	$(vtsdir)/$(PROFILE)_TestLib/2.0/Address.dll \
	$(vtsdir)/$(PROFILE)_TestLib/3.0/Address.dll \
	$(vtsdir)/$(PROFILE)_TestLib/4.0/Address.dll \
	$(vtsdir)/$(PROFILE)_TestLib/5.0/Address.dll \
	$(vtsdir)/$(PROFILE)_TestLib/6.0/Address.dll

test-vts: $(vtslibs) $(vtsdir)/$(PROFILE)_TestLib/BinarySerializationOverVersions.exe

$(vtsdir)/$(PROFILE)_TestLib/%/Address.dll: $(vtsdir)/VersionTolerantSerializationTestLib/%/Address.cs
	@mkdir -p $(dir $@)
	$(CSCOMPILE) -target:library -r:$(corlib) -warn:0 -out:$@ $^

$(vtsdir)/$(PROFILE)_TestLib/BinarySerializationOverVersions.exe: $(vtsdir)/BinarySerializationOverVersions.cs $(vtsdir)/$(PROFILE)_TestLib/1.0/Address.dll
	$(CSCOMPILE) $(test_nunit_ref) -warn:0 -r:$(corlib) \
		-r:$(vtsdir)/$(PROFILE)_TestLib/1.0/Address.dll \
		$(vtsdir)/BinarySerializationOverVersions.cs -out:$@
	@cp $(vtsdir)/$(PROFILE)_TestLib/1.0/Address.dll $(vtsdir)/$(PROFILE)_TestLib

# Need to define MONO_PATH to an absolute dir since the test is ran from a subdir
run-test-vts: test-vts
	@echo Running vts tests...
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" $(TEST_RUNTIME) $(RUNTIME_FLAGS) $(TEST_HARNESS) -noshadow \
		$(vtsdir)/$(PROFILE)_TestLib/BinarySerializationOverVersions.exe
test: test-vts
run-test: run-test-vts

EXTRA_DISTFILES += \
	$(vtsdir)/VersionTolerantSerializationTestLib/1.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/2.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/3.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/4.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/5.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/6.0/Address.cs \
	$(vtsdir)/BinarySerializationOverVersions.cs
