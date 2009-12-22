thisdir = docs
SUBDIRS = 
include $(topdir)/build/rules.make

ASSEMBLED_DOCS = \
	mono-file-formats.tree mono-file-formats.zip  \
	mono-tools.tree mono-tools.zip                \
	monoapi.tree monoapi.zip

convert.exe: convert.cs AgilityPack.dll
	$(CSCOMPILE) -out:$@ $< -r:AgilityPack.dll

AgilityPack.dll:
	$(CSCOMPILE) -target:library -out:$@ HtmlAgilityPack/*.cs

monoapi.zip: monoapi.tree
	@test -f $@ || { rm -f $< && $(MAKE) $<; }
monoapi.tree: toc.xml docs.make
	$(MDOC) assemble -o monoapi -f hb $<

mono-tools.zip: mono-tools.tree
	@test -f $@ || { rm -f $< && $(MAKE) $<; }
mono-tools.tree: mono-tools.config docs.make
	$(MDOC) assemble -o mono-tools -f man $<

mono-file-formats.zip: mono-file-formats.tree
	@test -f $@ || { rm -f $< && $(MAKE) $<; }
mono-file-formats.tree: mono-file-formats.config docs.make
	$(MDOC) assemble -o mono-file-formats -f man $<

.doc-stamp:

