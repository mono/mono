
AM_CPPFLAGS = -I$(top_srcdir) -I$(top_srcdir)/mono $(LIBGC_CPPFLAGS) $(GLIB_CFLAGS) $(SHARED_CFLAGS)

if DISABLE_EXECUTABLES
bin_PROGRAMS =
else
if DISABLE_LIBRARIES
bin_PROGRAMS =
else
if SUPPORT_SGEN
bin_PROGRAMS = pedump
endif
endif
endif

pedump_SOURCES =		\
	pedump.c

pedump_LDADD = 			\
	$(top_builddir)/mono/metadata/libmonoruntimesgen-static.la \
	$(top_builddir)/mono/sgen/libmonosgen-static.la \
	$(top_builddir)/mono/io-layer/libwapi.la	\
	$(top_builddir)/mono/utils/libmonoutils.la \
	$(LLVM_LIBS)			\
	$(LLVM_LDFLAGS)			\
	$(GLIB_LIBS)			\
	$(LIBICONV)

if PLATFORM_DARWIN
pedump_LDFLAGS=-framework CoreFoundation -framework Foundation
endif
