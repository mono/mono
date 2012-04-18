thisdir = tools/svcutil
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = \
	-r:System.Core.dll \
	-r:System.Runtime.Serialization.dll \
	-r:System.ServiceModel.dll \
	-r:System.Web.Services.dll \
	-r:System.Configuration.dll
PROGRAM = svcutil.exe

include ../../build/executable.make
