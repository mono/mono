#
# Makefile for NUnit.Framework.dll
#
# Authors:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#   Gonzalo Paniagua Javier (gonzalo@ximian.com)
#

topdir = ../..
PROGRAM = $(topdir)/class/lib/NUnit.Framework.dll

PROGRAM_LIST = list.unix
PROGRAM_FLAGS = /resource:Transform.resources,NUnit.Framework.Transform.resources

include $(topdir)/class/executable.make

RUNTIME=mono
MCS = $(RUNTIME) $(topdir)/mcs/mcs.exe
MCS_FLAGS = --target library

