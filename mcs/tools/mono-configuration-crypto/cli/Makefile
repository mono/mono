thisdir = tools/mono-configuration-crypto/cli
SUBDIRS = 
include ../../../build/rules.make

LOCAL_MCS_FLAGS = -r:Mono.Security.dll \
		  -r:System.Security.dll \
		  -r:System.Configuration.dll \
		  -r:System.dll \
		  -r:$(topdir)/class/lib/$(PROFILE)/Mono.Configuration.Crypto.dll

PROGRAM = mono-configuration-crypto.exe
PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/mono-configuration-crypto/$(FRAMEWORK_VERSION)

include ../../../build/executable.make
