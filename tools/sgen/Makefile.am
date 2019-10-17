if !ENABLE_MSVC_ONLY

bin_PROGRAMS = sgen-grep-binprot

AM_CPPFLAGS =  $(GLIB_CFLAGS) -I$(top_srcdir)

glib_libs = $(top_builddir)/mono/eglib/libeglib.la

noinst_LIBRARIES = libsgen-grep-binprot.a libsgen-grep-binprot32p.a libsgen-grep-binprot64p.a

# Main is in a library so it can be C++.
noinst_LIBRARIES += libmain.a

endif # !ENABLE_MSVC_ONLY

# Default to C so the executable is linked as C and does not use libstdc++.
# Also the -xc++ flag mishandles .o and .a files.
CFLAGS = $(filter-out @CXX_REMOVE_CFLAGS@, @CFLAGS@)

if !ENABLE_MSVC_ONLY

# Libraries are C++ if enabled.
libmain_a_CFLAGS = @CXX_ADD_CFLAGS@
libsgen_grep_binprot_a_CFLAGS = @CXX_ADD_CFLAGS@
libsgen_grep_binprot32p_a_CFLAGS = @CXX_ADD_CFLAGS@
libsgen_grep_binprot64p_a_CFLAGS = @CXX_ADD_CFLAGS@

libsgen_grep_binprot_a_SOURCES = sgen-grep-binprot.c sgen-grep-binprot.h
libsgen_grep_binprot_a_CPPFLAGS = $(GLIB_CFLAGS) -I$(top_srcdir)
libsgen_grep_binprot32p_a_SOURCES = sgen-grep-binprot.c sgen-grep-binprot.h
libsgen_grep_binprot32p_a_CPPFLAGS = $(GLIB_CFLAGS) -I$(top_srcdir) -DBINPROT_SIZEOF_VOID_P=4 -DBINPROT_HAS_HEADER
libsgen_grep_binprot64p_a_SOURCES = sgen-grep-binprot.c sgen-grep-binprot.h
libsgen_grep_binprot64p_a_CPPFLAGS = $(GLIB_CFLAGS) -I$(top_srcdir) -DBINPROT_SIZEOF_VOID_P=8 -DBINPROT_HAS_HEADER

# Executable has no source files, just links to object files and libraries.
sgen_grep_binprot_SOURCES =

libmain_a_SOURCES = \
	sgen-grep-binprot-main.c	\
	sgen-entry-stream.c	\
	sgen-entry-stream.h

# Link to the libmain object files instead of library for higher fidelity with old behavior.
sgen_grep_binprot_LDADD = \
	libmain_a-sgen-grep-binprot-main.$(OBJEXT) \
	libmain_a-sgen-entry-stream.$(OBJEXT) \
	$(glib_libs) libsgen-grep-binprot.a libsgen-grep-binprot32p.a libsgen-grep-binprot64p.a

endif # !ENABLE_MSVC_ONLY
