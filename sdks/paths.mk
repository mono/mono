-include $(TOP)/sdks/Make.config

#Default paths

SDK_DIR = $(abspath $(HOME)/Library/Developer/Xamarin/android-sdk-macosx/)
NDK_DIR = $(abspath $(HOME)/Library/Developer/Xamarin/android-ndk/android-ndk-r14b)

XCODE_DIR = /Applications/Xcode.app/Contents/Developer

#Probe for alternative paths

#On some systems the sdk is named mac_x86
ifeq ($(and $(wildcard $(SDK_DIR)/tools),$(wildcard $(SDK_DIR)/platform-tools)),)
SDK_DIR = $(abspath $(HOME)/Library/Developer/Xamarin/android-sdk-mac_x86/)
endif

#Latest XS installs NDK in this PATH
ifeq ($(wildcard $(NDK_DIR)/platforms),)
NDK_DIR = $(abspath $(HOME)/Library/Developer/Xamarin/android-sdk-mac_x86/ndk-bundle)
endif

#Error if tools are not found

ifndef DISABLE_ANDROID
ifeq ($(and $(wildcard $(SDK_DIR)/tools),$(wildcard $(SDK_DIR)/platform-tools)),)
$(error Could not find Android SDK in $(SDK_DIR))
endif

ifeq ($(wildcard $(NDK_DIR)/platforms),)
$(error Could not find Android NDK in $(NDK_DIR))
endif

endif

ifndef DISABLE_IOS
ifeq ($(wildcard $(XCODE_DIR)),)
$(error Could not find XCode in $(XCODE_DIR))
endif

endif