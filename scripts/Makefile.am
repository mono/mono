EXTRA_DIST = mcs.in

bin_SCRIPTS = mcs

CLEANFILES = mcs

mcs: mcs.in
	sed -e 's^\@bindir\@^$(bindir)^g' < $(srcdir)/mcs.in > mcs.tmp \
	&& mv mcs.tmp mcs 
