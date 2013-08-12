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
BATCH_IN=script.bat.in
BATCH_FILTER=sed -e 's,\.bat\.exe,\.exe,g' -e 's,/,\\,g'
BATCH_SUFFIX=.bat
SCRIPT_IN=script.in
SCRIPT_FILTER=cat
SCRIPT_SUFFIX=

COMPAT_1_0_SUFFIX=1$(SCRIPT_SUFFIX)
COMPAT_2_0_SUFFIX=2$(SCRIPT_SUFFIX)
COMPAT_4_0_SUFFIX=4$(SCRIPT_SUFFIX)
COMPAT_1_0_BATCH_SUFFIX=1$(BATCH_SUFFIX)
COMPAT_2_0_BATCH_SUFFIX=2$(BATCH_SUFFIX)
COMPAT_4_0_BATCH_SUFFIX=4$(BATCH_SUFFIX)

bin_SCRIPTS = \
	$(scripts_1_0)		\
	$(scripts_1_0_umask)	\
	$(scripts_1_0_compat)	\
	$(scripts_defaults)	\
	$(scripts_2_0)		\
	$(scripts_service)	\
	$(scripts_nunit)	\
	$(scripts_rpmhelpers)	\
	$(MDOC_SUBCOMMANDS)	\
	$(MDOC_COMPAT)		\
	mod$(SCRIPT_SUFFIX)	\
	mono-test-install

bin_BATCHES = \
	$(batches_defaults)	\
	$(batches_2_0)		\
	$(batches_service)	\
	$(batches_nunit)	

if INSTALL_4_0
bin_SCRIPTS += $(scripts_4_0)
endif

scripts_1_0_compat = \
	al1$(SCRIPT_SUFFIX)			\
	genxs1$(SCRIPT_SUFFIX)			\
	ilasm1$(SCRIPT_SUFFIX)			\
	mcs1$(SCRIPT_SUFFIX)			\
	mkbundle1$(SCRIPT_SUFFIX)		\
	gacutil1$(SCRIPT_SUFFIX)			\
	monop1$(SCRIPT_SUFFIX)			\
	resgen1$(SCRIPT_SUFFIX)			\
	wsdl1$(SCRIPT_SUFFIX)

scripts_1_0 = \
	caspol$(SCRIPT_SUFFIX)			\
	cert2spc$(SCRIPT_SUFFIX)		\
	certmgr$(SCRIPT_SUFFIX)			\
	chktrust$(SCRIPT_SUFFIX)		\
	cilc$(SCRIPT_SUFFIX)			\
	disco$(SCRIPT_SUFFIX)			\
	dtd2xsd$(SCRIPT_SUFFIX)			\
	dtd2rng$(SCRIPT_SUFFIX)			\
	installvst$(SCRIPT_SUFFIX)		\
	genxs$(SCRIPT_SUFFIX)			\
	macpack$(SCRIPT_SUFFIX)			\
	mcs$(SCRIPT_SUFFIX)			\
	mjs$(SCRIPT_SUFFIX)			\
	mono-cil-strip$(SCRIPT_SUFFIX)		\
	mozroots$(SCRIPT_SUFFIX)		\
	permview$(SCRIPT_SUFFIX)		\
	prj2make$(SCRIPT_SUFFIX)		\
	secutil$(SCRIPT_SUFFIX)			\
	setreg$(SCRIPT_SUFFIX)			\
	signcode$(SCRIPT_SUFFIX)		\
	soapsuds$(SCRIPT_SUFFIX)		\
	xsd$(SCRIPT_SUFFIX)

scripts_1_0_umask = \
	makecert$(SCRIPT_SUFFIX)		\
	sn$(SCRIPT_SUFFIX)

scripts_2_0 = \
	al2$(SCRIPT_SUFFIX)			\
	csharp$(SCRIPT_SUFFIX)			\
	gacutil2$(SCRIPT_SUFFIX)		\
	gmcs$(SCRIPT_SUFFIX)			\
	httpcfg$(SCRIPT_SUFFIX)			\
	ilasm2$(SCRIPT_SUFFIX)			\
	lc$(SCRIPT_SUFFIX)			\
	mconfig$(SCRIPT_SUFFIX)			\
	mkbundle2$(SCRIPT_SUFFIX)		\
	mdoc$(SCRIPT_SUFFIX)                    \
	monolinker$(SCRIPT_SUFFIX)		\
	mono-api-info$(SCRIPT_SUFFIX) 		\
	mono-shlib-cop$(SCRIPT_SUFFIX) 		\
	monop2$(SCRIPT_SUFFIX) 			\
	resgen2$(SCRIPT_SUFFIX)			\
	pdb2mdb$(SCRIPT_SUFFIX)			\
	wsdl2$(SCRIPT_SUFFIX)			\
	xsd2$(SCRIPT_SUFFIX)			\
	mono-xmltool$(SCRIPT_SUFFIX)		\
	sgen$(SCRIPT_SUFFIX)			\
	sqlmetal$(SCRIPT_SUFFIX)		\
	sqlsharp$(SCRIPT_SUFFIX)		\
	svcutil$(SCRIPT_SUFFIX)			\
	xbuild$(SCRIPT_SUFFIX)

