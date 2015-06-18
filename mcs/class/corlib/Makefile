thisdir = class/corlib
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/corlib

LIBRARY = corlib.dll
LIBRARY_NAME = mscorlib.dll

LIB_MCS_FLAGS = $(REFERENCE_SOURCES_FLAGS) $(RESOURCE_FILES:%=-resource:%)

#LIBRARY_USE_INTERMEDIATE_FILE = yes

ifeq (2, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += --runtime:v4
else
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += --runtime:v4
else
$(error Unknown framework version)
endif
endif

RESOURCE_STRINGS = ../../../external/referencesource/mscorlib/mscorlib.txt

LIBRARY_COMPILE = $(BOOT_COMPILE)
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

RESOURCE_FILES = \
	resources/charinfo.nlp \
	resources/collation.core.bin \
	resources/collation.tailoring.bin \
	resources/collation.cjkCHS.bin \
	resources/collation.cjkCHT.bin \
	resources/collation.cjkJA.bin \
	resources/collation.cjkKO.bin \
	resources/collation.cjkKOlv2.bin

REFERENCE_SOURCES_FLAGS = -d:FEATURE_PAL,GENERICS_WORK,FEATURE_LIST_PREDICATES,FEATURE_SERIALIZATION,FEATURE_ASCII,FEATURE_LATIN1,FEATURE_UTF7,FEATURE_UTF32,MONO_HYBRID_ENCODING_SUPPORT,FEATURE_ASYNC_IO,NEW_EXPERIMENTAL_ASYNC_IO,FEATURE_UTF32,FEATURE_EXCEPTIONDISPATCHINFO,FEATURE_CORRUPTING_EXCEPTIONS,FEATURE_EXCEPTION_NOTIFICATIONS,FEATURE_STRONGNAME_MIGRATION,FEATURE_USE_LCID,FEATURE_FUSION,FEATURE_CRYPTO,FEATURE_X509_SECURESTRINGS,FEATURE_SYNCHRONIZATIONCONTEXT,FEATURE_SYNCHRONIZATIONCONTEXT_WAIT

MOBILE_STATIC := $(filter mobile_static monotouch monotouch_runtime monotouch_watch, $(PROFILE))

ifndef MOBILE_STATIC
REFERENCE_SOURCES_FLAGS += -d:FEATURE_REMOTING,MONO_COM,FEATURE_COMINTEROP,FEATURE_ROLE_BASED_SECURITY
endif

LOCAL_MCS_FLAGS = -unsafe -nostdlib -nowarn:612,618 -d:INSIDE_CORLIB,MONO_CULTURE_DATA -d:LIBC $(REFERENCE_SOURCES_FLAGS)
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

satellite_assembly1 = es-ES/$(patsubst %.dll,%.resources.dll,$(test_lib))
satellite_assembly2 = nn-NO/$(patsubst %.dll,%.resources.dll,$(test_lib))

$(test_lib): $(TEST_RESOURCES) $(satellite_assembly1) $(satellite_assembly2) 

$(satellite_assembly1): Test/resources/culture-es-ES.cs Test/resources/Resources.es-ES.resources
	@mkdir -p es-ES
	$(CSCOMPILE) -target:library -r:$(corlib) Test/resources/culture-es-ES.cs -resource:Test/resources/Resources.es-ES.resources -out:$@

$(satellite_assembly2): Test/resources/culture-nn-NO.cs Test/resources/Resources.nn-NO.resources
	@mkdir -p nn-NO
	$(CSCOMPILE) -target:library -r:$(corlib) Test/resources/culture-nn-NO.cs -resource:Test/resources/Resources.nn-NO.resources -out:$@

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

$(vtsdir)/$(PROFILE)_TestLib/BinarySerializationOverVersions.exe: $(vtsdir)/BinarySerializationOverVersions.cs $(vtsdir)/$(PROFILE)_TestLib/1.0/Address.dll $(test_nunit_dep)
	$(CSCOMPILE) $(test_nunit_ref) -warn:0 -r:$(corlib) \
		-r:$(vtsdir)/$(PROFILE)_TestLib/1.0/Address.dll \
		$(vtsdir)/BinarySerializationOverVersions.cs -out:$@
	@cp $(vtsdir)/$(PROFILE)_TestLib/1.0/Address.dll $(vtsdir)/$(PROFILE)_TestLib

# Need to define TEST_MONO_PATH to an absolute dir since the test is ran from a subdir
TEST_MONO_PATH=$(PWD)/../lib/$(PROFILE)

run-test-vts: test-vts
	@echo Running vts tests...
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" $(TEST_RUNTIME) $(RUNTIME_FLAGS) $(TEST_HARNESS) -noshadow \
		$(vtsdir)/$(PROFILE)_TestLib/BinarySerializationOverVersions.exe
ifndef NO_TEST
test: test-vts
run-test: run-test-vts
endif

EXTRA_DISTFILES += \
	$(vtsdir)/VersionTolerantSerializationTestLib/1.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/2.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/3.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/4.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/5.0/Address.cs \
	$(vtsdir)/VersionTolerantSerializationTestLib/6.0/Address.cs \
	$(vtsdir)/BinarySerializationOverVersions.cs

#
# Android TimeZoneInfo testing....
#

TMP_FILE1=tzi1.tmp
TMP_FILE2=tzi2.tmp

# Remove some code that is protected so we don't have to include all the corlib files.
tzi.exe:System/AndroidPlatform.cs $(wildcard System/TimeZone*.cs) ../../build/common/Consts.cs ../../build/common/Locale.cs ../Mono.Options/Mono.Options/Options.cs
	trap "rm -f $(TMP_FILE1) $(TMP_FILE2)" EXIT INT QUIT TERM && \
	sed 's/Environment\.GetResourceString/string.Format/g' ../../../external/referencesource/mscorlib/system/timezoneinfo.cs > $(TMP_FILE1) && \
	sed 's/StringBuilder\.DefaultCapacity/100/g' ../../../external/referencesource/mscorlib/system/text/stringbuildercache.cs > $(TMP_FILE2) && \
	$(MCS) /debug+ /out:$@ -unsafe -nowarn:612,618 -d:INSIDE_CORLIB,MONO_CULTURE_DATA -d:LIBC $(REFERENCE_SOURCES_FLAGS) "/d:SELF_TEST;NET_1_1;NET_2_0;NET_2_1;NET_3_5;NET_4_0;NET_4_5;MONO;DISABLE_CAS_USE;MOBILE;MOBILE_DYNAMIC;MONODROID;ANDROID" $^ $(TMP_FILE1) $(TMP_FILE2)

android-pull-tzdb:
	mkdir -p android/tzdb/usr/share/zoneinfo
	mkdir -p android/tzdb/misc/zoneinfo/zoneinfo
	android_root=`adb shell echo '$$ANDROID_ROOT' | tr -d "\r"` ; \
	android_data=`adb shell echo '$$ANDROID_DATA' | tr -d "\r"` ; \
	adb $(ADB_TARGET) pull $$android_root/usr/share/zoneinfo    android/tzdb/usr/share/zoneinfo || true ; \
	adb $(ADB_TARGET) pull $$android_data/misc/zoneinfo/tzdata  android/tzdb/misc/zoneinfo || true

android-dump-tzdata: tzi.exe android-pull-tzdb
	export
	__XA_OVERRIDE_TIMEZONE_ID__=America/New_York ANDROID_ROOT="`pwd`/android/tzdb" ANDROID_DATA="`pwd`/android/tzdb" $(TEST_RUNTIME) tzi.exe -o android/tzdata
