thisdir = tools/cil-stringreplacer
SUBDIRS =
include ../../build/rules.make

PROGRAM = cil-stringreplacer.exe
NO_INSTALL = yes

$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/Mono.Cecil.dll: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/Mono.Cecil.dll $(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/.stamp
	cp $< $@

$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/Mono.Cecil.Mdb.dll: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/Mono.Cecil.Mdb.dll $(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/.stamp
	cp $< $@

LIB_REFS = System

API = $(filter basic build, $(PROFILE))
ifdef API
# It can be run using system .net during boostrap
TARGET_NET_REFERENCE = v4.6
# Trick to make it work during boostrap where it has to run with system
# assemblies not the ones in lib/basic folder
PROGRAM_USE_INTERMEDIATE_FILE = 1

$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/cil-stringreplacer.exe: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/Mono.Cecil.dll

LIB_REFS += tmp/Mono.Cecil

ifdef MCS_MODE
$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/cil-stringreplacer.exe: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/Mono.Cecil.Mdb.dll

LIB_REFS += tmp/Mono.Cecil.Mdb
endif # MCS_MODE

else # Not basic/build
LIB_REFS += Mono.Cecil

ifdef MCS_MODE
LIB_REFS += Mono.Cecil.Mdb
endif # MCS_MODE

endif # Not basic/build

include ../../build/executable.make
