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
PROGRAM_FLAGS =

include $(topdir)/class/executable.make

RUNTIME=mono
MCS = $(RUNTIME) $(topdir)/mcs/mcs.exe
MCS_FLAGS = -L $(topdir)/class/lib /r:NUnit.Framework.dll /r:NUnit.Util.dll

