sourcesdir = $(prefix)/lib/monodoc/sources

sources_DATA = \
	mono-file-formats.source \
	mono-tools.source \
	monoapi.source \
	$(ASSEMBLED_DOCS)

ASSEMBLED_DOCS = \
	mono-file-formats.tree mono-file-formats.zip  \
	mono-tools.tree mono-tools.zip                \
	monoapi.tree monoapi.zip

EXTRA_DIST = \
	abc-removal.txt		\
	api-style.css		\
	assembly-bundle		\
	check-exports		\
	check-coverage		\
	convert.cs		\
	current-api		\
	deploy			\
	docs.make		\
	documented		\
	embedded-api		\
	exceptions		\
	exdoc			\
	file-share-modes	\
	gc-issues		\
	gc-variables-in-c	\
	glossary.txt		\
	ignore			\
	internal-calls		\
	ir-desc			\
	jit-imt			\
	jit-thoughts		\
	jit-trampolines		\
	local-regalloc.txt	\
	magic.diff		\
	mini-doc.txt		\
	mono-api-metadata.html	\
	mono-file-formats.config\
	mono-file-formats.source\
	mono_handle_d		\
	mono-tools.config       \
	mono-tools.source       \
	monoapi.source		\
	new-regalloc		\
	object-layout		\
	opcode-decomp.txt	\
	precise-gc		\
	produce-lists		\
	public			\
	public-api		\
	README			\
	release-notes-1.0.html	\
	remoting		\
	ssapre.txt		\
	stack-alignment		\
	stack-overflow.txt	\
	threading		\
	toc.xml			\
	TODO			\
	tree-mover.txt		\
	unmanaged-calls	

dist-hook:
	$(mkdir_p)  $(distdir)/sources
	$(mkdir_p)  $(distdir)/svgs
	$(mkdir_p)  $(distdir)/HtmlAgilityPack
	cp sources/*  $(distdir)/sources
	cp svgs/*     $(distdir)/svgs
	cp HtmlAgilityPack/*     $(distdir)/HtmlAgilityPack

clean-local:
	-rm -Rf html
	-rm -f deploy/* convert.exe* AgilityPack.dll*
	-rm -f $(ASSEMBLED_DOCS)

TOOL_MAKE=$(MAKE) -f $(srcdir)/docs.make topdir=$(srcdir)/../mcs srcdir=$(srcdir)

monoapi.zip: monoapi.tree

monoapi.tree: $(srcdir)/docs.make $(srcdir)/Makefile.am $(srcdir)/toc.xml $(srcdir)/deploy/.stamp 
	-mkdir -p deploy
	cp -f $(srcdir)/api-style.css deploy
	$(TOOL_MAKE) $@

mono-tools.zip: mono-tools.tree

mono-tools.tree: $(srcdir)/docs.make Makefile.am deploy/.stamp 
	$(TOOL_MAKE) $@

mono-file-formats.zip: mono-file-formats.tree 

mono-file-formats.tree: $(srcdir)/docs.make $(srcdir)/Makefile.am $(srcdir)/deploy/.stamp
	$(TOOL_MAKE) $@

deploy/.stamp: convert.exe Makefile.am 
	$(mkdir_p) html
	runtimedir=`cd ../runtime && pwd`; export runtimedir; \
	MONO_PATH=../mcs/class/lib/net_2_0 perl $(srcdir)/exdoc -h $(srcdir) -t . $(srcdir)/../mono/*/*.c
	touch $@

extract: deploy/.stamp

convert.exe: convert.cs AgilityPack.dll
	$(TOOL_MAKE) PROFILE=net_2_0 convert.exe

AgilityPack.dll: 
	$(TOOL_MAKE) PROFILE=net_2_0 AgilityPack.dll

