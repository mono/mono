thisdir = tools/ildasm
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:Mono.Cecil.dll -D:MONO_DATACONVERTER_STATIC_METHODS -unsafe
PROGRAM = $(topdir)/class/lib/$(PROFILE)/ildasm.exe

include ../../build/executable.make
