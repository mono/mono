if PLATFORM_WIN32
SUPPORT=
else
SUPPORT=libMonoSupportW.la
endif
lib_LTLIBRARIES = 				\
	libMonoPosixHelper.la			\
	$(SUPPORT)

INCLUDES =					\
	$(GLIB_CFLAGS)				\
	-I$(top_srcdir)

# Source code which helps implement the ANSI C standards, and thus *should* be
# portable to any platform having a C compiler.
MPH_C_SOURCE =					\
	errno.c					\
	map.c					\
	map.h					\
	mph.h					\
	signal.c				\
	stdio.c					\
	string.c					\
	stdlib.c

# Source code which helps implement POSIX and other related Unix standards,
# and *may* be portable between Unix platforms.
MPH_UNIX_SOURCE =				\
	dirent.c				\
	fcntl.c					\
	fstab.c					\
	grp.c					\
	macros.c				\
	old-map.c				\
	old-map.h				\
	pwd.c					\
	serial.c				\
	sys-mman.c				\
	sys-sendfile.c				\
	sys-stat.c				\
	sys-statvfs.c				\
	sys-time.c				\
	sys-wait.c				\
	sys-xattr.c				\
	syslog.c				\
	time.c					\
	unistd.c				\
	utime.c					\
	x-struct-str.c

if PLATFORM_WIN32
MPH_SOURCE = $(MPH_C_SOURCE)
MPH_LIBS   =
else
MPH_SOURCE = $(MPH_C_SOURCE) $(MPH_UNIX_SOURCE)
MPH_LIBS   = $(GLIB_LIBS)
endif

ZLIB_SOURCES = \
	adler32.c	\
	compress.c	\
	crc32.c		\
	gzio.c		\
	uncompr.c	\
	deflate.c	\
	trees.c		\
	zutil.c		\
	inflate.c	\
	infback.c	\
	inftrees.c	\
	inffast.c	\
	crc32.h		\
	deflate.h  	\
	inffast.h  	\
	inffixed.h  	\
	inflate.h  	\
	inftrees.h  	\
	trees.h  	\
	zconf.h  	\
	zlib.h  	\
	zutil.h

if HAVE_ZLIB
Z_SOURCE = zlib_macros.c 
Z_LIBS= -lz
else
Z_SOURCE = zlib_macros.c $(ZLIB_SOURCES)
Z_LIBS=
endif

libMonoPosixHelper_la_SOURCES =			\
	$(MPH_SOURCE)				\
	$(Z_SOURCE)

libMonoPosixHelper_la_LIBADD =			\
	$(MPH_LIBS)				\
	$(Z_LIBS)				\
	$(XATTR_LIB)

# libMonoPosixHelper_la_LDFLAGS = -no-undefined -version-info 1:0:1
libMonoPosixHelper_la_LDFLAGS = -no-undefined -avoid-version
libMonoSupportW_la_LDFLAGS = -no-undefined -avoid-version

libMonoSupportW_la_SOURCES =			\
		supportw.c			\
		support-heap.c			\
		supportw.h

libMonoSupportW_la_LIBADD =			\
		$(GLIB_LIBS)

# 
# Use this target to refresh the values in map.[ch]
#
refresh:
	cp `pkg-config --variable=Programs create-native-map` . && \
	$(top_builddir)/runtime/mono-wrapper create-native-map.exe \
	--autoconf-member=d_off                               \
	--autoconf-member=d_reclen                            \
	--autoconf-member=d_type                              \
	--exclude-native-symbol=Mono_Posix_Stdlib_snprintf    \
	--impl-macro=_GNU_SOURCE --impl-macro=_XOPEN_SOURCE   \
	--impl-header="<sys/types.h>"                         \
	--impl-header="<sys/stat.h>"                          \
	--impl-header="<sys/time.h>"                          \
	--autoconf-header="<sys/poll.h>"                      \
	--autoconf-header="<sys/wait.h>"                      \
	--autoconf-header="<sys/statvfs.h>"                   \
	--autoconf-header="<sys/xattr.h>"                     \
	--autoconf-header="<sys/mman.h>"                      \
	--impl-header="<unistd.h>"                            \
	--impl-header="<fcntl.h>"                             \
	--impl-header="<signal.h>"                            \
	--autoconf-header="<poll.h>"                          \
	--autoconf-header="<grp.h>"                           \
	--impl-header="<errno.h>"                             \
	--autoconf-header="<syslog.h>"                        \
	--impl-header="<dirent.h>"                            \
	--impl-header="<utime.h>"                             \
	--impl-header="\"mph.h\""                             \
	--rename-member=st_atime=st_atime_                    \
	--rename-member=st_ctime=st_ctime_                    \
	--rename-member=st_mtime=st_mtime_                    \
	--rename-namespace=Mono.Unix.Native=Mono.Posix        \
	--library=MonoPosixHelper                             \
	$(mcs_topdir)/class/lib/net_2_0/Mono.Posix.dll map

