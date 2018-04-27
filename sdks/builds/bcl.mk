
.stamp-bcl-toolchain:
	touch $@

bcl-ios_CONFIGURE_FLAGS = \
       --with-monotouch=yes \
       --with-monotouch_tv=yes \
       --with-xammac=yes \
       --with-monotouch_watch=yes

bcl_CONFIGURE_FLAGS = \
       $(if $(DISABLE_DESKTOP),--with-profile4_x=no,--with-profile4_x=yes) \
       $(if $(DISABLE_ANDROID),,--with-monodroid=yes) \
       $(if $(DISABLE_IOS),,$(bcl-ios_CONFIGURE_FLAGS)) \
       $(if $(DISABLE_WASM),,--with-wasm=yes) \
       --with-mcs-docs=no \
       --disable-nls \
       --disable-btls-lib \
       --disable-support-build \
       --disable-boehm

.stamp-bcl-configure: $(TOP)/configure
	mkdir -p $(TOP)/sdks/builds/bcl
	cd $(TOP)/sdks/builds/bcl && $(TOP)/configure $(bcl_CONFIGURE_FLAGS)
	touch $@

$(TOP)/sdks/out/bcl/monodroid $(TOP)/sdks/out/bcl/monotouch $(TOP)/sdks/out/bcl/wasm $(TOP)/sdks/out/bcl/net_4_x:
	mkdir -p $@

.PHONY: package-bcl
package-bcl: | $(TOP)/sdks/out/bcl/net_4_x $(TOP)/sdks/out/bcl/monodroid $(TOP)/sdks/out/bcl/monotouch $(TOP)/sdks/out/bcl/wasm
	if [ -d $(TOP)/mcs/class/lib/monodroid ]; then cp -R $(TOP)/mcs/class/lib/monodroid/* $(TOP)/sdks/out/bcl/monodroid; fi
	if [ -d $(TOP)/mcs/class/lib/monotouch ]; then cp -R $(TOP)/mcs/class/lib/monotouch/* $(TOP)/sdks/out/bcl/monotouch; fi
	if [ -d $(TOP)/mcs/class/lib/wasm ]; then cp -R $(TOP)/mcs/class/lib/wasm/* $(TOP)/sdks/out/bcl/wasm; fi
	if [ -d $(TOP)/mcs/class/lib/net_4_x ]; then cp -R $(TOP)/mcs/class/lib/net_4_x/* $(TOP)/sdks/out/bcl/net_4_x; fi

.PHONY: clean-bcl
clean-bcl:
	rm -rf .stamp-bcl-toolchain .stamp-bcl-configure $(TOP)/sdks/builds/bcl $(TOP)/sdks/out/bcl

TARGETS += bcl
