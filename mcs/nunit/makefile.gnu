LIBRARY = ../class/lib/NUnitCore_mono.dll

LIB_LIST = list.unix
LIB_FLAGS = -r ../class/lib/corlib.dll -r ../class/lib/System.dll

include ../class/library.make

MCSTOOL = ../mcs-tool
MCS_FLAGS = --target library --noconfig

