#
# Scripts that we install
#
# Starting with Mono 2.0, the majority of the tools default to the 
# 2.0 profile.  Commands that support the 1.0 toolchain and must
# run under 1.0 have the number 1 appended to their command names
# (for example mcs1 is the C# 1.0 compiler) and are listed in the
# scripts_1_0 compat variable.
# 
# To preserve compatibility with old Makefiles and tools we keep the
# command names that had the suffix 2 (like wsdl2)
#
if USE_BATCH_FILES
SCRIPT_IN=script.bat.in
FILTER=sed -e 's,\.bat\.exe,\.exe,g' -e 's,/,\\,g'
SCRIPT_SUFFIX=.bat
else
SCRIPT_IN=script.in
FILTER=cat
SCRIPT_SUFFIX=
endif

COMPAT_1_0_SUFFIX=1$(SCRIPT_SUFFIX)
COMPAT_2_0_SUFFIX=2$(SCRIPT_SUFFIX)
COMPAT_4_0_SUFFIX=4$(SCRIPT_SUFFIX)

bin_SCRIPTS = \
	$(scripts_4_0_umask)	\
	$(scripts_defaults)	\
	$(scripts_2_0)		\
	$(scripts_service)	\
	$(scripts_rpmhelpers)	\
	$(MDOC_SUBCOMMANDS)	\
	$(MDOC_COMPAT)		\
	mono-test-install	\
	peverify		\
	mcs			\
	csc			\
	vbc			\
	csi			\
	mono-package-runtime	\
	mono-heapviz		\
	$(scripts_mono_configuration_crypto)

if INSTALL_4_x
bin_SCRIPTS += $(scripts_4_0)
bin_SCRIPTS += dmcs
bin_SCRIPTS += xbuild
endif

scripts_4_0_umask = \
	makecert$(SCRIPT_SUFFIX)		\
	sn$(SCRIPT_SUFFIX)

deprecated = 					\
	cilc(SCRIPT_SUFFIX)			\
	mjs(SCRIPT_SUFFIX)

scripts_2_0 = \
	al2$(SCRIPT_SUFFIX)			\
	gacutil2$(SCRIPT_SUFFIX)		\
	monop2$(SCRIPT_SUFFIX) 			\
	resgen2$(SCRIPT_SUFFIX)			\
	wsdl2$(SCRIPT_SUFFIX)

scripts_4_0 = \
	aprofutil$(SCRIPT_SUFFIX)		\
	disco$(SCRIPT_SUFFIX)			\
	dtd2xsd$(SCRIPT_SUFFIX)			\
	dtd2rng$(SCRIPT_SUFFIX)			\
	installvst$(SCRIPT_SUFFIX)		\
	genxs$(SCRIPT_SUFFIX)			\
	macpack$(SCRIPT_SUFFIX)			\
	mdoc$(SCRIPT_SUFFIX)                    \
	mono-cil-strip$(SCRIPT_SUFFIX)		\
	soapsuds$(SCRIPT_SUFFIX)		\
	caspol$(SCRIPT_SUFFIX)			\
	cert-sync$(SCRIPT_SUFFIX)		\
	cert2spc$(SCRIPT_SUFFIX)		\
	certmgr$(SCRIPT_SUFFIX)			\
	chktrust$(SCRIPT_SUFFIX)		\
	crlupdate$(SCRIPT_SUFFIX)		\
	csharp$(SCRIPT_SUFFIX)			\
	httpcfg$(SCRIPT_SUFFIX)			\
	illinkanalyzer$(SCRIPT_SUFFIX)		\
	lc$(SCRIPT_SUFFIX)			\
	mconfig$(SCRIPT_SUFFIX)			\
	mod$(SCRIPT_SUFFIX)			\
	monolinker$(SCRIPT_SUFFIX)		\
	mono-api-info$(SCRIPT_SUFFIX) 		\
	mono-api-html$(SCRIPT_SUFFIX) 		\
	mono-shlib-cop$(SCRIPT_SUFFIX) 		\
	mozroots$(SCRIPT_SUFFIX)		\
	permview$(SCRIPT_SUFFIX)		\
	pdb2mdb$(SCRIPT_SUFFIX)			\
	xsd$(SCRIPT_SUFFIX)			\
	mono-xmltool$(SCRIPT_SUFFIX)		\
	setreg$(SCRIPT_SUFFIX)			\
	secutil$(SCRIPT_SUFFIX)			\
	signcode$(SCRIPT_SUFFIX)		\
	sgen$(SCRIPT_SUFFIX)			\
	sqlmetal$(SCRIPT_SUFFIX)		\
	sqlsharp$(SCRIPT_SUFFIX)		\
	svcutil$(SCRIPT_SUFFIX)			\
	ccrewrite$(SCRIPT_SUFFIX)		\
	cccheck$(SCRIPT_SUFFIX)			\
	mdbrebase$(SCRIPT_SUFFIX)		\
	ikdasm$(SCRIPT_SUFFIX)			\
	mono-symbolicate$(SCRIPT_SUFFIX)

