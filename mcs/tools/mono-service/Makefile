thisdir = tools/mono-service
SUBDIRS = 
include ../../build/rules.make

PROGRAM = mono-service.exe

PROGRAM_SNK = ../../class/mono.snk

LOCAL_MCS_FLAGS = -unsafe -publicsign -keyfile:../../class/mono.snk
LIB_REFS = System.ServiceProcess Mono.Posix System

include ../../build/executable.make

# Copied from library.make

# -- begin --
ifeq ($(BUILD_PLATFORM), win32)
GACDIR = `cygpath -w $(mono_libdir)`
GACROOT = `cygpath -w $(DESTDIR)$(mono_libdir)`
test_flags += -d:WINDOWS
else
GACDIR = $(mono_libdir)
GACROOT = $(DESTDIR)$(mono_libdir)
endif

ifndef RUNTIME_HAS_CONSISTENT_GACDIR
gacdir_flag = /gacdir $(GACDIR)
endif

# -- end --

install-local: install-extras
uninstall-local: uninstall-extras

install-extras: $(gacutil)
	$(GACUTIL) /i $(the_lib) /f $(gacdir_flag) /root $(GACROOT)

uninstall-extras: $(gacutil)
	-$(GACUTIL) /u $(the_lib:.exe=) $(gacdir_flag) /root $(GACROOT)
