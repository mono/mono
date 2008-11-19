thisdir = docs
SUBDIRS = 
include $(topdir)/build/rules.make

ASSEMBLED_DOCS = \
	mono-file-formats.tree mono-file-formats.zip  \
	mono-tools.tree mono-tools.zip                \
	monoapi.tree monoapi.zip

convert.exe: convert.cs AgilityPack.dll
	$(CSCOMPILE) -out:$@ $< -r:AgilityPack.dll

monoapi.zip : monoapi.tree
monoapi.tree: toc.xml docs.make
	$(MDOC) assemble -o monoapi -f hb $<

mono-tools.zip : mono-tools.tree
mono-tools.tree: mono-tools.config docs.make
	$(MDOC) assemble -o mono-tools -f man $<

mono-file-formats.zip : mono-file-formats.tree
mono-file-formats.tree: mono-file-formats.config docs.make
	$(MDOC) assemble -o mono-file-formats -f man $<

.doc-stamp:
	
