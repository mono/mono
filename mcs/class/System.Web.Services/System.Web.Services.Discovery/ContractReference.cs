// 
// System.Web.Services.Discovery.ContractReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.IO;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {

	[XmlRootAttribute("contractRef", Namespace="https://schemas.xmlsoap.org/disco/scl/", IsNullable=true)]
	public class ContractReference : DiscoveryReference {

		#region Fields
		
		public const string Namespace = "http://schemas.xmlsoap.org/disco/scl/";

		private ServiceDescription contract;
		private string defaultFilename;
		private string docRef;
		private string href;
		private string url;
		
		#endregion // Fields
		
		#region Constructors

		[MonoTODO]
		public ContractReference () 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public ContractReference (string href) : this() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public ContractReference (string href, string docRef) : this(href) 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public ServiceDescription Contract {
			get { return contract; }			
		}

		[XmlIgnore]
		public override string DefaultFilename {
			get { return defaultFilename; }
		}
		
		[XmlAttribute("docRef")]
		public string DocRef {
			get { return docRef; }
			set { docRef = value; }
		}
		
		[XmlAttribute("ref")]
		public string Ref {
			get { return href; }
			set { href = value; }
		}
		
		[XmlIgnore]
		public override string Url {
			get { return url;}			
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
                public override void WriteDocument (object document, Stream stream) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
