EXTRA_DIST = mcs.in mbas.in ilasm.in cilc.in monoresgen.in resgen.in secutil.in sqlsharp.in xsd.in wsdl2.in wsdl.in genxs.in soapsuds.in chktrust.in signcode.in disco.in sn.in al.in cert2spc.in setreg.in makecert.in certmgr.in monop.in gacutil.in gmcs.in mono-find-provides.in mono-find-requires.in

bin_SCRIPTS = mcs mbas ilasm cilc monoresgen resgen secutil sqlsharp xsd wsdl wsdl2 al genxs soapsuds chktrust signcode disco sn cert2spc setreg makecert certmgr monop gacutil gmcs mono-find-requires mono-find-provides

CLEANFILES = $(bin_SCRIPTS)

if USE_JIT
MONO_INTERP = mono
else
MONO_INTERP = mint
endif

if PLATFORM_WIN32
plat_bindir = $(shell cygpath -m $(libdir))
mono_one_instdir = $(shell cygpath -m $(libdir))/mono/1.0
monO_two_instdir = $(shell cygpath -m $(libdir))/mono/2.0
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
	-e 's,@''mono_interp@,$(MONO_INTERP),g'

$(bin_SCRIPTS): Makefile.am
	$(REWRITE) $(srcdir)/$@.in > $@.tmp
	mv $@.tmp $@

mcs: mcs.in
gmcs: gmcs.in
mbas: mbas.in
ilasm: ilasm.in
cilc: cilc.in
monoresgen: monoresgen.in
resgen: resgen.in
secutil: secutil.in
sqlsharp: sqlsharp.in
xsd: xsd.in
wsdl: wsdl.in
wsdl2: wsdl2.in
al: al.in
genxs: genxs.in
disco: disco.in
soapsuds: soapsuds.in
chktrust: chktrust.in
signcode: signcode.in
sn: sn.in
cert2spc: cert2spc.in
setreg: setreg.in
makecert: makecert.in
certmgr: certmgr.in
monop: monop.in
gacutil: gacutil.in
mono-find-provides: mono-find-provides.in
mono-find-requires: mono-find-requires.in
