//
// System.Runtime.Serialization.Formatter.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization {
#if NET_2_0
	[System.Runtime.InteropServices.ComVisibleAttribute (true)]
#endif
	public class FormatterConverter : IFormatterConverter {

		public FormatterConverter ()
		{
		}

		public object Convert (object value, Type type)
		{
			return System.Convert.ChangeType (value, type);
		}

		public object Convert (object value, TypeCode typeCode)
		{
			return System.Convert.ChangeType (value, typeCode);
		}

		public bool ToBoolean (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");
		
			return System.Convert.ToBoolean (value);
		}

		public byte ToByte (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToByte (value);
		}

		public char ToChar (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToChar (value);
		}

		public DateTime ToDateTime (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToDateTime (value);
		}

		public decimal ToDecimal (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToDecimal (value);
		}

		public double ToDouble (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToDouble (value);
		}

		public short ToInt16 (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToInt16 (value);
		}

		public int ToInt32 (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToInt32 (value);
		}

		public long ToInt64 (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToInt64 (value);
		}

		public float ToSingle (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToSingle (value);
		}

		public string ToString (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToString (value);
		}

		[CLSCompliant (false)]
		public sbyte ToSByte (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToSByte (value);
		}

		[CLSCompliant (false)]
		public ushort ToUInt16 (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToUInt16 (value);
		}

		[CLSCompliant (false)]
		public uint ToUInt32 (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToUInt32 (value);
		}

		[CLSCompliant (false)]
		public ulong ToUInt64 (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value is null.");

			return System.Convert.ToUInt64 (value);
		}
	}
}
