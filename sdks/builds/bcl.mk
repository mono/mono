
.stamp-bcl-toolchain:
	touch $@

bcl_ios_CONFIGURE_FLAGS = \
	--with-monotouch=yes \
	--with-monotouch_tv=yes \
	--with-xammac=yes \
	--with-monotouch_watch=yes

bcl_CONFIGURE_FLAGS = \
	--with-profile4_x=no \
	$(if $(DISABLE_ANDROID),,--with-monodroid=yes) \
	$(if $(DISABLE_IOS),,$(bcl_ios_CONFIGURE_FLAGS)) \
	$(if $(DISABLE_WASM),,--with-wasm=yes) \
	--with-mcs-docs=no \
	--disable-nls \
	--disable-boehm

.stamp-bcl-configure: $(TOP)/configure
	mkdir -p $(TOP)/sdks/builds/bcl
	cd $(TOP)/sdks/builds/bcl && $(TOP)/configure $(bcl_CONFIGURE_FLAGS)
	touch $@

$(TOP)/sdks/out/bcl/monodroid $(TOP)/sdks/out/bcl/monotouch $(TOP)/sdks/out/bcl/wasm:
	mkdir -p $@

.PHONY: package-bcl
package-bcl: | $(TOP)/sdks/out/bcl/monodroid $(TOP)/sdks/out/bcl/monotouch $(TOP)/sdks/out/bcl/wasm
	if [ -d $(TOP)/mcs/class/lib/monodroid ]; then cp -R $(TOP)/mcs/class/lib/monodroid/* $(TOP)/sdks/out/bcl/monodroid; fi
	if [ -d $(TOP)/mcs/class/lib/monotouch ]; then cp -R $(TOP)/mcs/class/lib/monotouch/* $(TOP)/sdks/out/bcl/monotouch; fi
	if [ -d $(TOP)/mcs/class/lib/wasm ]; then cp -R $(TOP)/mcs/class/lib/wasm/* $(TOP)/sdks/out/bcl/wasm; fi

.PHONY: clean-bcl
clean-bcl:
	rm -rf .stamp-bcl-toolchain .stamp-bcl-configure $(TOP)/sdks/builds/bcl $(TOP)/sdks/out/bcl

TARGETS += bcl
