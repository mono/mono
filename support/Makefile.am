if PLATFORM_WIN32
lib_LTLIBRARIES= 
else
lib_LTLIBRARIES= libMonoPosixHelper.la
endif

libMonoPosixHelper_la_SOURCES = \
	map.c map.h macros.c zlib_macros.c

libMonoPosixHelper_la_LIBADD = -lz

# 
# Use this target to refresh the values in map.[ch]
#
refresh:
	mono ../../mcs/class/Mono.Posix/Mono.Posix/make-map.exe ../../mcs/class/lib/Mono.Posix.dll map

