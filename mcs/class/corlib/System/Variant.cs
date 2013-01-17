//
// System.Variant
//
// Authors:
//   Jonathan Chambers <jonathan.chambers@ansys.com>
//
// Copyright (C) 2006 Novell (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System
{
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct Variant
	{
		[FieldOffset(0)]
		public short vt;

		[FieldOffset(2)]
		public ushort wReserved1;

		[FieldOffset(4)]
		public ushort wReserved2;

		[FieldOffset(6)]
		public ushort wReserved3;

		[FieldOffset(8)]
		public long llVal;

		[FieldOffset(8)]
		public int lVal;

		[FieldOffset(8)]
		public byte bVal;

		[FieldOffset(8)]
		public short iVal;

		[FieldOffset(8)]
		public float fltVal;

		[FieldOffset(8)]
		public double dblVal;

		[FieldOffset(8)]
		public short boolVal;

		[FieldOffset(8)]
		public IntPtr bstrVal;

		[FieldOffset(8)]
		public sbyte cVal;

		[FieldOffset(8)]
		public ushort uiVal;

		[FieldOffset(8)]
		public uint ulVal;

		[FieldOffset(8)]
		public ulong ullVal;

		[FieldOffset(8)]
		public int intVal;

		[FieldOffset(8)]
		public uint uintVal;

		[FieldOffset(8)]
		public IntPtr pdispVal;

		[FieldOffset(8)]
		public BRECORD bRecord;

		public void SetValue(object obj) {
			vt = (short)VarEnum.VT_EMPTY;
			if (obj == null)
				return;

			Type t = obj.GetType();
			if (t.IsEnum)
				t = Enum.GetUnderlyingType (t);

			if (t == typeof(sbyte))
			{
				vt = (short)VarEnum.VT_I1;
				cVal = (sbyte)obj;
			}
			else if (t == typeof(byte))
			{
				vt = (short)VarEnum.VT_UI1;
				bVal = (byte)obj;
			}
			else if (t == typeof(short))
			{
				vt = (short)VarEnum.VT_I2;
				iVal = (short)obj;
			}
			else if (t == typeof(ushort))
			{
				vt = (short)VarEnum.VT_UI2;
				uiVal = (ushort)obj;
			}
			else if (t == typeof(int))
			{
				vt = (short)VarEnum.VT_I4;
				lVal = (int)obj;
			}
			else if (t == typeof(uint))
			{
				vt = (short)VarEnum.VT_UI4;
				ulVal = (uint)obj;
			}
			else if (t == typeof(long))
			{
				vt = (short)VarEnum.VT_I8;
				llVal = (long)obj;
			}
			else if (t == typeof(ulong))
			{
				vt = (short)VarEnum.VT_UI8;
				ullVal = (ulong)obj;
			}
			else if (t == typeof(float))
			{
				vt = (short)VarEnum.VT_R4;
				fltVal = (float)obj;
			}
			else if (t == typeof(double))
			{
				vt = (short)VarEnum.VT_R8;
				dblVal = (double)obj;
			}
			else if (t == typeof(string))
			{
				vt = (short)VarEnum.VT_BSTR;
				bstrVal = Marshal.StringToBSTR((string)obj);
			}
			else if (t == typeof(bool))
			{
				vt = (short)VarEnum.VT_BOOL;
				lVal = ((bool)obj) ? -1 : 0;
			}
			else if (t == typeof (BStrWrapper))
			{
				vt = (short)VarEnum.VT_BSTR;
				bstrVal = Marshal.StringToBSTR(((BStrWrapper)obj).WrappedObject);
			}
#if !FULL_AOT_RUNTIME
			else if (t == typeof (UnknownWrapper))
			{
				vt = (short)VarEnum.VT_UNKNOWN;
				pdispVal = Marshal.GetIUnknownForObject(((UnknownWrapper)obj).WrappedObject);
			}
			else if (t == typeof (DispatchWrapper))
			{
				vt = (short)VarEnum.VT_DISPATCH;
				pdispVal = Marshal.GetIDispatchForObject(((DispatchWrapper)obj).WrappedObject);
			}
#endif
			else
			{
#if FULL_AOT_RUNTIME
				throw new NotImplementedException(string.Format("Variant couldn't handle object of type {0}", obj.GetType()));
#else
				try 
				{
					pdispVal = Marshal.GetIDispatchForObject(obj);
					vt = (short)VarEnum.VT_DISPATCH;
					return;
				}
				catch { }
				try 
				{
					vt = (short)VarEnum.VT_UNKNOWN;
					pdispVal = Marshal.GetIUnknownForObject(obj);
				}
				catch (Exception ex)
				{
					throw new NotImplementedException(string.Format("Variant couldn't handle object of type {0}", obj.GetType()), ex);
				}
#endif
			}
		}

		public object GetValue() {
			object obj = null;
			switch ((VarEnum)vt)
			{
			case VarEnum.VT_I1:
			obj = cVal;
			break;
			case VarEnum.VT_UI1:
				obj = bVal;
				break;
			case VarEnum.VT_I2:
				obj = iVal;
				break;
			case VarEnum.VT_UI2:
				obj = uiVal;
				break;
			case VarEnum.VT_I4:
				obj = lVal;
				break;
			case VarEnum.VT_UI4:
				obj = ulVal;
				break;
			case VarEnum.VT_I8:
				obj = llVal;
				break;
			case VarEnum.VT_UI8:
				obj = ullVal;
				break;
			case VarEnum.VT_R4:
				obj = fltVal;
				break;
			case VarEnum.VT_R8:
				obj = dblVal;
				break;
			case VarEnum.VT_BOOL:
				obj = !(boolVal == 0);
				break;
			case VarEnum.VT_BSTR:
				obj = Marshal.PtrToStringBSTR(bstrVal);
				break;
#if !FULL_AOT_RUNTIME
			case VarEnum.VT_UNKNOWN:
			case VarEnum.VT_DISPATCH:
				if (pdispVal != IntPtr.Zero)
					obj = Marshal.GetObjectForIUnknown(pdispVal);
				break;
#endif
			}
			return obj;
		}

		public void Clear ()
		{
			if ((VarEnum)vt == VarEnum.VT_BSTR) {
				Marshal.FreeBSTR (bstrVal);
			}
			else if ((VarEnum)vt == VarEnum.VT_DISPATCH || (VarEnum)vt == VarEnum.VT_UNKNOWN) {
				if (pdispVal != IntPtr.Zero)
					Marshal.Release (pdispVal);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct BRECORD
	{
        #pragma warning disable 169
		IntPtr pvRecord;
		IntPtr pRecInfo;
        #pragma warning restore 169
	}
}
