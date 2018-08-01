thisdir = tools/linker
SUBDIRS =
include ../../build/rules.make

PROGRAM = monolinker.exe

LIB_REFS = System System.Core System.Xml Mono.Cecil

TEST_CASES := \
	mscorlib/test-array.cs \
	mscorlib/test-remoting.cs \
	mscorlib/test-string-01.cs \
	mscorlib/test-string-02.cs \
	mscorlib/test-string-03.cs \
	System/test-security.cs

ifndef AOT_FRIENDLY_PROFILE
TEST_CASES += \
	mscorlib/test-reflection.cs
endif

SIZE_TEST_CASES := \
	link-all/link-all.cs \
	link-sdk/link-sdk.cs

TESTS_COMPILER = $(MCS) -nologo -noconfig -unsafe -debug:portable -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll

check: compile-tests
	$(MAKE) run-tests

compile-tests: $(TEST_CASES) $(SIZE_TEST_CASES)

mscorlib/test-%.cs:
	$(TESTS_COMPILER) Tests/$@ /out:Tests/$(@:.cs=.exe)

System/test-%.cs:
	$(TESTS_COMPILER)  -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.dll Tests/$@ /out:Tests/$(@:.cs=.exe)

link-all/link-all.cs: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/nunitlite.dll
	$(TESTS_COMPILER) -resource:Tests/LinkerDescriptor/link-all.xml -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/I18N.MidEast.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/I18N.Other.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Core.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Xml.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Runtime.Serialization.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.ComponentModel.Composition.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Data.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Net.Http.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/nunitlite.dll $(wildcard Tests/link-all/*.cs) /out:Tests/$(@:.cs=.exe)

link-sdk/link-sdk.cs: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/nunitlite.dll
	$(TESTS_COMPILER) -resource:Tests/LinkerDescriptor/link-sdk.xml -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Core.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Xml.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Xml.Linq.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Runtime.Serialization.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Runtime.Serialization.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.ComponentModel.Composition.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Data.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Net.Http.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Data.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Data.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.ServiceModel.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.ServiceModel.Web.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Data.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Json.dll  -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Data.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/Mono.Data.Sqlite.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/nunitlite.dll $(wildcard Tests/link-sdk/*.cs) /out:Tests/$(@:.cs=.exe)

$(topdir)/class/lib/$(PROFILE_DIRECTORY)/nunitlite.dll:
	$(MAKE) -C $(topdir)/tools/nunit-lite

run-tests: $(TEST_CASES:.cs=.exe)

LINKER_OUTPUT := illink-output-$(PROFILE_DIRECTORY)
PROFILE_PATH = $(topdir)/class/lib/$(PROFILE_DIRECTORY)
LINKER = MONO_PATH=$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE) $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/monolinker.exe -c link -out $(LINKER_OUTPUT) -b true -d $(PROFILE_PATH)
TEST_EXEC = MONO_PATH=$(LINKER_OUTPUT) $(RUNTIME) $(RUNTIME_FLAGS) --debug -O=-aot

.NOTPARALLEL:

System/test-%.exe mscorlib/test-%.exe link-all/link-%.exe link-sdk/link-%.exe:
	@rm -rf $(LINKER_OUTPUT)
	@mkdir $(LINKER_OUTPUT)
	@echo Testing $@
	$(LINKER) -a Tests/$@
	$(TEST_EXEC) $(LINKER_OUTPUT)/$(@F)
	@rm -rf $(LINKER_OUTPUT)

BCL_ASSEMBLIES_CORE=mscorlib.dll System.dll System.Core.dll System.Xml.dll
BCL_ASSEMBLIES=$(BCL_ASSEMBLIES_CORE) $(sort $(filter-out $(BCL_ASSEMBLIES_CORE), $(notdir $(wildcard $(PROFILE_PATH)/System.*.dll)) ))

COMMA=,
REPORT_FILE=$(PROFILE)-linked-size.csv

bcl-size-current: compile-tests
	@echo "App,$$(echo '$(BCL_ASSEMBLIES)' | tr ' ' ',')" > $(REPORT_FILE)
	@for app in $(sort $(SIZE_TEST_CASES:.cs=.exe) $(TEST_CASES:.cs=.exe)); do \
		rm -rf $(LINKER_OUTPUT); \
		mkdir $(LINKER_OUTPUT); \
		echo Checking linked BCL size for $$app; \
		$(LINKER) -a Tests/$$app; \
		sizes=""; \
		for asm in $(BCL_ASSEMBLIES); do \
			if [ -f "$(LINKER_OUTPUT)/$$asm" ]; then size=$$(wc -c < "$(LINKER_OUTPUT)/$$asm" | tr -d "[:space:]"); else size="0"; fi; \
			sizes="$$sizes,$$size"; \
		done; \
		echo "$$app$$sizes" >> $(REPORT_FILE); \
		rm -rf $(LINKER_OUTPUT); \
	done;

bcl-size-diff:
	@echo "Regenerating BCL Linked Size diff..."
	$(Q) $(MAKE) bcl-size-current PROFILE=net_4_x   || true
	$(Q) $(MAKE) bcl-size-current PROFILE=monotouch || true
	$(Q) $(MAKE) bcl-size-current PROFILE=monodroid || true
	@echo "Checking size differences..."
	@mkdir -p sizediff
	$(Q) git diff HEAD "*.csv" > temp.patch
	$(Q) git show HEAD:./net_4_x-linked-size.csv > net_4_x-linked-size.old.csv
	$(Q) git show HEAD:./monotouch-linked-size.csv > monotouch-linked-size.old.csv
	$(Q) git show HEAD:./monodroid-linked-size.csv > monodroid-linked-size.old.csv
	$(Q) sed -e "/@diffdata@/r temp.patch" -e "/@diffdata@/d" -e "/@olddata-net_4_x@/r net_4_x-linked-size.old.csv" -e "/@olddata-net_4_x@/d" -e "/@olddata-monotouch@/r monotouch-linked-size.old.csv" -e "/@olddata-monotouch@/d" -e "/@olddata-monodroid@/r monodroid-linked-size.old.csv" -e "/@olddata-monodroid@/d" -e "/@newdata-net_4_x@/r net_4_x-linked-size.csv" -e "/@newdata-net_4_x@/d" -e "/@newdata-monotouch@/r monotouch-linked-size.csv" -e "/@newdata-monotouch@/d" -e "/@newdata-monodroid@/r monodroid-linked-size.csv" -e "/@newdata-monodroid@/d" linked-size-diff.html.in > sizediff/index.html
	$(Q) if [ -s temp.patch ]; then echo "Error: Found BCL Linked Size differences, see mcs/tools/linker/sizediff/index.html."; rm -f temp.patch *.old.csv; exit 1; else echo "No differences found."; rm -f temp.patch *.old.csv; fi

include ../../build/executable.make
