topdir = ../..

LIBRARY = $(topdir)/class/lib/ByteFX.Data.dll

LIB_LIST = list
LIB_FLAGS = -r corlib.dll -r System.dll -r System.Xml.dll -r System.Data.dll -r ../lib/ICSharpCode.SharpZipLib.dll

SOURCES_INCLUDE=./*.cs ./Common/*.cs ./mysqlclient/*.cs
SOURCES_EXCLUDE=\
	./Test*

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
