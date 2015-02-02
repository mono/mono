thisdir = class/System.Core
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Core.dll

REFERENCE_SOURCES_FLAGS = -d:FEATURE_PAL,PFX_LEGACY_3_5
LIB_MCS_FLAGS = $(REFERENCE_SOURCES_FLAGS) -d:INSIDE_SYSCORE -d:LIBC /r:System.dll -unsafe

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_5 -nowarn:1720
endif

MOBILE_DYNAMIC := $(filter monodroid xammac mobile, $(PROFILE))
MOBILE_STATIC := $(filter mobile_static monotouch monotouch_runtime, $(PROFILE))

ifdef MOBILE_DYNAMIC
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR,FEATURE_REFEMIT
endif

system_core_plain_libdir = $(the_libdir_base)plaincore

ifdef MOBILE_STATIC
extra_test_flags := -exclude:NotWorkingInterpreter
system_core_library_deps = $(the_libdir_base)Mono.Dynamic.Interpreter.dll $(system_core_plain_libdir)/System.Core.dll
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR

ifneq (plaincore/,$(intermediate))
LIB_MCS_FLAGS += -d:MONO_INTERPRETER -r:Mono.Dynamic.Interpreter.dll
endif

endif

ifeq (monotouch_runtime, $(PROFILE))
LIB_MCS_FLAGS += -d:FULL_AOT_RUNTIME
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR,FEATURE_REFEMIT,FEATURE_PDBEMIT
endif

ifneq (basic, $(PROFILE))
CLR_PROFILE := $(filter 2.0 4.0 4.5, $(FRAMEWORK_VERSION))
endif

ifdef CLR_PROFILE
LIB_MCS_FLAGS += -r:Mono.Posix.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make

$(the_libdir_base)System.Core.dll: $(system_core_library_deps)

.NOTPARALLEL: $(system_core_plain_libdir)/System.Core.dll $(the_libdir_base)Mono.Dynamic.Interpreter.dll

ifneq (plaincore/,$(intermediate))
$(system_core_plain_libdir)/System.Core.dll:
	$(MAKE) intermediate=plaincore/ $(system_core_plain_libdir)/System.Core.dll
endif

$(the_libdir_base)Mono.Dynamic.Interpreter.dll:
	(cd ../Mono.Dynamic.Interpreter; $(MAKE))

TEST_HARNESS_EXCLUDES += $(extra_test_flags)

CLEAN_FILES = $(system_core_plain_libdir)/System.Core.dll

ifndef intermediate
ifneq ($(PROFILE),basic)
csproj-local:
	$(MAKE) csproj-local intermediate=plaincore/
endif
endif

#
# Android TimeZoneInfo testing....
#

tzi.exe: ../corlib/System/AndroidPlatform.cs $(wildcard System/TimeZone*.cs) ../../build/common/Consts.cs ../Mono.Options/Mono.Options/Options.cs
	mcs /debug+ /out:$@ /unsafe "/d:INSIDE_CORLIB;MONODROID;MOBILE;NET_4_5;NET_4_0;LIBC;SELF_TEST" $^

android-pull-tzdb:
	mkdir -p android/tzdb/usr/share/zoneinfo
	mkdir -p android/tzdb/misc/zoneinfo/zoneinfo
	android_root=`adb shell echo '$$ANDROID_ROOT' | tr -d "\r"` ; \
	android_data=`adb shell echo '$$ANDROID_DATA' | tr -d "\r"` ; \
	adb $(ADB_TARGET) pull $$android_root/usr/share/zoneinfo    android/tzdb/usr/share/zoneinfo ; \
	adb $(ADB_TARGET) pull $$android_data/misc/zoneinfo/tzdata  android/tzdb/misc/zoneinfo

android-dump-tzdata: tzi.exe android-pull-tzdb
	__XA_OVERRIDE_TIMEZONE_ID__=America/New_York ANDROID_ROOT="`pwd`/android/tzdb" ANDROID_DATA="`pwd`/android/tzdb" mono --debug tzi.exe -o android/tzdata
