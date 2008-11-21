thisdir = tools/corcompare
SUBDIRS = 
include ../../build/rules.make

ALL_PROGRAMS = CorCompare.exe mono-api-info.exe mono-api-diff.exe transform.exe

CECIL = ../../class/lib/net_1_1/Mono.Cecil.dll

COMMON_SOURCES = \
	AssemblyResolver.cs	\
	CompletionInfo.cs \
	MissingAttribute.cs \
	MissingBase.cs \
	MissingConstructor.cs \
	MissingEvent.cs \
	MissingField.cs \
	MissingInterface.cs \
	MissingMember.cs \
	MissingMethod.cs \
	MissingNameSpace.cs \
	MissingNestedType.cs \
	MissingProperty.cs \
	MissingType.cs \
	ToDoAssembly.cs	\
	Util.cs

PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

CORCOMPARE_SOURCES = CorCompare.cs $(COMMON_SOURCES)
APIINFO_SOURCES = mono-api-info.cs $(COMMON_SOURCES)
APIDIFF_SOURCES = mono-api-diff.cs $(COMMON_SOURCES)

DISTFILES= $(COMMON_SOURCES) $(CORCOMPARE_SOURCES) $(APIINFO_SOURCES) $(APIDIFF_SOURCES) cormissing.xsl mono-api.xsl transform.cs

all-local: $(ALL_PROGRAMS)

doc-update-local:

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

CorCompare.exe: $(CORCOMPARE_SOURCES)
	$(CSCOMPILE) -r:$(CECIL) -out:$@ $^

mono-api-diff.exe: $(APIDIFF_SOURCES)
	$(CSCOMPILE) -r:$(CECIL) -out:$@ $^

mono-api-info.exe: $(APIINFO_SOURCES)
	$(CSCOMPILE) -r:$(CECIL) -out:$@ $^

transform.exe: transform.cs
	$(CSCOMPILE) -out:$@ $^
