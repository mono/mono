// 
// System.Xml.Serialization.SoapSchemaMember 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

namespace System.Xml.Serialization {
	public class SoapSchemaMember {

		#region Fields

		string memberName;
		XmlQualifiedName memberType;

		#endregion

		#region Constructors

		public SoapSchemaMember ()
		{
		}

		#endregion // Constructors

		#region Properties

		public string MemberName {
			get { return memberName; }
			set { memberName = value; }
		}

		public XmlQualifiedName MemberType {
			get { return memberType; }
			set { memberType = value; }
		}

		#endregion // Properties
	}
}
