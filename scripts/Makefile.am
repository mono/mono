EXTRA_DIST = mcs.in mbas.in ilasm.in cilc.in monoresgen.in secutil.in sqlsharp.in xsd.in wsdl.in genxs.in soapsuds.in chktrust.in signcode.in disco.in sn.in al.in cert2spc.in setreg.in makecert.in certmgr.in

bin_SCRIPTS = mcs mbas ilasm cilc monoresgen secutil sqlsharp xsd wsdl al genxs soapsuds chktrust signcode disco sn cert2spc setreg makecert certmgr

CLEANFILES = mcs mbas ilasm cilc monoresgen secutil sqlsharp xsd wsdl al genxs soapsuds chktrust signcode disco sn cert2spc setreg makecert certmgr

if X86
MONO_INTERP = mono
else
MONO_INTERP = mint
endif

if PLATFORM_WIN32
plat_bindir = $(shell cygpath -m $(bindir))
else
plat_bindir = $(bindir)
endif

mcs: mcs.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/mcs.in > mcs.tmp \
	&& mv mcs.tmp mcs 

mbas: mbas.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/mbas.in > mbas.tmp \
	&& mv mbas.tmp mbas 

ilasm: ilasm.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/ilasm.in > ilasm.tmp \
	&& mv ilasm.tmp ilasm

cilc: cilc.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/cilc.in > cilc.tmp \
	&& mv cilc.tmp cilc

monoresgen: monoresgen.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/monoresgen.in > monoresgen.tmp \
	&& mv monoresgen.tmp monoresgen

secutil: secutil.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/secutil.in > secutil.tmp \
	&& mv secutil.tmp secutil

sqlsharp: sqlsharp.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/sqlsharp.in > sqlsharp.tmp \
	&& mv sqlsharp.tmp sqlsharp

xsd: xsd.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/xsd.in > xsd.tmp \
	&& mv xsd.tmp xsd

wsdl: wsdl.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/wsdl.in > wsdl.tmp \
	&& mv wsdl.tmp wsdl

al: al.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/al.in > al.tmp \
	&& mv al.tmp al

genxs: genxs.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/genxs.in > genxs.tmp \
	&& mv genxs.tmp genxs

disco: disco.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/disco.in > disco.tmp \
	&& mv disco.tmp disco

soapsuds: soapsuds.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/soapsuds.in > soapsuds.tmp \
	&& mv soapsuds.tmp soapsuds

chktrust: chktrust.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/chktrust.in > chktrust.tmp \
	&& mv chktrust.tmp chktrust

signcode: signcode.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/signcode.in > signcode.tmp \
	&& mv signcode.tmp signcode

sn: sn.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/sn.in > sn.tmp \
	&& mv sn.tmp sn

cert2spc: cert2spc.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/cert2spc.in > cert2spc.tmp \
	&& mv cert2spc.tmp cert2spc

setreg: setreg.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/setreg.in > setreg.tmp \
	&& mv setreg.tmp setreg

makecert: makecert.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/makecert.in > makecert.tmp \
	&& mv makecert.tmp makecert

certmgr: certmgr.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' -e's^\@plat_bindir\@^$(plat_bindir)^g' < $(srcdir)/certmgr.in > certmgr.tmp \
	&& mv certmgr.tmp certmgr
