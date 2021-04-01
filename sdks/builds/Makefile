TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config

MAKEFLAGS += --no-builtin-rules

CONFIGURATION?=release

RELEASE=$(if $(filter $(CONFIGURATION),release),1)

lowercase=$(shell echo "$(1)" | tr '[:upper:]' '[:lower:]')

UNAME=$(shell uname)

ifneq (,$(findstring CYGWIN,$(UNAME)))
UNAME=Windows
endif

ifdef ENABLE_WSL_WINDOWS_BUILD_HOST
ifeq ($(UNAME),Linux)
UNAME_WSL_CHECK=$(shell uname -a)
ifneq (,$(findstring Microsoft,$(UNAME_WSL_CHECK)))
UNAME=Windows
endif
endif
endif

ifneq ($(UNAME),Darwin)
ifneq ($(UNAME),Linux)
ifneq ($(UNAME),Windows)
$(error "Unsupported UNAME=$(UNAME)")
endif
endif
endif

ifeq ($(UNAME),Windows)
# x86_64 or i686
HOST_ARCH_MINGW32=$(shell uname -m)
endif

ifneq ($(UNAME),Darwin)
# iOS and Mac requires Xcode to be available, and Xcode is only available on macOS
ENABLE_IOS=
ENABLE_MAC=
ENABLE_MACCAT=
endif

# On Windows, we will just trigger LLVM and Android builds using this Makefile.
ifeq ($(UNAME),Windows)
ENABLE_IOS=
ENABLE_MAC=
ENABLE_MACCAT=
ENABLE_WASM=
ENABLE_WASM_CROSS=
ENABLE_DESKTOP=
DISABLE_CCACHE=1
DISABLE_LIBTOOLIZE=1
IGNORE_PROVISION_MXE=1
endif

CCACHE:=$(if $(DISABLE_CCACHE),,$(shell which ccache))
NINJA:=$(shell which ninja)

include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk

ifndef DISABLE_LIBTOOLIZE
#brew's libtool is not compatible with some of the deps needed (I.E. V8) so in those systems we need to explicit add to the path
#this is due mono not being compatible with xcode's libtool, which is what's on path by default
ifeq (, $(shell which glibtoolize))
EXTRA_PATH=$(wildcard /usr/local/Cellar/libtool/*/bin/)
endif
endif

.PHONY: all
all:

## Common Mono targets

.PHONY: configure-mono
configure-mono: $(TOP)/configure


ifeq ($(UNAME),Darwin)
.stamp-ulimit-check:
	@if [ $$(ulimit -n) -lt 1024 ] ; then \
		echo "Error: Increase ulimit -n to at least 1024"; \
		exit 1; \
	fi
	touch $@
else
.stamp-ulimit-check:
	touch $@
endif

$(TOP)/configure: $(TOP)/configure.ac $(TOP)/autogen.sh .stamp-ulimit-check
	cd $(TOP) && PATH=$(EXTRA_PATH):$$PATH NOCONFIGURE=1 ./autogen.sh $(if $(wildcard $(TOP)/../mono-extensions),--enable-extension-module=xamarin --enable-extension-module)

## Archive targets

ifdef ENABLE_ANDROID
android_ARCHIVE=
endif

ifdef ENABLE_IOS
ios_ARCHIVE=
endif

ifdef ENABLE_MAC
mac_ARCHIVE=
endif

ifdef ENABLE_MACCAT
maccat_ARCHIVE=
endif

ifdef ENABLE_WASM
wasm_ARCHIVE=
endif

##
# Parameters:
#  $(1): target (android, ios, mac, wasm)
#  $(2): compression format (7z, zip)
define ArchiveTemplate
_$(1)_HASH = $$(shell git -C $$(TOP) rev-parse HEAD)
_$(1)_PACKAGE = $(1)-$$(CONFIGURATION)-$$(UNAME)-$$(_$(1)_HASH).$(2)

ifeq ($(2),7z)
_$(1)_COMPRESSION_ARGS = -t7z -mx=9
endif

.PHONY: archive-$(1)
archive-$(1):
	cd $$(TOP)/sdks/out && 7z a $$(_$(1)_COMPRESSION_ARGS) $$(TOP)/$$(_$(1)_PACKAGE) $$(sort $$($(1)_ARCHIVE))
endef

ifdef ENABLE_ANDROID
$(eval $(call ArchiveTemplate,android,7z))
endif

ifdef ENABLE_IOS
$(eval $(call ArchiveTemplate,ios,7z))
endif

ifdef ENABLE_MAC
$(eval $(call ArchiveTemplate,mac,7z))
endif

ifdef ENABLE_MACCAT
$(eval $(call ArchiveTemplate,maccat,7z))
endif

ifdef ENABLE_WASM
$(eval $(call ArchiveTemplate,wasm,zip))
endif

## Targets

.PHONY: build-custom-%
build-custom-%:
	$(MAKE) -C $*

.PHONY: setup-custom-%
setup-custom-%:
	mkdir -p $(TOP)/sdks/out/$*

##
# Parameters:
# $(1): product
# $(2): target
define TargetTemplate

.PHONY: toolchain-$(1)-$(2)
toolchain-$(1)-$(2): .stamp-$(1)-$(2)-toolchain

.PHONY: toolchain
toolchain: toolchain-$(1)-$(2)

.stamp-$(1)-$(2)-configure: .stamp-$(1)-$(2)-toolchain

.PHONY: configure-$(1)-$(2)
configure-$(1)-$(2): .stamp-$(1)-$(2)-configure

.PHONY: configure
configure: configure-$(1)-$(2)

.PHONY: build-$(1)-$(2)
build-$(1)-$(2): .stamp-$(1)-$(2)-configure
	$$(MAKE) build-custom-$(1)-$(2)

.PHONY: build
build: build-$(1)-$(2)

.PHONY: setup-$(1)-$(2)
setup-$(1)-$(2):
	$$(MAKE) setup-custom-$(1)-$(2)

.PHONY: package-$(1)-$(2)
package-$(1)-$(2): setup-$(1)-$(2) build-$(1)-$(2)

.PHONY: clean-$(1)-$(2)
clean-$(1)-$(2):

.PHONY: clean
clean: clean-$(1)-$(2)

endef

## Products

include runtime.mk
include bcl.mk

## MXE targets
ifeq ($(and $(DISABLE_ANDROID),$(DISABLE_WASM_CROSS)),)
# FIXME add iOS support(?)
include mxe.mk
endif

## LLVM targets
include llvm.mk

## Android targets
ifdef ENABLE_ANDROID
include android.mk
endif

## iOS targets
ifdef ENABLE_IOS
include ios.mk
endif

## Mac targets
ifdef ENABLE_MAC
include mac.mk
endif

## Mac Catalyst targets
ifdef ENABLE_MACCAT
include maccat.mk
endif

## Desktop targets
## To run host-side tests
ifdef ENABLE_DESKTOP
include desktop.mk
endif

## WASM targets
include wasm.mk
