//
// XPathItem.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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

		public virtual object ValueAs (Type type)
		{
			return ValueAs (type, null);
		}

		public abstract object ValueAs (Type type, IXmlNamespaceResolver nsResolver);

		public abstract bool IsNode { get; }

		public abstract object TypedValue { get; }

		public abstract string Value { get; }

		public abstract bool ValueAsBoolean { get; }

		public abstract DateTime ValueAsDateTime { get; }

		public abstract decimal ValueAsDecimal { get; }

		public abstract double ValueAsDouble { get; }

		public abstract int ValueAsInt32 { get; }

		public abstract long ValueAsInt64 { get; }

		public abstract ICollection ValueAsList { get; }

		public abstract float ValueAsSingle { get; }

		public abstract Type ValueType { get; }

		public abstract XmlSchemaType XmlType { get; }
	}
}
#endif
