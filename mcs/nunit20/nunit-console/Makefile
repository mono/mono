thisdir = nunit20/nunit-console
SUBDIRS = 
include ../../build/rules.make

PROGRAM = $(topdir)/class/lib/$(PROFILE)/nunit-console.exe
LOCAL_MCS_FLAGS = /r:nunit.framework.dll /r:nunit.util.dll /r:nunit.core.dll
EXTRA_DISTFILES = App.ico nunit-console.csproj \
	nunit-console.exe.config.net_2_0 \
	nunit-console.exe.config.net_3_5

include ../../build/executable.make
