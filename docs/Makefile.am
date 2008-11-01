sourcesdir = $(prefix)/lib/monodoc/sources

sources_DATA = \
	monoapi.source \
	$(ASSEMBLED_DOCS)

ASSEMBLED_DOCS = \
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
	mono_handle_d		\
	monoapi.make		\
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

monoapi.tree monoapi.zip: monoapi.make Makefile.am toc.xml $(srcdir)/deploy/.stamp
	cp $(srcdir)/api-style.css $(srcdir)/deploy/
	cd $(srcdir) && $(MAKE) -f monoapi.make topdir=$(mcs_topdir_from_srcdir) assemble

$(srcdir)/deploy/.stamp: convert.exe Makefile.am
	$(mkdir_p) $(srcdir)/html
	runtimedir=`cd ../runtime && pwd`; export runtimedir; \
	cd $(srcdir) && MONO_PATH=$(mcs_topdir_from_srcdir)/class/lib/net_2_0 perl ./exdoc -h . ../mono/*/*.c
	touch $@

extract: $(srcdir)/deploy/.stamp

convert.exe: monoapi.make convert.cs Makefile.am
	cd $(srcdir) && $(MAKE) -f monoapi.make topdir=$(mcs_topdir_from_srcdir) convert.exe