batches_2_0 = \
	al2$(BATCH_SUFFIX)			\
	csharp$(BATCH_SUFFIX)			\
	gacutil2$(BATCH_SUFFIX)		\
	gmcs$(BATCH_SUFFIX)			\
	httpcfg$(BATCH_SUFFIX)			\
	ilasm2$(BATCH_SUFFIX)			\
	lc$(BATCH_SUFFIX)			\
	mconfig$(BATCH_SUFFIX)			\
	mkbundle2$(BATCH_SUFFIX)		\
	mdoc$(BATCH_SUFFIX)                    \
	monolinker$(BATCH_SUFFIX)		\
	mono-api-info$(BATCH_SUFFIX) 		\
	mono-shlib-cop$(BATCH_SUFFIX) 		\
	monop2$(BATCH_SUFFIX) 			\
	resgen2$(BATCH_SUFFIX)			\
	pdb2mdb$(BATCH_SUFFIX)			\
	wsdl2$(BATCH_SUFFIX)			\
	xsd2$(BATCH_SUFFIX)			\
	mono-xmltool$(BATCH_SUFFIX)		\
	sgen$(BATCH_SUFFIX)			\
	sqlmetal$(BATCH_SUFFIX)		\
	sqlsharp$(BATCH_SUFFIX)		\
	svcutil$(BATCH_SUFFIX)			\
	xbuild$(BATCH_SUFFIX)

scripts_4_0 = \
	dmcs$(SCRIPT_SUFFIX)

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

batches_defaults = 		\
	al$(BATCH_SUFFIX)			\
	ilasm$(BATCH_SUFFIX)			\
	gacutil$(BATCH_SUFFIX)			\
	mkbundle$(BATCH_SUFFIX)		\
	monop$(BATCH_SUFFIX)			\
	resgen$(BATCH_SUFFIX)			\
	wsdl$(BATCH_SUFFIX)

scripts_service = mono-service mono-service2
scripts_nunit = nunit-console$(SCRIPT_SUFFIX) nunit-console2$(SCRIPT_SUFFIX)
batches_nunit = nunit-console$(BATCH_SUFFIX) nunit-console2$(BATCH_SUFFIX)
scripts_rpmhelpers = mono-find-provides mono-find-requires

CLEANFILES = $(scripts_1_0) $(scripts_1_0_umask) $(scripts_2_0) $(scripts_defaults) $(batches_2_0) $(batches_defaults) $(scripts_1_0_compat) $(scripts_4_0) mono-service mono-service2 nunit-console nunit-console2 mono-find-provides mono-find-requires mod $(MDOC_SUBCOMMANDS)
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
	mono-test-install	\
	$(MDOC_COMPAT)		\
	patch-quiet.sh

if USE_JIT
mono_interp = mono
else
mono_interp = mint
endif

if PLATFORM_WIN32
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

REWRITE1 = $(REWRITE) -e 's,@''framework_version@,1.0,g'
REWRITE2 = $(REWRITE) -e 's,@''framework_version@,2.0,g'
REWRITE2_1 = $(REWRITE) -e 's,@''framework_version@,2.1,g'
REWRITE4 = $(REWRITE) -e 's,@''framework_version@,4.0,g'
REWRITE1_DEBUG = $(REWRITE_DEBUG) -e 's,@''framework_version@,1.0,g'
REWRITE2_DEBUG = $(REWRITE_DEBUG) -e 's,@''framework_version@,2.0,g'


mono-service: mono-service.in Makefile
	$(REWRITE1) -e 's,@''exe_name@,$@,g' $(srcdir)/mono-service.in > $@.tmp
	mv $@.tmp $@

mono-service2: mono-service.in Makefile
	$(REWRITE2) -e 's,@''exe_name@,$@,g' $(srcdir)/mono-service.in > $@.tmp
	mv $@.tmp $@

