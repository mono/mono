
MXE_SRC?=$(TOP)/sdks/builds/toolchains/mxe
MXE_PREFIX?=$(TOP)/sdks/out/mxe

$(TOP)/sdks/builds/toolchains/mxe:
	git clone -b xamarin https://github.com/xamarin/mxe.git $@
	cd $@ && git checkout $(MXE_HASH)

.stamp-mxe-toolchain: | $(MXE_SRC)
	touch $@

.stamp-mxe-configure:
	touch $@

.PHONY: build-custom-mxe
build-custom-mxe:
	$(MAKE) -C $(MXE_SRC) gcc cmake zlib pthreads dlfcn-win32 mman-win32 \
		MXE_TARGETS="i686-w64-mingw32.static x86_64-w64-mingw32.static" PREFIX="$(MXE_PREFIX)"
			OS_SHORT_NAME="disable-native-plugins" PATH="$$PATH:$(dir $(shell brew list gettext | grep autopoint$))"

.PHONY: setup-custom-mxe
setup-custom-mxe:

.PHONY: package-mxe
package-mxe:

.PHONY: clean-mxe
clean-mxe:
	rm -rf $(TOP)/sdks/builds/toolchains/mxe $(MXE_PREFIX)

TARGETS += mxe
