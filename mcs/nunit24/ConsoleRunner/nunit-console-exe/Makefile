thisdir = nunit24/ConsoleRunner/nunit-console-exe
SUBDIRS = 
include ../../../build/rules.make

PROGRAM = nunit-console.exe
LOCAL_MCS_FLAGS = \
	/r:nunit.framework.dll /r:nunit.util.dll /r:nunit.core.dll \
	/r:nunit-console-runner.dll
EXTRA_DISTFILES = App.ico \
	nunit-console.exe.csproj \
	nunit-console.exe_VS2005.csproj \
	nunit-console.exe.config.net_2_0 \
	nunit-console.exe.config.net_3_5

include ../../../build/executable.make
