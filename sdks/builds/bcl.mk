
_bcl_CONFIGURE_FLAGS = \
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
#  $(1): target
#  $(2): build profiles
#  $(3): test profiles
define BclTemplate

.stamp-$(1)-toolchain:
	touch $$@

.stamp-$(1)-configure: .stamp-bcl-configure
	touch $$@

.PHONY: setup-custom-$(1)
setup-custom-$(1):
	mkdir -p $$(TOP)/sdks/out/$(1) $$(foreach profile,$(2),$$(TOP)/sdks/out/$(1)/$$(profile))

.PHONY: build-$(1)
build-$(1): build-bcl

.PHONY: build-custom-$(1)
build-custom-$(1):
	$$(MAKE) -C bcl -C runtime all-mcs build_profiles="$(2)"
	$$(if $(3),$$(MAKE) -C bcl -C runtime test xunit-test test_profiles="$(3)")

.PHONY: package-$(1)
package-$(1):
	$$(foreach profile,$(2), \
		cp -R $$(TOP)/mcs/class/lib/$$(profile)/* $$(TOP)/sdks/out/$(1)/$$(profile);)

.PHONY: clean-$(1)
clean-$(1): clean-bcl
	rm -rf $$(TOP)/sdks/out/$(1) $$(foreach profile,$(2),$$(TOP)/sdks/out/$(1)/$$(profile))

TARGETS += $(1)

endef
