#
# Makefile for NUnit.Util.dll
#
# Authors:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#   Gonzalo Paniagua Javier (gonzalo@ximian.com)
#

topdir = ../..
LIBRARY = $(topdir)/class/lib/NUnit.Util.dll

LIB_LIST = list.unix
LIB_FLAGS =

SOURCES_INCLUDE=*.cs

export MONO_PATH = $(topdir)/class/lib:

include $(topdir)/class/library.make

