// 
// System.Web.Services.Protocols.DiscoveryDocumentReference.cs
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
	public sealed class DiscoveryDocumentReference : DiscoveryReference {

		#region Fields

		string href;

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
		
		[MonoTODO]
		[XmlIgnore]
		public DiscoveryDocument Document {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		[XmlIgnore]
		public override string DefaultFilename {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO ("Set the XmlAttribute on this.")]
		public string Ref {
			get { return href; }
			set { href = value; }
		}
		
		[XmlIgnore]
		public override string Url {
			get { return Ref; }
			set { Ref = value; }
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
