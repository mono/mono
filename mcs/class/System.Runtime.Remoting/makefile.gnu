topdir = ../..

LIBRARY = $(topdir)/class/lib/System.Runtime.Remoting.dll

LIB_LIST = unix.args
LIB_FLAGS = -r corlib -r System -r System.Web \
	    -r System.Runtime.Serialization.Formatters.Soap

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
