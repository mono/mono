
$(TOP)/sdks/builds/toolchains/mxe:
	git clone -b xamarin https://github.com/xamarin/mxe.git $@

.stamp-mxe-toolchain: | $(TOP)/sdks/builds/toolchains/mxe
	cd $(TOP)/sdks/builds/toolchains/mxe && git checkout $(MXE_HASH)
	touch $@

.stamp-mxe-configure:
	touch $@

.PHONY: build-custom-mxe
build-custom-mxe:
	PATH="$$PATH:$(dir $(shell which autopoint))" $(MAKE) -C $(TOP)/sdks/builds/toolchains/mxe gcc cmake zlib pthreads dlfcn-win32 mman-win32 \
		MXE_TARGETS="i686-w64-mingw32.static" PREFIX="$(TOP)/sdks/out/mxe" OS_SHORT_NAME="disable-native-plugins"
	PATH="$$PATH:$(dir $(shell which autopoint))" $(MAKE) -C $(TOP)/sdks/builds/toolchains/mxe gcc cmake zlib pthreads dlfcn-win32 mman-win32 \
		MXE_TARGETS="x86_64-w64-mingw32.static" PREFIX="$(TOP)/sdks/out/mxe" OS_SHORT_NAME="disable-native-plugins"

.PHONY: setup-custom-mxe
setup-custom-mxe:

.PHONY: package-mxe
package-mxe:

.PHONY: clean-mxe
clean-mxe:
	$(MAKE) -C $(TOP)/sdks/builds/toolchains/mxe clean \
		MXE_TARGETS="i686-w64-mingw32.static" PREFIX="$(TOP)/sdks/out/mxe"
	$(MAKE) -C $(TOP)/sdks/builds/toolchains/mxe clean \
		MXE_TARGETS="x86_64-w64-mingw32.static" PREFIX="$(TOP)/sdks/out/mxe"

TARGETS += mxe
