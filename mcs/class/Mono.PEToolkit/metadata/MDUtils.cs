/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
//using System.Runtime.InteropServices;

namespace Mono.PEToolkit.Metadata {

	public sealed class MDUtils {

		// Holds true if machine is little-endian.
		private static bool isLE;

		static MDUtils()
		{
			isLE = BitConverter.IsLittleEndian;
		}

		private MDUtils()
		{
		}


		public static int CompressData(int data, out int res)
		{
			res = data;
			int len = 4;

			if (data < 0) {
				// data is actually unsigned,
				// that's why this empty clause is needed.
			} else if (data < 0x80) {
				res = data;
				len = 1;
			} else if (data < 0x4000) {
				res = ((data >> 8) | 0x80) + ((data & 0xFF) << 8);
				len = 2;
			} else if (data < 0x1FFFFFFF) {
				res = ((data >> 24) | 0xC0)        |
				      (((data >> 16) & 0xFF) << 8) |
				      (((data >> 8) & 0xFF) << 16) |
				      ((data & 0xFF) << 24);
				len = 4;
			}

			return len;
		}

		unsafe public static int CompressData(void* pData, void* pRes)
		{
			byte* p = (byte*) pData;

			int data = (isLE)
			           ? *(int*)p
			           : p [0] + (p [1] << 8) + (p [2] << 16) + (p [3] << 24);

			int res = 0;
			int len = CompressData(data, out res);
			p = (byte*) pRes;

			if (isLE) {
				*(int*)p = res;
			} else {
				*p++ = (byte) (res & 0xFF);
				*p++ = (byte) (res >> 8);
				*p++ = (byte) (res >> 16);
				*p++ = (byte) (res >> 24);
			}
			return len;
		}


		public static int Max(params int [] list)
		{
			int len = (list != null) ? list.Length : 0;
			if (len == 0) return 0;
			int max = list [0];
			for (int i = 1; i < len; i++) {
				if (list [i] > max) max = list [i];
			}
			return max;
		}


	}

}

