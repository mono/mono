topdir = ../../..

LIBRARY = $(topdir)/class/lib/I18N.CJK.dll

LIB_LIST = list.unix
LIB_FLAGS = --unsafe --resource big5.table --resource jis.table -r corlib -r mscorlib -r I18N

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
