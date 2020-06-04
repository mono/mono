MAKEFLAGS := $(MAKEFLAGS) --no-builtin-rules

AM_CPPFLAGS = -I$(top_srcdir) -I$(top_srcdir)/mono $(GLIB_CFLAGS) $(SHARED_CFLAGS)

CFLAGS := $(filter-out @CXX_REMOVE_CFLAGS@, @CFLAGS@)

noinst_LTLIBRARIES = libmono-culture.la

libmono_culture_la_SOURCES = \
	culture-info.h	\
	culture-info-tables.h	\
	locales.c
libmono_culture_la_CPPFLAGS = $(AM_CPPFLAGS) @CXX_ADD_CFLAGS@
