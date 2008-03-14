thisdir = tools/mono-service
SUBDIRS = 
include ../../build/rules.make

PROGRAM = $(topdir)/class/lib/$(PROFILE)/mono-service.exe

include ../../build/executable.make

LOCAL_MCS_FLAGS = -r:System.ServiceProcess.dll -r:Mono.Posix.dll -unsafe

# Copied from library.make

# -- begin --
ifeq ($(PLATFORM), win32)
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
	$(GACUTIL) /i $(PROGRAM) /f $(gacdir_flag) /root $(GACROOT)

uninstall-extras: $(gacutil)
	-$(GACUTIL) /u $(PROGRAM:.exe=) $(gacdir_flag) /root $(GACROOT)
