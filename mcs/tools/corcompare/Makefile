thisdir = tools/corcompare
SUBDIRS =
include ../../build/rules.make

ALL_PROGRAMS = mono-api-info.exe mono-api-html.exe

PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

APIINFO_SOURCES = \
	mono-api-info.cs \
	AssemblyResolver.cs	\
	Util.cs	\
	WellFormedXmlWriter.cs \
	../../class/Mono.Options/Mono.Options/Options.cs

APIHTML_SOURCES = \
	mono-api-html/ApiChange.cs \
	mono-api-html/ApiDiff.cs \
	mono-api-html/AssemblyComparer.cs \
	mono-api-html/ClassComparer.cs \
	mono-api-html/Comparer.cs \
	mono-api-html/ConstructorComparer.cs \
	mono-api-html/EventComparer.cs \
	mono-api-html/FieldComparer.cs \
	mono-api-html/Helpers.cs \
	mono-api-html/InterfaceComparer.cs \
	mono-api-html/MemberComparer.cs \
	mono-api-html/MethodComparer.cs \
	mono-api-html/NamespaceComparer.cs \
	mono-api-html/PropertyComparer.cs \
	../../class/Mono.Options/Mono.Options/Options.cs


DISTFILES= $(APIINFO_SOURCES) $(APIHTML_SOURCES)

all-local: $(ALL_PROGRAMS)

csproj-local doc-update-local:

install-local: $(ALL_PROGRAMS)
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	for i in $(ALL_PROGRAMS) ; do \
		$(INSTALL_BIN) $$i $(DESTDIR)$(PROGRAM_INSTALL_DIR) ; \
	done

uninstall-local:
	for i in $(ALL_PROGRAMS) ; do \
	    rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/`basename $$i` ; \
	done

test-local:

run-test-local run-test-ondotnet-local:

clean-local:
	rm -f *.exe *.pdb

dist-local: dist-default

mono-api-info.exe: $(APIINFO_SOURCES)
	$(CSCOMPILE) -r:Mono.Cecil.dll -r:System.Xml.dll -r:System.Core.dll -r:System.dll -out:$@ $^

mono-api-html.exe: $(APIHTML_SOURCES)
	$(CSCOMPILE) -r:Mono.Cecil.dll -r:System.Xml.dll -r:System.Core.dll -r:System.dll -r:System.Xml.Linq.dll -out:$@ $^
