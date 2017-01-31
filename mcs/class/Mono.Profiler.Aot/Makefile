thisdir = class/Mono.Profiler.Aot
include ../../build/rules.make

LIBRARY = Mono.Profiler.Aot.dll
LIBRARY_SNK = ../mono.snk

LIB_REFS =
LIB_MCS_FLAGS = /unsafe -D:MONO_DATACONVERTER_STATIC_METHODS -keyfile:$(LIBRARY_SNK) /publicsign

NO_TEST=1
check:

NO_INSTALL=1

include ../../build/library.make
