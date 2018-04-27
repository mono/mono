
MXE_SRC?=$(TOP)/sdks/builds/toolchains/mxe
MXE_PREFIX_DIR?=$(TOP)/sdks/out

# This is not overridable
MXE_PREFIX:=$(MXE_PREFIX_DIR)/mxe-$(shell echo $(MXE_HASH) | head -c 7)

$(MXE_SRC)/Makefile:
	git clone -b xamarin https://github.com/xamarin/mxe.git $(dir $@)
	cd $(dir $@) && git checkout $(MXE_HASH)

$(MXE_PREFIX)/.stamp: $(MXE_SRC)/Makefile
	$(MAKE) -C $(MXE_SRC) gcc cmake zlib pthreads dlfcn-win32 mman-win32 \
		PREFIX="$(MXE_PREFIX)" MXE_TARGETS="i686-w64-mingw32.static x86_64-w64-mingw32.static" \
			OS_SHORT_NAME="disable-native-plugins" PATH="$$PATH:$(MXE_PREFIX)/bin:$(dir $(shell brew list gettext | grep bin/autopoint$))"
	touch $@

.PHONY: provision-mxe
provision-mxe: $(MXE_PREFIX)/.stamp
