thisdir = class/Mono.Options
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Options.dll
LIBRARY_PACKAGE = none

NO_INSTALL = yes

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll
TEST_MCS_FLAGS = /r:Mono.Posix.dll /r:System.dll

mono_sourcelibs_DIR  = $(DESTDIR)$(mono_libdir)/mono-source-libs
mono_options_DATA = Mono.Options/Options.cs

include ../../build/library.make

install-local: install-source

uninstall-local: uninstall-source

install-source:
	-$(MKINSTALLDIRS) $(mono_sourcelibs_DIR)
	$(INSTALL) -m 644 $(mono_options_DATA) $(mono_sourcelibs_DIR)

uninstall-source:
	-rm -f $(mono_options_DIR)/Options.cs

fixup-docs:
	for f in `find Documentation/en -name \*.xml` ; do \
		sed -i 's/NDesk.Options/Mono.Options/g' $$f ; \
	done

DOC_EXAMPLES_OUTPUT = \
	Documentation/en/examples/bundling.txt \
	Documentation/en/examples/context.txt \
	Documentation/en/examples/greet.txt \
	Documentation/en/examples/localization.txt \
	Documentation/en/examples/subclass.txt

doc-update-local: $(DOC_EXAMPLES_OUTPUT) 

Documentation/en/examples/Mono.Options.dll: $(the_lib)
	cp $^ $@
	-cp $^.mdb $@.mdb

%.exe: %.cs Documentation/en/examples/Mono.Options.dll
	$(CSCOMPILE) -debug+ -r:Mono.Posix.dll -r:System.Core.dll -lib:Documentation/en/examples -r:Mono.Options.dll -out:$@ $<

Documentation/en/examples/locale/es/LC_MESSAGES/localization.mo: Documentation/en/examples/localization-es.po
	msgfmt $< -o $@

Documentation/en/examples/localization.exe: Documentation/en/examples/locale/es/LC_MESSAGES/localization.mo

%.txt: %.in %.exe
	-rm $@
	exec 3<$< ; \
	while read line 0<&3 ; do \
		if test -n "$$line" ; then \
			echo "$$ $$line" | sed 's#Documentation/en/examples/##' >> $@ ; \
			sh -c "$$line" >> $@ 2>&1 ; \
		else \
			echo "" >> $@ ; \
		fi ; \
	done ; \
	exec 3>&-;


CLEAN_FILES = \
	Documentation/en/examples/Mono.Options.dll* \
	Documentation/en/examples/*.exe*
