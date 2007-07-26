thisdir := smcs
SUBDIRS :=
include ../build/rules.make

LOCAL_MCS_FLAGS += -d:GMCS_SOURCE -d:NET_1_1 -d:NET_2_0 /noconfig -warn:0 -debug -r:mscorlib.dll -r:System.dll

PROGRAM = smcs.exe
PROGRAM_COMPILE = MONO_PATH="$(topdir)/class/lib/net_2_1$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) ../gmcs/gmcs.exe $(LOCAL_MCS_FLAGS)

CLEAN_FILES = *.exe *.mdb

include ../build/executable.make

