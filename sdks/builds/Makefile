TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config

all: package

MAKEFLAGS += --no-builtin-rules

CONFIGURATION?=release

RELEASE=$(if $(filter $(CONFIGURATION),release),1)

lowercase=$(shell echo "$(1)" | tr '[:upper:]' '[:lower:]')

UNAME=$(shell uname)

ifneq (,$(findstring CYGWIN,$(UNAME)))
UNAME=Windows
endif

ifeq ($(UNAME),Linux)
UNAME_WSL_CHECK=$(shell uname -a)
ifneq (,$(findstring Microsoft,$(UNAME_WSL_CHECK)))
UNAME=Windows
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
DISABLE_IOS=1
DISABLE_MAC=1
endif

# On Windows, we will just trigger LLVM and Android builds using this Makefile.
ifeq ($(UNAME),Windows)
DISABLE_IOS=1
DISABLE_MAC=1
DISABLE_WASM=1
DISABLE_WASM_CROSS=1
DISABLE_DESKTOP=1
DISABLE_CCACHE=1
DISABLE_LIBTOOLIZE=1
IGNORE_PROVISION_MXE=1
endif

CCACHE:=$(if $(DISABLE_CCACHE),,$(shell which ccache))
NINJA:=$(shell which ninja)

include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk

#brew's libtool is not compatible with some of the deps needed (I.E. V8) so in those systems we need to explicit add to the path
#this is due mono not being compatible with xcode's libtool, which is what's on path by default
ifeq (, $(shell which glibtoolize))
EXTRA_PATH=$(wildcard /usr/local/Cellar/libtool/*/bin/)
endif

all: package

## Common Mono targets

.PHONY: configure-mono
configure-mono: $(TOP)/configure

$(TOP)/configure: $(TOP)/configure.ac $(TOP)/autogen.sh
	cd $(TOP) && PATH=$(EXTRA_PATH):$$PATH NOCONFIGURE=1 ./autogen.sh $(if $(wildcard $(TOP)/../mono-extensions),--enable-extension-module=xamarin --enable-extension-module)

## Archive targets

ifndef DISABLE_ANDROID
android_ARCHIVE=
endif

ifndef DISABLE_IOS
ios_ARCHIVE=
endif

ifndef DISABLE_MAC
mac_ARCHIVE=
endif

ifndef DISABLE_WASM
wasm_ARCHIVE=
endif

##
# Parameters:
#  $(1): target (android, ios, mac, wasm)
define ArchiveTemplate
_$(1)_HASH = $$(shell git -C $$(TOP) rev-parse HEAD)
_$(1)_PACKAGE = $(1)-$$(CONFIGURATION)-$$(UNAME)-$$(_$(1)_HASH).zip

.PHONY: archive-$(1)
archive-$(1):
	cd $$(TOP)/sdks/out && 7z a $$(TOP)/$$(_$(1)_PACKAGE) $$(sort $$($(1)_ARCHIVE))
endef

ifndef DISABLE_ANDROID
$(eval $(call ArchiveTemplate,android))
endif

ifndef DISABLE_IOS
$(eval $(call ArchiveTemplate,ios))
endif

ifndef DISABLE_MAC
$(eval $(call ArchiveTemplate,mac))
endif

ifndef DISABLE_WASM
$(eval $(call ArchiveTemplate,wasm))
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

.PHONY: package
package: package-$(1)-$(2)

.PHONY: clean-$(1)-$(2)
clean-$(1)-$(2):

.PHONY: clean
clean: clean-$(1)-$(2)

endef

## Products

include runtime.mk
include bcl.mk

## MXE targets
ifneq ($(UNAME),Windows)
ifeq ($(and $(DISABLE_ANDROID),$(DISABLE_WASM_CROSS)),)
# FIXME add iOS support(?)
include mxe.mk
endif
endif

## LLVM targets
include llvm.mk

## Android targets
ifndef DISABLE_ANDROID
include android.mk
endif

## iOS targets
ifndef DISABLE_IOS
include ios.mk
endif

## Mac targets
ifndef DISABLE_MAC
include mac.mk
endif

## Desktop targets
## To run host-side tests
ifndef DISABLE_DESKTOP
include desktop.mk
endif

## WASM targets
ifndef DISABLE_WASM
include wasm.mk
endif
