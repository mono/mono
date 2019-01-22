thisdir = tools/cil-stringreplacer
SUBDIRS =
include ../../build/rules.make

PROGRAM = cil-stringreplacer.exe
NO_INSTALL = yes

TARGET_NET_REFERENCE = $(BOOTSTRAP_BIN_PROFILE)
PROGRAM_USE_INTERMEDIATE_FILE = 1

ifneq ($(PROFILE_RUNTIME),$(EXTERNAL_RUNTIME))

LIB_REFS = System Mono.Cecil

ifdef MCS_MODE
LIB_REFS += Mono.Cecil.Mdb
endif # MCS_MODE

else

$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/Mono.Cecil: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/Mono.Cecil
	cp $@ $<

$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tmp/Mono.Cecil.Mdb: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/Mono.Cecil.Mdb
	cp $@ $<

LIB_REFS = System tmp/Mono.Cecil

ifdef MCS_MODE
LIB_REFS += tmp/Mono.Cecil.Mdb
endif # MCS_MODE

endif # Not external mono

include ../../build/executable.make
