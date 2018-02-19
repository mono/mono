
$(TOP)/sdks/builds/toolchains/mxe:
	git clone -b xamarin https://github.com/xamarin/mxe.git $@

##
# Parameters
#  $(1): target
#  $(2): arch
define MxeTemplate

.stamp-mxe-$(1)-toolchain: | $$(TOP)/sdks/builds/toolchains/mxe
	cd $$(TOP)/sdks/builds/toolchains/mxe && git checkout $$(MXE_HASH)
	touch $$@

.stamp-mxe-$(1)-configure:
	touch $$@

.PHONY: build-custom-mxe-$(1)
build-custom-mxe-$(1):
	PATH="$$$$PATH:$$(dir $$(shell which autopoint))" $$(MAKE) -C $$(TOP)/sdks/builds/toolchains/mxe gcc cmake zlib pthreads dlfcn-win32 mman-win32 \
		MXE_TARGETS="$(2)-w64-mingw32.static" PREFIX="$$(TOP)/sdks/out/mxe-$(1)" OS_SHORT_NAME="disable-native-plugins"

.PHONY: setup-custom-mxe-$(1)
setup-custom-mxe-$(1):

.PHONY: package-mxe-$(1)
package-mxe-$(1):

.PHONY: clean-mxe-$(1)
clean-mxe-$(1):
	$$(MAKE) -C $$(TOP)/sdks/builds/toolchains/mxe clean \
		MXE_TARGETS="$(2)-w64-mingw32.static" PREFIX="$$(TOP)/sdks/out/mxe-$(1)"

TARGETS += mxe-$(1)

endef

$(eval $(call MxeTemplate,Win32,i686))
$(eval $(call MxeTemplate,Win64,x86_64))
