thisdir = tools/mdoc
SUBDIRS = 
include ../../build/rules.make

MONODOC_RESOURCES = \
	Resources/mdoc-html-format.xsl    \
	Resources/mdoc-html-utils.xsl    \
	Resources/mdoc-sections-css.xsl  \
	Resources/mono-ecma-css.xsl

MDOC_RESOURCES = \
	Resources/defaulttemplate.xsl \
	Resources/monodoc-ecma.xsd \
	Resources/msitomsx.xsl \
	Resources/overview.xsl \
	Resources/stylesheet.xsl

MDOC_RESOURCES_COMMAND = $(foreach file,$(MDOC_RESOURCES),/resource:../../../external/api-doc-tools/mdoc/$(file),$(notdir $(file)))

MONODOC_RESOURCES_COMMAND = $(foreach file,$(MONODOC_RESOURCES),/resource:../../../external/api-doc-tools/monodoc/$(file),$(notdir $(file)))

LIB_REFS = monodoc System System.Xml System.Core Mono.Cecil ICSharpCode.SharpZipLib System.Xml.Linq System.Web
	
LOCAL_MCS_FLAGS = $(MDOC_RESOURCES_COMMAND) $(MONODOC_RESOURCES_COMMAND) -define:NEW_CECIL
PROGRAM = $(topdir)/class/lib/$(PROFILE)/mdoc.exe
PROGRAM_DEPS = $(topdir)/class/lib/$(PROFILE)/monodoc.dll
MSCORLIB = -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll


MDOC_TEST_FILES = \
	Test/CLILibraryTypes.dtd				  \
	Test/DocTest-v1.cs                                        \
	Test/DocTest-v2.patch                                     \
	Test/msxdoc-expected.importslashdoc.xml                   \
	Test/TestEcmaDocs.xml                                     \
	Test/validate.check.monodocer                             \
	Test/validate.check.monodocer.importslashdoc              \
	Test/validate.check.monodocer.since

MULTI-CLASSIC = Test/DocTest-DropNS-classic.dll Test/DocTest-DropNS-classic-multitest.dll
MULTI-UNIFIED = Test/DocTest-DropNS-unified.dll Test/DocTest-DropNS-unified-multitest.dll

$(PROGRAM): $(PROGRAM_DEPS)

PROGRAM_COMPILE = $(CSCOMPILE)

ifdef MCS_MODE
NO_INSTALL=1
NO_BUILD=1
NO_TEST=1
endif

include ../../build/executable.make

MONO = \
	MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" \
	$(RUNTIME) $(RUNTIME_FLAGS)

DIFF = diff -rup
DIFF_QUIET = diff --brief
ifeq ($(BUILD_PLATFORM), win32)
DIFF = diff -rupZ
DIFF_QUIET = diff --brief -Z
endif

dist-local: dist-default dist-tests

dist-tests:
	find Test/en.expected* -name '*.xml'     > .files
	find Test/html.expected* -name '*.html' >> .files
	tar cTf .files - | (cd $(distdir); tar xf -)
	rm .files ; \

test-local: $(PROGRAM)

clean-local: cleanup

cleanup:
	-rm -Rf Test/en.actual Test/html.actual
	-rm -f monodocer1.exe*

Test/DocTest-addNonGeneric.dll:
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-addNonGeneric.cs

Test/DocTest-addNonGeneric-v2.dll:
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-addNonGeneric.cs /define:V2

Test/DocTest-DropNS-classic-secondary.dll:
	@echo $(value @)
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-DropNS-classic-secondary.cs -doc:Test/DocTest-DropNS-classic-secondary.xml

Test/DocTest-DropNS-classic.dll:
	@echo $(value @)
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-DropNS-classic.cs -doc:Test/DocTest-DropNS-classic.xml

Test/DocTest-DropNS-unified.dll:
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-DropNS-unified.cs

Test/DocTest-DropNS-unified-multitest.dll:
	rm -f $@
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-DropNS-unified.cs /define:MULTITEST

Test/DocTest-DropNS-classic-multitest.dll:
	rm -f $@
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-DropNS-classic.cs /define:MULTITEST

Test/DocTest-DropNS-unified-deletetest.dll:
	rm -f Test/DocTest-DropNS-unified-deletetest.dll
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-DropNS-unified.cs /define:DELETETEST

