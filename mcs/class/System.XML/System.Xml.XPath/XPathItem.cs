//
// XPathItem.cs
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

using System.Collections;
using System.Xml.Schema;

namespace System.Xml.XPath
{
	public abstract class XPathItem
	{
		protected XPathItem ()
		{
		}

		public virtual object ValueAs (Type returnType)
		{
			return ValueAs (returnType, null);
		}

		public abstract object ValueAs (Type returnType, IXmlNamespaceResolver nsResolver);

		public abstract bool IsNode { get; }

		public abstract object TypedValue { get; }

		public abstract string Value { get; }

		public abstract bool ValueAsBoolean { get; }

		public abstract DateTime ValueAsDateTime { get; }

		public abstract double ValueAsDouble { get; }

		public abstract int ValueAsInt { get; }

		public abstract long ValueAsLong { get; }

		public abstract Type ValueType { get; }

		public abstract XmlSchemaType XmlType { get; }
	}
}
#endif
