thisdir = tools/mkbundle
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS=-r:Mono.Posix -resource:template.c

PROGRAM = mkbundle.exe

include ../../build/executable.make

mkbundle.exe: mkbundle.cs template.c
