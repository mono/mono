/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	public sealed class PEUtils {

		private PEUtils()
		{
		}


		unsafe internal static string GetString (sbyte* data, int start, int len, Encoding encoding)
		{
			byte[] data_array = new byte[len-start];
			
			for (int i=start; i<len; i++)
				data_array[i-start] = (byte)*data++;
			
			return encoding.GetString (data_array);
		}

		/// <summary>
		/// Reads structure from the input stream preserving its endianess.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="pStruct"></param>
		/// <param name="len"></param>
		unsafe internal static void ReadStruct(BinaryReader reader, void* pStruct, int len)
		{
			byte* p = (byte*) pStruct;

			if (System.BitConverter.IsLittleEndian) {
				// On a little-endian machine read data in 64-bit chunks,
				// this won't work on big-endian machine because
				// BinaryReader APIs are little-endian while
				// memory writes are platform-native.
				// This seems faster than ReadBytes/Copy method
				// in the "else" clause, especially if used often
				// (no extra memory allocation for byte[]?).
				int whole = len >> 3;
				int rem = len & 7;

				for (int i = whole; --i >= 0;) {
					long qw = reader.ReadInt64();
					Marshal.WriteInt64((IntPtr) p, qw);
					p += sizeof (long);
				}
				for (int i = rem; --i >= 0;) {
					*p++ = (byte) reader.ReadByte();
				}
			} else {
				byte [] buff = reader.ReadBytes(len);
				Marshal.Copy(buff, 0, (IntPtr) p, len);
			}
		}

		/// <summary>
		/// Reads structure from the input stream
		/// changing its endianess if required
		/// (if running on big-endian hardware).
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="pStruct"></param>
		/// <param name="len"></param>
		/// <param name="type"></param>
		unsafe internal static void ReadStruct(BinaryReader reader, void* pStruct, int len, Type type)
		{
			ReadStruct(reader, pStruct, len);
			if (!System.BitConverter.IsLittleEndian) {
				ChangeStructEndianess(pStruct, type);
			}
		}


		unsafe private static int SwapByTypeCode(byte* p, TypeCode tcode)
		{
			int inc = 0;
			switch (tcode) {
				case TypeCode.Int16 :
					short* sp = (short*) p;
					short sx = *sp;
					sx = LEBitConverter.SwapInt16(sx);
					*sp = sx;
					inc = sizeof (short);
					break;
				case TypeCode.UInt16 :
					ushort* usp = (ushort*) p;
					ushort usx = *usp;
					usx = LEBitConverter.SwapUInt16(usx);
					*usp = usx;
					inc = sizeof (ushort);
					break;
				case TypeCode.Int32 :
					int* ip = (int*) p;
					int ix = *ip;
					ix = LEBitConverter.SwapInt32(ix);
					*ip = ix;
					inc = sizeof (int);
					break;
				case TypeCode.UInt32 :
					uint* uip = (uint*) p;
					uint uix = *uip;
					uix = LEBitConverter.SwapUInt32(uix);
					*uip = uix;
					inc = sizeof (uint);
					break;
				case TypeCode.Int64 :
					long* lp = (long*) p;
					long lx = *lp;
					lx = LEBitConverter.SwapInt64(lx);
					*lp = lx;
					inc = sizeof (long);
					break;
				case TypeCode.UInt64 :
					ulong* ulp = (ulong*) p;
					ulong ulx = *ulp;
					ulx = LEBitConverter.SwapUInt64(ulx);
					*ulp = ulx;
					inc = sizeof (ulong);
					break;
				case TypeCode.Byte :
				case TypeCode.SByte :
					inc = sizeof (byte);
					break;
				default :
					break;
			}
			return inc;
		}

		unsafe internal static int ChangeStructEndianess(void* pStruct, Type type)
		{
			if (type == null || !type.IsValueType) return 0;
			if (!type.IsLayoutSequential && !type.IsExplicitLayout) {
				throw new Exception("Internal error: struct must have explicit or sequential layout.");
			}

			bool seq = type.IsLayoutSequential;
			byte* p = (byte*) pStruct;
			int offs = 0;
			int inc;
			FieldInfo [] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			
			foreach (FieldInfo fi in fields) {
				if (!seq) offs = Marshal.OffsetOf(type, fi.Name).ToInt32 ();
				Type ft = fi.FieldType;
				TypeCode tcode = Type.GetTypeCode(ft);
				if (tcode == TypeCode.Object) {
					// not a primitive type, process recursively.
					inc = ChangeStructEndianess(p + offs, ft);
				} else {
					inc = SwapByTypeCode(p + offs, tcode);
				}
				if (seq) offs += inc;
			}

			return offs;
		}

	}

}

