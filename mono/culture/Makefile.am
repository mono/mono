MAKEFLAGS := $(MAKEFLAGS) --no-builtin-rules

AM_CPPFLAGS = -I$(top_srcdir) -I$(top_srcdir)/mono $(LIBGC_CPPFLAGS) $(GLIB_CFLAGS) $(SHARED_CFLAGS) $(METADATA_CFLAGS)

noinst_LTLIBRARIES = libmono-culture.la

libmono_culture_la_SOURCES = \
	culture-info.h	\
	culture-info-tables.h	\
	locales.c


