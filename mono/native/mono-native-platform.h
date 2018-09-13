#pragma once

#include "mono/utils/mono-publib.h"

// Keep in sync with mcs/class/System/Mono/MonoNativePlatformType.cs

typedef enum {
	MONO_NATIVE_PLATFORM_TYPE_UNKNOWN	= 0,
	MONO_NATIVE_PLATFORM_TYPE_MACOS		= 1,
	MONO_NATIVE_PLATFORM_TYPE_IOS		= 2,
	MONO_NATIVE_PLATFORM_TYPE_LINUX		= 3,

	MONO_NATIVE_PLATFORM_TYPE_SIMULATOR	= 0x1000,
	MONO_NATIVE_PLATFORM_TYPE_DEVICE	= 0x2000,

	MONO_NATIVE_PLATFORM_TYPE_COMPAT	= 0x4000,
	MONO_NATIVE_PLATFORM_TYPE_UNIFIED	= 0x8000
} MonoNativePlatformType;

MONO_API int32_t
mono_native_get_platform_type (void);
