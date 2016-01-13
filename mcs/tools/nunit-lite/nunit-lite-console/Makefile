thisdir = tools/nunit-lite/nunit-lite-console
SUBDIRS = 
include ../../../build/rules.make

PROGRAM = nunit-lite-console.exe
LOCAL_MCS_FLAGS = /r:nunitlite.dll 

include ../../../build/executable.make

all-local-aot:
	$(TEST_RUNTIME) $(AOT_BUILD) ../../class/lib/$(PROFILE)/nunit-lite-console.exe


