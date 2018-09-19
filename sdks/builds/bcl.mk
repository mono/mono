

# $(BCL_PROFILES) controls, essentially, which profiles get nunitlite built (maybe more later)
# $(BCL_TEST_PROFILES) controls which profiles get test suites built. Right now it's only nunit-based corlib, System and System.Core. To be expanded.

ifndef DISABLE_ANDROID
BCL_PROFILES += monodroid
endif

ifndef DISABLE_IOS
BCL_PROFILES += monotouch
endif

ifndef DISABLE_DESKTOP
BCL_PROFILES += net_4_x
endif

ifndef DISABLE_WASM
BCL_PROFILES += wasm
BCL_TEST_PROFILES += wasm
endif

.stamp-bcl-toolchain:
	touch $@

bcl-ios_CONFIGURE_FLAGS = \
       --with-monotouch=yes \
       --with-monotouch_tv=yes \
       --with-xammac=yes \
       --with-monotouch_watch=yes

bcl_CONFIGURE_FLAGS = \
       $(if $(ENABLE_CXX),-enable-cxx) \
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

build-custom-bcl:
	$(MAKE) -C bcl
	@for the_profile in $(BCL_PROFILES); do $(MAKE) -C $(TOP)/mcs/tools/nunit-lite PROFILE=$$the_profile; done
	@for the_profile in $(BCL_TEST_PROFILES); do \
		$(MAKE) -C $(TOP)/mcs/class/corlib test-local PROFILE=$$the_profile; \
		$(MAKE) -C $(TOP)/mcs/class/System test-local PROFILE=$$the_profile; \
		$(MAKE) -C $(TOP)/mcs/class/System.Core test-local PROFILE=$$the_profile; \
	done

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
