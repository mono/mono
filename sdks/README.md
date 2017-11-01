Build scripts and sample apps for mono targeting its supported platforms.

# Build instructions

...

# Android NDK

- Mac OS X: [android-ndk-r11c-darwin-x86_64.zip](https://dl.google.com/android/repository/android-ndk-r11c-darwin-x86_64.zip). sha1: 4ce8e7ed8dfe08c5fe58aedf7f46be2a97564696
- Linux 64-bit: [android-ndk-r11c-linux-x86_64.zip](https://dl.google.com/android/repository/android-ndk-r11c-linux-x86_64.zip). sha1: de5ce9bddeee16fb6af2b9117e9566352aa7e279



# Open Tasks

## SDKs

- Build nunit-lite as part of the bcl build and ship it there
- Build all test suites as part of the sdk itself
- Move the ARM_CFLAGS & friends to the android makefile generator

## Android

- Add android SDK probing ~/Library/Developer/Xamarin/android-sdk-macosx.  Figure out if we should switch to Android Studio for SDK related stuff (Google got rid of the old SDK scheme).
- Add remaining Android targets
- Make the bcl build use the same setup as the android targets
- Add cross-compiler targets for Android

### XTC specifics

Most Android devices are crap for testing, so we hand pick those that produce reliable and useful results.

Here's the device set to pick in general:

arm32: HTC One
aarch64: Google Pixel
x86_32: Acer Iconia A1-830


## iOS

Everything

## BCL

- Build depends on building a runtime, fix the build system to support using system mono/csc
- Build will generate all profiles instead of those we care.

## General

Write the iOS driver.

