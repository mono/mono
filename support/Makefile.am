lib_LTLIBRARIES= libMonoPosixHelper.la

INCLUDES = \
	$(GLIB_CFLAGS) \
	-I$(top_srcdir)

# Source code which helps implement the ANSI C standards, and thus *should* be
# portable to any platform having a C compiler.
MPH_C_SOURCE =   \
	errno.c 	     \
	map.c          \
	map.h          \
	mph.h          \
	signal.c       \
	stdio.c        \
	stdlib.c

# Source code which helps implement POSIX and other related Unix standards,
# and *may* be portable between Unix platforms.
MPH_UNIX_SOURCE = \
	dirent.c 	\
	fcntl.c 	\
	fstab.c   \
	grp.c 		\
	macros.c	\
	old-map.c 	\
	old-map.h 	\
	pwd.c 		\
	serial.c	\
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

if PLATFORM_WIN32
MPH_SOURCE = $(MPH_C_SOURCE)
MPH_LIBS   =
else
MPH_SOURCE = $(MPH_C_SOURCE) $(MPH_UNIX_SOURCE)
MPH_LIBS   = $(GLIB_LIBS)
endif

if HAVE_ZLIB
Z_SOURCE = \
	zlib_macros.c
Z_LIBS   = \
	-lz
else
Z_SOURCE = 
Z_LIBS   = 
endif

libMonoPosixHelper_la_SOURCES = \
	$(MPH_SOURCE) \
	$(Z_SOURCE)

libMonoPosixHelper_la_LIBADD = \
	$(MPH_LIBS) \
	$(Z_LIBS)

libMonoPosixHelper_la_LDFLAGS = -no-undefined -version-info 1:0:1

# 
# Use this target to refresh the values in map.[ch]
#
refresh:
	$(top_builddir)/runtime/mono-wrapper $(mcs_topdir)/class/Mono.Posix/Mono.Unix/make-map.exe $(mcs_topdir)/class/lib/default/Mono.Posix.dll map

