topdir = ../../..
PROGRAM = NUnitConsole_mono.exe

PROGRAM_LIST = list.unix
PROGRAM_FLAGS =	\
	-r ../../../class/lib/corlib.dll	\
	-r ../../../class/lib/System.dll 	\
	-r ../NUnitCore/NUnitCore_mono.dll

include ../../../class/executable.make

MCS = mono $(topdir)/mcs/mcs.exe
MCS_FLAGS = --target exe --noconfig

