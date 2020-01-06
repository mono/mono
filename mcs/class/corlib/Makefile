thisdir = class/corlib
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/corlib

LIBRARY = corlib.dll
LIBRARY_NAME = mscorlib.dll

LIB_MCS_FLAGS = $(REFERENCE_SOURCES_FLAGS) $(RESOURCE_FILES:%=-resource:%)

USE_XTEST_REMOTE_EXECUTOR = YES
LIBRARY_WARN_AS_ERROR = yes
#LIBRARY_USE_INTERMEDIATE_FILE = yes

ifeq (2, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += -runtimemetadataversion:v4.0.30319
else
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += -runtimemetadataversion:v4.0.30319
else
$(error Unknown framework version)
endif
endif

ifeq ($(PROFILE),build)
CSC_RUNTIME_FLAGS=--profile=aot:output=$(topdir)/class/lib/$(PROFILE_DIRECTORY)/csc.$(LIBRARY).aotprofile
endif

RESX_EXTRA_ARGUMENTS = \
	--in=ReferenceSources/SR.cs \
	--in=ReferenceSources/SR2.cs \
	--in=corefx/SR.missing.cs

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Collections/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Collections.Concurrent/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Collections.NonGeneric/src/Resources/Strings.resx \
	../../../external/corefx/src/System.ComponentModel.TypeConverter/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Buffers/src/Resources/Strings.resx \
	../../../external/corefx/src/System.IO.FileSystem/src/Resources/Strings.resx \
	../../../external/corefx/src/System.IO.FileSystem.Watcher/src/Resources/Strings.resx \
	../../../external/corefx/src/System.IO.Ports/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Memory/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Net.HttpListener/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Net.Requests/src/Resources/Strings.resx	\
	../../../external/corefx/src/System.Net.Sockets/src/Resources/Strings.resx	\
	../../../external/corefx/src/System.Net.Http/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Numerics.Vectors/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Reflection.TypeExtensions/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Runtime.Extensions/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Runtime.InteropServices.RuntimeInformation/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Runtime.Numerics/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Security.Cryptography.Encoding/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Security.Cryptography.Pkcs/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Security.Cryptography.Primitives/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Security.Cryptography.ProtectedData/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Security.Cryptography.Xml/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Security.Cryptography.X509Certificates/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Text.RegularExpressions/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Threading.Tasks.Parallel/src/Resources/Strings.resx \
	../../../external/corefx/src/System.ObjectModel/src/Resources/Strings.resx \
	../../../external/corert/src/System.Private.CoreLib/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Private.Uri/src/Resources/Strings.resx	\
	../../../external/corefx/src/Microsoft.Win32.Registry/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Security.Cryptography.Algorithms/src/Resources/Strings.resx

LIBRARY_COMPILE = $(BOOT_COMPILE)
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

include il/il.make

RESOURCE_STRINGS_FILES = --mscorlib-debug

RESOURCE_STRINGS = ../referencesource/mscorlib/mscorlib.txt
MODULE_DEPS = $(IL_REPLACE) LinkerDescriptor/mscorlib.xml


ifdef WASM
MANAGED_COLLATOR_RESOURCES_FILES =
else
MANAGED_COLLATOR_RESOURCES_FILES = \
	resources/collation.core.bin \
	resources/collation.tailoring.bin \
	resources/collation.cjkCHS.bin \
	resources/collation.cjkCHT.bin \
	resources/collation.cjkJA.bin \
	resources/collation.cjkKO.bin \
	resources/collation.cjkKOlv2.bin
endif

RESOURCE_FILES = \
	resources/charinfo.nlp \
	$(MANAGED_COLLATOR_RESOURCES_FILES) \
	LinkerDescriptor/mscorlib.xml

REFERENCE_SOURCES_FLAGS = -d:FEATURE_PAL,GENERICS_WORK,FEATURE_LIST_PREDICATES,FEATURE_SERIALIZATION,FEATURE_ENCODINGNLS,FEATURE_ASCII,FEATURE_LATIN1,FEATURE_UTF7,FEATURE_UTF32,MONO_HYBRID_ENCODING_SUPPORT,FEATURE_ASYNC_IO,NEW_EXPERIMENTAL_ASYNC_IO,FEATURE_UTF32,FEATURE_EXCEPTIONDISPATCHINFO,FEATURE_CORRUPTING_EXCEPTIONS,FEATURE_EXCEPTION_NOTIFICATIONS,FEATURE_STRONGNAME_MIGRATION,FEATURE_USE_LCID,FEATURE_FUSION,FEATURE_CRYPTO,FEATURE_X509_SECURESTRINGS,FEATURE_SYNCHRONIZATIONCONTEXT,FEATURE_SYNCHRONIZATIONCONTEXT_WAIT,FEATURE_DEFAULT_INTERFACES,HAS_CORLIB_CONTRACTS

ifndef NO_SRE
LIB_MCS_FLAGS += -d:MONO_FEATURE_SRE
endif

ifndef MOBILE_PROFILE
REFERENCE_SOURCES_FLAGS += -d:FEATURE_MACL
endif

ifndef AOT_FRIENDLY_PROFILE
ifndef DISABLE_REMOTING
REFERENCE_SOURCES_FLAGS += -d:FEATURE_REMOTING,MONO_COM,FEATURE_COMINTEROP,FEATURE_ROLE_BASED_SECURITY
else
REFERENCE_SOURCES_FLAGS += -d:FEATURE_ROLE_BASED_SECURITY
endif
endif

ifndef NO_THREAD_ABORT
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_THREAD_ABORT
TEST_MCS_FLAGS += -d:MONO_FEATURE_THREAD_ABORT
endif

ifndef NO_THREAD_SUSPEND_RESUME
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_THREAD_SUSPEND_RESUME
TEST_MCS_FLAGS += -d:MONO_FEATURE_THREAD_SUSPEND_RESUME
endif

ifndef NO_MULTIPLE_APPDOMAINS
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_MULTIPLE_APPDOMAINS
TEST_MCS_FLAGS += -d:MONO_FEATURE_MULTIPLE_APPDOMAINS
endif

ifndef NO_CONSOLE
LIB_MCS_FLAGS += -d:MONO_FEATURE_CONSOLE
endif

ifdef MONO_FEATURE_APPLETLS
LIB_MCS_FLAGS += -d:MONO_FEATURE_APPLETLS
endif

ifdef ONLY_APPLETLS
LIB_MCS_FLAGS += -d:ONLY_APPLETLS
endif

ifdef MONO_FEATURE_APPLE_X509
LIB_MCS_FLAGS += -d:MONO_FEATURE_APPLE_X509
endif

COREFX_FLAGS := -d:BIT64,REGISTRY_ASSEMBLY

WARNING_ABOUT_DISABLED_WARNING=1635
LOCAL_MCS_FLAGS = -unsafe -nostdlib -nowarn:612,618,3001,3002,3003,$(WARNING_ABOUT_DISABLED_WARNING) -d:INSIDE_CORLIB,MONO_CULTURE_DATA -d:LIBC $(REFERENCE_SOURCES_FLAGS) $(COREFX_FLAGS)
DEFAULT_REFERENCES =

TEST_LIB_REFS = System.Core System

XTEST_LIB_REFS = System System.Core Facades/System.Threading.Tasks Facades/System.Text.RegularExpressions System.Numerics System.Runtime.CompilerServices.Unsafe
XTEST_LIB_FLAGS = -d:netcoreapp -publicsign -keyfile:../mono.snk
XTEST_MCS_FLAGS = -resource:../../../external/corefx/src/System.Reflection/tests/Resources/ResourceTextFile.txt,ResourceTextFile.txt

ifndef AOT_FRIENDLY_PROFILE
ifneq ($(PROFILE),xammac)
ifneq ($(PROFILE),testing_aot_hybrid)
ifneq ($(PROFILE),testing_aot_full_interp)
TEST_LIB_REFS += Mono.Posix
endif
endif
endif
endif

# System.IO/DirectoryInfoTest.cs needs Mono.Posix
TEST_MCS_FLAGS += -debug -nowarn:168,219,618,672 -unsafe -nowarn:67,114,162,169,649,659 \
				 -define:MONO_DATACONVERTER_STATIC_METHODS $(TEST_RESX_RESOURCES:%=-resource:%) -resource:LinkerDescriptor/mscorlib_test.xml,$(test_lib:.dll=.xml)

ifeq ($(PROFILE),net_4_x)
LIB_MCS_FLAGS += -d:FEATURE_CRYPTO_CONFIGURABLE
TEST_MCS_FLAGS += -d:FEATURE_CRYPTO_CONFIGURABLE
endif

XTEST_SPLIT_ASSEMBLIES := $(filter monotouch%, $(PROFILE))

CC_PROFILE := $(filter monotouch% xammac, $(PROFILE))
ifdef CC_PROFILE

BUILT_SOURCES = \
	CommonCrypto/MD5CryptoServiceProvider.g.cs	\
	CommonCrypto/SHA256Managed.g.cs \
	CommonCrypto/RC2CryptoServiceProvider.g.cs \
	CommonCrypto/SHA384Managed.g.cs \
	CommonCrypto/DESCryptoServiceProvider.g.cs \
	CommonCrypto/SHA1CryptoServiceProvider.g.cs	\
	CommonCrypto/SHA512Managed.g.cs \
	CommonCrypto/MD2Managed.g.cs \
	CommonCrypto/SHA1Managed.g.cs \
	CommonCrypto/TripleDESCryptoServiceProvider.g.cs \
	CommonCrypto/MD4Managed.g.cs

CommonCrypto/%.g.cs:
	$(MAKE) -C CommonCrypto

endif

EXTRA_DISTFILES = \
	Test/resources/culture-*.cs		\
	$(RESOURCE_FILES) \
	$(TEST_RESOURCE_FILES) \
	$(TEST_RESOURCES:.resources=.resx) \
	LinkerDescriptor/mscorlib_test.xml

TEST_RESOURCE_FILES = \
	Test/resources/MyResources.resources	\
	Test/resources/MyResources.de.resources	\
	Test/resources/Empty.resources		\
	Test/resources/AFile.txt		\
	Test/resources/StreamTest.resources	\
	Test/resources/StreamTest.ja.resources	\
	Test/resources/bug81759.resources	\
	Test/resources/415628.bin		\
	Test/resources/Fergie.GED

TEST_MCS_FLAGS += $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

TEST_RESX_RESOURCES = \
	Test/resources/Resources.resources

TEST_RESX_RESOURCES_SATELITE = \
	Test/resources/Resources.es-ES.resources \
	Test/resources/Resources.nn-NO.resources

CLEAN_FILES += $(TEST_RESX_RESOURCES) $(TEST_RESX_RESOURCES_SATELITE) $(IL_REPLACE)

include $(topdir)/build/library.make

$(TEST_RESX_RESOURCES) $(TEST_RESX_RESOURCES_SATELITE): %.resources: %.resx
	$(RESGEN) $< || cp $@.prebuilt $@

TEST_RESOURCES = $(TEST_RESX_RESOURCES) $(TEST_RESX_RESOURCES_SATELITE)

satellite_assembly1 = $(test_lib_dir)/es-ES/$(patsubst %.dll,%.resources.dll,$(test_lib))
satellite_assembly2 = $(test_lib_dir)/nn-NO/$(patsubst %.dll,%.resources.dll,$(test_lib))

$(test_lib_output): $(TEST_RESOURCES) $(satellite_assembly1) $(satellite_assembly2) | $(test_lib_dir)

mscorlib_for_profile = $(topdir)/class/lib/$(PROFILE)/mscorlib.dll

$(satellite_assembly1): Test/resources/culture-es-ES.cs Test/resources/Resources.es-ES.resources
	@mkdir -p $(dir $@)
	$(CSCOMPILE) -target:library -r:$(mscorlib_for_profile) Test/resources/culture-es-ES.cs -resource:Test/resources/Resources.es-ES.resources -out:$@

$(satellite_assembly2): Test/resources/culture-nn-NO.cs Test/resources/Resources.nn-NO.resources
	@mkdir -p $(dir $@)
	$(CSCOMPILE) -target:library -r:$(mscorlib_for_profile) Test/resources/culture-nn-NO.cs -resource:Test/resources/Resources.nn-NO.resources -out:$@

test_load_assembly = ${test_lib_dir}/TestLoadAssembly.dll

$(test_load_assembly): ../../../external/corefx/src/System.Runtime/tests/TestLoadAssembly/TestLoadAssembly.cs | $(test_lib_dir)
	$(CSCOMPILE) -target:library -r:$(mscorlib_for_profile) -out:${test_lib_dir}/TestLoadAssembly.dll

test_module = $(test_lib_dir)/System.Reflection.TestModule.dll

$(test_module): ../../../external/corefx/src/System.Runtime/tests/TestModule/System.Reflection.TestModule.dll | $(test_lib_dir)
	cp $< $(test_lib_dir)

XTEST_LIB_FLAGS += -r:$(test_module)

$(xtest_lib_output): $(test_load_assembly) $(test_module) $(test_lib_dir)

vtsdir_src = Test/System.Runtime.Serialization.Formatters.Binary/VersionTolerantSerialization
vtsdir = $(test_lib_dir)/vts
vtslibs = \
	$(vtsdir)/1.0/Address.dll \
	$(vtsdir)/2.0/Address.dll \
	$(vtsdir)/3.0/Address.dll \
	$(vtsdir)/4.0/Address.dll \
	$(vtsdir)/5.0/Address.dll \
	$(vtsdir)/6.0/Address.dll

test-vts: $(vtslibs) $(test_lib_dir)/BinarySerializationOverVersionsTest.dll

$(vtsdir)/%/Address.dll: $(vtsdir_src)/VersionTolerantSerializationTestLib/%/Address.cs
	@mkdir -p $(dir $@)
	$(CSCOMPILE) -target:library -warn:0 -r:$(mscorlib_for_profile) -out:$@ $^

$(test_lib_dir)/BinarySerializationOverVersionsTest.dll: $(vtsdir_src)/BinarySerializationOverVersionsTest.cs $(vtsdir)/1.0/Address.dll $(test_nunit_dep)
	@mkdir -p $(dir $@)
	$(CSCOMPILE) $(test_nunit_ref) -warn:0 \
		-r:$(vtsdir)/1.0/Address.dll \
		-r:$(mscorlib_for_profile) \
		-r:$(topdir)/class/lib/$(PROFILE)/System.dll \
		-target:library \
		$(vtsdir_src)/BinarySerializationOverVersionsTest.cs -out:$@
	cp -f $(topdir)/tools/nunit-lite/nunit-lite-console/nunit-lite-console.exe.config.tmpl $(test_lib_dir)/BinarySerializationOverVersionsTest.dll.nunitlite.config

run-test-vts: test-vts
	@echo Running vts tests...
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" $(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(TEST_HARNESS) $(NOSHADOW_FLAG) \
		$(test_lib_dir)/BinarySerializationOverVersionsTest.dll

ifdef NO_TEST
NO_VTS_TEST = yes
endif

ifdef DISABLE_REMOTING
NO_VTS_TEST = yes
endif

ifndef NO_VTS_TEST
test-local: test-vts
run-test-local: run-test-vts
endif

EXTRA_DISTFILES += \
	$(vtsdir_src)/VersionTolerantSerializationTestLib/1.0/Address.cs \
	$(vtsdir_src)/VersionTolerantSerializationTestLib/2.0/Address.cs \
	$(vtsdir_src)/VersionTolerantSerializationTestLib/3.0/Address.cs \
	$(vtsdir_src)/VersionTolerantSerializationTestLib/4.0/Address.cs \
	$(vtsdir_src)/VersionTolerantSerializationTestLib/5.0/Address.cs \
	$(vtsdir_src)/VersionTolerantSerializationTestLib/6.0/Address.cs \
	$(vtsdir_src)/BinarySerializationOverVersionsTest.cs \
	System.Runtime.CompilerServices/Unsafe.il \
	il/il.make \
	il/Makefile

#
# Android TimeZoneInfo testing....
#

TMP_FILE1=tzi1.tmp
TMP_FILE2=tzi2.tmp

# Remove some code that is protected so we don't have to include all the corlib files.
tzi.exe:System/AndroidPlatform.cs $(wildcard System/TimeZone*.cs) ../../build/common/Consts.cs ../../build/common/Locale.cs ../Mono.Options/Mono.Options/Options.cs
	trap "rm -f $(TMP_FILE1) $(TMP_FILE2)" EXIT INT QUIT TERM && \
	sed 's/Environment\.GetResourceString/string.Format/g' ../referencesource/mscorlib/system/timezoneinfo.cs > $(TMP_FILE1) && \
	sed 's/StringBuilder\.DefaultCapacity/100/g' ../referencesource/mscorlib/system/text/stringbuildercache.cs > $(TMP_FILE2) && \
	$(MCS) /debug+ /out:$@ -unsafe -nowarn:612,618 -d:INSIDE_CORLIB,MONO_CULTURE_DATA -d:LIBC $(REFERENCE_SOURCES_FLAGS) "/d:SELF_TEST;NET_1_1;NET_2_0;MOBILE;NET_3_5;NET_4_0;NET_4_5;MONO;MOBILE;MOBILE_DYNAMIC;MONODROID;ANDROID" $^ $(TMP_FILE1) $(TMP_FILE2)

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
