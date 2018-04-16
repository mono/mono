thisdir = tools/cil-strip
SUBDIRS =
include ../../build/rules.make

LOCAL_MCS_FLAGS =
LIB_REFS = System

# IMPORTANT: we need to keep the old copy of Cecil we have in this folder since newer Cecil
# doesn't support keeping metadata tokens the same which would break AOT compilation

PROGRAM = mono-cil-strip.exe

include ../../build/executable.make
