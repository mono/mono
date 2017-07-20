thisdir = class/Mono.Profiler.Log
include ../../build/rules.make

LIBRARY = Mono.Profiler.Log.dll
LIBRARY_SNK = ../mono.snk

LIB_REFS = System System.Core
LIB_MCS_FLAGS = /unsafe /keyfile:$(LIBRARY_SNK) /publicsign

NO_TEST = yes

include ../../build/library.make
