topdir = ../..

LIBRARY = $(topdir)/class/lib/System.Drawing.dll

LIB_LIST = list.unix
# to use JPEG decoder add -r ./cdeclRedirector/cdeclCallback.dll 
# and remove -define:DECLARE_CDECL_DELEGATES
# cdeclCallback.dll should be manually copied to library folder
LIB_FLAGS = --unsafe -r corlib -r System -define:DECLARE_CDECL_DELEGATES

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
