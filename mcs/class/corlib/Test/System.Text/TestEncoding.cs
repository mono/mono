//
// TestEncoding.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//
// Used for testing custom encoding.
//

using System;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Text
{
	internal class MyEncoding : Encoding
	{
		public MyEncoding ()
		{
		}

		public MyEncoding (int codepage)
			: base (codepage)
		{
		}

		public override int GetByteCount (char [] chars, int index, int count)
		{
			return 0;
		}

		public override int GetBytes (char [] chars, int charIndex, int charCount, byte [] bytes, int byteIndex)
		{
			return 0;
		}

		public override int GetCharCount (byte [] bytes, int index, int count)
		{
			return 0;
		}

		public override int GetChars (byte [] bytes, int byteIndex, int byteCount, char [] chars, int charIndex)
		{
			return 0;
		}

		public override int GetMaxByteCount (int length)
		{
			return 0;
		}

		public override int GetMaxCharCount (int length)
		{
			return 0;
		}
	}
}
