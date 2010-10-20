thisdir = tools/mdoc
SUBDIRS = 
include ../../build/rules.make

MDOC_COMMON_FLAGS = \
	/resource:../monodoc/Resources/mdoc-html-format.xsl,mdoc-html-format.xsl    \
	/resource:../monodoc/Resources/mdoc-html-utils.xsl,mdoc-html-utils.xsl      \
	/resource:../monodoc/Resources/mdoc-sections-css.xsl,mdoc-sections-css.xsl  \
	/resource:../monodoc/Resources/mono-ecma-css.xsl,mono-ecma-css.xsl          \
	/resource:Resources/defaulttemplate.xsl,defaulttemplate.xsl                 \
	/resource:Resources/monodoc-ecma.xsd,monodoc-ecma.xsd                       \
	/resource:Resources/msitomsx.xsl,msitomsx.xsl                               \
	/resource:Resources/overview.xsl,overview.xsl                               \
	/resource:Resources/stylesheet.xsl,stylesheet.xsl                           \
	/r:System.Web.dll                                                           \
	/r:System.Xml.Linq.dll                                                      \
	/r:ICSharpCode.SharpZipLib.dll                                              \
	/r:Mono.Cecil.dll

LOCAL_MCS_FLAGS = $(MDOC_COMMON_FLAGS) \
	/r:$(topdir)/class/lib/net_2_0/monodoc.dll
PROGRAM = mdoc.exe
PROGRAM_DEPS = $(topdir)/class/lib/net_2_0/monodoc.dll

ifdef NET
all : copy-with-deps

copy-with-deps:
	-mkdir mdoc-net
	cp $(PROGRAM) mdoc-net
	cp $(topdir)/class/lib/net_4_0/Commons.Xml.Relaxng.dll mdoc-net
	cp $(topdir)/class/lib/net_4_0/ICSharpCode.SharpZipLib.dll mdoc-net
	cp $(topdir)/class/lib/net_4_0/Mono.Cecil.dll mdoc-net
	cp $(topdir)/class/lib/net_2_0/monodoc.dll mdoc-net
endif

MONODOC_RESOURCES = \
	../monodoc/Resources/mdoc-html-utils.xsl    \
	../monodoc/Resources/mdoc-sections-css.xsl  \
	../monodoc/Resources/mono-ecma-css.xsl

MDOC_RESOURCES = \
	Resources/defaulttemplate.xsl     \
	Resources/monodoc-ecma.xsd        \
	Resources/msitomsx.xsl            \
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

$(PROGRAM) : $(MDOC_RESOURCES) $(MONODOC_RESOURCES) $(PROGRAM_DEPS)

include ../../build/executable.make

$(PROGRAM) : Makefile

MONO = \
	MONO_PATH="$(topdir)/class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" \
	/home/jbevain/sources/mono/mono/mini/mono $(RUNTIME_FLAGS)

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

check-monodocer-update: $(PROGRAM)
	find Test/en.expected -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update --exceptions=all -o Test/en.expected Test/DocTest.dll

check-monodocer: $(PROGRAM)
	-rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update --exceptions=all -o Test/en.actual Test/DocTest.dll
	diff --exclude=.svn -rup Test/en.expected Test/en.actual
	$(MONO) $(PROGRAM) update --exceptions=all -o Test/en.actual Test/DocTest.dll 
	diff --exclude=.svn -rup Test/en.expected Test/en.actual

check-monodocer-since-update: $(PROGRAM)
	find Test/en.expected.since -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update --exceptions=all -o Test/en.expected.since Test/DocTest.dll 
	$(MAKE) Test/DocTest.dll-v2
	$(MONO) $(PROGRAM) update --exceptions=all --since="Version 2.0" \
		-o Test/en.expected.since Test/DocTest.dll 

check-monodocer-since: $(PROGRAM)
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update --exceptions=all -o Test/en.actual Test/DocTest.dll 
	$(MAKE) Test/DocTest.dll-v2
	$(MONO) $(PROGRAM) update --exceptions=all --since="Version 2.0" \
		-o Test/en.actual Test/DocTest.dll 
	diff --exclude=.svn -rup Test/en.expected.since Test/en.actual

