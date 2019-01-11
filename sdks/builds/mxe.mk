
ifeq ($(UNAME),Linux)
LINUX_FLAVOR=$(shell ./determine-linux-flavor.sh)
endif

LINUX_WITH_MINGW=:Ubuntu:,:Debian:,:Debian GNU/Linux:
LINUX_HAS_MINGW=$(if $(findstring :$(LINUX_FLAVOR):,$(LINUX_WITH_MINGW)),yes)

ifeq ($(LINUX_HAS_MINGW),yes)
MXE_PREFIX=/usr

.PHONY: provision-mxe
provision-mxe:
	@echo $(LINUX_FLAVOR) Linux does not require mxe provisioning. mingw from packages is used instead
else
MXE_SRC?=$(TOP)/sdks/builds/toolchains/mxe
MXE_PREFIX_DIR?=$(HOME)/android-toolchain

# This is not overridable
MXE_PREFIX:=$(MXE_PREFIX_DIR)/mxe-$(shell echo $(MXE_HASH) | head -c 7)

$(MXE_PREFIX)/.stamp:
	rm -rf $(MXE_PREFIX) $(MXE_SRC)
	git clone -b xamarin https://github.com/xamarin/mxe.git $(MXE_SRC) \
		&& git -C $(MXE_SRC) checkout $(MXE_HASH)
	$(MAKE) -C $(MXE_SRC) gcc cmake zlib pthreads dlfcn-win32 mman-win32 \
		PREFIX="$(MXE_PREFIX)" MXE_TARGETS="i686-w64-mingw32.static x86_64-w64-mingw32.static" \
			OS_SHORT_NAME="disable-native-plugins" PATH="$$PATH:$(MXE_PREFIX)/bin:$(dir $(shell brew list gettext | grep bin/autopoint$))"
	touch $@

.PHONY: provision-mxe
provision-mxe: $(MXE_PREFIX)/.stamp
endif

.PHONY: provision
provision: provision-mxe
