// 
// System.Web.Services.Disocvery.DiscoveryClientResult.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//


using System.Xml.Serialization;

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryClientResult {
	
		#region Fields

		private string filename;
		private string referenceTypeName;
		private string url;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public DiscoveryClientResult () 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public DiscoveryClientResult (Type referenceType, string url, string filename) : this() 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties	
	
		[XmlAttribute("filename")]
		public string Filename {
			get { return filename; }
			set { filename = value; }
		}
		
		[XmlAttribute("referenceType")]
		public string ReferenceTypeName {
			get { return referenceTypeName; }
			set { referenceTypeName = value; }
		}
		
		[XmlAttribute("url")]
		public string Url {
			get { return url; }
			set { url = value; }
		}
		
		#endregion // Properties
	}
}
