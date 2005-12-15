thisdir = class/Microsoft.Build.Framework
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Framework.dll

ifeq (1.0, $(FRAMEWORK_VERSION))
LIBRARY_NAME = dummy-Microsoft.Build.Framework.dll
NO_INSTALL = yes
NO_TEST = yes
NO_SIGN_ASSEMBLY = yes
endif

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll

include ../../build/library.make

EXTRA_DISTFILES = \
	Mono.XBuild.Framework/AssemblyLoadInfo.cs
