topdir = ../..

LIBRARY = $(topdir)/class/lib/ByteFX.Data.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System -r System.Xml -r System.Data -r ICSharpCode.SharpZipLib

SOURCES_INCLUDE=./*.cs ./Common/*.cs ./mysqlclient/*.cs
SOURCES_EXCLUDE=\
	./Test*

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
