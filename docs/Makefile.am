sourcesdir = $(prefix)/lib/monodoc/sources

sources_DATA = \
	mono-file-formats.source \
	mono-tools.source \
	monoapi.source \
	$(ASSEMBLED_DOCS)

if DISABLE_MCS_DOCS
ASSEMBLED_DOCS = 
else
ASSEMBLED_DOCS = \
	mono-file-formats.tree mono-file-formats.zip  \
	mono-tools.tree mono-tools.zip                \
	monoapi.tree monoapi.zip
endif

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
	-rm -Rf $(srcdir)/html
	-rm -f $(srcdir)/deploy/* $(srcdir)/convert.exe* $(srcdir)/AgilityPack.dll*
	-cd $(srcdir) && rm -f $(ASSEMBLED_DOCS)

monoapi.zip: monoapi.tree
	@test -f $@ || { rm -f $(srcdir)/monoapi.tree && $(MAKE) monoapi.tree; }
monoapi.tree: docs.make Makefile.am toc.xml $(srcdir)/deploy/.stamp
	cd $(srcdir) && cp api-style.css deploy
	cd $(srcdir) && $(MAKE) -f docs.make topdir=$(mcs_topdir_from_srcdir) $@

mono-tools.zip: mono-tools.tree
	@test -f $@ || { rm -f $(srcdir)/mono-tools.tree && $(MAKE) mono-tools.tree; }
mono-tools.tree: docs.make Makefile.am $(srcdir)/deploy/.stamp
	cd $(srcdir) && $(MAKE) -f docs.make topdir=$(mcs_topdir_from_srcdir) $@

mono-file-formats.zip: mono-file-formats.tree
	@test -f $@ || { rm -f $(srcdir)/mono-file-formats.tree && $(MAKE) mono-file-formats.tree; }
mono-file-formats.tree: docs.make Makefile.am $(srcdir)/deploy/.stamp
	cd $(srcdir) && $(MAKE) -f docs.make topdir=$(mcs_topdir_from_srcdir) $@

$(srcdir)/deploy/.stamp: convert.exe Makefile.am
	$(mkdir_p) $(srcdir)/html
	runtimedir=`cd ../runtime && pwd`; export runtimedir; \
	cd $(srcdir) && MONO_PATH=$(mcs_topdir_from_srcdir)/class/lib/net_2_0 perl ./exdoc -h . ../mono/*/*.c
	touch $@

extract: $(srcdir)/deploy/.stamp

convert.exe: convert.cs AgilityPack.dll
	cd $(srcdir) && $(MAKE) PROFILE=net_2_0 -f docs.make topdir=$(mcs_topdir_from_srcdir) convert.exe

AgilityPack.dll:
	cd $(srcdir) && $(MAKE) PROFILE=net_2_0 -f docs.make topdir=$(mcs_topdir_from_srcdir) AgilityPack.dll
