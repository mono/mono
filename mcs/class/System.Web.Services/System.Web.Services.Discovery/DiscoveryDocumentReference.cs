// 
// System.Web.Services.Protocols.DiscoveryDocumentReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.IO;
using System.Web.Services.Description;

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryDocumentReference : DiscoveryReference {

		#region Constructors

		[MonoTODO]
		public DiscoveryDocumentReference () 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public DiscoveryDocumentReference (string href) : this() 
		{
			throw new NotImplementedException ();
		}		
		
		#endregion // Constructors

		#region Properties
		
		[MonoTODO]
		public DiscoveryDocument Document {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override string DefaultFilename {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string Ref {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override string Url {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			
			[MonoTODO]
			set { throw new NotImplementedException (); }
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
