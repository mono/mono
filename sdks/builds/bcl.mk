
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
define BclTemplate

.stamp-$(1)-bcl-toolchain:
	touch $$@

.PHONY: .stamp-$(1)-bcl-configure
.stamp-$(1)-bcl-configure: .stamp-bcl-configure

.PHONY: setup-custom-$(1)-bcl
setup-custom-$(1)-bcl:
	mkdir -p $$(TOP)/sdks/out/$(1)-bcl $$(foreach profile,$(2),$$(TOP)/sdks/out/$(1)-bcl/$$(profile))

.PHONY: build-custom-$(1)-bcl
build-custom-$(1)-bcl: build-bcl
	$$(MAKE) -C bcl -C runtime all-mcs build_profiles="$(2)"

.PHONY: package-$(1)-bcl
package-$(1)-bcl:
	$$(foreach profile,$(2), \
		cp -R $$(TOP)/mcs/class/lib/$$(profile)/* $$(TOP)/sdks/out/$(1)-bcl/$$(profile);)

.PHONY: clean-$(1)-bcl
clean-$(1)-bcl: clean-bcl
	rm -rf $$(TOP)/sdks/out/$(1)-bcl $$(foreach profile,$(2),$$(TOP)/sdks/out/$(1)-bcl/$$(profile))

TARGETS += $(1)-bcl

endef
