topdir = ../../..

LIBRARY = ../../lib/I18N.CJK.dll

LIB_LIST = list.unix
LIB_FLAGS = --unsafe -r corlib -r mscorlib -r I18N

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=

export MONO_PATH_PREFIX = ../../lib:

include ../../library.make
