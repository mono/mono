
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
	$$(MAKE) -C bcl -C runtime all-mcs build_profiles="$(2)"
	$$(if $(3),$$(MAKE) -C bcl -C runtime test xunit-test test_profiles="$(3)")

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

.PHONY: archive-$(1)
archive-$(1): package-$(1)-bcl

$(1)_ARCHIVE += $(1)-bcl

endef

##
# Parameters
#  $(1): product
#  $(2): profile platform
#  $(3): build profiles
#  $(4): test profiles
define BclCrossTemplate

.stamp-$(1)-bcl-cross-$(2)-toolchain:
	touch $$@

# TODO: this depends on the same .stamp-bcl-configure as the non-cross BclTemplate, so we build out of the same tree.  May want to rethink that.
.stamp-$(1)-bcl-cross-$(2)-configure: .stamp-bcl-configure
	touch $$@

.PHONY: setup-custom-$(1)-bcl-cross-$(2)
setup-custom-$(1)-bcl-cross-$(2):
	mkdir -p $$(TOP)/sdks/out/$(1)-bcl-cross-$(2) $$(foreach profile,$(3),$$(TOP)/sdks/out/$(1)-bcl-cross-$(2)/$$(profile))

.PHONY: build-$(1)-bcl-cross-$(2)
build-$(1)-bcl-cross-$(2): build-bcl

# First we build the "build" profile of the build platform (we need at least
# gensources), then we build the platform-specific versions of the requested
# profiles.
.PHONY: build-custom-$(1)-bcl-cross-$(2)
build-custom-$(1)-bcl-cross-$(2):
	$$(MAKE) -C bcl -C runtime NO_DIR_CHECK=1 build_profiles=build all-mcs
	-$$(MAKE) -C bcl -C runtime all-mcs build_profiles="$(3)" PROFILE_PLATFORM="$(2)"
# FIXME: get rid of that '-' above.  It's just there until all the assemblies in the profile build properly
# FIXME: add tests also
# $$(if $(4),$$(MAKE) -C bcl -C runtime test xunit-test test_profiles="$(4)")

.PHONY: package-$(1)-bcl-cross-$(2)
package-$(1)-bcl-cross-$(2):
	$$(foreach profile,$(3), \
		cp -R $$(TOP)/mcs/class/lib/$$(profile)-$(2)/* $$(TOP)/sdks/out/$(1)-bcl-cross-$(2)/$$(profile);)

.PHONY: clean-$(1)-bcl-cross-$(2)
clean-$(1)-bcl-cross-$(2): clean-bcl
	rm -rf $$(TOP)/sdks/out/$(1)-bcl-cross-$(2) $$(foreach profile,$(3),$$(TOP)/sdks/out/$(1)-bcl-cross-$(2)/$$(profile))

$$(eval $$(call TargetTemplate,$(1),bcl-cross-$(2)))

.PHONY: configure-$(1)
configure-$(1): configure-$(1)-bcl-cross-$(2)

.PHONY: build-$(1)
build-$(1): build-$(1)-bcl-cross-$(2)

.PHONY: archive-$(1)
archive-$(1): package-$(1)-bcl-cross-$(2)

$(1)_ARCHIVE += $(1)-bcl-cross-$(2)

endef

