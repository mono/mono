sourcesdir = $(prefix)/lib/monodoc/sources

sources_DATA = \
	monoapi.source \
	$(ASSEMBLED_DOCS)

ASSEMBLED_DOCS = \
	mono-file-formats.tree mono-file-formats.zip  \
	mono-tools.tree mono-tools.zip                \
	monoapi.tree monoapi.zip

EXTRA_DIST = \
	abc-removal.txt		\
	AgilityPack.dll		\
	aot-compiler.txt	\
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
	exception-handling.txt	\
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
	jit-regalloc		\
	jit-thoughts		\
	jit-trampolines		\
	local-regalloc.txt	\
	magic.diff		\
	mini-doc.txt		\
	mini-porting.txt	\
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
	thread-safety.txt	\
	threading		\
	toc.xml			\
	TODO			\
	tree-mover.txt		\
	unmanaged-calls	

dist-hook:
	$(mkdir_p)  $(distdir)/sources
	$(mkdir_p)  $(distdir)/svgs
	cp sources/*  $(distdir)/sources
	cp svgs/*     $(distdir)/svgs

clean-local:
	-rm -Rf $(srcdir)/html
	-rm -f $(srcdir)/deploy/* $(srcdir)/convert.exe*
	-cd $(srcdir) && rm -f $(ASSEMBLED_DOCS)

monoapi.zip : monoapi.tree
monoapi.tree: docs.make Makefile.am toc.xml deploy/.stamp
	cp api-style.css deploy
	cd $(srcdir) && $(MAKE) -f $< topdir=$(mcs_topdir_from_srcdir) $@

mono-tools.zip : mono-tools.tree
mono-tools.tree: docs.make Makefile.am deploy/.stamp
	cd $(srcdir) && $(MAKE) -f $< topdir=$(mcs_topdir_from_srcdir) $@

mono-file-formats.zip : mono-file-formats.tree
mono-file-formats.tree: docs.make Makefile.am deploy/.stamp
	cd $(srcdir) && $(MAKE) -f $< topdir=$(mcs_topdir_from_srcdir) $@

$(srcdir)/deploy/.stamp: convert.exe Makefile.am
	$(mkdir_p) $(srcdir)/html
	runtimedir=`cd ../runtime && pwd`; export runtimedir; \
	cd $(srcdir) && MONO_PATH=$(mcs_topdir_from_srcdir)/class/lib/net_2_0 perl ./exdoc -h . ../mono/*/*.c
	touch $@

extract: $(srcdir)/deploy/.stamp

convert.exe: docs.make convert.cs Makefile.am
	cd $(srcdir) && $(MAKE) -f docs.make topdir=$(mcs_topdir_from_srcdir) convert.exe

