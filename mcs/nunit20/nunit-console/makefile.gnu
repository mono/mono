#
# Makefile for nunit-console.exe
#
# Authors:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#   Gonzalo Paniagua Javier (gonzalo@ximian.com)
#

topdir = ../..
PROGRAM = ../nunit-console.exe

PROGRAM_LIST = list.unix
PROGRAM_FLAGS = /r:NUnit.Framework.dll \
		/r:NUnit.Util.dll

MONO_PATH_PREFIX=$(topdir)/class/lib:

include $(topdir)/class/executable.make

