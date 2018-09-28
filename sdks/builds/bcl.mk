
# controls, essentially, which profiles get built (maybe more later)
BCL_PROFILES=

ifndef DISABLE_ANDROID
BCL_PROFILES += monodroid monodroid_tools
endif

ifndef DISABLE_IOS
BCL_PROFILES += monotouch monotouch_tv monotouch_watch monotouch_runtime monotouch_tv_runtime monotouch_watch_runtime xammac xammac_net_4_5
endif

ifndef DISABLE_DESKTOP
BCL_PROFILES += net_4_x
endif

ifndef DISABLE_WASM
BCL_PROFILES += wasm
endif

.stamp-bcl-toolchain:
	touch $@

bcl_CONFIGURE_FLAGS = \
	--with-mcs-docs=no \
	--disable-nls \
	--disable-btls-lib \
	--disable-support-build \
	--disable-boehm

.stamp-bcl-configure: $(TOP)/configure
	mkdir -p $(TOP)/sdks/builds/bcl
	cd $(TOP)/sdks/builds/bcl && $(TOP)/configure $(bcl_CONFIGURE_FLAGS)
	touch $@

.PHONY: setup-custom-bcl
setup-custom-bcl:
	mkdir -p $(TOP)/sdks/out/bcl $(foreach profile,$(BCL_PROFILES),$(TOP)/sdks/out/bcl/$(profile))

.PHONY: build-custom-bcl
build-custom-bcl:
	$(MAKE) -C bcl -C mono
	$(MAKE) -C bcl -C runtime all-mcs build_profiles="$(BCL_PROFILES)"

.PHONY: package-bcl
package-bcl:
	$(foreach profile,$(BCL_PROFILES), \
		cp -R $(TOP)/mcs/class/lib/$(profile)/* $(TOP)/sdks/out/bcl/$(profile);)

.PHONY: clean-bcl
clean-bcl:
	rm -rf .stamp-bcl-toolchain .stamp-bcl-configure $(TOP)/sdks/builds/bcl $(TOP)/sdks/out/bcl

TARGETS += bcl
