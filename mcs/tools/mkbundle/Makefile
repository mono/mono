thisdir = tools/mkbundle
SUBDIRS = 
include ../../build/rules.make

PROGRAM = mkbundle.exe

OTHER_RES = template.c template_z.c template_main.c

RESOURCE_FILES = $(OTHER_RES)

LOCAL_MCS_FLAGS=-r:Mono.Posix.dll -r:ICSharpCode.SharpZipLib.dll \
		$(OTHER_RES:%=-resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES)

include ../../build/executable.make

mkbundle.exe: $(RESOURCE_FILES)
