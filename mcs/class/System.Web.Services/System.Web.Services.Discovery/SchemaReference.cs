// 
// System.Web.Services.Discovery.SchemaReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
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
		private string targetNamespace;
		private XmlSchema schema;
		
		#endregion // Fields
		
		#region Constructors

		public SchemaReference () 
		{
		}
		
		public SchemaReference (string href) : this() 
		{
			this.href = href;
		}		
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public override string DefaultFilename {
			get { return FilenameFromUrl (Url) + ".xsd"; }
		}
		
		[XmlAttribute("ref")]
		public string Ref {
			get { return href; }
			set { href = value; }
		}
		
		[XmlIgnore]
		public override string Url {
			get { return href; }
			set { href = value; }
		}
		
		[DefaultValue("")]
		[XmlAttribute("targetNamespace")]
		public string TargetNamespace {
			get { return targetNamespace; }
			set { targetNamespace = targetNamespace; }
		}

		[XmlIgnore]
		public XmlSchema Schema {
			get { 
				if (ClientProtocol == null) 
					throw new InvalidOperationException ("The ClientProtocol property is a null reference");
				
				XmlSchema doc = ClientProtocol.Documents [Url] as XmlSchema;
				if (doc == null)
					throw new Exception ("The Documents property of ClientProtocol does not contain a schema with the url " + Url);
					
				return doc; 
			}
			
		}
		
		#endregion // Properties

		#region Methods

		public override object ReadDocument (Stream stream)
		{
			return XmlSchema.Read (stream, null);
		}
                
		protected internal override void Resolve (string contentType, Stream stream) 
		{
			XmlSchema doc = XmlSchema.Read (stream, null);
			ClientProtocol.Documents.Add (Url, doc);
			if (!ClientProtocol.References.Contains (Url))
				ClientProtocol.References.Add (this);
		}
                
		public override void WriteDocument (object document, Stream stream) 
		{
			((XmlSchema)document).Write (stream);
		}

		#endregion // Methods
	}
}
