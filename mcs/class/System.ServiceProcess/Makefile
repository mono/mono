thisdir = class/System.ServiceProcess
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceProcess.dll
LIB_MCS_FLAGS = /r:System.dll /r:System.Configuration.Install.dll \
	/warnaserror /nowarn:0618
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /warnaserror

EXTRA_DISTFILES =

include ../../build/library.make
