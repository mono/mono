lib_LTLIBRARIES = libMonoPosixHelper.la

libMonoPosixHelper_la_SOURCES = \
	map.c map.h macros.c

# 
# Use this target to refresh the values in map.[ch]
#
refresh:
	mono ../../mcs/class/Mono.Posix/Mono.Posix/make-map.exe ../../mcs/class/lib/Mono.Posix.dll map

