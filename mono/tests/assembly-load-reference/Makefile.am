include $(top_srcdir)/mk/common.mk

### buildtree stuff

CLASS=$(mcs_topdir)/class/lib/$(DEFAULT_PROFILE)

if FULL_AOT_TESTS
PROFILE_MCS_FLAGS = -d:MOBILE,MOBILE_LEGACY,FULL_AOT_DESKTOP 
endif

if HYBRID_AOT_TESTS
PROFILE_MCS_FLAGS = -d:MOBILE,MOBILE_LEGACY 
endif

### test runner stuff

if HOST_WIN32
PLATFORM_PATH_SEPARATOR=;
else
PLATFORM_PATH_SEPARATOR=:
endif

RUNTIME= $(top_builddir)/runtime/mono-wrapper
TEST_RUNTIME = MONO_PATH=$(CLASS) $(RUNTIME)
TOOLS_RUNTIME= MONO_PATH=$(mcs_topdir)/class/lib/build $(RUNTIME)

TEST_RUNNER = ../test-runner.exe --runtime $(RUNTIME) --runtime-args "--assembly-loader=strict"

if HOST_WIN32
TEST_RUNNER_ARGS=--runtime $(if $(MONO_EXECUTABLE),$(shell cygpath -w -a $(MONO_EXECUTABLE) | sed 's/\\/\\\\/g'),mono)
else
TEST_RUNNER_ARGS=--runtime $(if $(MONO_EXECUTABLE),$(MONO_EXECUTABLE),mono)
endif

TEST_RUNNER_ARGS += $(if $(V), --verbose,)

if FULL_AOT_TESTS
TEST_RUNNER += --runtime-args "$(AOT_RUN_FLAGS)"
endif

if HYBRID_AOT_TESTS
TEST_RUNNER += --runtime-args "$(AOT_RUN_FLAGS)"
endif

### tools

GACUTIL= $(mcs_topdir)/class/lib/build/gacutil.exe
SN= $(mcs_topdir)/class/lib/build/sn.exe

MCS= $(TOOLS_RUNTIME) $(CSC) -noconfig -nologo -debug:portable -target:library $(PROFILE_MCS_FLAGS)

### testcase stuff

EXTRA_DIST= README $(SIGNING_KEY) $(GACTESTLIB_SRCS) 

### Test cases

.PHONY: runtest compile-tests

TESTS_CS = \
	samedir/LoadFromMain.cs \
	samedir/LoadFileMain.cs \
	separatedir/LoadFromMain.cs \
	separatedir/LoadFileMain.cs \
	mainanddep/LoadFromMain.cs \
	mainanddep/LoadFileMain.cs

TESTS_EXE = $(TESTS_CS:.cs=.exe)

REFERENCED_ASMS = \
	samedir/Mid.dll \
	samedir/Dep.dll \
	separatedir/middle/Mid.dll \
	separatedir/middle/Dep.dll \
	mainanddep/middle/Mid.dll \
	mainanddep/Dep.dll

runtest:
	true
	$(MAKE) run-assembly-load-reference-tests


compile-tests: compile-samedir compile-separatedir compile-mainanddep

compile-samedir: samedir/LoadFromMain.exe samedir/LoadFileMain.exe samedir/Dep.dll samedir/Mid.dll

samedir/%.dll: src/%.dll
	cp $< $@

compile-separatedir: separatedir/LoadFromMain.exe separatedir/LoadFileMain.exe separatedir/middle/Dep.dll separatedir/middle/Mid.dll

separatedir/middle/%.dll: src/%.dll
	mkdir -p $(@D)
	cp $< $@

compile-mainanddep: mainanddep/LoadFromMain.exe mainanddep/LoadFileMain.exe mainanddep/Dep.dll mainanddep/middle/Mid.dll

mainanddep/Dep.dll: src/Dep.dll
	cp $< $@

mainanddep/middle/Mid.dll: src/Mid.dll
	mkdir -p $(@D)
	cp $< $@

src/Dep.dll: src/Dep.cs
	$(MCS) -out:$@ $< -target:library

src/Mid.dll: src/Mid.cs src/Dep.dll
	$(MCS) -out:$@ $< -r:src/Dep.dll -target:library

%/LoadFromMain.exe: %/LoadFromMain.cs
	$(MCS) -out:$@ $< -target:exe

%/LoadFileMain.exe: %/LoadFileMain.cs
	$(MCS) -out:$@ $< -target:exe

run-assembly-load-reference-tests: $(TESTS_EXE) $(REFERENCED_ASMS)
	$(TOOLS_RUNTIME) $(TEST_RUNNER) $(TEST_RUNNER_ARGS) --testsuite-name $@ $(TESTS_EXE)
