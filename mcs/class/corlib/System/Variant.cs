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
		public IntPtr parray;

		[FieldOffset(8)]
		public BRECORD bRecord;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SAFEARRAYBOUND {
			public UInt32 cElements;
			public UInt32 lLbound;
		};

		[DllImport ("oleaut32.dll", EntryPoint = "SafeArrayCreate")]
		static extern IntPtr _SafeArrayCreate(
			VarEnum vt,
			UInt32 cDims,
			SAFEARRAYBOUND[] rgsabound);

		[DllImport ("oleaut32.dll", EntryPoint = "SafeArrayDestroy")]
		static extern int _SafeArrayDestroy(
			IntPtr psa);

		bool do_array(object obj) {
			Type elem_type = obj.GetType().GetElementType();
			Array arr = (Array)obj;
			VarEnum elem_vt;
			int sz;

			if (elem_type == typeof(sbyte))
			{
				elem_vt = VarEnum.VT_I1;
				sz = 1;
			}
			else if (elem_type == typeof(byte))
			{
				elem_vt = VarEnum.VT_UI1;
				sz = 1;
			}
			else if (elem_type == typeof(short))
			{
				elem_vt = VarEnum.VT_I2;
				sz = 2;
			}
			else if (elem_type == typeof(ushort))
			{
				elem_vt = VarEnum.VT_UI2;
				sz = 2;
			}
			else if (elem_type == typeof(int))
			{
				elem_vt = VarEnum.VT_I4;
				sz = 4;
			}
			else if (elem_type == typeof(uint))
			{
				elem_vt = VarEnum.VT_UI4;
				sz = 4;
			}
			else if (elem_type == typeof(long))
			{
				elem_vt = VarEnum.VT_I8;
				sz = 8;
			}
			else if (elem_type == typeof(ulong))
			{
				elem_vt = VarEnum.VT_UI8;
				sz = 8;
			}
			else if (elem_type == typeof(float))
			{
				elem_vt = VarEnum.VT_R4;
				sz = 4;
			}
			else if (elem_type == typeof(double))
			{
				elem_vt = VarEnum.VT_R8;
				sz = 8;
			}
			else
				return false;

			SAFEARRAYBOUND[] bounds = new SAFEARRAYBOUND[arr.Rank];

			int num_bytes = 0;

			for (int i = 0; i < arr.Rank; ++i)
			{
				num_bytes += arr.GetLength(i);
				bounds[i].cElements = (uint)arr.GetLength(i);
				bounds[i].lLbound = (uint)arr.GetLowerBound(i);
			}

			num_bytes *= sz;

			vt = (short)(VarEnum.VT_ARRAY | elem_vt);
			parray = _SafeArrayCreate(elem_vt, (uint)arr.Rank, bounds);

			IntPtr pvData;
			if (sizeof(IntPtr) == 4)
				pvData = Marshal.ReadIntPtr(parray, 12); /* offset of SAFEARRAY.pvData */
			else if (sizeof(IntPtr) == 8)
				pvData = Marshal.ReadIntPtr(parray, 16); /* offset of SAFEARRAY.pvData */
			else
			{
				/* unsupported architecture */
				_SafeArrayDestroy(parray);
				return false;
			}

			GCHandle pinned = GCHandle.Alloc(arr);

			if (arr.Rank == 1)
			{
				/* fast path for single-dimension arrays */
				IntPtr src = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);
				for (int i = 0; i < num_bytes; ++i)
				{
					Marshal.WriteByte(pvData, i, Marshal.ReadByte(src, i));
				}
			}
			else
			{
				/* multidimensional SAFEARRAY data is stored right-most index first */
				IntPtr src = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);

				int[] idxs = new int[arr.Rank];
				int in_ofs = 0;
				int out_ofs = 0;
				int stride = 1;
				for (int i = 0; i < arr.Rank - 1; ++i)
					stride *= arr.GetLength(i);
				stride *= sz;

				while (true)
				{
					switch (sz){
					case 1:
						Marshal.WriteByte(pvData, out_ofs, Marshal.ReadByte(src, in_ofs));
						break;

					case 2:
						Marshal.WriteInt16(pvData, out_ofs, Marshal.ReadInt16(src, in_ofs));
						break;

					case 4:
						Marshal.WriteInt32(pvData, out_ofs, Marshal.ReadInt32(src, in_ofs));
						break;

					case 8:
						Marshal.WriteInt64(pvData, out_ofs, Marshal.ReadInt64(src, in_ofs));
						break;
					}

					in_ofs += sz;
					out_ofs += stride;

					int r = arr.Rank - 1;
					while (r >= 0 && idxs[r] == arr.GetLength(r) - 1)
					{
						idxs[r] = 0;
						out_ofs = 0;
						--r;
					}

					if (r < 0)
						/* done */
						break;

					idxs[r]++;

					if (out_ofs == 0)
					{
						/* recalc start offset */
						/* TODO: can we combine this with the index increment loop above? */
						int dim_size = 1;
						for (r = 0; r < arr.Rank; ++r)
						{
							out_ofs += dim_size * idxs[r];
							dim_size *= arr.GetLength(r);
						}
						out_ofs *= sz;
					}
				}
			}

			pinned.Free();

			return true;
		}

		public void SetValue(object obj) {
			vt = (short)VarEnum.VT_EMPTY;
			if (obj == null)
				return;

			Type t = obj.GetType();
			if (t.IsEnum)
				t = Enum.GetUnderlyingType (t);

			if (t.IsArray && do_array(obj))
			{
				/* noop */
			}
			else if (t == typeof(sbyte))
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
			case VarEnum.VT_I4:
				obj = Marshal.ReadInt32(addr);
				break;
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
				obj = Marshal.PtrToStringBSTR(Marshal.ReadIntPtr(addr));
				break;
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
				break;
			}
			return obj;
		}

		public void Clear ()
		{
			if ((VarEnum)vt == VarEnum.VT_BSTR) {
				Marshal.FreeBSTR (bstrVal);
			}
			else if ((vt & (short)VarEnum.VT_ARRAY) != 0) {
				_SafeArrayDestroy (parray);
			}
#if !DISABLE_COM
			else if ((VarEnum)vt == VarEnum.VT_DISPATCH || (VarEnum)vt == VarEnum.VT_UNKNOWN) {
				if (pdispVal != IntPtr.Zero)
					Marshal.Release (pdispVal);
			}
#endif
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
