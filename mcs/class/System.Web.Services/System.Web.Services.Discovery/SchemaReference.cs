// 
// System.Web.Services.Discovery.SchemaReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.IO;
using System.Xml.Schema;

namespace System.Web.Services.Discovery {
	public sealed class SchemaReference : DiscoveryReference {

		#region Fields
		
		public const string Namespace = "http://schemas/xmlsoap.org/disco/schema/";
		
		#endregion // Fields
		
		#region Constructors

		[MonoTODO]
		public SchemaReference () 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public SchemaReference (string href) : this() 
		{
			throw new NotImplementedException ();
		}		
		
		#endregion // Constructors

		#region Properties
		
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
		
		[MonoTODO]
		public string TargetNamespace {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public XmlSchema Schema {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			
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
                public override void WriteDocument (object document, Stream stream) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
