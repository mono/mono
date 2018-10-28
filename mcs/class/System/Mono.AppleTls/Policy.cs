// 
// Policy.cs: Implements the managed SecPolicy wrapper.
//
// Authors: 
//	Miguel de Icaza
//  Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2010 Novell, Inc
// Copyright 2012-2014 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Runtime.InteropServices;
using ObjCRuntimeInternal;
using Mono.Net;

namespace Mono.AppleTls {
	partial class SecPolicy : INativeObject, IDisposable {
		IntPtr handle;

		internal SecPolicy (IntPtr handle, bool owns = false)
		{
			if (handle == IntPtr.Zero)
				throw new Exception ("Invalid handle");

			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static IntPtr /* SecPolicyRef */ SecPolicyCreateSSL (bool server, IntPtr /* CFStringRef */ hostname);

		static public SecPolicy CreateSslPolicy (bool server, string hostName)
		{
			CFString host = hostName == null ? null : CFString.Create (hostName);
			IntPtr handle = host == null ? IntPtr.Zero : host.Handle; 
			SecPolicy policy = new SecPolicy (SecPolicyCreateSSL (server, handle), true);
			if (host != null)
				host.Dispose ();
			return policy;
		}

		~SecPolicy ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public IntPtr Handle {
			get { return handle; }
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
