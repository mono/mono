thisdir = tools/wasm-tuner
SUBDIRS =
include ../../build/rules.make

PROGRAM = wasm-tuner.exe

LIB_REFS = System System.Core Mono.Cecil
LOCAL_MCS_FLAGS = \
	-nowarn:3021

include ../../build/executable.make
