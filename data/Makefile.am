monodir = $(sysconfdir)/mono

EXTRA_DIST = config.in machine.config

mono_DATA = config machine.config

DISTCLEANFILES = config

config: config.in Makefile
	sed -e 's^\@LIBC\@^libc.so.6^g' < $(srcdir)/config.in > config.tmp \
	&& mv config.tmp config
