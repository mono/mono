thisdir = class/System.ServiceProcess
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceProcess.dll
LIB_MCS_FLAGS = /r:System.dll /r:System.Configuration.Install.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169

EXTRA_DISTFILES =

include ../../build/library.make
