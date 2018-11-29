
##
# Parameters:
#  $(1): arch
#  $(2): host_triple
#
# Flags:
#  desktop-$(1)_CFLAGS
#  desktop-$(1)_CXXFLAGS
#  desktop-$(1)_LDFLAGS
define DesktopTemplate

_desktop-$(1)_CC=cc

_desktop-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-iconv \
	--disable-mcs-build \
	--disable-nls \
	--enable-dynamic-btls \
	--enable-maintainer-mode \
	--with-sigaltstack=yes \
	--with-tls=pthread \
	--without-ikvm-native

$$(eval $$(call RuntimeTemplate,desktop,$(1),$(2)))

endef

$(eval $(call DesktopTemplate,x86_64,x86_64-apple-darwin17.2.0))

$(eval $(call BclTemplate,desktop,net_4_x,))
