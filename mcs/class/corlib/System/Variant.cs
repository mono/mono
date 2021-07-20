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

using System.Runtime.CompilerServices;
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

		[StructLayout(LayoutKind.Sequential)]
		internal unsafe struct DECIMAL
		{
			public ushort wReserved;
			public byte scale;
			public byte sign;
			public int Hi32;
			public ulong Lo64;
		}

		[FieldOffset(0)]
		public DECIMAL decVal;

		public static void SetValueAt(object obj, int vt, IntPtr addr)
		{
			Type t = obj.GetType();
			switch ((VarEnum)vt)
			{
			case VarEnum.VT_I1:
				*((sbyte*)addr) = (sbyte)obj;
				break;
			case VarEnum.VT_UI1:
				*((byte*)addr) = (byte)obj;
				break;
			case VarEnum.VT_I2:
				*((short*)addr) = (short)obj;
				break;
			case VarEnum.VT_UI2:
				*((ushort*)addr) = (ushort)obj;
				break;
			case VarEnum.VT_INT:
			case VarEnum.VT_I4:
				*((int*)addr) = (int)obj;
				break;
			case VarEnum.VT_UINT:
			case VarEnum.VT_UI4:
				*((uint*)addr) = (uint)obj;
				break;
			case VarEnum.VT_I8:
				*((long*)addr) = (long)obj;
				break;
			case VarEnum.VT_UI8:
				*((ulong*)addr) = (ulong)obj;
				break;
			case VarEnum.VT_R4:
				*((float*)addr) = (float)obj;
				break;
			case VarEnum.VT_R8:
				*((double*)addr) = (double)obj;
				break;
			case VarEnum.VT_BOOL:
				*((short*)addr) = (short)((bool)obj ? -1 : 0);
				break;
			case VarEnum.VT_BSTR:
				if (t == typeof(BStrWrapper))
					*((IntPtr*)addr) = Marshal.StringToBSTR(((BStrWrapper)obj).WrappedObject);
				else
					*((IntPtr*)addr) = Marshal.StringToBSTR((string)obj);
				break;
			case VarEnum.VT_CY:
				if (t == typeof(CurrencyWrapper))
					*((long*)addr) = Decimal.ToOACurrency(((CurrencyWrapper)obj).WrappedObject);
				else
					*((long*)addr) = Decimal.ToOACurrency((Decimal)obj);
				break;
			case VarEnum.VT_DATE:
				*((double*)addr) = ((DateTime)obj).ToOADate();
				break;
			case VarEnum.VT_DECIMAL:
			{
				int[] parts = Decimal.GetBits((Decimal)obj);
				DECIMAL* dec = (DECIMAL*)addr;
				dec->scale = (byte)((parts[3] >> 16) & 0x7F);
				dec->sign = (byte)(parts[3] >> 24);
				dec->Hi32 = parts[2];
				dec->Lo64 = (uint)parts[0] | ((ulong)(uint)parts[1] << 32);
				break;
			}
			case VarEnum.VT_ERROR:
				if (t == typeof(ErrorWrapper))
					*((int*)addr) = ((ErrorWrapper)obj).ErrorCode;
				else
					*((int*)addr) = (int)obj;
				break;
			case VarEnum.VT_VARIANT:
			{
				Variant v = default(Variant);
				v.SetValue(obj);
				*((Variant*)addr) = v;
				break;
			}
#if FEATURE_COMINTEROP
			case VarEnum.VT_UNKNOWN:
				if (t == typeof(UnknownWrapper))
					*((IntPtr*)addr) = Marshal.GetIUnknownForObject(((UnknownWrapper)obj).WrappedObject);
				else
					*((IntPtr*)addr) = Marshal.GetIUnknownForObject(obj);
				break;
			case VarEnum.VT_DISPATCH:
				if (t == typeof(DispatchWrapper))
					*((IntPtr*)addr) = Marshal.GetIDispatchForObject(((DispatchWrapper)obj).WrappedObject);
				else
					*((IntPtr*)addr) = Marshal.GetIDispatchForObject(obj);
				break;
#endif
			default:
				if (((VarEnum)vt & VarEnum.VT_ARRAY) != 0)
				{
					int tmp;
					*((IntPtr*)addr) = SafeArrayFromArrayInternal((Array)obj, out tmp);
					break;
				}
				throw new NotImplementedException(string.Format("Variant.SetValueAt couldn't handle VT {0}", vt));
			}
		}

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
			else if (t == typeof(ushort) || t == typeof(char))
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
			else if (t == typeof (CurrencyWrapper))
			{
				vt = (short)VarEnum.VT_CY;
				llVal = Decimal.ToOACurrency(((CurrencyWrapper)obj).WrappedObject);
			}
			else if (t == typeof (DateTime))
			{
				vt = (short)VarEnum.VT_DATE;
				dblVal = ((DateTime)obj).ToOADate();
			}
			else if (t == typeof (Decimal))
			{
				vt = (short)VarEnum.VT_DECIMAL;
				int[] parts = Decimal.GetBits((Decimal)obj);
				decVal.scale = (byte)((parts[3] >> 16) & 0x7F);
				decVal.sign = (byte)(parts[3] >> 24);
				decVal.Hi32 = parts[2];
				decVal.Lo64 = (uint)parts[0] | ((ulong)(uint)parts[1] << 32);
			}
			else if (t == typeof (ErrorWrapper))
			{
				vt = (short)VarEnum.VT_ERROR;
				lVal = ((ErrorWrapper)obj).ErrorCode;
			}
