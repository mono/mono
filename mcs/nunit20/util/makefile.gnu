#
# Makefile for NUnit.Util.dll
#
# Authors:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#   Gonzalo Paniagua Javier (gonzalo@ximian.com)
#

topdir = ../..
PROGRAM = $(topdir)/class/lib/NUnit.Util.dll

PROGRAM_LIST = list.unix
PROGRAM_FLAGS =

include $(topdir)/class/executable.make

RUNTIME=mono
MCS = $(RUNTIME) $(topdir)/mcs/mcs.exe
MCS_FLAGS = --target library -L $(topdir)/class/lib /r:NUnit.Framework.dll

