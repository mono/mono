EXTRA_DIST = mcs.in mbas.in ilasm.in cilc.in

bin_SCRIPTS = mcs mbas ilasm cilc

CLEANFILES = mcs mbas ilasm cilc

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
