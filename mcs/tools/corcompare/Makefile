thisdir = tools/corcompare
SUBDIRS = 
include ../../build/rules.make

ALL_PROGRAMS = CorCompare.exe mono-api-info.exe mono-api-diff.exe
EXTRA_DISTFILES = cormissing.xsl TODO transform.js mono-api.xsl
CORCOMPARE_SOURCES = \
	CompletionInfo.cs \
	CorCompare.cs \
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
	ToDoAssembly.cs

PROGRAM_INSTALL_DIR = $(prefix)/lib/mono/$(FRAMEWORK_VERSION)

APIINFO_SOURCES = mono-api-info.cs
APIDIFF_SOURCES = mono-api-diff.cs

DISTFILES= $(CORCOMPARE_SOURCES) $(APIINFO_SOURCES) $(APIDIFF_SOURCES)

all-local: $(ALL_PROGRAMS)

install-local: $(ALL_PROGRAMS)
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	for i in $(ALL_PROGRAMS) ; do \
		$(INSTALL_BIN) $$i $(DESTDIR)$(PROGRAM_INSTALL_DIR) ; \
	done

uninstall-local:
	for i in $(ALL_PROGRAMS) ; do \
	    rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$$i ; \
	done

test-local:

run-test-local:

clean-local:
	rm -f *.exe *.pdb

dist-local: dist-default

CorCompare.exe: $(CORCOMPARE_SOURCES)
	$(CSCOMPILE) -out:$@ $^

mono-api-info.exe: $(APIINFO_SOURCES)
	$(CSCOMPILE) -out:$@ $^

mono-api-diff.exe: $(APIDIFF_SOURCES)
	$(CSCOMPILE) -out:$@ $^