check-monodocer-importslashdoc-update: $(PROGRAM)
	find Test/en.expected.importslashdoc -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1 TEST_CSCFLAGS=-doc:Test/DocTest.xml
	$(MONO) $(PROGRAM) update --exceptions=all -i Test/DocTest.xml \
		-o Test/en.expected.importslashdoc Test/DocTest.dll 

check-monodocer-importslashdoc: $(PROGRAM)
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1 TEST_CSCFLAGS=-doc:Test/DocTest.xml
	$(MONO) $(PROGRAM) update --exceptions=all -i Test/DocTest.xml \
		-o Test/en.actual Test/DocTest.dll 
	diff --exclude=.svn -rup Test/en.expected.importslashdoc Test/en.actual

check-monodocer-importecmadoc-update: $(PROGRAM)
	find Test/en.expected.importecmadoc -name \*.xml -exec rm "{}" \;
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update --exceptions=all -i Test/TestEcmaDocs.xml \
		'--type=System.Action`1' --type=System.AsyncCallback \
		--type=System.Environment --type=System.Array \
		-o Test/en.expected.importecmadoc Test/DocTest.dll 

check-monodocer-importecmadoc: $(PROGRAM)
	rm -Rf Test/en.actual
	$(MAKE) Test/DocTest.dll-v1
	$(MONO) $(PROGRAM) update --exceptions=all -i Test/TestEcmaDocs.xml \
		'--type=System.Action`1' --type=System.AsyncCallback \
		--type=System.Environment --type=System.Array \
		-o Test/en.actual Test/DocTest.dll 
	diff --exclude=.svn -rup Test/en.expected.importecmadoc Test/en.actual

check-mdoc-export-html-update: $(PROGRAM)
	find Test/html.expected -name \*.html -exec rm "{}" \;
	$(MONO) $(PROGRAM) export-html -o Test/html.expected \
		Test/en.expected.importslashdoc 

check-mdoc-export-html: check-monodocer $(PROGRAM)
	rm -Rf Test/html.actual
	$(MONO) $(PROGRAM) export-html -o Test/html.actual \
		Test/en.expected.importslashdoc
	diff --exclude=.svn -rup Test/html.expected Test/html.actual

check-mdoc-export-html-with-version: $(PROGRAM)
	rm -Rf Test/html.actual.v0 Test/html.actual.since-with-v0 .v0.txt .v2.txt
	$(MONO) $(PROGRAM) export-html -o Test/html.actual.v0 \
		Test/en.expected
	$(MONO) $(PROGRAM) export-html -o Test/html.actual.since-with-v0 \
		Test/en.expected.since -with-version 0.0.0.0
	(cd Test/html.actual.v0            && find . -type f) | sort > .v0.txt
	(cd Test/html.actual.since-with-v0 && find . -type f) | sort > .v2.txt
	diff -rup .v0.txt .v2.txt   # assert no types added

check-md-html-dir: $(PROGRAM)
	rm -Rf Test/html.actual
	$(MONO) $(PROGRAM) export-html -dest:Test/html.actual $(DIR) 
	diff --exclude=.svn -rup Test/html.expected Test/html.actual

check-mdoc-export-msxdoc-update:
	$(MONO) $(PROGRAM) export-msxdoc -o - Test/en.expected.importslashdoc \
		> Test/msxdoc-expected.importslashdoc.xml

check-mdoc-export-msxdoc:
	$(MONO) $(PROGRAM) export-msxdoc -o - Test/en.expected.importslashdoc \
		| diff --brief - Test/msxdoc-expected.importslashdoc.xml

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
		diff - Test/validate.check.monodocer
	$(MONO) $(PROGRAM) validate -f ecma Test/en.expected.importslashdoc 2>&1 | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		diff --brief - Test/validate.check.monodocer.importslashdoc
	$(MONO) $(PROGRAM) validate -f ecma Test/en.expected.since 2>&1 | \
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

