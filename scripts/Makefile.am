EXTRA_DIST = script.in script2.in

bin2dir = $(bindir)

bin_SCRIPTS = \
	al			\
	cert2spc		\
	certmgr			\
	chktrust		\
	cilc			\
	disco			\
	gacutil			\
	genxs			\
	ilasm			\
	makecert		\
	mbas			\
	mcs			\
	mkbundle		\
	mono-find-provides	\
	mono-find-requires	\
	monop			\
	monoresgen		\
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

CLEANFILES = $(bin_SCRIPTS) $(bin2_SCRIPTS)

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
mono_one_instdir = $(libdir)/mono/1.0
mono_two_instdir = $(libdir)/mono/2.0
endif

REWRITE = sed \
	-e 's,@''bindir@,$(bindir),g'				\
	-e 's,@''plat_bindir@,$(plat_bindir),g'			\
	-e 's,@''mono_one_instdir@,$(mono_one_instdir),g'	\
	-e 's,@''mono_two_instdir@,$(mono_two_instdir),g'	\
	-e 's,@''gtkdir@,$(gtkdir),g'				\
	-e 's,@''exe_file@,$@.exe,g'				\
	-e 's,@''mono_interp@,$(MONO_INTERP),g'

$(bin_SCRIPTS): script.in Makefile.am
	$(REWRITE) $(srcdir)/script.in > $@.tmp
	mv $@.tmp $@

$(bin2_SCRIPTS): script2.in Makefile.am
	$(REWRITE) $(srcdir)/script2.in > $@.tmp
	mv $@.tmp $@