MDOC_SUBCOMMANDS = \
	mdoc-assemble \
	mdoc-export-html \
	mdoc-export-msxdoc \
	mdoc-update \
	mdoc-validate

MDOC_COMPAT = \
	mdassembler		\
	mdvalidater		\
	monodocer		\
	monodocs2html		\
	monodocs2slashdoc

#
# these are the new defaults, 2.0 profile-based commands
#
# we can move scripts_1_0 scripts here as we see fit, if we need to
# keep a 1.0 command available, we should additionally put it on
# the scripts_1_0_compat list
#
scripts_defaults = 		\
	al$(SCRIPT_SUFFIX)			\
	ilasm$(SCRIPT_SUFFIX)			\
	gacutil$(SCRIPT_SUFFIX)			\
	mkbundle$(SCRIPT_SUFFIX)		\
	monop$(SCRIPT_SUFFIX)			\
	resgen$(SCRIPT_SUFFIX)			\
	wsdl$(SCRIPT_SUFFIX)

scripts_service = mono-service mono-service2
scripts_rpmhelpers = mono-find-provides mono-find-requires
scripts_mono_configuration_crypto = mono-configuration-crypto$(SCRIPT_SUFFIX)

CLEANFILES = $(scripts_mono_configuration_crypto) $(scripts_4_0_umask) $(scripts_2_0) $(scripts_defaults) $(scripts_4_0) mono-service mono-service2 mono-find-provides mono-find-requires mod $(MDOC_SUBCOMMANDS)
DISTCLEANFILES = $(pkgconfig_DATA) $(scripts_rpmhelpers)

EXTRA_DIST =			\
	script.in		\
	script.bat.in		\
	script_umask.in		\
	script_umask.bat.in	\
	mod.in			\
	mono-service.in		\
	mono-find-provides.in	\
	mono-find-requires.in	\
	peverify.in			\
	xbuild.in			\
	update_submodules.sh	\
	mcs.in				\
	csc.in				\
	vbc.in				\
	dmcs.in				\
	csi.in			\
	mono-package-runtime	\
	mono-test-install	\
	mono-heapviz		\
	$(MDOC_COMPAT)		\
	mono-configuration-crypto.in	\
	submodules/versions.mk	\
	submodules/versions.py

mono_interp = mono

if HOST_WIN32
if CROSS_COMPILING
plat_bindir = $(bindir)
mono_instdir = $(prefix)/lib/mono
else
plat_bindir = $(shell cygpath -m $(libdir))
mono_instdir = $(shell cygpath -m $(libdir))/mono
endif
else
plat_bindir = $(bindir)
mono_instdir = $(prefix)/lib/mono
endif


REWRITE_COMMON = sed \
	-e 's,@''bindir@,$(bindir),g'				\
	-e 's,@''plat_bindir@,$(plat_bindir),g'			\
	-e 's,@''mono_instdir@,$(mono_instdir),g'		\
	-e 's,@''gtkdir@,$(gtkdir),g'

REWRITE = $(REWRITE_COMMON) -e 's,@''mono_interp@,$(mono_interp),g'
REWRITE_DEBUG = $(REWRITE_COMMON) -e 's,@''mono_interp@,$(mono_interp) --debug,g'

