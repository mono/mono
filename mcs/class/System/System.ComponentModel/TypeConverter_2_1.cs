//
// TypeConverter for SL 2
//
// Copyright (C) 2008 Novell, Inc.
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

#if NET_2_1

using System;

namespace System.ComponentModel {

	public class TypeConverter {

		public virtual bool CanConvertFrom (Type sourceType)
		{
			if (sourceType == null)
				return false;

			return sourceType == typeof (string);
		}

		public virtual object ConvertFrom (object value)
		{
			return null;
		}

		public virtual object ConvertFromString (string text)
		{
			return ConvertFrom (text);
		}

		public virtual bool CanConvertTo (Type destinationType)
		{
			return false;
		}

		public virtual object ConvertTo (object value, Type destinationType)
		{
			throw new NotImplementedException ();
		}

		public virtual string ConvertToString (object value)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
