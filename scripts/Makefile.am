EXTRA_DIST = mcs.in mbas.in ilasm.in cilc.in monoresgen.in secutil.in sqlsharp.in xsd.in

bin_SCRIPTS = mcs mbas ilasm cilc monoresgen secutil sqlsharp xsd

CLEANFILES = mcs mbas ilasm cilc monoresgen secutil sqlsharp xsd

if X86
MONO_INTERP = mono
else
MONO_INTERP = mint
endif

mcs: mcs.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' < $(srcdir)/mcs.in > mcs.tmp \
	&& mv mcs.tmp mcs 


mbas: mbas.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' < $(srcdir)/mbas.in > mbas.tmp \
	&& mv mbas.tmp mbas 

ilasm: ilasm.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' < $(srcdir)/ilasm.in > ilasm.tmp \
	&& mv ilasm.tmp ilasm

cilc: cilc.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' < $(srcdir)/cilc.in > cilc.tmp \
	&& mv cilc.tmp cilc

monoresgen: monoresgen.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' < $(srcdir)/monoresgen.in > monoresgen.tmp \
	&& mv monoresgen.tmp monoresgen

secutil: secutil.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' < $(srcdir)/secutil.in > secutil.tmp \
	&& mv secutil.tmp secutil

sqlsharp: sqlsharp.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' < $(srcdir)/sqlsharp.in > sqlsharp.tmp \
	&& mv sqlsharp.tmp sqlsharp

xsd: xsd.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' < $(srcdir)/xsd.in > xsd.tmp \
	&& mv xsd.tmp xsd