Test/DocTest-DropNS-unified-deletetest-V2.dll:
	rm -f Test/DocTest-DropNS-unified-deletetest.dll
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:Test/DocTest-DropNS-unified-deletetest.dll Test/DocTest-DropNS-unified.cs /define:DELETETEST,V2

Test/DocTest-DropNS-classic-deletetest.dll:
	rm -f Test/DocTest-DropNS-classic-deletetest.dll
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-DropNS-classic.cs /define:DELETETEST

Test/DocTest-DropNS-classic-deletetest-V2.dll:
	rm -f Test/DocTest-DropNS-classic-deletetest.dll
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:Test/DocTest-DropNS-classic-deletetest.dll Test/DocTest-DropNS-classic.cs /define:DELETETEST,V2

Test/DocTest.dll: 
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest.cs -r:$(topdir)/class/lib/$(PROFILE)/System.Core.dll -r:$(topdir)/class/lib/$(PROFILE)/Microsoft.CSharp.dll

Test/DocTest-InternalInterface.dll: 
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-InternalInterface.cs

Test/DocTest.dll-v1: 
	-rm -f Test/DocTest.cs
	cp Test/DocTest-v1.cs Test/DocTest.cs
	-rm -f Test/DocTest.dll
	$(MAKE) TEST_CSCFLAGS=$(TEST_CSCFLAGS) Test/DocTest.dll

Test/DocTest.dll-v2: 
	-rm -f Test/DocTest.cs
	cp Test/DocTest-v1.cs Test/DocTest.cs
	cd Test && patch -p0 < DocTest-v2.patch
	-rm -f Test/DocTest.dll
	$(MAKE) TEST_CSCFLAGS=$(TEST_CSCFLAGS) Test/DocTest.dll

Test/DocTest-enumerations.dll: 
	$(CSCOMPILE) $(TEST_CSCFLAGS) $(MSCORLIB) -unsafe -target:library -out:$@ Test/DocTest-enumerations.cs

check-monodocer-addNonGeneric: $(PROGRAM)
	-rm -Rf Test/en.actual
	# first, make a docset with the generic method
	$(MAKE) Test/DocTest-addNonGeneric.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-addNonGeneric.dll

	# now add a non-generic version of the method and update several times
	$(MAKE) Test/DocTest-addNonGeneric-v2.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-addNonGeneric-v2.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-addNonGeneric-v2.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-addNonGeneric-v2.dll
	$(DIFF) Test/en.expected-addNonGeneric Test/en.actual

check-monodocer-dropns-classic: $(PROGRAM)
	# tests the simplest --dropns case, a single class where the root namespace was dropped.
	-rm -Rf Test/en.actual
	$(MAKE) Test/DocTest-DropNS-classic.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-DropNS-classic.dll --api-style=classic
	$(MAKE) update-monodocer-dropns-unified
	$(DIFF) Test/en.expected-dropns-classic-v1 Test/en.actual

check-monodocer-dropns-multi: $(PROGRAM)
	-rm -Rf Test/en.actual
	$(MAKE) Test/DocTest-DropNS-classic.dll
	$(MAKE) Test/DocTest-DropNS-unified.dll
	$(MAKE) Test/DocTest-DropNS-classic-multitest.dll
	$(MAKE) Test/DocTest-DropNS-unified-multitest.dll

	# mdoc update for both classic and unified
	$(MONO) $(PROGRAM) update -o Test/en.actual $(MULTI-CLASSIC) --api-style=classic 
	$(MONO) $(PROGRAM) update -o Test/en.actual $(MULTI-UNIFIED) --api-style=unified --dropns Test/DocTest-DropNS-unified.dll=MyFramework --dropns Test/DocTest-DropNS-unified-multitest.dll=MyFramework 
	
	# now run it again to verify idempotency
	$(MONO) $(PROGRAM) update -o Test/en.actual $(MULTI-CLASSIC) --api-style=classic 
	$(MONO) $(PROGRAM) update -o Test/en.actual $(MULTI-UNIFIED) --api-style=unified --dropns Test/DocTest-DropNS-unified.dll=MyFramework --dropns Test/DocTest-DropNS-unified-multitest.dll=MyFramework 
	
	$(DIFF) Test/en.expected-dropns-multi Test/en.actual


