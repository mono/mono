bin_PROGRAMS = sgen-grep-binprot

AM_CPPFLAGS =  $(GLIB_CFLAGS) -I$(top_srcdir)

noinst_LIBRARIES = libsgen-grep-binprot.a libsgen-grep-binprotp.a
libsgen_grep_binprot_a_SOURCES = sgen-grep-binprot.c
libsgen_grep_binprot_a_CPPFLAGS = $(GLIB_CFLAGS) -I$(top_srcdir)
libsgen_grep_binprotp_a_SOURCES = sgen-grep-binprot.c
libsgen_grep_binprotp_a_CPPFLAGS = $(GLIB_CFLAGS) -I$(top_srcdir) -DBINPROT_HAS_HEADER

sgen_grep_binprot_SOURCES = \
	sgen-grep-binprot-main.c	\
	sgen-grep-binprot.c		\
	sgen-entry-stream.c

sgen_grep_binprot_LDADD = \
	$(GLIB_LIBS) $(LIBICONV) libsgen-grep-binprot.a libsgen-grep-binprotp.a
