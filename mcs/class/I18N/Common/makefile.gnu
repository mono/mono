topdir = ../../..

LIBRARY = ../../lib/I18N.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r mscorlib

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=

export MONO_PATH_PREFIX = ../../lib:

include ../../library.make
