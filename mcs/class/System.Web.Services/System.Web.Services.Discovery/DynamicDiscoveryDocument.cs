// 
// System.Web.Services.Discovery.DynamicDiscoveryDocument.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.IO;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {

	[XmlRootAttribute("dynamicDiscovery", Namespace="urn:schemas-dynamicdiscovery:disco.2000-03-17", IsNullable=true)]
	public sealed class DynamicDiscoveryDocument {

		#region Fields
		
		public const string Namespace = "urn:schemas-dynamicdiscovery:disco.2000-03-17";
		
		ExcludePathInfo[] excludes;
		
		#endregion // Fields
		
		#region Constructors

		public DynamicDiscoveryDocument () 
		{
		}
		
		#endregion // Constructors

		#region Properties
		
		[XmlElement("exclude", typeof(ExcludePathInfo))]
		public ExcludePathInfo[] ExcludePaths {
			get { return excludes; }
			set { excludes = value; }
		}
		
		#endregion // Properties

		#region Methods

		public static DynamicDiscoveryDocument Load (Stream stream)
		{
			XmlSerializer ser = new XmlSerializer (typeof(DynamicDiscoveryDocument));
			return (DynamicDiscoveryDocument) ser.Deserialize (stream);
		}

		public void Write (Stream stream)
		{
			XmlSerializer ser = new XmlSerializer (typeof(DynamicDiscoveryDocument));
			ser.Serialize (stream, this);
		}
		
		internal bool IsExcluded (string path)
		{
			if (excludes == null) return false;
			
			foreach (ExcludePathInfo ex in excludes)
				if (ex.Path == path) return true;
				
			return false;
		}

		#endregion // Methods
	}
}
