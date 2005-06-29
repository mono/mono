
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

// Auto-generated file - DO NOT EDIT!
// Please edit bitconverter.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit {

	/// <summary>
	/// Little-endian bit converter.
	/// </summary>
	public sealed class LEBitConverter {

		internal interface IConverter {

			short ToInt16(byte [] val, int idx);
			ushort ToUInt16(byte [] val, int idx);
			int ToInt32(byte [] val, int idx);
			uint ToUInt32(byte [] val, int idx);
			long ToInt64(byte [] val, int idx);
			ulong ToUInt64(byte [] val, int idx);

		}

		public static readonly bool Native = System.BitConverter.IsLittleEndian;

		private static readonly IConverter impl = System.BitConverter.IsLittleEndian
		                        ? new LEConverter() as IConverter
		                        : new BEConverter() as IConverter;




		private LEBitConverter()
		{
			// Never instantiated.
		}

		///<summary></summary>
		unsafe public static short SwapInt16(short x)
		{
			short* p = stackalloc short [1];
			*p = x;
			byte* bp = (byte*) p;
			byte b = bp [0];
			bp [0] = bp [1];
			bp [1] = b;
			return *p;
		}

		///<summary></summary>
		unsafe public static ushort SwapUInt16(ushort x)
		{
			ushort* p = stackalloc ushort [1];
			*p = x;
			byte* bp = (byte*) p;
			byte b = bp [0];
			bp [0] = bp [1];
			bp [1] = b;
			return *p;
		}

		///<summary></summary>
		unsafe public static int SwapInt32(int x)
		{
			int* p = stackalloc int [1];
			*p = x;
			byte* bp = (byte*) p;
			byte b = bp [0];
			bp [0] = bp [3];
			bp [3] = b;
			b = bp [1];
			bp [1] = bp [2];
			bp [2] = b;
			return *p;
		}

		///<summary></summary>
		unsafe public static uint SwapUInt32(uint x)
		{
			uint* p = stackalloc uint [1];
			*p = x;
			byte* bp = (byte*) p;
			byte b = bp [0];
			bp [0] = bp [3];
			bp [3] = b;
			b = bp [1];
			bp [1] = bp [2];
			bp [2] = b;
			return *p;
		}

		///<summary></summary>
		unsafe public static long SwapInt64(long x)
		{
			long* p = stackalloc long [1];
			*p = x;
			byte* bp = (byte*) p;
			byte b = bp [0];
			bp [0] = bp [7];
			bp [7] = b;
			b = bp [1];
			bp [1] = bp [6];
			bp [6] = b;
			b = bp [2];
			bp [2] = bp [5];
			bp [5] = b;
			b = bp [3];
			bp [3] = bp [4];
			bp [4] = b;
			return *p;
		}

		///<summary></summary>
		unsafe public static ulong SwapUInt64(ulong x)
		{
			ulong* p = stackalloc ulong [1];
			*p = x;
			byte* bp = (byte*) p;
			byte b = bp [0];
			bp [0] = bp [7];
			bp [7] = b;
			b = bp [1];
			bp [1] = bp [6];
			bp [6] = b;
			b = bp [2];
			bp [2] = bp [5];
			bp [5] = b;
			b = bp [3];
			bp [3] = bp [4];
			bp [4] = b;
			return *p;
		}





		internal sealed class LEConverter : IConverter {
			///<summary></summary>
			public short ToInt16(byte [] val, int idx)
			{
				return BitConverter.ToInt16(val, idx);
			}
			///<summary></summary>
			public ushort ToUInt16(byte [] val, int idx)
			{
				return BitConverter.ToUInt16(val, idx);
			}
			///<summary></summary>
			public int ToInt32(byte [] val, int idx)
			{
				return BitConverter.ToInt32(val, idx);
			}
			///<summary></summary>
			public uint ToUInt32(byte [] val, int idx)
			{
				return BitConverter.ToUInt32(val, idx);
			}
			///<summary></summary>
			public long ToInt64(byte [] val, int idx)
			{
				return BitConverter.ToInt64(val, idx);
			}
			///<summary></summary>
			public ulong ToUInt64(byte [] val, int idx)
			{
				return BitConverter.ToUInt64(val, idx);
			}

		}

		internal sealed class BEConverter : IConverter {
			///<summary></summary>
			public short ToInt16(byte [] val, int idx)
			{
				return SwapInt16(BitConverter.ToInt16(val, idx));
			}
			///<summary></summary>
			public ushort ToUInt16(byte [] val, int idx)
			{
				return SwapUInt16(BitConverter.ToUInt16(val, idx));
			}
			///<summary></summary>
			public int ToInt32(byte [] val, int idx)
			{
				return SwapInt32(BitConverter.ToInt32(val, idx));
			}
			///<summary></summary>
			public uint ToUInt32(byte [] val, int idx)
			{
				return SwapUInt32(BitConverter.ToUInt32(val, idx));
			}
			///<summary></summary>
			public long ToInt64(byte [] val, int idx)
			{
				return SwapInt64(BitConverter.ToInt64(val, idx));
			}
			///<summary></summary>
			public ulong ToUInt64(byte [] val, int idx)
			{
				return SwapUInt64(BitConverter.ToUInt64(val, idx));
			}

		}




		///<summary></summary>
		public static short ToInt16(byte [] val, int idx)
		{
			return impl.ToInt16(val, idx);
		}

		///<summary></summary>
		public static ushort ToUInt16(byte [] val, int idx)
		{
			return impl.ToUInt16(val, idx);
		}

		///<summary></summary>
		public static int ToInt32(byte [] val, int idx)
		{
			return impl.ToInt32(val, idx);
		}

		///<summary></summary>
		public static uint ToUInt32(byte [] val, int idx)
		{
			return impl.ToUInt32(val, idx);
		}

		///<summary></summary>
		public static long ToInt64(byte [] val, int idx)
		{
			return impl.ToInt64(val, idx);
		}

		///<summary></summary>
		public static ulong ToUInt64(byte [] val, int idx)
		{
			return impl.ToUInt64(val, idx);
		}



	}

}

