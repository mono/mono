//
// System.Data.ObjectSpaces.Schema.ExtendedProperty.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Xml;

namespace System.Data.ObjectSpaces.Schema {
	public class ExtendedProperty 
	{
		#region Fields

		XmlQualifiedName qname;
		object propertyValue;
		string prefix;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ExtendedProperty (XmlQualifiedName qualifiedName, object propertyValue)
		{
			QualifiedName = qualifiedName;
			PropertyValue = propertyValue;
		}

		public ExtendedProperty (XmlQualifiedName qualifiedName, object propertyValue, string prefix)
			: this (qualifiedName, propertyValue)
		{
			Prefix = prefix;
		}

		#endregion // Constructors

		#region Properties

		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}

		public object PropertyValue {
			get { return propertyValue; }
			set { propertyValue = value; }
		}

		public XmlQualifiedName QualifiedName {
			get { return qname; }
			set { qname = value; }
		}

		#endregion // Properties
	}
}

#endif // NET_1_2
