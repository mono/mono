thisdir = tools/mdoc
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = \
	/resource:../monodoc/Resources/mdoc-html-format.xsl,mdoc-html-format.xsl    \
	/resource:../monodoc/Resources/mdoc-html-utils.xsl,mdoc-html-utils.xsl      \
	/resource:../monodoc/Resources/mdoc-sections-css.xsl,mdoc-sections-css.xsl  \
	/resource:../monodoc/Resources/mono-ecma-css.xsl,mono-ecma-css.xsl          \
	/resource:Resources/defaulttemplate.xsl,defaulttemplate.xsl                 \
	/resource:Resources/monodoc-ecma.xsd,monodoc-ecma.xsd                       \
	/resource:Resources/overview.xsl,overview.xsl                               \
	/resource:Resources/stylesheet.xsl,stylesheet.xsl                           \
	/r:System.Web.dll                                                           \
	/r:System.Xml.Linq.dll                                                      \
	/r:ICSharpCode.SharpZipLib.dll                                              \
	/r:$(topdir)/class/lib/net_1_1/monodoc.dll                                  \
	/r:$(topdir)/class/lib/net_1_1/Mono.Cecil.dll

MONODOC_RESOURCES = \
	../monodoc/Resources/mdoc-html-utils.xsl    \
	../monodoc/Resources/mdoc-sections-css.xsl  \
	../monodoc/Resources/mono-ecma-css.xsl

MDOC_RESOURCES = \
	Resources/defaulttemplate.xsl     \
	Resources/monodoc-ecma.xsd        \
	Resources/overview.xsl            \
	Resources/stylesheet.xsl

MDOC_TEST_FILES = \
	Test/DocTest-v1.cs                                        \
	Test/DocTest-v2.patch                                     \
	Test/msxdoc-expected.importslashdoc.xml                   \
	Test/TestEcmaDocs.xml                                     \
	Test/validate.check.monodocer                             \
	Test/validate.check.monodocer.importslashdoc              \
	Test/validate.check.monodocer.since

EXTRA_DISTFILES = \
	$(MDOC_RESOURCES) \
	$(MDOC_TEST_FILES)

PROGRAM = mdoc.exe

$(PROGRAM) : $(MDOC_RESOURCES) $(MONODOC_RESOURCES) $(topdir)/class/lib/net_1_1/monodoc.dll

include ../../build/executable.make

$(PROGRAM) : Makefile

MONO = \
	MONO_PATH="$(topdir)/class/lib/2.0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" \
	$(RUNTIME) $(RUNTIME_FLAGS)

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
	-rm monodocer1.exe*

Test/DocTest.dll: 
	$(CSCOMPILE) $(TEST_CSCFLAGS) -debug -unsafe -target:library -out:$@ Test/DocTest.cs

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

check-monodocer-update: mdoc.exe
	find Test/en.expected -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) mdoc.exe update --exceptions=all -o Test/en.expected Test/DocTest.dll

check-monodocer: mdoc.exe
	-rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) mdoc.exe update --exceptions=all -o Test/en.actual Test/DocTest.dll
	diff --exclude=.svn -rup Test/en.expected Test/en.actual
	$(MONO) mdoc.exe update --exceptions=all -o Test/en.actual Test/DocTest.dll 
	diff --exclude=.svn -rup Test/en.expected Test/en.actual

check-monodocer-since-update: mdoc.exe
	find Test/en.expected.since -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) mdoc.exe update --exceptions=all -o Test/en.expected.since Test/DocTest.dll 
	$(MAKE) Test/DocTest.dll-v2
	$(MONO) mdoc.exe update --exceptions=all --since="Version 2.0" \
		-o Test/en.expected.since Test/DocTest.dll 

check-monodocer-since: mdoc.exe
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) mdoc.exe update --exceptions=all -o Test/en.actual Test/DocTest.dll 
	$(MAKE) Test/DocTest.dll-v2
	$(MONO) mdoc.exe update --exceptions=all --since="Version 2.0" \
		-o Test/en.actual Test/DocTest.dll 
	diff --exclude=.svn -rup Test/en.expected.since Test/en.actual

check-monodocer-importslashdoc-update: mdoc.exe
	find Test/en.expected.importslashdoc -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1 TEST_CSCFLAGS=-doc:Test/DocTest.xml
	$(MONO) mdoc.exe update --exceptions=all -i Test/DocTest.xml \
		-o Test/en.expected.importslashdoc Test/DocTest.dll 

