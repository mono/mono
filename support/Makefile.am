if PLATFORM_WIN32
lib_LTLIBRARIES= 
else
lib_LTLIBRARIES= libMonoPosixHelper.la
endif

INCLUDES = \
	$(GLIB_CFLAGS) \
	-I$(top_srcdir)

MPH_SOURCE = \
	mph.h 		\
	old-map.h 	\
	map.h 		\
	dirent.c 	\
	errno.c 	\
	fcntl.c 	\
	fstab.c   \
	grp.c 		\
	macros.c	\
	map.c 		\
	pwd.c 		\
	old-map.c 	\
	serial.c	\
	stdio.c 	\
	stdlib.c 	\
	sys-mman.c 	\
	sys-sendfile.c 	\
	sys-stat.c 	\
	sys-statvfs.c 	\
	sys-time.c 	\
	sys-wait.c 	\
	time.c 		\
	unistd.c	 \
	utime.c    \
	x-struct-str.c

if HAVE_ZLIB
libMonoPosixHelper_la_SOURCES = \
	$(MPH_SOURCE) zlib_macros.c

libMonoPosixHelper_la_LIBADD = -lz
else
libMonoPosixHelper_la_SOURCES = $(MPH_SOURCE)
endif

libMonoPosixHelper_la_LDFLAGS = -no-undefined -version-info 1:0:1

# 
# Use this target to refresh the values in map.[ch]
#
refresh:
	$(top_builddir)/runtime/mono-wrapper $(mcs_topdir)/class/Mono.Posix/Mono.Posix/make-map.exe $(mcs_topdir)/class/lib/Mono.Posix.dll map

