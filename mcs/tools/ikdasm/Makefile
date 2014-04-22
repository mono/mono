thisdir = tools/ikdasm
SUBDIRS =
include ../../build/rules.make

PROGRAM = ikdasm.exe

LOCAL_MCS_FLAGS = \
	-d:NO_SYMBOL_WRITER /r:System.Security.dll

#EXTRA_DISTFILES = LICENSE

include ../../build/executable.make
