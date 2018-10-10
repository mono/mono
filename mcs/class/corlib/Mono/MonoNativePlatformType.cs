//
// MonoNativePlatformType.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace Mono
{
	// Keep in sync with mono/native/mono-native-platform-type.h
	[Flags]
	enum MonoNativePlatformType
	{
		MONO_NATIVE_PLATFORM_TYPE_UNKNOWN	= 0,
		MONO_NATIVE_PLATFORM_TYPE_MACOS		= 1,
		MONO_NATIVE_PLATFORM_TYPE_IOS		= 2,
		MONO_NATIVE_PLATFORM_TYPE_LINUX		= 3,

		MONO_NATIVE_PLATFORM_TYPE_IPHONE	= 0x100,
		MONO_NATIVE_PLATFORM_TYPE_TV		= 0x200,
		MONO_NATIVE_PLATFORM_TYPE_WATCH		= 0x400,

		MONO_NATIVE_PLATFORM_TYPE_COMPAT	= 0x1000,
		MONO_NATIVE_PLATFORM_TYPE_UNIFIED	= 0x2000,

		MONO_NATIVE_PLATFORM_TYPE_SIMULATOR	= 0x4000,
		MONO_NATIVE_PLATFORM_TYPE_DEVICE	= 0x8000
	}
}
