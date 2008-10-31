thisdir = docs
SUBDIRS = 
include $(topdir)/build/rules.make

assemble: toc.xml
	$(MDOC) assemble -o monoapi -f hb toc.xml

convert.exe: convert.cs AgilityPack.dll
	$(CSCOMPILE) -out:$@ $< -r:AgilityPack.dll

.doc-stamp:
	
