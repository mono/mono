//
// XPathAtomicValue.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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
using System.Xml;
using System.Xml.Schema;

namespace System.Xml.XPath
{
	public sealed class XPathAtomicValue
	{

		#region Constructors

		[MonoTODO]
		public XPathAtomicValue (bool value, XmlSchemaType xmlType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathAtomicValue (DateTime value, XmlSchemaType xmlType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathAtomicValue (decimal value, XmlSchemaType xmlType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathAtomicValue (double value, XmlSchemaType xmlType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathAtomicValue (int value, XmlSchemaType xmlType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathAtomicValue (long value, XmlSchemaType xmlType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathAtomicValue (object value, XmlSchemaType xmlType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathAtomicValue (float value, XmlSchemaType xmlType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathAtomicValue (string value, XmlSchemaType xmlType)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Methods

		[MonoTODO]
		public XPathAtomicValue Clone ()
		{
			throw new NotImplementedException ();
		}

		public object ValueAs (Type type)
		{
			return ValueAs (type, null);
		}

		[MonoTODO]
		public object ValueAs (Type type, IXmlNamespaceResolver nsResolver)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		[MonoTODO]
		public bool IsNode {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object TypedValue {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Value {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool ValueAsBoolean {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DateTime ValueAsDateTime {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public decimal ValueAsDecimal {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public double ValueAsDouble {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int ValueAsInt32 {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public long ValueAsInt64 {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ICollection ValueAsList {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public float ValueAsSingle {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Type ValueType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlSchemaType XmlType {
			get { throw new NotImplementedException (); }
		}

		#endregion
	}
}

#endif
