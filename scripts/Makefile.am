EXTRA_DIST = script.in script2.in mono-service.in mono-nunit.pc.in
DISTCLEANFILES = mono-nunit.pc

bin2dir = $(bindir)
servicedir = $(bindir)

bin_SCRIPTS = \
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
	makecert		\
	macpack			\
	mbas			\
	mcs			\
	mjs			\
	mkbundle		\
	mono-find-provides	\
	mono-find-requires	\
	monop			\
	permview		\
	prj2make		\
	resgen			\
	secutil			\
	setreg			\
	signcode		\
	sn			\
	soapsuds		\
	sqlsharp		\
	wsdl			\
	xsd

bin2_SCRIPTS = gmcs wsdl2

service_SCRIPTS = mono-service

CLEANFILES = $(bin_SCRIPTS) $(bin2_SCRIPTS) mono-service

if USE_JIT
MONO_INTERP = mono
else
MONO_INTERP = mint
endif

if PLATFORM_WIN32
plat_bindir = $(shell cygpath -m $(libdir))
mono_one_instdir = $(shell cygpath -m $(libdir))/mono/1.0
mono_two_instdir = $(shell cygpath -m $(libdir))/mono/2.0
else
plat_bindir = $(bindir)
mono_one_instdir = $(prefix)/lib/mono/1.0
mono_two_instdir = $(prefix)/lib/mono/2.0
endif

REWRITE = sed \
	-e 's,@''bindir@,$(bindir),g'				\
	-e 's,@''plat_bindir@,$(plat_bindir),g'			\
	-e 's,@''mono_one_instdir@,$(mono_one_instdir),g'	\
	-e 's,@''mono_two_instdir@,$(mono_two_instdir),g'	\
	-e 's,@''gtkdir@,$(gtkdir),g'				\
	-e 's,@''exe_file@,$@.exe,g'				\
	-e 's,@''mono_interp@,$(MONO_INTERP),g'

mono-service: mono-service.in Makefile
	$(REWRITE) $(srcdir)/mono-service.in > $@.tmp
	mv $@.tmp $@

$(bin_SCRIPTS): script.in Makefile
	$(REWRITE) $(srcdir)/script.in > $@.tmp
	mv $@.tmp $@

$(bin2_SCRIPTS): script2.in Makefile
	$(REWRITE) $(srcdir)/script2.in > $@.tmp
	mv $@.tmp $@

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA= mono-nunit.pc

