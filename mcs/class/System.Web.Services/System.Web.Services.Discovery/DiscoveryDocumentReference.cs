// 
// System.Web.Services.Discovery.DiscoveryDocumentReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
// Copyright (C) Tim Coleman, 2002
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

		#endregion // Fields

		#region Constructors

		public DiscoveryDocumentReference () 
		{
			href = String.Empty;
		}
		
		public DiscoveryDocumentReference (string href)
		{
			this.href = href;
		}		
		
		#endregion // Constructors

		#region Properties
		
		[XmlIgnore]
		public DiscoveryDocument Document {
			get {
				if (ClientProtocol == null) 
					throw new InvalidOperationException ("The ClientProtocol property is a null reference");
				
				DiscoveryDocument doc = ClientProtocol.Documents [Url] as DiscoveryDocument;
				if (doc == null)
					throw new Exception ("The Documents property of ClientProtocol does not contain a discovery document with the url " + Url);
					
				return doc; 
			}
		}
		
		[XmlIgnore]
		public override string DefaultFilename {
			get { return FilenameFromUrl (Url) + ".disco"; }
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
		
		#endregion // Properties

		#region Methods

		public override object ReadDocument (Stream stream)
		{
			return DiscoveryDocument.Read (stream);
		}
                
		protected internal override void Resolve (string contentType, Stream stream) 
		{
			DiscoveryDocument doc = DiscoveryDocument.Read (stream);
			ClientProtocol.Documents.Add (Url, doc);
			if (!ClientProtocol.References.Contains (Url))
				ClientProtocol.References.Add (this);
				
			foreach (DiscoveryReference re in doc.References)
			{
				re.ClientProtocol = ClientProtocol;
				ClientProtocol.References.Add (re.Url, re);
			}
		}

		public void ResolveAll () 
		{
			if (ClientProtocol.Documents.Contains (Url)) 	// Already resolved
				return;
				
			Resolve ();
			DiscoveryDocument doc = document;
			foreach (DiscoveryReference re in doc.References)
			{
				try
				{
					if (re is DiscoveryDocumentReference)
						((DiscoveryDocumentReference)re).ResolveAll ();
					else
						re.Resolve ();
				}
				catch (Exception ex)
				{
					ReportError (re.Url, ex);
				}
			}
		}
		
		public override void WriteDocument (object document, Stream stream) 
		{
			((DiscoveryDocument)document).Write (stream);
		}

		#endregion // Methods
	}
}
