 // 
// System.Web.Services.Configuration.XmlFormatExtensionPrefixAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Configuration {
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class XmlFormatExtensionPrefixAttribute : Attribute {

		#region Fields

		string prefix;
		string ns;

		#endregion // Fields

		#region Constructors

		public XmlFormatExtensionPrefixAttribute ()
		{
		}

		public XmlFormatExtensionPrefixAttribute (string prefix, string ns)
			: this ()
		{
			this.prefix = prefix;
			this.ns = ns;
		}

		#endregion // Constructors

		#region Properties

		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		#endregion // Properties
	}
}
