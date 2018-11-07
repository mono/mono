TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config
include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk

UNAME=$(shell uname)

SDK_DIR = $(ANDROID_TOOLCHAIN_DIR)/sdk
NDK_DIR = $(ANDROID_TOOLCHAIN_DIR)/ndk

ADB       = $(SDK_DIR)/platform-tools/adb
ANDROID   = $(SDK_DIR)/tools/android
ANT       = ant
NDK_BUILD = $(NDK_DIR)/ndk-build

PACKAGE   = org.mono.android.AndroidTestRunner
RUNNER    = org.mono.android.AndroidRunner
ACTIVITY  = org.mono.android.AndroidActivity

CONFIGURATION?=release

APK_DIR=apk

BCL_RUNNER_EXE = app/assets/asm/main.exe
DEBUGGER_RUNNER_EXE = app/assets/asm/dtest-app.exe
DEBUGGER_RUNNER_PDB = app/assets/asm/dtest-app.pdb


BCL_CHECKS= \
	corlib \
	System \
	System.Core \
	System.Data \
	System.IO.Compression.FileSystem \
	System.IO.Compression \
	System.Json \
	System.Net.Http \
	System.Numerics \
	System.Runtime.Serialization \
	System.ServiceModel \
	System.ServiceModel.Web \
	System.Transactions \
	System.Xml \
	System.Xml.Linq \
	Mono.CSharp \
	Mono.Data.Sqlite \
	Mono.Data.Tds \
	Mono.Security

CHECKS= \
	mini \
	$(BCL_CHECKS)

.PHONY: all
all: package

#This is hilarious, but we can't clean unless we got the 2 property files around
.PHONY: clean
clean:
	./gradlew clean

.PHONY: check
check: $(patsubst %,check-%,$(CHECKS))

## Helper targets

.PHONY: undeploy
undeploy:
	$(ADB) uninstall $(PACKAGE)

.PHONY: kill
kill:
	$(ADB) shell am force-stop $(PACKAGE)

.PHONY: logcat
logcat:
	$(ADB) logcat

.PHONY: shell
shell:
	$(ADB) shell

## Accept Android License

.PHONY: accept-android-license
accept-android-license:
	yes | $(SDK_DIR)/tools/bin/sdkmanager --licenses

## Check targets

BCL_LIBS=$(subst corlib.dll,mscorlib.dll,$(patsubst %,%.dll,$(BCL_CHECKS)))
BCL_PDBS=$(subst corlib.pdb,mscorlib.pdb,$(patsubst %,%.pdb,$(BCL_CHECKS)))
BCL_REFERENCES=$(patsubst %,-r:$(TOP)/mcs/class/lib/monodroid/%,$(BCL_LIBS))

NUNIT_LIBS= \
	nunitlite.dll

NUNIT_REFERENCES=$(patsubst %,-r:$(TOP)/mcs/class/lib/monodroid/%,$(NUNIT_LIBS))

local.properties:
	echo "sdk.dir=$(SDK_DIR)" >  $@
	echo "ndk.dir=$(NDK_DIR)" >> $@

app/assets/asm:
	mkdir -p $@

## Package the monosgen-2.0 and MonoPosixHelper libraries

MONO_LIBS= \
	libmonosgen-2.0.so \
	libMonoPosixHelper.so

##
# Parameters:
#  $(1): arch
define RuntimeTemplate
app/src/main/jniLibs/$(1):
	mkdir -p $$@

$$(patsubst %,app/src/main/jniLibs/$(1)/%,$$(MONO_LIBS)): app/src/main/jniLibs/$(1)/%: $$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION)/lib/% | app/src/main/jniLibs/$(1)
	cp $$< $$@

.PHONY: package
package: $$(patsubst %,app/src/main/jniLibs/$(1)/%,$$(MONO_LIBS))
endef

$(eval $(call RuntimeTemplate,armeabi-v7a))
$(eval $(call RuntimeTemplate,arm64-v8a))
$(eval $(call RuntimeTemplate,x86))
$(eval $(call RuntimeTemplate,x86_64))

## Package the main.exe and Mono.Android.dll

app/assets/asm/main.exe: managed/main.cs | app/assets/asm
	csc /nostdlib /noconfig /langversion:latest /unsafe /out:$@ $(BCL_REFERENCES) $(NUNIT_REFERENCES) $<

app/assets/asm/Mono.Android.dll: managed/fake-monodroid.cs | app/assets/asm
	csc /nostdlib /noconfig /langversion:latest /t:library /out:$@ $(BCL_REFERENCES) $<

.PHONY: package
package: app/assets/asm/main.exe app/assets/asm/Mono.Android.dll

## Package the BCL assemblies

$(patsubst %,app/assets/asm/%,$(BCL_LIBS)) $(patsubst %,app/assets/asm/%,$(BCL_PDBS)) $(patsubst %,app/assets/asm/%,$(NUNIT_LIBS)): app/assets/asm/%: $(TOP)/mcs/class/lib/monodroid/% | app/assets/asm
	cp $< $@

.PHONY: package
package: $(patsubst %,app/assets/asm/%,$(BCL_LIBS)) $(patsubst %,app/assets/asm/%,$(BCL_PDBS)) $(patsubst %,app/assets/asm/%,$(NUNIT_LIBS))

