thisdir = class/Mono.Dynamic.Interpreter
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Dynamic.Interpreter.dll

LIB_MCS_FLAGS = -r:System.dll -r:System.Core.dll \
	-d:FEATURE_CORE_DLR,FEATURE_DBNULL,FEATURE_DEFAULT_PARAMETER_VALUE,FEATURE_GET_TYPE_INFO,FEATURE_VARIANCE,FEATURE_SERIALIZATION,CLR45 \
	-d:MONO_INTERPRETER \
	-delaysign -keyfile:../mono.pub

include ../../build/library.make

