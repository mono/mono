// 
// System.Web.Services.Discovery.SchemaReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//


using System.ComponentModel;
using System.IO;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {

	[XmlRootAttribute("schemaRef", Namespace="http://schemas/xmlsoap.org/disco/schema/", IsNullable=true)]
	public sealed class SchemaReference : DiscoveryReference {

		#region Fields
		
		public const string Namespace = "http://schemas/xmlsoap.org/disco/schema/";

		private string defaultFilename;
		private string href;
		private string url;
		private string targetNamespace;
		private XmlSchema schema;
		
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
		
		[DefaultValue("")]
		[XmlAttribute("targetNamespace")]
		public string TargetNamespace {
			get { return targetNamespace; }
			set { targetNamespace = targetNamespace; }
		}

		[XmlIgnore]
		public XmlSchema Schema {
			get { return schema; }
			
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
