thisdir = class/Mono.Dynamic.Interpreter
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Dynamic.Interpreter.dll

LIB_MCS_FLAGS = -r:System.dll -r:System.Core.dll \
	-d:FEATURE_CORE_DLR,FEATURE_DBNULL,FEATURE_DEFAULT_PARAMETER_VALUE,FEATURE_GET_TYPE_INFO,FEATURE_VARIANCE,FEATURE_SERIALIZATION,CLR45 \
	-d:MONO_INTERPRETER \
	-delaysign -keyfile:../mono.pub

ifeq (monotouch, $(subst _runtime,,$(PROFILE)))
mono_dynamic_interpreter_deps = $(the_libdir_base)plaincore/System.Core.dll
LIB_MCS_FLAGS += -lib:$(the_libdir_base)plaincore
endif

include ../../build/library.make

$(the_libdir_base)Mono.Dynamic.Interpreter.dll: $(mono_dynamic_interpreter_deps)

$(the_libdir_base)plaincore/System.Core.dll:
	(cd ../System.Core; $(MAKE) $@)

.NOTPARALLEL: $(the_libdir_base)plaincore/System.Core.dll