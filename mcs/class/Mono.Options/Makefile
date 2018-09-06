thisdir = class/Mono.Options
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Options.dll
LIBRARY_PACKAGE = none

NO_INSTALL = yes

LIB_REFS = System
KEYFILE = ../mono.pub
LIB_MCS_FLAGS =
TEST_MCS_FLAGS =
TEST_LIB_REFS = Mono.Posix System System.Core

mono_sourcelibs_DIR  = $(DESTDIR)$(mono_libdir)/mono-source-libs
mono_options_DATA = Mono.Options/Options.cs

include ../../build/library.make

test-local: Mono.Options-PCL.dll

clean-local: clean-pcl

Mono.Options-PCL.dll: Mono.Options.dll.sources $(shell cat Mono.Options.dll.sources)
	$(CSCOMPILE) -target:library -out:$@ -d:PCL -r:../lib/$(PROFILE)/mscorlib.dll -r:../lib/$(PROFILE)/System.dll @$<

clean-pcl:
	-rm Mono.Options-PCL.dll

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
	Documentation/en/examples/commands.txt \
	Documentation/en/examples/context.txt \
	Documentation/en/examples/greet.txt \
	Documentation/en/examples/localization.txt \
	Documentation/en/examples/subclass.txt

$(the_libdir)/.doc-stamp: $(DOC_EXAMPLES_OUTPUT) 

Documentation/en/examples/Mono.Options.dll: $(the_lib)
	cp $^ $@

%.exe: %.cs Documentation/en/examples/Mono.Options.dll
	$(CSCOMPILE) -debug:portable -r:$(topdir)/class/lib/$(PROFILE)/Mono.Posix.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Core.dll -r:$(topdir)/class/lib/$(PROFILE)/System.dll \
	-r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll \
	-r:$(topdir)/class/lib/$(PROFILE)/Mono.Options.dll -out:$@ $<

Documentation/en/examples/locale/es/LC_MESSAGES/localization.mo: Documentation/en/examples/localization-es.po
	msgfmt $< -o $@

Documentation/en/examples/localization.exe: Documentation/en/examples/locale/es/LC_MESSAGES/localization.mo

%.txt: %.in %.exe
	-rm $@
	exec 3<$< ; \
	while read line 0<&3 ; do \
		if test -n "$$line" ; then \
			echo "$$ $$line" | sed 's#Documentation/en/examples/##' >> $@ ; \
			cmd=`echo "$$line" | sed 's,mono ,MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)" $(RUNTIME) ,g'` ; \
			sh -c "$$cmd" >> $@ 2>&1 ; \
		else \
			echo "" >> $@ ; \
		fi ; \
	done ; \
	exec 3>&-;

EXTRA_DISTFILES = \
	$(wildcard Documentation/en/examples/*.cs) \
	$(wildcard Documentation/en/examples/*.in) \
	$(wildcard Documentation/en/examples/*.po) \
	$(wildcard Documentation/en/examples/*.txt) \
	Documentation/en/examples/locale/es/LC_MESSAGES/localization.mo

CLEAN_FILES = \
	Documentation/en/examples/Mono.Options.dll* \
	Documentation/en/examples/*.exe*
