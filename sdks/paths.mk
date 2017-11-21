-include $(TOP)/sdks/Make.config

ifndef DISABLE_ANDROID

CheckSdkDir=$(or $(and $(wildcard $(1)/tools),$(wildcard $(1)/platform-tools)),$(warning Could not find Android SDK in "$(1)"))

ifneq ($(call CheckSdkDir,$(HOME)/android-toolchain/sdk),)
SDK_DIR = $(abspath $(HOME)/android-toolchain/sdk)
else
ifneq ($(call CheckSdkDir,$(HOME)/Library/Developer/Xamarin/android-sdk-macosx),)
SDK_DIR = $(abspath $(HOME)/Library/Developer/Xamarin/android-sdk-macosx)
else
ifneq ($(call CheckSdkDir,$(HOME)/Library/Developer/Xamarin/android-sdk-mac_x86),)
SDK_DIR = $(abspath $(HOME)/Library/Developer/Xamarin/android-sdk-mac_x86)
else
$(error Could not find Android SDK)
endif
endif
endif

CheckNdkDir=$(or $(and $(wildcard $(1)/platforms)),$(warning Could not find Android NDK in "$(1)"))

ifneq ($(call CheckNdkDir,$(HOME)/android-toolchain/ndk),)
NDK_DIR=$(abspath $(HOME)/android-toolchain/ndk)
else
ifneq ($(call CheckNdkDir,$(HOME)/Library/Developer/Xamarin/android-ndk/android-ndk-r14b),)
NDK_DIR=$(abspath $(HOME)/Library/Developer/Xamarin/android-ndk/android-ndk-r14b)
else
ifneq ($(call CheckNdkDir,$(HOME)/Library/Developer/Xamarin/android-sdk-mac_x86/ndk-bundle),)
NDK_DIR=$(abspath $(HOME)/Library/Developer/Xamarin/android-sdk-mac_x86/ndk-bundle)
else
$(error Could not find Android NDK)
endif
endif
endif

endif # DISABLE_ANDROID

ifndef DISABLE_IOS

CheckXcodeDir=$(or $(and $(wildcard $(1))),$(warning Could not find Xcode in "$(1)"))

ifneq ($(call CheckXcodeDir,/Applications/Xcode.app/Contents/Developer),)
XCODE_DIR=/Applications/Xcode.app/Contents/Developer
else
$(error Could not find Xcode)
endif

endif # DISABLE_IOS
