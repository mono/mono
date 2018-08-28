TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config

MAKEFLAGS += --no-builtin-rules

CONFIGURATION?=release

RELEASE=$(if $(filter $(CONFIGURATION),release),1)

CCACHE:=$(if $(DISABLE_CCACHE),,$(shell which ccache))
NINJA:=$(shell which ninja)

UNAME=$(shell uname)
ifneq ($(UNAME),Darwin)
# iOS requires Xcode to be available, and Xcode is only available on macOS
DISABLE_IOS=1
endif

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
	cd $(TOP) && PATH=$(EXTRA_PATH):$$PATH NOCONFIGURE=1 ./autogen.sh

TARGETS=

## MXE targets
ifndef DISABLE_ANDROID
# FIXME add iOS support(?)
include mxe.mk
endif

## LLVM targets
ifeq ($(and $(DISABLE_ANDROID),$(DISABLE_IOS)),)
include llvm.mk
endif

## BCL targets
ifndef DISABLE_BCL
include bcl.mk
endif

## Android targets
ifndef DISABLE_ANDROID
include android.mk
endif

## iOS targets
ifndef DISABLE_IOS
include ios.mk
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

## Generic targets
.PHONY: $(foreach target,$(TARGETS),toolchain-$(target))
$(foreach target,$(TARGETS),toolchain-$(target)): toolchain-%: .stamp-%-toolchain

$(foreach target,$(TARGETS),.stamp-$(target)-configure): .stamp-%-configure: .stamp-%-toolchain

.PHONY: $(foreach target,$(TARGETS),configure-$(target))
$(foreach target,$(TARGETS),configure-$(target)): configure-%: .stamp-%-configure

.PHONY: build-custom-%
build-custom-%:
	$(MAKE) -C $*

.PHONY: $(foreach target,$(TARGETS),build-$(target))
$(foreach target,$(TARGETS),build-$(target)): build-%: .stamp-%-configure
	$(MAKE) build-custom-$*

.PHONY: setup-custom-%
setup-custom-%:
	mkdir -p $(TOP)/sdks/out/$*

.PHONY: $(foreach target,$(TARGETS),setup-$(target))
$(foreach target,$(TARGETS),setup-$(target)): setup-%:
	$(MAKE) setup-custom-$*

.PHONY: $(foreach target,$(TARGETS),package-$(target))
$(foreach target,$(TARGETS),package-$(target)): package-%: setup-% build-%

.PHONY: $(foreach target,$(TARGETS),clean-$(target))
$(foreach target,$(TARGETS),clean-$(target)): clean-%:

## Global targets
.PHONY: toolchain
toolchain: $(foreach target,$(TARGETS),toolchain-$(target))

.PHONY: configure
configure: $(foreach target,$(TARGETS),configure-$(target))

.PHONY: build
build: $(foreach target,$(TARGETS),build-$(target))

.PHONY: package
package: $(foreach target,$(TARGETS),package-$(target))

.PHONY: clean
clean: $(foreach target,$(TARGETS),clean-$(target))

all: package