check-monodocer-dropns-multi-withexisting: $(PROGRAM)
	-rm -Rf Test/en.actual
	$(MAKE) Test/DocTest-DropNS-classic.dll
	$(MAKE) Test/DocTest-DropNS-unified.dll
	$(MAKE) Test/DocTest-DropNS-classic-multitest.dll
	$(MAKE) Test/DocTest-DropNS-unified-multitest.dll

	# mdoc update to show a pre-existing set of documents
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-DropNS-classic.dll --api-style=classic
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-DropNS-unified.dll --api-style=unified --dropns Test/DocTest-DropNS-unified.dll=MyFramework 
	
	# mdoc update for both classic and unified
	$(MONO) $(PROGRAM) update -o Test/en.actual $(MULTI-CLASSIC) --api-style=classic 
	$(MONO) $(PROGRAM) update -o Test/en.actual $(MULTI-UNIFIED) --api-style=unified --dropns Test/DocTest-DropNS-unified.dll=MyFramework --dropns Test/DocTest-DropNS-unified-multitest.dll=MyFramework 
	
	$(DIFF) Test/en.expected-dropns-multi-withexisting Test/en.actual

check-monodocer-dropns-delete: $(PROGRAM)
	-rm -Rf Test/en.actual
	-rm -Rf Test/actual_statistics.txt
	rm -Rf Test/DocTest-DropNS-classic-deletetest.dll
	rm -Rf Test/DocTest-DropNS-unified-deletetest.dll
	$(MAKE) Test/DocTest-DropNS-classic-deletetest.dll
	$(MONO) $(PROGRAM) update --delete -o Test/en.actual Test/DocTest-DropNS-classic-deletetest.dll --api-style=classic
	$(MAKE) Test/DocTest-DropNS-unified-deletetest.dll
	$(MONO) $(PROGRAM) update --delete -o Test/en.actual Test/DocTest-DropNS-unified-deletetest.dll --api-style=unified --dropns Test/DocTest-DropNS-unified-deletetest.dll=MyFramework
	$(MAKE) Test/DocTest-DropNS-classic-deletetest-V2.dll
	$(MONO) $(PROGRAM) update --delete -o Test/en.actual Test/DocTest-DropNS-classic-deletetest.dll --api-style=classic
	$(MAKE) Test/DocTest-DropNS-unified-deletetest-V2.dll
	$(MONO) $(PROGRAM) update --delete -o Test/en.actual Test/DocTest-DropNS-unified-deletetest.dll --api-style=unified --dropns Test/DocTest-DropNS-unified-deletetest.dll=MyFramework -statistics Test/actual_statistics.txt
	$(DIFF) Test/en.expected-dropns-delete Test/en.actual
	$(DIFF) Test/expected_remove_statistics.txt Test/actual_statistics.txt

check-monodocer-dropns-classic-withsecondary: $(PROGRAM)
	# tests case where a secondary assembly is included with a --dropns parameter
	-rm -Rf Test/en.actual
	$(MAKE) Test/DocTest-DropNS-classic.dll
	$(MAKE) Test/DocTest-DropNS-classic-secondary.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-DropNS-classic.dll Test/DocTest-DropNS-classic-secondary.dll --api-style=classic
	$(MAKE) update-monodocer-dropns-unified-withsecondary
	$(DIFF) Test/en.expected-dropns-classic-withsecondary Test/en.actual

update-monodocer-dropns-unified: $(PROGRAM)
	$(MAKE) Test/DocTest-DropNS-unified.dll
	$(MONO) $(PROGRAM) update --debug -o Test/en.actual Test/DocTest-DropNS-unified.dll --api-style=unified --dropns Test/DocTest-DropNS-unified.dll=MyFramework

update-monodocer-dropns-unified-withsecondary: $(PROGRAM)
	$(MAKE) Test/DocTest-DropNS-unified.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-DropNS-unified.dll Test/DocTest-DropNS-classic-secondary.dll --api-style=unified --dropns Test/DocTest-DropNS-unified.dll=MyFramework

update-monodocer-dropns-classic-secondary: $(PROGRAM)
	$(MAKE) Test/DocTest-DropNS-classic-secondary.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-DropNS-classic-secondary.dll --api-style=classic