REWRITE4 = $(REWRITE) -e 's,@''framework_version@,4.5,g'
REWRITE4_DEBUG = $(REWRITE_DEBUG) -e 's,@''framework_version@,4.5,g'

peverify: peverify.in Makefile
	$(REWRITE_COMMON) $(srcdir)/peverify.in > $@.tmp
	mv -f $@.tmp $@

mcs: mcs.in Makefile
	$(REWRITE_COMMON) $(srcdir)/mcs.in > $@.tmp
	mv -f $@.tmp $@

csc: csc.in Makefile
	$(REWRITE_COMMON) $(srcdir)/csc.in > $@.tmp
	mv -f $@.tmp $@

vbc: vbc.in Makefile
	$(REWRITE_COMMON) $(srcdir)/vbc.in > $@.tmp
	mv -f $@.tmp $@

dmcs: dmcs.in Makefile
	$(REWRITE_COMMON) $(srcdir)/dmcs.in > $@.tmp
	mv -f $@.tmp $@

csi: csi.in Makefile
	$(REWRITE_COMMON) $(srcdir)/csi.in > $@.tmp
	mv -f $@.tmp $@

mono-service: mono-service.in Makefile
	$(REWRITE4) -e 's,@''exe_name@,$@,g' $(srcdir)/mono-service.in > $@.tmp
	mv -f $@.tmp $@

mono-service2: mono-service.in Makefile
	$(REWRITE4) -e 's,@''exe_name@,$@,g' $(srcdir)/mono-service.in > $@.tmp
	mv -f $@.tmp $@

xbuild: xbuild.in Makefile
	$(REWRITE_COMMON) $(srcdir)/xbuild.in > $@.tmp
	mv -f $@.tmp $@

#FIXME: this is not the correct rule for mod.bat.
mod$(SCRIPT_SUFFIX): mod.in Makefile
	$(REWRITE4) -e "s,@""exe_name@,mod,g" $(srcdir)/mod.in > $@.tmp
	mv -f $@.tmp $@

#FIXME: this is not the correct rule for bat files.
$(scripts_4_0_umask): script_umask.in Makefile
	n=`echo $@ | sed 's,$(COMPAT_4_0_SUFFIX)$$,,'`; \
	$(REWRITE4) -e "s,@""exe_name@,$$n,g" $(srcdir)/script_umask.in | $(FILTER) > $@.tmp
	mv -f $@.tmp $@

$(scripts_defaults): $(SCRIPT_IN) Makefile
	n=`echo $@ | sed 's,$(COMPAT_4_0_SUFFIX)$$,,'`; \
	$(REWRITE4) -e "s,@""exe_name@,$$n,g" $(srcdir)/$(SCRIPT_IN) | $(FILTER) > $@.tmp
	mv -f $@.tmp $@

$(scripts_2_0): $(SCRIPT_IN) Makefile
	n=`echo $@ | sed 's,$(COMPAT_2_0_SUFFIX)$$,,'`; \
	$(REWRITE4) -e "s,@""exe_name@,$$n,g" $(srcdir)/$(SCRIPT_IN) | $(FILTER) > $@.tmp
	mv -f $@.tmp $@

$(scripts_4_0): $(SCRIPT_IN) Makefile
	n=`echo $@ | sed 's,$(COMPAT_4_0_SUFFIX)$$,,'`; \
	$(REWRITE4) -e "s,@""exe_name@,$$n,g" $(srcdir)/$(SCRIPT_IN) | $(FILTER) > $@.tmp
	mv -f $@.tmp $@

$(scripts_mono_configuration_crypto): mono-configuration-crypto.in
	$(REWRITE4)  -e "s,@exe_name@,mono-configuration-crypto,g" $(srcdir)/mono-configuration-crypto.in > $@.tmp
	mv -f $@.tmp $@

$(MDOC_SUBCOMMANDS): Makefile
	for script in $(MDOC_SUBCOMMANDS) ; do                  \
		cmd=`echo $$script | sed 's/mdoc-//'` ;         \
		echo "#!/bin/sh" > $$script ;                   \
		echo "exec mdoc $$cmd \"\$$@\"" >> $$script;    \
	done

