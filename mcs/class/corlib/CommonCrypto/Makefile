thisdir = class/corlib/CommonCrypto
SUBDIRS =
include ../../../build/rules.make

.NOTPARALLEL:
all:
	MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BOOTSTRAP_PROFILE)/commoncryptogenerator.exe

