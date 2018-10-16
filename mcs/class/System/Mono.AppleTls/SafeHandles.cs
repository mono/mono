//
// SafeHandles.Mono.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin Inc. (http://www.xamarin.com)
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
using System.Runtime.InteropServices;
using System.Security.Cryptography.Apple;
using Microsoft.Win32.SafeHandles;
using Mono;

namespace System.Security.Cryptography.X509Certificates
{
	sealed partial class SafeSecIdentityHandle : SafeKeychainItemHandle
	{
		public SafeSecIdentityHandle ()
		{
		}
	}

	sealed partial class SafeSecCertificateHandle : SafeKeychainItemHandle
	{
		public SafeSecCertificateHandle ()
		{
		}
	}
}

namespace System.Security.Cryptography.Apple
{
	partial class SafeKeychainItemHandle : SafeHandle
	{
		internal SafeKeychainItemHandle ()
		    : base (IntPtr.Zero, ownsHandle: true)
		{
		}

		protected override bool ReleaseHandle ()
		{
			if (handle != IntPtr.Zero)
				CFObject.CFRelease (handle);
			SetHandle (IntPtr.Zero);
			return true;
		}

		public override bool IsInvalid => handle == IntPtr.Zero;
	}

	partial class SafeSecKeyRefHandle : SafeKeychainItemHandle
	{
	}
}
