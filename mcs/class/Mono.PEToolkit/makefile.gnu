topdir = ../..

LIBRARY = $(topdir)/class/lib/Mono.PEToolkit.dll

LIB_LIST = list.unix
LIB_FLAGS = --unsafe -r corlib -r System.Xml -r mscorlib

SOURCES_INCLUDE=
SOURCES_EXCLUDE=

# WARNING!!! This does not build with mcs
# Don't forget to remove the 'echo' at the end of
# the following line to activate the build with mcs.
export MONO_PATH_PREFIX = $(topdir)/class/lib: echo

include $(topdir)/class/library.make
