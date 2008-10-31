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

CLEANFILES = \
	convert.exe* \
	deploy/.stamp \
	$(wildcard deploy/*) \
	$(ASSEMBLED_DOCS)

clean-local:
	-rm -Rf html

monoapi.tree monoapi.zip: monoapi.make Makefile.am toc.xml deploy/.stamp
	cp api-style.css deploy
	$(MAKE) -f $< topdir=$(mcs_topdir) assemble

deploy/.stamp: convert.exe Makefile.am
	$(mkdir_p) html
	perl exdoc -h . ../mono/*/*.c
	touch $@

extract : deply/.stamp

convert.exe: monoapi.make convert.cs Makefile.am
	$(MAKE) -f $< topdir=$(mcs_topdir) convert.exe

