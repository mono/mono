thisdir = class/Mono.CSharp.Debugger
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.CSharp.Debugger.dll
LIB_MCS_FLAGS = /r:$(corlib)
NO_TEST = yes

ifeq (win32, $(PLATFORM))
ifeq (net_2_0, $(PROFILE))
LIBRARY_COMPILE = MONO_PATH="../lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(gmcs) $(USE_MCS_FLAGS) /define:NET_1_1 /define:NET_2_0
else
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif
endif

include ../../build/library.make
