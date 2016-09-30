//
// MonoBtlsX509NameList.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
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
#if SECURITY_DEP
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.Btls
{
	class MonoBtlsX509NameList : MonoBtlsObject
	{
		internal class BoringX509NameListHandle : MonoBtlsHandle
		{
			bool dontFree;

			internal BoringX509NameListHandle (IntPtr handle, bool ownsHandle)
				: base (handle, ownsHandle)
			{
				this.dontFree = !ownsHandle;
			}

			protected override bool ReleaseHandle ()
			{
				if (!dontFree)
					mono_btls_x509_name_list_free (handle);
				return true;
			}
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static IntPtr mono_btls_x509_name_list_new ();

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_x509_name_list_get_count (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_x509_name_list_add (IntPtr handle, IntPtr name);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static IntPtr mono_btls_x509_name_list_get_item (IntPtr handle, int index);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_x509_name_list_free (IntPtr handle);

		new internal BoringX509NameListHandle Handle {
			get { return (BoringX509NameListHandle)base.Handle; }
		}

		internal MonoBtlsX509NameList (BoringX509NameListHandle handle)
			: base (handle)
		{
		}

		internal MonoBtlsX509NameList ()
			: this (Create_internal ())
		{
		}

		static BoringX509NameListHandle Create_internal ()
		{
			var handle = mono_btls_x509_name_list_new ();
			if (handle == IntPtr.Zero)
				throw new MonoBtlsException ();
			return new BoringX509NameListHandle (handle, true);
		}

		public int GetCount ()
		{
			CheckThrow ();
			return mono_btls_x509_name_list_get_count (
				Handle.DangerousGetHandle ());
		}

		public MonoBtlsX509Name GetItem (int index)
		{
			CheckThrow ();
			if (index < 0 || index >= GetCount ())
				throw new ArgumentOutOfRangeException ();
			var ptr = mono_btls_x509_name_list_get_item (
				Handle.DangerousGetHandle (), index);
			if (ptr == IntPtr.Zero)
				return null;
			return new MonoBtlsX509Name (
				new MonoBtlsX509Name.BoringX509NameHandle (ptr, true));
		}

		public void Add (MonoBtlsX509Name name)
		{
			CheckThrow ();
			mono_btls_x509_name_list_add (
				Handle.DangerousGetHandle (),
				name.Handle.DangerousGetHandle ());
		}
	}
}
#endif
