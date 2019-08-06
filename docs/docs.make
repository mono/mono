#
# This makefile is here because it uses the configuration from the
# in-place built mono to run and compile a few C# tools
#

thisdir = docs
SUBDIRS = 
include $(topdir)/build/rules.make

ASSEMBLED_DOCS = \
	mono-file-formats.tree mono-file-formats.zip  \
	mono-tools.tree mono-tools.zip                \
	monoapi.tree monoapi.zip

convert.exe: $(srcdir)/convert.cs AgilityPack.dll
	$(CSCOMPILE) -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Xml.dll -out:$@ $< -r:AgilityPack.dll

AgilityPack.dll:
	$(CSCOMPILE) -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll -r:$(topdir)/class/lib/$(PROFILE)/System.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Xml.dll -target:library -out:$@ $(srcdir)/HtmlAgilityPack/*.cs

ifdef MCS_MODE
monoapi.zip:
	touch $@

monoapi.tree: monoapi.zip
	touch $@

mono-tools.zip:
	touch $@

mono-tools.tree: mono-tools.zip
	touch $@

mono-file-formats.zip:
	touch $@

mono-file-formats.tree: mono-file-formats.zip
	touch $@

else

monoapi.zip: monoapi.tree
	@test -f $@ || { rm -f $< && $(MAKE) $<; }

monoapi.tree: $(srcdir)/toc.xml $(srcdir)/docs.make
	$(MDOC) assemble -o monoapi -f hb $<

mono-tools.zip: mono-tools.tree
	@test -f $@ || { rm -f $< && $(MAKE) $<; }

mono-tools.tree: $(srcdir)/mono-tools.config $(srcdir)/docs.make
	$(MDOC) assemble -o mono-tools -f man $<

mono-file-formats.zip: mono-file-formats.tree
	@test -f $@ || { rm -f $< && $(MAKE) $<; }

mono-file-formats.tree: $(srcdir)/mono-file-formats.config $(srcdir)/docs.make
	$(MDOC) assemble -o mono-file-formats -f man $<

endif

.doc-stamp:

