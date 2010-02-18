thisdir = tools/mono-configuration-crypto/lib
SUBDIRS =
include ../../../build/rules.make

LOCAL_MCS_FLAGS = -r:System.dll -r:Mono.Security.dll -r:System.Security.dll -r:System.Configuration.dll -r:System.Xml.dll
LIBRARY = Mono.Configuration.Crypto.dll
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/mono-configuration-crypto/$(FRAMEWORK_VERSION)

include ../../../build/library.make
