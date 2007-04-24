//
// System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Jonathan Chambers (joncham@gmail.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2007 Jonathan Chambers
//

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
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Runtime.InteropServices.CustomMarshalers
{
	public class EnumeratorToEnumVariantMarshaler : ICustomMarshaler
	{
		static EnumeratorToEnumVariantMarshaler instance;
		public void CleanUpManagedData (object pManagedObj) {
			throw new NotImplementedException ();
		}

		public void CleanUpNativeData (IntPtr pNativeData) {
			Marshal.Release (pNativeData);
		}

		public static ICustomMarshaler GetInstance (string pstrCookie) {
			if (instance == null)
				instance = new EnumeratorToEnumVariantMarshaler ();
			return instance;
		}

		public int GetNativeDataSize () {
			throw new NotImplementedException ();
		}

		public IntPtr MarshalManagedToNative (object pManagedObj) {
			throw new NotImplementedException ();
		}

		public object MarshalNativeToManaged (IntPtr pNativeData) {
			IEnumVARIANT ienumvariant = (IEnumVARIANT)Marshal.GetObjectForIUnknown (pNativeData);
			VARIANTEnumerator e = new VARIANTEnumerator (ienumvariant);
			return e;
		}

		[ComImport]
		[Guid ("00020404-0000-0000-C000-000000000046")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		interface IEnumVARIANT
		{
			[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void Next (int celt, [MarshalAs (UnmanagedType.Struct)]out object rgvar, out uint pceltFetched);
			[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void Skip (uint celt);
			[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void Reset ();
			[return: MarshalAs (UnmanagedType.Interface)]
			[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			IEnumVARIANT Clone ();

		}

		class VARIANTEnumerator : IEnumerator
		{
			IEnumVARIANT com_enum;
			object current;
			public VARIANTEnumerator (IEnumVARIANT com_enum) {
				this.com_enum = com_enum;
			}
			#region IEnumerator Members

			public object Current {
				get {
					return current;
				}
			}

			public bool MoveNext () {
				object val;
				uint fetched = 0;
				com_enum.Next (1, out val, out fetched);
				if (fetched == 0)
					return false;
				current = val;
				return true;
			}

			public void Reset () {
				com_enum.Reset ();
			}

			#endregion
		}
	}
}
