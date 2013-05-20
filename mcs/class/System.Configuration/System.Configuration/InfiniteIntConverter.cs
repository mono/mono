//
// System.Configuration.InfiniteIntConverter.cs
//
// Authors:
// 	Chris Toshok (toshok@ximian.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System.ComponentModel;
using System.Globalization;

namespace System.Configuration
{
	public sealed class InfiniteIntConverter: ConfigurationConverterBase
	{
		public InfiniteIntConverter ()
		{
		}

		public override object ConvertFrom (ITypeDescriptorContext ctx, CultureInfo ci, object data)
		{
			if ((string)data == "Infinite")
				return Int32.MaxValue;
			else
				return Convert.ToInt32 ((string)data, 10);
		}

		public override object ConvertTo (ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
		{
			/* don't use "value is TimeSpan" here, since
			 * we want to generate both a NRE on null
			 * value, and ArgumentException on non-null,
			 * but non-TimeSpan. */
			if (value.GetType () != typeof (int))
				throw new ArgumentException ();

			if (((int)value) == Int32.MaxValue)
				return "Infinite";
			else
				return value.ToString();
		}
	}
}

