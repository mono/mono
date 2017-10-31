TOP=$(realpath $(CURDIR)/../..)
include $(TOP)/sdks/paths.mk

IS_RELEASE?=true

#brew's libtool is not compatible with some of the deps needed (I.E. V8) so in those systems we need to explicit add to the path
#this is due mono not being compatible with xcode's libtool, which is what's on path by default
ifeq (, $(shell which glibtoolize))
EXTRA_PATH=$(wildcard /usr/local/Cellar/libtool/*/bin/)
endif

$(TOP)/configure: $(TOP)/configure.ac $(TOP)/autogen.sh
	cd $(TOP) && PATH=$(EXTRA_PATH):$$PATH NOCONFIGURE=1 ./autogen.sh

TARGETS=
#Targets with custom build rules should append themselves here and we won't generate a generic target
CUSTOM_BUILD_TARGETS=


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

## WASM targets
ifndef DISABLE_WASM
include wasm.mk
endif

## Generic targets
.PHONY: $(foreach target,$(TARGETS),toolchain-$(target))
$(foreach target,$(TARGETS),toolchain-$(target)): toolchain-%: .stamp-%-toolchain

.PHONY: $(foreach target,$(TARGETS),configure-$(target))
$(foreach target,$(TARGETS),configure-$(target)): configure-%: .stamp-%-configure

.PHONY: $(foreach target,$(TARGETS),build-$(target))
$(filter-out $(CUSTOM_BUILD_TARGETS), $(foreach target,$(TARGETS),build-$(target))): build-%: .stamp-%-configure
	$(MAKE) -C $*

.PHONY: $(foreach target,$(TARGETS),setup-$(target))
$(foreach target,$(TARGETS),setup-$(target)): setup-%:
	mkdir -p $(TOP)/sdks/out/$*

.PHONY: $(foreach target,$(TARGETS),package-$(target))
$(foreach target,$(TARGETS),package-$(target)): package-%: setup-% build-%

.PHONY: $(foreach target,$(TARGETS),clean-$(target))
$(foreach target,$(TARGETS),clean-$(target)):: clean-%:
	rm -rf $(TOP)/sdks/out/$*

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
