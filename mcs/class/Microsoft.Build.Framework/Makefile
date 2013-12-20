thisdir = class/Microsoft.Build.Framework
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Framework.dll

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll

include ../../build/library.make

export TESTING_MONO=a
include $(XBUILD_DIR)/xbuild_targets.make

EXTRA_DISTFILES = \
	Mono.XBuild.Framework/AssemblyLoadInfo.cs
