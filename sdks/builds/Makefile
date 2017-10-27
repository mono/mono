TOP=$(realpath $(CURDIR)/../..)
include $(TOP)/sdks/paths.mk

IS_RELEASE?=true

$(TOP)/configure: $(TOP)/configure.ac $(TOP)/autogen.sh
	cd $(TOP) && NOCONFIGURE=1 ./autogen.sh

TARGETS=

## BCL targets
include bcl.mk

## Android targets
include android.mk

## iOS targets
include ios.mk

## Generic targets
.PHONY: $(foreach target,$(TARGETS),toolchain-$(target))
$(foreach target,$(TARGETS),toolchain-$(target)): toolchain-%: .stamp-%-toolchain

.PHONY: $(foreach target,$(TARGETS),configure-$(target))
$(foreach target,$(TARGETS),configure-$(target)): configure-%: .stamp-%-configure

.PHONY: $(foreach target,$(TARGETS),build-$(target))
$(foreach target,$(TARGETS),build-$(target)): build-%: .stamp-%-configure
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