check-monodocer-internal-interface: $(PROGRAM)
	# Tests to make sure internal interfaces that are explicitly implemented are not documented
	-rm -Rf Test/en.actual
	$(MAKE) Test/DocTest-InternalInterface.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-InternalInterface.dll -lang VB.NET
	$(DIFF) Test/en.expected-internal-interface Test/en.actual

check-monodocer-enumerations: $(PROGRAM)
	-rm -Rf Test/en.actual
	$(MAKE) Test/DocTest-enumerations.dll
	$(MONO) $(PROGRAM) update -o Test/en.actual Test/DocTest-enumerations.dll
	$(DIFF) Test/en.expected-enumerations Test/en.actual

check-monodocer-update: $(PROGRAM)
	find Test/en.expected -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update -o Test/en.expected Test/DocTest.dll -lang docid -lang vb.net -lang fsharp -lang javascript -lang c++/cli -lang c++/cx -lang c++/winrt

check-monodocer: $(PROGRAM)
	-rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update --debug -o Test/en.actual Test/DocTest.dll -lang docid -lang vb.net -lang fsharp -lang javascript -lang c++/cli -lang c++/cx -lang c++/winrt
	$(DIFF) Test/en.expected Test/en.actual
	$(MONO) $(PROGRAM) update --debug -o Test/en.actual Test/DocTest.dll -lang docid -lang vb.net -lang fsharp -lang javascript -lang c++/cli -lang c++/cx -lang c++/winrt
	$(DIFF) Test/en.expected Test/en.actual

check-monodocer-since-update: $(PROGRAM)
	find Test/en.expected.since -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update -o Test/en.expected.since Test/DocTest.dll 
	$(MAKE) Test/DocTest.dll-v2
	$(MONO) $(PROGRAM) update --since="Version 2.0" \
		-o Test/en.expected.since Test/DocTest.dll 

check-monodocer-since: $(PROGRAM)
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) --debug update -o Test/en.actual Test/DocTest.dll 
	$(MAKE) Test/DocTest.dll-v2
	$(MONO) $(PROGRAM) --debug update --since="Version 2.0" \
		-o Test/en.actual Test/DocTest.dll 
	$(DIFF) Test/en.expected.since Test/en.actual

check-monodocer-delete-update: $(PROGRAM)
	find Test/en.expected.delete -type f -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update -o Test/en.expected.delete Test/DocTest.dll
	$(MAKE) Test/DocTest.dll-v2
	$(MONO) $(PROGRAM) update -o Test/en.expected.delete Test/DocTest.dll
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update -fno-assembly-versions --delete \
		-o Test/en.expected.delete Test/DocTest.dll

check-monodocer-delete: $(PROGRAM)
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) --debug update -o Test/en.actual Test/DocTest.dll
	$(MAKE) Test/DocTest.dll-v2
	$(MONO) $(PROGRAM) --debug update -o Test/en.actual Test/DocTest.dll
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) --debug update -fno-assembly-versions --delete -o Test/en.actual Test/DocTest.dll
	$(DIFF) Test/en.expected.delete Test/en.actual

check-monodocer-importslashdoc-update: $(PROGRAM)
	find Test/en.expected.importslashdoc -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1 TEST_CSCFLAGS=-doc:Test/DocTest.xml
	$(MONO) $(PROGRAM) --debug update -i Test/DocTest.xml \
		-o Test/en.expected.importslashdoc Test/DocTest.dll 

check-monodocer-importslashdoc: $(PROGRAM)
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1 TEST_CSCFLAGS=-doc:Test/DocTest.xml
	$(MONO) $(PROGRAM) --debug update -i Test/DocTest.xml \
		-o Test/en.actual Test/DocTest.dll 
	$(DIFF) Test/en.expected.importslashdoc Test/en.actual

check-monodocer-importecmadoc-update: $(PROGRAM)
	find Test/en.expected.importecmadoc -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) --debug update -i Test/TestEcmaDocs.xml \
		'--type=System.Action`1' --type=System.AsyncCallback \
		--type=System.Environment --type=System.Array \
		-o Test/en.expected.importecmadoc Test/DocTest.dll 

check-monodocer-importecmadoc: $(PROGRAM)
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) --debug update -i Test/TestEcmaDocs.xml \
		'--type=System.Action`1' --type=System.AsyncCallback \
		--type=System.Environment --type=System.Array \
		-o Test/en.actual Test/DocTest.dll 
	$(DIFF) Test/en.expected.importecmadoc Test/en.actual

