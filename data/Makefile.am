monodir = $(sysconfdir)/mono

EXTRA_DIST = config.in

mono_DATA = config

config: config.in Makefile
	sed -e 's^\@LIBC\@^libc.so.6^g' < $(srcdir)/config.in > config.tmp \
	&& mv config.tmp config