check-monodocer-importslashdoc: mdoc.exe
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1 TEST_CSCFLAGS=-doc:Test/DocTest.xml
	$(MONO) mdoc.exe update --exceptions=all -i Test/DocTest.xml \
		-o Test/en.actual Test/DocTest.dll 
	diff --exclude=.svn -rup Test/en.expected.importslashdoc Test/en.actual

check-monodocer-importecmadoc-update: mdoc.exe
	find Test/en.expected.importecmadoc -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) mdoc.exe update --exceptions=all -i Test/TestEcmaDocs.xml \
		'--type=System.Action`1' --type=System.AsyncCallback \
		--type=System.Environment --type=System.Array \
		-o Test/en.expected.importecmadoc Test/DocTest.dll 

check-monodocer-importecmadoc: mdoc.exe
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) mdoc.exe update --exceptions=all -i Test/TestEcmaDocs.xml \
		'--type=System.Action`1' --type=System.AsyncCallback \
		--type=System.Environment --type=System.Array \
		-o Test/en.actual Test/DocTest.dll 
	diff --exclude=.svn -rup Test/en.expected.importecmadoc Test/en.actual

check-mdoc-export-html-update: mdoc.exe
	find Test/html.expected -name \*.html -exec rm "{}" \;
	$(MONO) mdoc.exe export-html -o Test/html.expected \
		Test/en.expected.importslashdoc 

check-mdoc-export-html: check-monodocer mdoc.exe
	rm -Rf Test/html.actual
	$(MONO) mdoc.exe export-html -o Test/html.actual \
		Test/en.expected.importslashdoc
	diff --exclude=.svn -rup Test/html.expected Test/html.actual

check-mdoc-export-html-with-version: mdoc.exe
	rm -Rf Test/html.actual.v0 Test/html.actual.since-with-v0 .v0.txt .v2.txt
	$(MONO) mdoc.exe export-html -o Test/html.actual.v0 \
		Test/en.expected
	$(MONO) mdoc.exe export-html -o Test/html.actual.since-with-v0 \
		Test/en.expected.since -with-version 0.0.0.0
	(cd Test/html.actual.v0            && find . -type f) | sort > .v0.txt
	(cd Test/html.actual.since-with-v0 && find . -type f) | sort > .v2.txt
	diff -rup .v0.txt .v2.txt   # assert no types added

check-md-html-dir: mdoc.exe
	rm -Rf Test/html.actual
	$(MONO) mdoc.exe export-html -dest:Test/html.actual $(DIR) 
	diff --exclude=.svn -rup Test/html.expected Test/html.actual

check-mdoc-export-msxdoc-update:
	$(MONO) mdoc.exe export-msxdoc -o - Test/en.expected.importslashdoc \
		> Test/msxdoc-expected.importslashdoc.xml

check-mdoc-export-msxdoc:
	$(MONO) mdoc.exe export-msxdoc -o - Test/en.expected.importslashdoc \
		| diff --brief - Test/msxdoc-expected.importslashdoc.xml

my_abs_top_srcdir = $(shell cd . && pwd)

check-mdoc-validate-update: mdoc.exe
	$(MONO) mdoc.exe validate -f ecma Test/en.expected 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' > \
		Test/validate.check.monodocer
	$(MONO) mdoc.exe validate -f ecma Test/en.expected.importslashdoc 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' > \
		Test/validate.check.monodocer.importslashdoc
	$(MONO) mdoc.exe validate -f ecma Test/en.expected.since 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' > \
		Test/validate.check.monodocer.since

check-mdoc-validate: mdoc.exe
	$(MONO) mdoc.exe validate -f ecma Test/en.expected 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		diff - Test/validate.check.monodocer
	$(MONO) mdoc.exe validate -f ecma Test/en.expected.importslashdoc 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		diff --brief - Test/validate.check.monodocer.importslashdoc
	$(MONO) mdoc.exe validate -f ecma Test/en.expected.since 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		diff --brief - Test/validate.check.monodocer.since

run-test-local: check-doc-tools

run-test-update : check-doc-tools-update

check-doc-tools: check-monodocer-since \
	check-monodocer-importecmadoc \
	check-monodocer-importslashdoc \
	check-monodocer \
	check-mdoc-export-html \
	check-mdoc-export-html-with-version \
	check-mdoc-export-msxdoc \
	check-mdoc-validate

check-doc-tools-update: check-monodocer-since-update \
	check-monodocer-importecmadoc-update \
	check-monodocer-importslashdoc-update \
	check-monodocer-update \
	check-mdoc-export-html-update \
	check-mdoc-export-msxdoc-update \
	check-mdoc-validate-update

