// 
// System.Web.Services.Discovery.SchemaReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {

	[XmlRootAttribute("schemaRef", Namespace="http://schemas.xmlsoap.org/disco/schema/", IsNullable=true)]
	public sealed class SchemaReference : DiscoveryReference {

		#region Fields
		
		public const string Namespace = "http://schemas.xmlsoap.org/disco/schema/";

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
		
		[DefaultValue(null)]
		[XmlAttribute("targetNamespace")]
		public string TargetNamespace {
			get { return targetNamespace; }
			set { targetNamespace = value; }
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
                
		internal void ResolveInternal (DiscoveryClientProtocol prot, XmlSchema xsd) 
		{
			if (xsd.Includes.Count == 0) return;
			
			foreach (XmlSchemaExternal ext in xsd.Includes)
			{
				if (ext.SchemaLocation == null)
					continue;

				// Make relative uris to absoulte

				Uri uri = new Uri (BaseUri, ext.SchemaLocation);
				string url = uri.ToString ();

				if (prot.Documents.Contains (url)) 	// Already resolved
					continue;

				try
				{
					string contentType = null;
					Stream stream = prot.Download (ref url, ref contentType);
					XmlTextReader reader = new XmlTextReader (url, stream);
					reader.XmlResolver = null;
					reader.MoveToContent ();
					
					DiscoveryReference refe;
					XmlSchema schema = XmlSchema.Read (reader, null);
					refe = new SchemaReference ();
					refe.ClientProtocol = prot;
					refe.Url = url;
					prot.Documents.Add (url, schema);
					((SchemaReference)refe).ResolveInternal (prot, schema);
					
					if (!prot.References.Contains (url))
						prot.References.Add (refe);
						
					stream.Close ();
				}
				catch (Exception ex)
				{
					ReportError (url, ex);
				}
			}
		}

		public override void WriteDocument (object document, Stream stream) 
		{
			((XmlSchema)document).Write (stream);
		}

		#endregion // Methods
	}
}
