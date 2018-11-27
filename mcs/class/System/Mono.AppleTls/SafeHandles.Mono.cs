//
// SafeHandles.cs
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
	partial class SafeSecIdentityHandle
	{
		public SafeSecIdentityHandle (IntPtr handle, bool ownsHandle = false)
			: base (handle, ownsHandle)
		{
		}
	}

	partial class SafeSecCertificateHandle
	{
		public SafeSecCertificateHandle (IntPtr handle, bool ownsHandle = false)
			: base (handle, ownsHandle)
		{
		}
	}
}

namespace System.Security.Cryptography.Apple
{
	partial class SafeKeychainItemHandle
	{
		public SafeKeychainItemHandle (IntPtr handle, bool ownsHandle)
			: base (handle, ownsHandle)
		{
			if (!ownsHandle)
				CFObject.CFRetain (handle);
		}
	}

	partial class SafeSecKeyRefHandle
	{
		public SafeSecKeyRefHandle (IntPtr handle, bool ownsHandle = false)
			: base (handle, ownsHandle)
		{
		}

		IntPtr owner;

		/*
		 * SecItemImport() returns a SecArrayRef.  We need to free the array, not the items inside it.
		 * 
		 */
		public SafeSecKeyRefHandle (IntPtr handle, IntPtr owner)
			: base (handle, false)
		{
			this.owner = owner;
			CFObject.CFRetain (owner);
		}

		protected override bool ReleaseHandle ()
		{
			if (owner != IntPtr.Zero) {
				CFObject.CFRelease (owner);
				owner = IntPtr.Zero;
				SetHandle (IntPtr.Zero);
				return true;
			}
			return base.ReleaseHandle ();
		}
	}

}
