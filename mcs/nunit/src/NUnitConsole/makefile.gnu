PROGRAM = NUnitConsole_mono.exe

PROGRAM_LIST = list.unix
PROGRAM_FLAGS =	\
	-r ../../../class/lib/corlib.dll	\
	-r ../../../class/lib/System.dll 	\
	-r ../../../class/lib/NUnitCore_mono.dll

include ../../../class/executable.make

MCSTOOL = ../../../mcs-tool
MCS_FLAGS = --target exe --noconfig

