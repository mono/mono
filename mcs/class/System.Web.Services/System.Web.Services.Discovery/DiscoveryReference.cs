// 
// System.Web.Services.Protocols.DiscoveryReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.IO;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {
	public abstract class DiscoveryReference {

		#region Constructors

		[MonoTODO]
		protected DiscoveryReference () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public DiscoveryClientProtocol ClientProtocol {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			
			[MonoTODO]
			set { throw new NotImplementedException (); }
			
		}
		
		[XmlIgnore]
		public virtual string DefaultFilename {
			[MonoTODO]
			get { throw new NotImplementedException (); }			
		}
		
		[XmlIgnore]
		public abstract string Url {
			get;		
			set;
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected static string FilenameFromUrl (string url)
		{
                        throw new NotImplementedException ();
		}
		
		public abstract object ReadDocument (Stream stream);
		
                [MonoTODO]
		public void Resolve () 
		{
                        throw new NotImplementedException ();
		}
                
                protected internal abstract void Resolve (string contentType, Stream stream);
                
                public abstract void WriteDocument (object document, Stream stream);		

		#endregion // Methods
	}
}
