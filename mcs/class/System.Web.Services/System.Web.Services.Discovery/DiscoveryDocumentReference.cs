// 
// System.Web.Services.Discovery.DiscoveryDocumentReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Dave Bettin, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {

	[XmlRootAttribute("discoveryRef", Namespace="http://schemas.xmlsoap.org/disco/", IsNullable=true)]
	public sealed class DiscoveryDocumentReference : DiscoveryReference {
		
		#region Fields
		
		private DiscoveryDocument document;
		private string defaultFilename;
		private string href;
		private string url;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public DiscoveryDocumentReference () 
		{
			href = String.Empty;
		}
		
		public DiscoveryDocumentReference (string href) : this () 
		{
			this.href = href;
		}		
		
		#endregion // Constructors

		#region Properties
		
		[XmlIgnore]
		public DiscoveryDocument Document {
			get { return Document; }
		}
		
		[XmlIgnore]
		public override string DefaultFilename {
			get { return defaultFilename; }
		}
		
		[XmlAttribute("ref")]
		public string Ref {
			get { return href; }
			set { href = value; }
		}
		
		[XmlIgnore]
		public override string Url {
			get { return url; }
			set { url = value; }
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override object ReadDocument (Stream stream)
		{
			throw new NotImplementedException ();
		}
                
		[MonoTODO]
                protected internal override void Resolve (string contentType, Stream stream) 
		{
			throw new NotImplementedException ();
		}
                
		[MonoTODO]
                public void ResolveAll () 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
                public override void WriteDocument (object document, Stream stream) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
