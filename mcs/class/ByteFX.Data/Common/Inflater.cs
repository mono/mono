using System;
using System.IO;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for Inflater.
	/// </summary>
	public class Inflater
	{
		private BitStream _input;

		public Inflater()
		{
		}

		public void SetInput(byte[] input, int offset, int len)
		{
			_input = new BitStream(input, offset, len);
		}

		public void Inflate(byte[] output, int offset, int size)
		{
			byte cmf = (byte)_input.GetBits(8);
			byte flag = (byte)_input.GetBits(8);

			if ((cmf & 0x0f) != 8)
				throw new Exception("Only deflate format data is supported");

			if (((cmf*256+flag) % 31) != 0)
				throw new Exception("Data is not in proper deflate format");



		}
	}
}
