//
// XPathAtomicValue.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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
