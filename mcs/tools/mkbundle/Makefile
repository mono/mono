thisdir = tools/mkbundle
SUBDIRS = 
include ../../build/rules.make

PROGRAM = mkbundle.exe

OTHER_RES = template.c

RESOURCE_FILES = $(OTHER_RES)

LOCAL_MCS_FLAGS=-r:Mono.Posix $(OTHER_RES:%=-resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES)

include ../../build/executable.make

mkbundle.exe: $(RESOURCE_FILES)
