thisdir = tools/mkbundle
SUBDIRS = 
include ../../build/rules.make

PROGRAM = mkbundle.exe

OTHER_RES = template.c template_z.c template_main.c

RESOURCE_FILES = $(OTHER_RES)

LOCAL_MCS_FLAGS= $(OTHER_RES:%=-resource:%)

LOCAL_MCS_FLAGS += -d:STATIC,NO_SYMBOL_WRITER,NO_AUTHENTICODE

EXTRA_DISTFILES = $(RESOURCE_FILES)

include ../../build/executable.make

mkbundle.exe: $(RESOURCE_FILES)
