bin_SCRIPTS = \
	$(scripts_1_0)		\
	$(scripts_1_0_umask)	\
	$(scripts_2_0)		\
	$(scripts_2_1)		\
	$(scripts_service)	\
	$(scripts_nunit)	\
	$(scripts_rpmhelpers)	\
	mono-test-install

scripts_1_0 = \
	al			\
	caspol			\
	cert2spc		\
	certmgr			\
	chktrust		\
	cilc			\
	disco			\
	dtd2xsd			\
	dtd2rng			\
	gacutil			\
	genxs			\
	ilasm			\
	installvst		\
	macpack			\
	mbas			\
	mcs			\
	mjs			\
	mkbundle		\
	monop			\
	monolinker		\
	mono-api-diff 		\
	mono-api-info 		\
	mono-shlib-cop 		\
	mono-xmltool		\
	mozroots		\
	permview		\
	prj2make		\
	resgen			\
	secutil			\
	setreg			\
	signcode		\
	soapsuds		\
	sqlsharp		\
	wsdl			\
	xsd

scripts_1_0_umask = \
	makecert		\
	sn

scripts_2_0 = \
	al2			\
	gmcs			\
	httpcfg			\
	ilasm2			\
	mconfig			\
	mkbundle2		\
	mono-api-info2 		\
	monop2 			\
	resgen2			\
	wsdl2			\
	sgen			\
	xbuild

scripts_2_1 = smcs
scripts_service = mono-service mono-service2
scripts_nunit = nunit-console nunit-console2
scripts_rpmhelpers = mono-find-provides mono-find-requires

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA= mono-nunit.pc

CLEANFILES = $(scripts_1_0) $(scripts_1_0_umask) $(scripts_2_0) $(scripts_2_1) gmcs mono-service mono-service2
DISTCLEANFILES = $(pkgconfig_DATA) $(scripts_rpmhelpers)

EXTRA_DIST =                   \
	script.in              \
	script_umask.in        \
	smcs.in			\
	mono-service.in        \
	mono-nunit.pc.in       \
	mono-find-provides.in  \
	mono-find-requires.in	\
	mono-test-install

if USE_JIT
mono_interp = mono
else
mono_interp = mint
endif

if PLATFORM_WIN32
plat_bindir = $(shell cygpath -m $(libdir))
mono_instdir = $(shell cygpath -m $(libdir))/mono
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
REWRITE1_DEBUG = $(REWRITE_DEBUG) -e 's,@''framework_version@,1.0,g'
REWRITE2_DEBUG = $(REWRITE_DEBUG) -e 's,@''framework_version@,2.0,g'

mono-service: mono-service.in Makefile
	$(REWRITE1) -e 's,@''exe_name@,$@,g' $(srcdir)/mono-service.in > $@.tmp
	mv $@.tmp $@

mono-service2: mono-service.in Makefile
	$(REWRITE2) -e 's,@''exe_name@,$@,g' $(srcdir)/mono-service.in > $@.tmp
	mv $@.tmp $@

nunit-console: script.in Makefile
	$(REWRITE1_DEBUG) -e 's,@''exe_name@,nunit-console,g' $(srcdir)/script.in > $@.tmp
	mv $@.tmp $@

nunit-console2: script.in Makefile
	$(REWRITE2_DEBUG) -e 's,@''exe_name@,nunit-console,g' $(srcdir)/script.in > $@.tmp
	mv $@.tmp $@

smcs: smcs.in Makefile
	$(REWRITE2_1) -e "s,@""exe_name@,smcs,g" $(srcdir)/smcs.in > $@.tmp
	mv $@.tmp $@

$(scripts_1_0): script.in Makefile
	$(REWRITE1) -e 's,@''exe_name@,$@,g' $(srcdir)/script.in > $@.tmp
	mv $@.tmp $@

$(scripts_1_0_umask): script_umask.in Makefile
	$(REWRITE1) -e 's,@''exe_name@,$@,g' $(srcdir)/script_umask.in > $@.tmp
	mv $@.tmp $@

$(scripts_2_0): script.in Makefile
	n=`echo $@ | sed 's,2$$,,'`; \
	$(REWRITE2) -e "s,@""exe_name@,$$n,g" $(srcdir)/script.in > $@.tmp
	mv $@.tmp $@
