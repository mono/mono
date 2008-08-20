//
// XmlDictionaryReaderQuotas.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
#if NET_2_0
namespace System.Xml
{
	public sealed class XmlDictionaryReaderQuotas
	{
		static XmlDictionaryReaderQuotas max;

		static XmlDictionaryReaderQuotas ()
		{
			max = new XmlDictionaryReaderQuotas (true);
		}

		readonly bool is_readonly;
		int array_len, bytes, depth, nt_chars, text_len;

		public XmlDictionaryReaderQuotas ()
			: this (false)
		{
		}

		private XmlDictionaryReaderQuotas (bool max)
		{
			is_readonly = max;
			array_len = max ? int.MaxValue : 0x4000;
			bytes = max ? int.MaxValue : 0x1000;
			depth = max ? int.MaxValue : 0x20;
			nt_chars = max ? int.MaxValue : 0x4000;
			text_len = max ? int.MaxValue : 0x2000;
		}

		public static XmlDictionaryReaderQuotas Max {
			get { return max; }
		}

		public int MaxArrayLength {
			get { return array_len; }
			set { array_len = Check (value); }
		}

		public int MaxBytesPerRead {
			get { return bytes; }
			set { bytes = Check (value); }
		}

		public int MaxDepth {
			get { return depth; }
			set { depth = Check (value); }
		}

		public int MaxNameTableCharCount {
			get { return nt_chars; }
			set { nt_chars = Check (value); }
		}

		public int MaxStringContentLength {
			get { return text_len; }
			set { text_len = Check (value); }
		}

		private int Check (int value)
		{
			if (is_readonly)
				throw new InvalidOperationException ("This quota is read-only.");
			if (value <= 0)
				throw new ArgumentException ("Value must be positive integer.");
			return value;
		}

		public void CopyTo (XmlDictionaryReaderQuotas quota)
		{
			quota.array_len = array_len;
			quota.bytes = bytes;
			quota.depth = depth;
			quota.nt_chars = nt_chars;
			quota.text_len = text_len;
		}
	}
}
#endif
