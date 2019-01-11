thisdir = tools/upload-to-sentry
SUBDIRS =
include ../../build/rules.make

PROGRAM = upload-to-sentry.exe
LIB_REFS = \
	System \
	System.Core \
	System.Data \
	System.Runtime.Serialization \
	System.Xml \
	System.Xml.Linq \
	Mono.Posix \
	System Mono.Cecil

NO_INSTALL = yes

upload-crashes:
	MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/upload-to-sentry.exe

# It can be run using system .net during boostrap
TARGET_NET_REFERENCE = v4.6
# Trick to make it work during boostrap where it has to run with system
# assemblies not the ones in lib/basic folder
PROGRAM_USE_INTERMEDIATE_FILE = 1

ifdef MCS_MODE
LIB_REFS += Mono.Cecil.Mdb
endif

include ../../build/executable.make
