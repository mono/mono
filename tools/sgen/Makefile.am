bin_PROGRAMS = sgen-grep-binprot

AM_CPPFLAGS =  $(GLIB_CFLAGS) -I$(top_srcdir)

noinst_LIBRARIES = libsgen-grep-binprot.a libsgen-grep-binprot32p.a libsgen-grep-binprot64p.a
libsgen_grep_binprot_a_SOURCES = sgen-grep-binprot.c sgen-grep-binprot.h
libsgen_grep_binprot_a_CPPFLAGS = $(GLIB_CFLAGS) -I$(top_srcdir)
libsgen_grep_binprot32p_a_SOURCES = sgen-grep-binprot.c sgen-grep-binprot.h
libsgen_grep_binprot32p_a_CPPFLAGS = $(GLIB_CFLAGS) -I$(top_srcdir) -DBINPROT_SIZEOF_VOID_P=4 -DBINPROT_HAS_HEADER
libsgen_grep_binprot64p_a_SOURCES = sgen-grep-binprot.c sgen-grep-binprot.h
libsgen_grep_binprot64p_a_CPPFLAGS = $(GLIB_CFLAGS) -I$(top_srcdir) -DBINPROT_SIZEOF_VOID_P=8 -DBINPROT_HAS_HEADER

sgen_grep_binprot_SOURCES = \
	sgen-grep-binprot-main.c	\
	sgen-entry-stream.c	\
	sgen-entry-stream.h

sgen_grep_binprot_LDADD = \
	$(GLIB_LIBS) $(LIBICONV) libsgen-grep-binprot.a libsgen-grep-binprot32p.a libsgen-grep-binprot64p.a
