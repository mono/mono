thisdir = tools/mono-configuration-crypto/lib
SUBDIRS =
include ../../../build/rules.make

LOCAL_MCS_FLAGS =
LIB_REFS = System Mono.Security System.Security System.Configuration System.Xml
LIBRARY = Mono.Configuration.Crypto.dll
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/mono-configuration-crypto/$(FRAMEWORK_VERSION)
NO_SIGN_ASSEMBLY = yes
include ../../../build/library.make
