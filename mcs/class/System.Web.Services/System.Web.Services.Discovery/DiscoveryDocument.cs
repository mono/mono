// 
// System.Web.Services.Protocols.DiscoveryDocument.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)  
//
// Copyright (C) Dave Bettin, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryDocument {

		#region Fields
		
		public const string Namespace = "http://schema.xmlsoap.org/disco/";
		
		#endregion // Fields
		
		#region Constructors

		[MonoTODO]
		public DiscoveryDocument () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties
	
		[XmlIgnore]
		public IList References {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static bool CanRead (XmlReader xmlReader)
		{
                        throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DiscoveryDocument Read (Stream stream)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static DiscoveryDocument Read (TextReader textReader)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static DiscoveryDocument Read (XmlReader xmlReader)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Write (Stream stream)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Write (TextWriter textWriter)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Write (XmlWriter xmlWriter)
		{
                        throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
