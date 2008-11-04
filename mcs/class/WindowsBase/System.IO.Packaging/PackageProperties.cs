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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Xml;

namespace System.IO.Packaging {

	public abstract class PackageProperties : IDisposable
	{
		internal const string NSProperties = "application/vnd.openxmlformats-package.core-properties+xml";
		const string NSDc = "http://purl.org/dc/elements/1.1/";
		const string NSDcTerms = "http://purl.org/dc/terms/";
		const string NSXsi = "http://www.w3.org/2001/XMLSchema-instance";
		
		protected PackageProperties ()
		{
			// Nothing
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			// Nothing
		}

		public abstract string Category { get; set; }
		public abstract string ContentStatus { get; set; }
		public abstract string ContentType { get; set; }
		public abstract DateTime? Created { get; set; }
		public abstract string Creator { get; set; }
		public abstract string Description { get; set; }
		public abstract string Identifier { get; set; }
		public abstract string Keywords { get; set; }
		public abstract string Language { get; set; }
		public abstract string LastModifiedBy { get; set; }
		public abstract DateTime? LastPrinted { get; set; }
		public abstract DateTime? Modified { get; set; }
		public abstract string Revision { get; set; }
		public abstract string Subject { get; set; }
		public abstract string Title { get; set; }
		public abstract string Version { get; set; }

		internal void LoadFrom (Stream stream)
		{
			XmlDocument doc = new XmlDocument ();
			XmlNamespaceManager manager = new XmlNamespaceManager (doc.NameTable);
			doc.Load (stream);

			XmlNode node;
			if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:category", manager)) != null)
				Category = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:contentStatus", manager)) != null)
				ContentStatus = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:contentType", manager)) != null)
				ContentType = node.InnerXml;
			if ((node = doc.SelectSingleNode ("prop:coreProperties/dcterms:created", manager)) != null)
				Created = DateTime.Parse (node.InnerXml);
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:creator", manager)) != null)
				Creator = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:description", manager)) != null)
				Description = node.InnerXml;
			if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:identifier", manager)) != null)
				Identifier = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:keywords", manager)) != null)
				Keywords = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:language", manager)) != null)
				Language = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:lastModifiedBy", manager)) != null)
				LastModifiedBy = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:lastPrinted", manager)) != null)
				LastPrinted = DateTime.Parse (node.InnerXml);
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dcterms:modified", manager)) != null)
				Modified = DateTime.Parse (node.InnerXml);
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:revision", manager)) != null)
				Revision = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:subject", manager)) != null)
				Subject = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:title", manager)) != null)
				Title = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:version", manager)) != null)
				Version = node.InnerXml;
		}

		internal void WriteTo (Stream stream)
		{
			XmlDocument doc = new XmlDocument ();
			XmlNamespaceManager manager = new XmlNamespaceManager (doc.NameTable);
			manager.AddNamespace ("prop", NSProperties);
			manager.AddNamespace ("dc", NSDc);
			manager.AddNamespace ("dcterms", NSDcTerms);
			manager.AddNamespace ("xsi", NSXsi);
			
			// Create XML declaration
			doc.AppendChild (doc.CreateXmlDeclaration ("1.0", "UTF-8", null));

			// Create root node with required namespace declarations
			XmlNode coreProperties = doc.AppendChild (doc.CreateNode (XmlNodeType.Element, "coreProperties", NSProperties));
			coreProperties.Attributes.Append (doc.CreateAttribute ("xmlns:dc")).Value = NSDc;
			coreProperties.Attributes.Append (doc.CreateAttribute ("xmlns:dcterms")).Value = NSDcTerms;
			coreProperties.Attributes.Append (doc.CreateAttribute ("xmlns:xsi")).Value = NSXsi;

			// Create the children
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "category", NSProperties)).InnerXml = "category";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "contentStatus", NSProperties)).InnerXml = "Version";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "contentType", NSProperties)).InnerXml = "";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dcterms", "created", NSDcTerms)).InnerXml = "Version";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "creator", NSDc)).InnerXml = "Version";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "description", NSDc)).InnerXml = "Version";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "identifier", NSDc)).InnerXml = "Version";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "keywords", NSProperties)).InnerXml = "Version";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "language", NSDc)).InnerXml = "Version";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "lastModifiedBy", NSProperties)).InnerXml = "Version";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "lastPrinted", NSProperties)).InnerXml = "Version";
			XmlNode modified = coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dcterms", "modified", NSDcTerms));
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "revision", NSProperties)).InnerXml = "Title";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "subject", NSDc)).InnerXml = "Title";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "title", NSDc)).InnerXml = "Title";
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "version", NSProperties)).InnerXml = "Title";
			
			XmlAttribute att = doc.CreateAttribute("xsi", "type", NSXsi);
			att.Value = "dcterms:W3CDTF";
			modified.Attributes.Append (att);

			doc.WriteContentTo (new XmlTextWriter (stream, System.Text.Encoding.UTF8));
		}
	}
}