//
// XmlValueConverter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//

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

using System;

namespace System.Xml.Schema
{
	public abstract class XmlValueConverter
	{
		[MonoTODO]
		protected XmlValueConverter ()
		{
		}

		public abstract object ChangeType (bool value, Type type);

		public abstract object ChangeType (DateTime value, Type type);

		public abstract object ChangeType (decimal value, Type type);

		public abstract object ChangeType (double value, Type type);

		public abstract object ChangeType (int value, Type type);

		public abstract object ChangeType (long value, Type type);

		public abstract object ChangeType (object value, Type type);

		public abstract object ChangeType (float value, Type type);

		public abstract object ChangeType (string value, Type type);

		public abstract object ChangeType (object value, Type type, IXmlNamespaceResolver nsResolver);

		public abstract object ChangeType (string value, Type type, IXmlNamespaceResolver nsResolver);

		public abstract bool ToBoolean (bool value);

		public abstract bool ToBoolean (DateTime value);

		public abstract bool ToBoolean (decimal value);

		public abstract bool ToBoolean (double value);

		public abstract bool ToBoolean (int value);

		public abstract bool ToBoolean (long value);

		public abstract bool ToBoolean (object value);

		public abstract bool ToBoolean (float value);

		public abstract bool ToBoolean (string value);

		public abstract DateTime ToDateTime (bool value);

		public abstract DateTime ToDateTime (DateTime value);

		public abstract DateTime ToDateTime (decimal value);

		public abstract DateTime ToDateTime (double value);

		public abstract DateTime ToDateTime (int value);

		public abstract DateTime ToDateTime (long value);

		public abstract DateTime ToDateTime (object value);

		public abstract DateTime ToDateTime (float value);

		public abstract DateTime ToDateTime (string value);

		public abstract decimal ToDecimal (bool value);

		public abstract decimal ToDecimal (DateTime value);

		public abstract decimal ToDecimal (decimal value);

		public abstract decimal ToDecimal (double value);

		public abstract decimal ToDecimal (int value);

		public abstract decimal ToDecimal (long value);

		public abstract decimal ToDecimal (object value);

		public abstract decimal ToDecimal (float value);

		public abstract decimal ToDecimal (string value);

		public abstract double ToDouble (bool value);

		public abstract double ToDouble (DateTime value);

		public abstract double ToDouble (decimal value);

		public abstract double ToDouble (double value);

		public abstract double ToDouble (int value);

		public abstract double ToDouble (long value);

		public abstract double ToDouble (object value);

		public abstract double ToDouble (float value);

		public abstract double ToDouble (string value);

		public abstract int ToInt32 (bool value);

		public abstract int ToInt32 (DateTime value);

		public abstract int ToInt32 (decimal value);

		public abstract int ToInt32 (double value);

		public abstract int ToInt32 (int value);

		public abstract int ToInt32 (long value);

		public abstract int ToInt32 (object value);

		public abstract int ToInt32 (float value);

		public abstract int ToInt32 (string value);

		public abstract long ToInt64 (bool value);

		public abstract long ToInt64 (DateTime value);

		public abstract long ToInt64 (decimal value);

		public abstract long ToInt64 (double value);

		public abstract long ToInt64 (int value);

		public abstract long ToInt64 (long value);

		public abstract long ToInt64 (object value);

		public abstract long ToInt64 (float value);

		public abstract long ToInt64 (string value);

		public abstract float ToSingle (bool value);

		public abstract float ToSingle (DateTime value);

		public abstract float ToSingle (decimal value);

		public abstract float ToSingle (double value);

		public abstract float ToSingle (int value);

		public abstract float ToSingle (long value);

		public abstract float ToSingle (object value);

		public abstract float ToSingle (float value);

		public abstract float ToSingle (string value);

		public abstract string ToString (bool value);

		public abstract string ToString (DateTime value);

		public abstract string ToString (decimal value);

		public abstract string ToString (double value);

		public abstract string ToString (int value);

		public abstract string ToString (long value);

		public abstract string ToString (object value, IXmlNamespaceResolver resolver);

		public abstract string ToString (float value);

		public abstract string ToString (string value, IXmlNamespaceResolver resolver);
	}
}

#endif
