thisdir = tools/corcompare
SUBDIRS =
include ../../build/rules.make

ALL_PROGRAMS = mono-api-info.exe

COMMON_SOURCES = \
	AssemblyResolver.cs	\
	Util.cs	\
	WellFormedXmlWriter.cs

PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

APIINFO_SOURCES = mono-api-info.cs $(COMMON_SOURCES)

DISTFILES= $(COMMON_SOURCES) $(APIINFO_SOURCES)

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
	$(CSCOMPILE) -r:Mono.Cecil.dll -out:$@ $^
