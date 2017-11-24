thisdir = class/Mono.Profiler.Log
include ../../build/rules.make

LIBRARY = Mono.Profiler.Log.dll
LIBRARY_SNK = ../mono.snk

LIB_REFS = System System.Core
KEYFILE = $(LIBRARY_SNK)
LIB_MCS_FLAGS = /unsafe /publicsign

LIBRARY_WARN_AS_ERROR = yes

NO_TEST = yes

include ../../build/library.make
