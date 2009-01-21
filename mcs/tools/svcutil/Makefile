thisdir = tools/svcutil
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = \
	-r:Mono.GetOptions.dll \
	-r:System.Runtime.Serialization.dll \
	-r:System.ServiceModel.dll \
	-r:System.Web.Services.dll
PROGRAM = svcutil.exe

include ../../build/executable.make
