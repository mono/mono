 // 
// System.Web.Services.Configuration.XmlFormatExtensionPointAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Configuration {
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class XmlFormatExtensionPointAttribute : Attribute {

		#region Fields

		bool allowElements;
		string memberName;

		#endregion // Fields

		#region Constructors

		public XmlFormatExtensionPointAttribute (string memberName)
		{
			allowElements = false; // FIXME
		}

		#endregion // Constructors

		#region Properties

		public bool AllowElements {
			get { return allowElements; }
			set { allowElements = value; }
		}

		public string MemberName {	
			get { return memberName; }
			set { memberName = value; }
		}

		#endregion // Properties
	}
}
