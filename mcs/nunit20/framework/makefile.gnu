#
# Makefile for NUnit.Framework.dll
#
# Authors:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#   Gonzalo Paniagua Javier (gonzalo@ximian.com)
#

topdir = ../..
LIBRARY= $(topdir)/class/lib/NUnit.Framework.dll

LIB_LIST = list.unix
LIB_FLAGS = /resource:Transform.resources,NUnit.Framework.Transform.resources \
	    /r:System.Xml.dll \
	    /r:System.dll

SOURCES_INCLUDE=*.cs

export MONO_PATH = $(topdir)/class/lib:

include $(topdir)/class/library.make

