using System;
using System.IO;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for BitStream.
	/// </summary>
	public class BitStream : MemoryStream
	{
		private byte[]	_input;
		private int		_start;
		private int		_end;
		private int		_bitindex;
		private uint	_bitbuffer;
		private int		_bits_in_buffer;

		public BitStream(byte[] input, int index, int len)
		{
			_bitindex = 0;
			_bitbuffer = 0;
			_bits_in_buffer = 0;
			_input = input;
			_start = index;
			_end = _start + len;
		}

		public int GetBits(int numbits)
		{
			return 0;
		}

		public int PeekBits(int numbits)
		{
			int val=0;

			int index=_start;
			while (numbits > 0)
			{
				val = (val << 8) | _input[index++];
				numbits -= 8;
			}

			while (_bits_in_buffer < numbits)
			{
				if (_start == _end)
					throw new Exception("Out of bits");
				byte b = _input[_start++];
			}
			return 0;
		}
	}
}
