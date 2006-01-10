bin_SCRIPTS = $(scripts_1_0) $(scripts_1_0_umask) $(scripts_2_0) mono-service $(scripts_rpmhelpers)

scripts_1_0 = \
	al			\
	caspol			\
	cert2spc		\
	certmgr			\
	chktrust		\
	cilc			\
	disco			\
	dtd2xsd			\
	gacutil			\
	genxs			\
	ilasm			\
	macpack			\
	mbas			\
	mcs			\
	mjs			\
	mkbundle		\
	monop			\
	mono-shlib-cop 		\
	mozroots		\
	nunit-console		\
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
	gmcs			\
	ilasm2			\
	monop2 			\
	nunit-console2		\
	wsdl2			\
	xbuild

scripts_rpmhelpers = mono-find-provides mono-find-requires

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA= mono-nunit.pc

CLEANFILES = $(scripts_1_0) $(scripts_1_0_umask) $(scripts_2_0) gmcs mono-service
DISTCLEANFILES = $(pkgconfig_DATA) $(scripts_rpmhelpers)

EXTRA_DIST =                   \
	script.in              \
	script_umask.in        \
	mono-service.in        \
	mono-nunit.pc.in       \
	mono-find-provides.in  \
	mono-find-requires.in

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

REWRITE = sed \
	-e 's,@''bindir@,$(bindir),g'				\
	-e 's,@''plat_bindir@,$(plat_bindir),g'			\
	-e 's,@''mono_instdir@,$(mono_instdir),g'		\
	-e 's,@''gtkdir@,$(gtkdir),g'				\
	-e 's,@''mono_interp@,$(mono_interp),g'

REWRITE1 = $(REWRITE) -e 's,@''framework_version@,1.0,g'
REWRITE2 = $(REWRITE) -e 's,@''framework_version@,2.0,g'

mono-service: mono-service.in Makefile
	$(REWRITE1) -e 's,@''exe_name@,$@,g' $(srcdir)/mono-service.in > $@.tmp
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
