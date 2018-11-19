
ifndef DISABLE_IOS

CheckXcodeDir=$(or $(and $(wildcard $(1))),$(warning Could not find Xcode in "$(1)"))

ifeq ($(call CheckXcodeDir,$(XCODE_DIR)),)
$(error Could not find Xcode at $(XCODE_DIR))
endif

endif

ifndef DISABLE_ANDROID

ANDROID_TOOLCHAIN_DIR?=$(HOME)/android-toolchain
ANDROID_TOOLCHAIN_CACHE_DIR?=$(HOME)/android-archives

endif