thisdir = tools/mono-service
SUBDIRS = 
include ../../build/rules.make

PROGRAM = mono-service.exe

include ../../build/executable.make

LOCAL_MCS_FLAGS = -r:System.ServiceProcess.dll -r:Mono.Posix.dll -unsafe
