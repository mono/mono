
AM_CPPFLAGS = $(GMODULE_CFLAGS)

lib_LTLIBRARIES = libikvm-native.la

libikvm_native_la_SOURCES = jni.c os.c jni.h

libikvm_native_la_LDFLAGS = -avoid-version
libikvm_native_la_LIBADD = $(GMODULE_LIBS)
