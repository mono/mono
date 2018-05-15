
AM_CPPFLAGS = $(GLIB_CFLAGS)

glib_libs = $(top_builddir)/mono/eglib/libeglib.la

lib_LTLIBRARIES = libikvm-native.la

libikvm_native_la_SOURCES = jni.c os.c jni.h

libikvm_native_la_LDFLAGS = -avoid-version
libikvm_native_la_LIBADD = $(glib_libs)
