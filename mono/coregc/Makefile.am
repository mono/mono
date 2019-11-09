include $(top_srcdir)/mk/common.mk

# FIXME
TMPFLAGS=-D_AMD64_ -DBIT64 -D_ASSERTE= -DFEATURE_REDHAWK -DPLATFORM_UNIX

AM_CPPFLAGS = -I$(top_srcdir) -I$(top_srcdir)/mono \
			  -I$(top_srcdir)/external/coreclr/src/gc \
			  -I$(top_srcdir)/external/coreclr/src/gc/env \
			  -I$(top_srcdir)/external/coreclr/src/gc/unix \
			  $(TMPFLAGS) \
			  $(LIBGC_CPPFLAGS) $(GLIB_CFLAGS) $(SHARED_CFLAGS)

if SUPPORT_COREGC
shared_libraries = libmonocoregc.la
endif

noinst_LTLIBRARIES = $(shared_libraries)

monocoregc_sources = \
	../../external/coreclr/src/gc/gceventstatus.cpp \
	../../external/coreclr/src/gc/gceventstatus.h \
	../../external/coreclr/src/gc/gcconfig.cpp \
	../../external/coreclr/src/gc/gcconfig.h \
	../../external/coreclr/src/gc/gccommon.cpp \
	../../external/coreclr/src/gc/gcsvr.cpp \
	../../external/coreclr/src/gc/gcwks.cpp \
	../../external/coreclr/src/gc/handletable.cpp \
	../../external/coreclr/src/gc/handletable.h \
	../../external/coreclr/src/gc/handletablepriv.h \
	../../external/coreclr/src/gc/handletablecore.cpp \
	../../external/coreclr/src/gc/handletablescan.cpp \
	../../external/coreclr/src/gc/objecthandle.cpp \
	../../external/coreclr/src/gc/objecthandle.h \
	../../external/coreclr/src/gc/softwarewritewatch.cpp \
	../../external/coreclr/src/gc/softwarewritewatch.h \
	../../external/coreclr/src/gc/gchandletable.cpp \
	../../external/coreclr/src/gc/gchandletableimpl.h \
	../../external/coreclr/src/gc/gceesvr.cpp \
	../../external/coreclr/src/gc/gceewks.cpp \
	../../external/coreclr/src/gc/gcload.cpp \
	../../external/coreclr/src/gc/handletablecache.cpp \
	../../external/coreclr/src/gc/unix/gcenv.unix.cpp \
	../../external/coreclr/src/gc/unix/events.cpp \
	../../external/coreclr/src/gc/unix/cgroup.cpp \
	../../external/coreclr/src/gc/unix/cgroup.h \
	../../external/coreclr/src/gc/gcscan.cpp \
	../../external/coreclr/src/gc/gcscan.h

#	../../external/coreclr/src/gc/WarningControl.h \
#	../../external/coreclr/src/gc/env \
#	../../external/coreclr/src/gc/gc.cpp \
#	../../external/coreclr/src/gc/gc.h \
#	../../external/coreclr/src/gc/gcdesc.h \
#	../../external/coreclr/src/gc/gcee.cpp \
#	../../external/coreclr/src/gc/gcenv.ee.standalone.inl \
#	../../external/coreclr/src/gc/gcenv.inl \
#	../../external/coreclr/src/gc/gcevent_serializers.h \
#	../../external/coreclr/src/gc/gcevents.h \
#	../../external/coreclr/src/gc/gcimpl.h \
#	../../external/coreclr/src/gc/gcinterface.dac.h \
#	../../external/coreclr/src/gc/gcinterface.dacvars.def \
#	../../external/coreclr/src/gc/gcinterface.ee.h \
#	../../external/coreclr/src/gc/gcinterface.h \
#	../../external/coreclr/src/gc/gcpriv.h \
#	../../external/coreclr/src/gc/gcrecord.h \
#	../../external/coreclr/src/gc/handletable.inl \
#	../../external/coreclr/src/gc/unix/CMakeLists.txt
#	../../external/coreclr/src/gc/unix/config.gc.h.in \
#	../../external/coreclr/src/gc/unix/configure.cmake \
#	../../external/coreclr/src/gc/unix/globals.h

libmonocoregc_la_SOURCES = $(monocoregc_sources)
libmonocoregc_la_CFLAGS = $(COREGC_DEFINES)
libmonocoregc_la_CXXFLAGS = $(COREGC_DEFINES)

CFLAGS := $(filter-out @CXX_REMOVE_CFLAGS@, @CFLAGS@) @CXX_ADD_CFLAGS@