## Package the BCL test assemblies

##
# Parameters:
#  $(1): assembly name (corlib, System, etc.)
define BCLTestAssemblyTemplate
.PHONY: $$(TOP)/mcs/class/lib/monodroid/tests/monodroid_$(1)_test.dll
$$(TOP)/mcs/class/lib/monodroid/tests/monodroid_$(1)_test.dll:
	$$(MAKE) PROFILE='monodroid' -C $$(TOP)/mcs/class/$(1) test-local

app/assets/asm/monodroid_$(1)_test.dll: $$(TOP)/mcs/class/lib/monodroid/tests/monodroid_$(1)_test.dll
	cp $$< $$@

.PHONY: package
package: app/assets/asm/monodroid_$(1)_test.dll
endef

$(foreach check,$(BCL_CHECKS),$(eval $(call BCLTestAssemblyTemplate,$(check))))

# corlib tests need additional resource assemblies
app/assets/asm/monodroid_corlib_test.dll: app/assets/asm/es-ES/monodroid_corlib_test.resources.dll app/assets/asm/nn-NO/monodroid_corlib_test.resources.dll

app/assets/asm/%/monodroid_corlib_test.resources.dll: $(TOP)/mcs/class/lib/monodroid/tests/%/monodroid_corlib_test.resources.dll
	mkdir -p $(dir $@)
	cp $< $@

## Package the mini test assembly

MINI_TEST_SOURCES= \
	$(TOP)/mono/mini/aot-tests.cs \
	$(TOP)/mono/mini/arrays.cs \
	$(TOP)/mono/mini/basic-calls.cs \
	$(TOP)/mono/mini/basic-float.cs \
	$(TOP)/mono/mini/basic-long.cs \
	$(TOP)/mono/mini/basic-math.cs \
	$(TOP)/mono/mini/basic.cs \
	$(TOP)/mono/mini/exceptions.cs \
	$(TOP)/mono/mini/generics.cs \
	$(TOP)/mono/mini/gshared.cs \
	$(TOP)/mono/mini/objects.cs \
	$(TOP)/mono/mini/TestDriver.cs \
	managed/mini-test-runner.cs

app/assets/asm/monodroid_mini_test.dll: $(MINI_TEST_SOURCES)
	csc /nostdlib /noconfig /langversion:latest /unsafe /t:library /out:$@ /d:__MOBILE__ $(BCL_REFERENCES) $(NUNIT_REFERENCES) $(MINI_TEST_SOURCES)

.PHONY: package
package: app/assets/asm/monodroid_mini_test.dll

## Package Mono.Debugger.Soft test assemblies

.PHONY: $(TOP)/mcs/class/lib/monodroid/tests/dtest-app.exe $(TOP)/mcs/class/lib/monodroid/tests/dtest-app.pdb
$(TOP)/mcs/class/lib/monodroid/tests/dtest-app.exe $(TOP)/mcs/class/lib/monodroid/tests/dtest-app.pdb:
	$(MAKE) PROFILE='monodroid' -C $(TOP)/mcs/class/Mono.Debugger.Soft build-dtest

app/assets/asm/dtest-app.exe: $(TOP)/mcs/class/lib/monodroid/tests/dtest-app.exe
	cp $< $@

app/assets/asm/dtest-app.pdb: $(TOP)/mcs/class/lib/monodroid/tests/dtest-app.pdb
	cp $< $@

package: app/assets/asm/dtest-app.exe app/assets/asm/dtest-app.pdb

## LLDB support

lldb-mono-0.1/xa-lldb: xa-lldb
	wget --no-verbose -O - 'https://github.com/mono/lldb-binaries/releases/download/v0.1/lldb-mono-0.1.tar.gz' | tar xvf -
	cp xa-lldb lldb-mono-0.1/xa-lldb

## Run Mono.Debugger.Soft and BCL tests

build-Mono.Debugger.Soft: package local.properties
	./gradlew uninstallAll assembleDebug installDebug -Pmono.debuggertests $(if $(MONO_WAIT_LLDB),-Pmono.waitforlldb)

check-Mono.Debugger.Soft: build-Mono.Debugger.Soft $(if $(MONO_WAIT_LLDB),lldb-mono-0.1/xa-lldb)
	$(ADB) reverse tcp:6100 tcp:6100
	$(if $(MONO_WAIT_LLDB),lldb-mono-0.1/xa-lldb,$(ADB) shell am instrument -w $(PACKAGE)/$(RUNNER))

.PHONY: build-%
build-%: package local.properties
	printf "monodroid_$*_test.dll" > app/assets/asm/testassembly.txt
	./gradlew uninstallAll assembleDebug installDebug -Pmono.bcltests $(if $(MONO_WAIT_LLDB),-Pmono.waitforlldb)

.PHONY: check-%
check-%: build-% $(if $(MONO_WAIT_LLDB),lldb-mono-0.1/xa-lldb)
	$(if $(MONO_WAIT_LLDB),lldb-mono-0.1/xa-lldb,$(ADB) shell am instrument -w $(PACKAGE)/$(RUNNER))
