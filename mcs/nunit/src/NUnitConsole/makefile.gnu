topdir = ../../..
PROGRAM = $(topdir)/class/lib/NUnitConsole_mono.exe

PROGRAM_LIST = list.unix
PROGRAM_FLAGS =	\
	-r $(topdir)/class/lib/corlib.dll	\
	-r $(topdir)/class/lib/System.dll 	\
	-r $(topdir)/class/lib/NUnitCore_mono.dll

include $(topdir)/class/executable.make

RUNTIME=mono
MCS = $(RUNTIME) $(topdir)/mcs/mcs.exe
MCS_FLAGS = --target exe --noconfig