check-mdoc-export-html-update: $(PROGRAM)
	find Test/html.expected -name \*.html -exec rm "{}" \;
	$(MONO) $(PROGRAM) export-html -o Test/html.expected \
		Test/en.expected.importslashdoc 

check-mdoc-export-html: check-monodocer $(PROGRAM)
	rm -Rf Test/html.actual
	$(MONO) $(PROGRAM) export-html -o Test/html.actual \
		Test/en.expected.importslashdoc
	$(DIFF) Test/html.expected Test/html.actual

check-mdoc-export-html-with-version: $(PROGRAM)
	rm -Rf Test/html.actual.v0 Test/html.actual.since-with-v0 .v0.txt .v2.txt
	$(MONO) $(PROGRAM) export-html -o Test/html.actual.v0 \
		Test/en.expected
	$(MONO) $(PROGRAM) export-html -o Test/html.actual.since-with-v0 \
		Test/en.expected.since -with-version 0.0.0.0
	(cd Test/html.actual.v0            && find . -type f) | sort > .v0.txt
	(cd Test/html.actual.since-with-v0 && find . -type f) | sort > .v2.txt
	$(DIFF) .v0.txt .v2.txt   # assert no types added

check-md-html-dir: $(PROGRAM)
	rm -Rf Test/html.actual
	$(MONO) $(PROGRAM) export-html -dest:Test/html.actual $(DIR) 
	$(DIFF) Test/html.expected Test/html.actual

check-mdoc-export-msxdoc-update:
	$(MONO) $(PROGRAM) export-msxdoc -o - Test/en.expected.importslashdoc \
		> Test/msxdoc-expected.importslashdoc.xml

check-mdoc-export-msxdoc:
	$(MONO) $(PROGRAM) export-msxdoc -o - Test/en.expected.importslashdoc \
		| $(DIFF) - Test/msxdoc-expected.importslashdoc.xml

my_abs_top_srcdir = $(shell cd . && pwd)

check-mdoc-validate-update: $(PROGRAM)
	$(MONO) $(PROGRAM) validate -f ecma Test/en.expected 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' > \
		Test/validate.check.monodocer
	$(MONO) $(PROGRAM) validate -f ecma Test/en.expected.importslashdoc 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' > \
		Test/validate.check.monodocer.importslashdoc
	$(MONO) $(PROGRAM) validate -f ecma Test/en.expected.since 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' > \
		Test/validate.check.monodocer.since

check-mdoc-validate: $(PROGRAM)
	$(MONO) $(PROGRAM) validate -f ecma Test/en.expected 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		$(DIFF_QUIET) - Test/validate.check.monodocer
	$(MONO) $(PROGRAM) validate -f ecma Test/en.expected.importslashdoc 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		$(DIFF_QUIET) - Test/validate.check.monodocer.importslashdoc
	$(MONO) $(PROGRAM) validate -f ecma Test/en.expected.since 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		$(DIFF_QUIET) - Test/validate.check.monodocer.since

run-test-local: check-doc-tools

run-test-update : check-doc-tools-update

ifdef MCS_MODE
check-doc-tools:
	@echo "WARNING: running in mcs mode, mdoc doesn't compile with mcs. Skipping."
else
check-doc-tools:  \
	check-monodocer-since \
	check-monodocer-importecmadoc \
	check-monodocer-importslashdoc \
	check-monodocer \
	check-monodocer-delete \
	check-mdoc-export-html \
	check-mdoc-export-html-with-version \
	check-mdoc-export-msxdoc \
	check-mdoc-validate \
	check-monodocer-dropns-classic \
	check-monodocer-dropns-classic-withsecondary \
	check-monodocer-internal-interface \
	check-monodocer-enumerations \
	check-monodocer-dropns-multi \
	check-monodocer-dropns-multi-withexisting

#check-monodocer-dropns-delete
endif

check-doc-tools-update: check-monodocer-since-update \
	check-monodocer-importecmadoc-update \
	check-monodocer-importslashdoc-update \
	check-monodocer-update \
	check-monodocer-delete-update \
	check-mdoc-export-html-update \
	check-mdoc-export-msxdoc-update \
	check-mdoc-validate-update 

check: check-doc-tools 

