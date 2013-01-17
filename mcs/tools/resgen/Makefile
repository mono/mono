thisdir = tools/resgen
SUBDIRS =
include ../../build/rules.make

PROGRAM = resgen.exe

CLEAN_FILES = resgen.exe

INSTALL_PROFILE := $(filter net_2_0 net_4_5, $(PROFILE))
ifndef INSTALL_PROFILE
NO_INSTALL = yes
endif

include ../../build/executable.make