#if FEATURE_COMINTEROP
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
			else if (t.UnderlyingSystemType.IsArray)
			{
				int tmp;
				pdispVal = SafeArrayFromArrayInternal((Array)obj, out tmp);
				vt = (short)(tmp | (int)VarEnum.VT_ARRAY);
			}
			else
			{
#if !FEATURE_COMINTEROP
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

		public static object GetValueAt(int vt, IntPtr addr)
		{
			object obj = null;
			switch ((VarEnum)vt)
			{
			case VarEnum.VT_I1:
				obj = (sbyte)Marshal.ReadByte(addr);
				break;
			case VarEnum.VT_UI1:
				obj = Marshal.ReadByte(addr);
				break;
			case VarEnum.VT_I2:
				obj = Marshal.ReadInt16(addr);
				break;
			case VarEnum.VT_UI2:
				obj = (ushort)Marshal.ReadInt16(addr);
				break;
			case VarEnum.VT_ERROR:
			case VarEnum.VT_INT:
			case VarEnum.VT_I4:
				obj = Marshal.ReadInt32(addr);
				break;
			case VarEnum.VT_UINT:
			case VarEnum.VT_UI4:
				obj = (uint)Marshal.ReadInt32(addr);
				break;
			case VarEnum.VT_I8:
				obj = Marshal.ReadInt64(addr);
				break;
			case VarEnum.VT_UI8:
				obj = (ulong)Marshal.ReadInt64(addr);
				break;
			case VarEnum.VT_R4:
				obj = Marshal.PtrToStructure(addr, typeof(float));
				break;
			case VarEnum.VT_R8:
				obj = Marshal.PtrToStructure(addr, typeof(double));
				break;
			case VarEnum.VT_BOOL:
				obj = !(Marshal.ReadInt16(addr) == 0);
				break;
			case VarEnum.VT_BSTR:
			{
				IntPtr ptr = Marshal.ReadIntPtr (addr);
				if (ptr != IntPtr.Zero)
					obj = Marshal.PtrToStringBSTR (ptr);
				else
					obj = null;
				break;
			}
			case VarEnum.VT_CY:
				obj = Decimal.FromOACurrency(Marshal.ReadInt64(addr));
				break;
			case VarEnum.VT_DATE:
				obj = DateTime.FromOADate((double)Marshal.PtrToStructure(addr, typeof(double)));
				break;
			case VarEnum.VT_DECIMAL:
			{
				DECIMAL* dec = (DECIMAL*)addr;
				obj = new Decimal((int)dec->Lo64, (int)(dec->Lo64 >> 32), dec->Hi32, dec->sign != 0, dec->scale);
				break;
			}
			case VarEnum.VT_RECORD:
				throw new NotImplementedException("VT_RECORD not implemented");
// GetObjectForIUnknown is excluded from Marshal using FULL_AOT_RUNTIME
#if !DISABLE_COM
			case VarEnum.VT_UNKNOWN:
			case VarEnum.VT_DISPATCH:
			{
				IntPtr ifaceaddr = Marshal.ReadIntPtr(addr);
				if (ifaceaddr != IntPtr.Zero)
					obj = Marshal.GetObjectForIUnknown(ifaceaddr);
				break;
			}
#endif
			case VarEnum.VT_VARIANT:
			{
				Variant v = *((Variant*)addr);
				obj = v.GetValue();
				break;
			}
			default:
				if (((VarEnum)vt & VarEnum.VT_ARRAY) != 0)
				{
					obj = SafeArrayToArrayInternal(*((IntPtr*)addr), vt & ~(short)VarEnum.VT_ARRAY);
				}
				break;
			}
			return obj;
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
			case VarEnum.VT_ERROR:
			case VarEnum.VT_INT:
			case VarEnum.VT_I4:
				obj = lVal;
				break;
			case VarEnum.VT_UINT:
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
				if (bstrVal != IntPtr.Zero)
					obj = Marshal.PtrToStringBSTR(bstrVal);
				else
					obj = null;
				break;
			case VarEnum.VT_CY:
				obj = Decimal.FromOACurrency(llVal);
				break;
			case VarEnum.VT_DATE:
				obj = DateTime.FromOADate(dblVal);
				break;
			case VarEnum.VT_DECIMAL:
				obj = new Decimal((int)decVal.Lo64, (int)(decVal.Lo64 >> 32), decVal.Hi32, decVal.sign != 0, decVal.scale);
				break;
			case VarEnum.VT_RECORD:
				throw new NotImplementedException("VT_RECORD not implemented");
#if FEATURE_COMINTEROP
			case VarEnum.VT_UNKNOWN:
			case VarEnum.VT_DISPATCH:
				if (pdispVal != IntPtr.Zero)
					obj = Marshal.GetObjectForIUnknown(pdispVal);
				break;
#endif
			default:
				if (((VarEnum)vt & VarEnum.VT_BYREF) == VarEnum.VT_BYREF &&
					pdispVal != IntPtr.Zero)
				{
					obj = GetValueAt(vt & ~(short)VarEnum.VT_BYREF, pdispVal);
				}
				else if (((VarEnum)vt & VarEnum.VT_ARRAY) != 0)
				{
					obj = SafeArrayToArrayInternal(pdispVal, vt & ~(short)VarEnum.VT_ARRAY);
				}
				break;
			}
			return obj;
		}

		public void Clear ()
		{
			if ((VarEnum)vt == VarEnum.VT_BSTR) {
				Marshal.FreeBSTR (bstrVal);
			}
#if !DISABLE_COM
			else if ((VarEnum)vt == VarEnum.VT_DISPATCH || (VarEnum)vt == VarEnum.VT_UNKNOWN) {
				if (pdispVal != IntPtr.Zero)
					Marshal.Release (pdispVal);
			}
			else if (((VarEnum)vt & VarEnum.VT_ARRAY) != 0) {
				SafeArrayDestroyInternal (pdispVal);
			}
#endif
			vt = (short)VarEnum.VT_EMPTY;
		}

#if !DISABLE_COM
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void SafeArrayDestroyInternal (IntPtr safearray);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static IntPtr SafeArrayFromArrayInternal (Array array, out int vt);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static Array SafeArrayToArrayInternal (IntPtr safearray, int vt);
#endif
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct BRECORD
	{
        #pragma warning disable 169
		public IntPtr pvRecord;
		public IntPtr pRecInfo;
        #pragma warning restore 169
	}
}
