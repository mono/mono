using System;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for HuffmanTree.
	/// </summary>
	public class HuffmanTree
	{
		const int MAX_BITS = 15;
		int[]	codes;

		public HuffmanTree(bool lit)
		{
			if (lit)
				BuildLitTree();
			else
				BuildLenTree();
		}

		private void BuildLitTree()
		{
			codes = new int[288];

			// fill the code slots with code lengths first
			for (int x=0; x < 144; x++)
				codes[x] = 8;
			for (int x=144; x < 256; x++)
				codes[x] = 9;
			for (int x=256; x < 280; x++)
				codes[x] = 7;
			for (int x=280; x < 288; x++)
				codes[x] = 8;

			BuildTreeCommon();
		}


		private void BuildLenTree()
		{
			BuildTreeCommon();
		}

		private void BuildTreeCommon()
		{
			int[] codecounts = new int[MAX_BITS];
			int[] codebase = new int[MAX_BITS];

			for (int i = 0; i < codes.Length; i++) 
			{
				int bit_len = codes[i];
				if (bit_len > 0)
					codecounts[bit_len]++;
			}

			// now we compute the intial value for each code length
			int code = 0;
			codecounts[0] = 0;
			for (int bits = 1; bits <= MAX_BITS; bits++) 
			{
				code = (code + codecounts[bits-1]) << 1;
				codebase[bits] = code;
			}

			// next we assign numerical values to each code
			for (int x=0;  x <= codes.Length; x++) 
			{
				if (codes[x] == 0) continue;
				int blen = codes[x];
				codes[x] = codebase[ blen ];
				codebase[ blen ] ++;
			}
		}

	}
}