nunit-console$(SCRIPT_SUFFIX): $(SCRIPT_IN) Makefile
	$(REWRITE1_DEBUG) -e 's,@''exe_name@,nunit-console,g' $(srcdir)/$(SCRIPT_IN) | $(SCRIPT_FILTER) > $@.tmp
	mv $@.tmp $@

nunit-console$(BATCH_SUFFIX): $(BATCH_IN) Makefile
	$(REWRITE1_DEBUG) -e 's,@''exe_name@,nunit-console,g' $(srcdir)/$(BATCH_IN) | $(BATCH_FILTER) > $@.tmp
	mv $@.tmp $@

nunit-console2$(SCRIPT_SUFFIX): $(SCRIPT_IN) Makefile
	$(REWRITE2_DEBUG) -e 's,@''exe_name@,nunit-console,g' $(srcdir)/$(SCRIPT_IN) | $(SCRIPT_FILTER) > $@.tmp
	mv $@.tmp $@

nunit-console2$(BATCH_SUFFIX): $(BATCH_IN) Makefile
	$(REWRITE2_DEBUG) -e 's,@''exe_name@,nunit-console,g' $(srcdir)/$(BATCH_IN) | $(BATCH_FILTER) > $@.tmp
	mv $@.tmp $@

#FIXME: this is not the correct rule for mod.bat.
mod$(SCRIPT_SUFFIX): mod.in Makefile
	$(REWRITE1) -e "s,@""exe_name@,mod,g" $(srcdir)/mod.in > $@.tmp
	mv $@.tmp $@

$(scripts_1_0): $(SCRIPT_IN) Makefile
	$(REWRITE1) -e 's,@''exe_name@,$@,g' $(srcdir)/$(SCRIPT_IN) | $(SCRIPT_FILTER) > $@.tmp
	mv $@.tmp $@

#FIXME: this is not the correct rule for bat files.
$(scripts_1_0_umask): script_umask.in Makefile
	$(REWRITE1) -e 's,@''exe_name@,$@,g' $(srcdir)/script_umask.in > $@.tmp
	mv $@.tmp $@

$(scripts_1_0_compat): $(SCRIPT_IN) Makefile
	t=`basename $@ $(COMPAT_1_0_SUFFIX)`; $(REWRITE1) -e "s,@exe_name@,$$t,g" $(srcdir)/$(SCRIPT_IN) | $(SCRIPT_FILTER) > $@.tmp
	mv $@.tmp $@

$(scripts_defaults): $(SCRIPT_IN) Makefile
	$(REWRITE2) -e "s,@exe_name@,$@,g" $(srcdir)/$(SCRIPT_IN) | $(SCRIPT_FILTER) > $@.tmp
	mv $@.tmp $@

$(batches_defaults): $(BATCH_IN) Makefile
	$(REWRITE2) -e "s,@exe_name@,$@,g" $(srcdir)/$(BATCH_IN) | $(BATCH_FILTER) > $@.tmp
	mv $@.tmp $@

$(scripts_2_0): $(SCRIPT_IN) Makefile
	n=`echo $@ | sed 's,$(COMPAT_2_0_SUFFIX)$$,,'`; \
	$(REWRITE2) -e "s,@""exe_name@,$$n,g" $(srcdir)/$(SCRIPT_IN) | $(SCRIPT_FILTER) > $@.tmp
	mv $@.tmp $@

$(batches_2_0): $(BATCH_IN) Makefile
	n=`echo $@ | sed 's,$(COMPAT_2_0_BATCH_SUFFIX)$$,,'`; \
	$(REWRITE2) -e "s,@""exe_name@,$$n,g" $(srcdir)/$(BATCH_IN) | $(BATCH_FILTER) > $@.tmp
	mv $@.tmp $@

$(scripts_4_0): $(SCRIPT_IN) Makefile
	n=`echo $@ | sed 's,$(COMPAT_4_0_SUFFIX)$$,,'`; \
	$(REWRITE4) -e "s,@""exe_name@,$$n,g" $(srcdir)/$(SCRIPT_IN) | $(SCRIPT_FILTER) > $@.tmp
	mv $@.tmp $@

$(MDOC_SUBCOMMANDS): Makefile
	for script in $(MDOC_SUBCOMMANDS) ; do                  \
		cmd=`echo $$script | sed 's/mdoc-//'` ;         \
		echo "#!/bin/sh" > $$script ;                   \
		echo "exec mdoc $$cmd \"\$$@\"" >> $$script;    \
	done

all: $(bin_SCRIPTS) $(bin_BATCHES)

