//
// MonoNativePlatform.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mono
{
	/*
	 * The purpose of this class is to be used by test such as for instance
	 * the xamarin-macios test suite to examine and test the Mono.Native
	 * library.
	 */
	static class MonoNativePlatform
	{
		[DllImport ("System.Native")]
		extern static int mono_native_get_platform_type ();

		public static MonoNativePlatformType GetPlatformType ()
		{
			return (MonoNativePlatformType)mono_native_get_platform_type ();
		}

		/*
		 * Test Suite Use Only.
		 */
		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int IncrementInternalCounter ();

		[DllImport ("System.Native")]
		extern static int mono_native_is_initialized ();

		[DllImport ("System.Native")]
		extern static int mono_native_initialize ();

		/*
		 * This method is called by the xamarin-macios test suite
		 * to register the `IncrementInternalCounter` icall.
		 *
		 * It ensures that the native library can call
		 * `mono_add_internal_call_with_flags` and the mtouch and mmp
		 * tools can correctly deal with it.
		 */
		public static void Initialize ()
		{
			mono_native_initialize ();
		}

		public static bool IsInitialized ()
		{
			return mono_native_is_initialized () != 0;
		}

		/*
		 * Test Suite Use Only.
		 */
		internal static int TestInternalCounter ()
		{
			// Atomically increments internal counter, for testing purposes only.
			return IncrementInternalCounter ();
		}
	}
}
