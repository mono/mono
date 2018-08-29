
AM_CPPFLAGS = -I$(top_srcdir) -I$(top_srcdir)/mono $(LIBGC_CPPFLAGS) $(GLIB_CFLAGS) $(SHARED_CFLAGS)

glib_libs = $(top_builddir)/mono/eglib/libeglib.la

if DISABLE_EXECUTABLES
bin_PROGRAMS =
else
if DISABLE_LIBRARIES
bin_PROGRAMS =
else
if SUPPORT_SGEN
bin_PROGRAMS = pedump
noinst_LIBRARIES = libpedump.a
endif
endif
endif

CFLAGS = $(filter-out @CXX_REMOVE_CFLAGS@, @CFLAGS@)
libpedump_a_CFLAGS = @CXX_ADD_CFLAGS@

pedump_SOURCES =
libpedump_a_SOURCES = pedump.c
pedump_LDADD = 			\
	libpedump_a-pedump.$(OBJEXT) \
	$(top_builddir)/mono/metadata/libmonoruntimesgen.la \
	$(top_builddir)/mono/sgen/libmonosgen.la \
	$(top_builddir)/mono/utils/libmonoutils.la \
	$(LLVM_LIBS)			\
	$(LLVM_LDFLAGS)			\
	$(glib_libs)

if HOST_DARWIN
pedump_LDFLAGS=-framework CoreFoundation -framework Foundation
endif

#Helper target to rebuild metadata as well, it's useful when working on the verifier as its source still on metadata
md:
	make -C ../../mono/metadata all
	make -C . all
