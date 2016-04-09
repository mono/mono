thisdir = tools/resgen
SUBDIRS =
include ../../build/rules.make

PROGRAM = resgen.exe

CLEAN_FILES = resgen.exe

LOCAL_MCS_FLAGS = -r:System.dll -r:System.Xml.dll -r:System.Core.dll

INSTALL_PROFILE := $(filter $(DEFAULT_PROFILE), $(PROFILE))
ifndef INSTALL_PROFILE
NO_INSTALL = yes
endif

include ../../build/executable.make
