
_bcl_CONFIGURE_FLAGS = \
	$(if $(filter $(UNAME),Windows),--host=$(HOST_ARCH_MINGW32)-w64-mingw32) \
	--disable-boehm \
	--disable-btls-lib \
	--disable-nls \
	--disable-support-build \
	--with-mcs-docs=no

.stamp-bcl-configure: $(TOP)/configure
	mkdir -p $(TOP)/sdks/builds/bcl
	./wrap-configure.sh $(TOP)/sdks/builds/bcl $(abspath $<) $(_bcl_CONFIGURE_FLAGS)
	touch $@

.PHONY: build-bcl
build-bcl: .stamp-bcl-configure
	$(MAKE) -C bcl -C mono

.PHONY: clean-bcl
clean-bcl:
	rm -rf .stamp-bcl-configure $(TOP)/sdks/builds/bcl

##
# Parameters
#  $(1): product
#  $(2): build profiles
#  $(3): test profiles
define BclTemplate

.stamp-$(1)-bcl-toolchain:
	touch $$@

.stamp-$(1)-bcl-configure: .stamp-bcl-configure
	touch $$@

.PHONY: setup-custom-$(1)-bcl
setup-custom-$(1)-bcl:
	mkdir -p $$(TOP)/sdks/out/$(1)-bcl $$(foreach profile,$(2),$$(TOP)/sdks/out/$(1)-bcl/$$(profile))

.PHONY: build-$(1)-bcl
build-$(1)-bcl: build-bcl

.PHONY: build-custom-$(1)-bcl
build-custom-$(1)-bcl:
	$$(MAKE) -C bcl -C runtime all-mcs build_profiles="$(2)" $$(_bcl_$(1)_BUILD_FLAGS)
	$$(if $(3),$$(MAKE) -C bcl -C runtime test xunit-test test_profiles="$(3)" $$(_bcl_$(1)_BUILD_FLAGS))

.PHONY: package-$(1)-bcl
package-$(1)-bcl:
	$$(foreach profile,$(2), \
		cp -R $$(TOP)/mcs/class/lib/$$(profile)/* $$(TOP)/sdks/out/$(1)-bcl/$$(profile);)

.PHONY: clean-$(1)-bcl
clean-$(1)-bcl: clean-bcl
	rm -rf $$(TOP)/sdks/out/$(1)-bcl $$(foreach profile,$(2),$$(TOP)/sdks/out/$(1)-bcl/$$(profile))

$$(eval $$(call TargetTemplate,$(1),bcl))

.PHONY: configure-$(1)
configure-$(1): configure-$(1)-bcl

.PHONY: build-$(1)
build-$(1): build-$(1)-bcl

.PHONY: package-$(1)
package-$(1): package-$(1)-bcl

.PHONY: archive-$(1)
archive-$(1): package-$(1)-bcl

$(1)_ARCHIVE += $(1)-bcl

endef
