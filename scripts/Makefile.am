EXTRA_DIST = mcs.in

bin_SCRIPTS = mcs

CLEANFILES = mcs

if X86
MONO_INTERP = mono
else
MONO_INTERP = mint
endif

mcs: mcs.in
	sed -e 's^\@bindir\@^$(bindir)^g' -e 's^\@mono_interp\@^$(MONO_INTERP)^g' < $(srcdir)/mcs.in > mcs.tmp \
	&& mv mcs.tmp mcs 
