//
// System.ComponentModel.Win32Exception.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.ComponentModel
{
	[Serializable, SuppressUnmanagedCodeSecurity]
	public class Win32Exception : ExternalException
	{
		private int native_error_code;

//		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
#if TARGET_JVM
		[MonoNotSupported("")]
#endif
		public Win32Exception ()
			: base (W32ErrorMessage (Marshal.GetLastWin32Error ()))
		{
			native_error_code = Marshal.GetLastWin32Error ();
		}

//		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public Win32Exception (int error)
			: base (W32ErrorMessage (error))
		{
			native_error_code = error;
		}

//		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public Win32Exception (int error, string message) 
			: base (message)
		{
			native_error_code = error;
		}
#if NET_2_0
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
#if TARGET_JVM
		[MonoNotSupported ("")]
#endif
		public Win32Exception (string message)
			: base (message)
		{
			native_error_code = Marshal.GetLastWin32Error ();
		}

#if TARGET_JVM
		[MonoNotSupported ("")]
#endif
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public Win32Exception (string message, Exception innerException)
			: base (message, innerException)
		{
			native_error_code = Marshal.GetLastWin32Error ();
		}
#endif
		protected Win32Exception(SerializationInfo info,
					 StreamingContext context)
			: base (info, context) {

			native_error_code = info.GetInt32 ("NativeErrorCode");
		}

		public int NativeErrorCode {
			get {
				return(native_error_code);
			}
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("NativeErrorCode", native_error_code);
			base.GetObjectData (info, context);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern string W32ErrorMessage (int error_code);
	}
}
