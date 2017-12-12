TOP=$(realpath $(CURDIR)/../..)
include $(TOP)/sdks/paths.mk

RELEASE?=true

CONFIGURATION=$(if $(filter $(RELEASE),true),release,debug)

#brew's libtool is not compatible with some of the deps needed (I.E. V8) so in those systems we need to explicit add to the path
#this is due mono not being compatible with xcode's libtool, which is what's on path by default
ifeq (, $(shell which glibtoolize))
EXTRA_PATH=$(wildcard /usr/local/Cellar/libtool/*/bin/)
endif

$(TOP)/configure: $(TOP)/configure.ac $(TOP)/autogen.sh
	cd $(TOP) && PATH=$(EXTRA_PATH):$$PATH NOCONFIGURE=1 ./autogen.sh

## LLVM

LLVM_REV=a9cfb50e5af3d632f8c1c9620ca52d0118523b53

# Download a prebuilt llvm
# Shared between ios and android
.stamp-llvm-$(LLVM_REV):
	./download-llvm.sh $@ $(LLVM_REV)

clean-llvm:
	$(RM) -r ../out/llvm64
	$(RM) .stamp-llvm-*

TARGETS=

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
