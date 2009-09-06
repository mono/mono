// PackagePropertiesPart.cs created with MonoDevelop
// User: alan at 11:07 04/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Xml;

namespace System.IO.Packaging
{
	class PackagePropertiesPart : PackageProperties
	{
		const string NSDc = "http://purl.org/dc/elements/1.1/";
		const string NSDcTerms = "http://purl.org/dc/terms/";
		const string NSXsi = "http://www.w3.org/2001/XMLSchema-instance";
		
		string category;
		string contentStatus;
		string contentType;
		DateTime? created;
		string creator;
		string description;
		string identifier;
		string keywords;
		string language;
		string lastModifiedBy;
		DateTime? lastPrinted;
		DateTime? modified;
		string revision;
		string subject;
		string title;
		string version;

		public PackagePropertiesPart ()
		{
			
		}

		public override string Category {
			get {
				return category;
			}
			set {
				Package.CheckIsReadOnly ();
				category = value;
			}
		}
		public override string ContentStatus {
			get {
				return contentStatus;
			}
			set {
				Package.CheckIsReadOnly ();
				contentStatus = value;
			}
		}
		public override string ContentType {
			get {
				return contentType;
			}
			set {
				Package.CheckIsReadOnly ();
				contentType = value;
			}
		}
		public override DateTime? Created {
			get {
				return created;
			}
			set {
				Package.CheckIsReadOnly ();
				created = value;
			}
		}
		public override string Creator {
			get {
				return creator;
			}
			set {
				Package.CheckIsReadOnly ();
				creator = value;
			}
		}
		public override string Description {
			get {
				return description;
			}
			set {
				Package.CheckIsReadOnly ();
				description = value;
			}
		}
		public override string Identifier {
			get {
				return identifier;
			}
			set {
				Package.CheckIsReadOnly ();
				identifier = value;
			}
		}
		public override string Keywords {
			get {
				return keywords;
			}
			set {
				Package.CheckIsReadOnly ();
				keywords = value;
			}
		}
		public override string Language {
			get {
				return language;
			}
			set {
				Package.CheckIsReadOnly ();
				language = value;
			}
		}
		public override string LastModifiedBy {
			get {
				return lastModifiedBy;
			}
			set {
				Package.CheckIsReadOnly ();
				lastModifiedBy = value;
			}
		}
		public override DateTime? LastPrinted {
			get {
				return lastPrinted;
			}
			set {
				Package.CheckIsReadOnly ();
				lastPrinted = value;
			}
		}
		public override DateTime? Modified {
			get {
				return modified;
			}
			set {
				Package.CheckIsReadOnly ();
				modified = value;
			}
		}
		public override string Revision {
			get {
				return revision;
			}
			set {
				Package.CheckIsReadOnly ();
				revision = value;
			}
		}
		public override string Subject {
			get {
				return subject;
			}
			set {
				Package.CheckIsReadOnly ();
				subject = value;
			}
		}
		public override string Title {
			get {
				return title;
			}
			set {
				Package.CheckIsReadOnly ();
				title = value;
			}
		}
		public override string Version {
			get {
				return version;
			}
			set {
				Package.CheckIsReadOnly ();
				version = value;
			}
		}
		
		internal override void LoadFrom (Stream stream)
		{
			if (stream.Length == 0)
				return;
			
			XmlDocument doc = new XmlDocument ();
			doc.Load (stream);
			
			XmlNamespaceManager manager = new XmlNamespaceManager (doc.NameTable);
			manager.AddNamespace ("prop", NSPackageProperties);
			manager.AddNamespace ("dc", NSDc);
			manager.AddNamespace ("dcterms", NSDcTerms);
			manager.AddNamespace ("xsi", NSXsi);

			XmlNode node;
			if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:category", manager)) != null)
				category = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:contentStatus", manager)) != null)
				contentStatus = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:contentType", manager)) != null)
				contentType = node.InnerXml;
			if ((node = doc.SelectSingleNode ("prop:coreProperties/dcterms:created", manager)) != null)
				created = DateTime.Parse (node.InnerXml);
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:creator", manager)) != null)
				creator = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:description", manager)) != null)
				description = node.InnerXml;
			if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:identifier", manager)) != null)
				identifier = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:keywords", manager)) != null)
				keywords = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:language", manager)) != null)
				language = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:lastModifiedBy", manager)) != null)
				lastModifiedBy = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:lastPrinted", manager)) != null)
				lastPrinted = DateTime.Parse (node.InnerXml);
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dcterms:modified", manager)) != null)
				modified = DateTime.Parse (node.InnerXml);
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:revision", manager)) != null)
				revision = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:subject", manager)) != null)
				subject = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/dc:title", manager)) != null)
				title = node.InnerXml;
            if ((node = doc.SelectSingleNode ("prop:coreProperties/prop:version", manager)) != null)
				version = node.InnerXml;
		}

		internal override void WriteTo(XmlTextWriter writer)
		{
			XmlDocument doc = new XmlDocument ();
			XmlNamespaceManager manager = new XmlNamespaceManager (doc.NameTable);
			manager.AddNamespace ("prop", NSPackageProperties);
			manager.AddNamespace ("dc", NSDc);
			manager.AddNamespace ("dcterms", NSDcTerms);
			manager.AddNamespace ("xsi", NSXsi);
			
			// Create XML declaration
			doc.AppendChild (doc.CreateXmlDeclaration ("1.0", "UTF-8", null));

			// Create root node with required namespace declarations
			XmlNode coreProperties = doc.AppendChild (doc.CreateNode (XmlNodeType.Element, "coreProperties", NSPackageProperties));
			coreProperties.Attributes.Append (doc.CreateAttribute ("xmlns:dc")).Value = NSDc;
			coreProperties.Attributes.Append (doc.CreateAttribute ("xmlns:dcterms")).Value = NSDcTerms;
			coreProperties.Attributes.Append (doc.CreateAttribute ("xmlns:xsi")).Value = NSXsi;

			// Create the children
			if (Category != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "category", NSPackageProperties)).InnerXml = Category;
			if (ContentStatus != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "contentStatus", NSPackageProperties)).InnerXml = ContentStatus;
			if (ContentType != null)
			coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "contentType", NSPackageProperties)).InnerXml = ContentType;
			if (Created.HasValue)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dcterms", "created", NSDcTerms)).InnerXml = Created.Value.ToString ();
			if (Creator != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "creator", NSDc)).InnerXml = Creator;
			if (Description != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "description", NSDc)).InnerXml = Description;
			if (Identifier != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "identifier", NSDc)).InnerXml = Identifier;
			if (Keywords != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "keywords", NSPackageProperties)).InnerXml = Keywords;
			if (Language != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "language", NSDc)).InnerXml = Language;
			if (LastModifiedBy != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "lastModifiedBy", NSPackageProperties)).InnerXml = LastModifiedBy;
			if (LastPrinted.HasValue)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "lastPrinted", NSPackageProperties)).InnerXml = LastPrinted.Value.ToString ();
			if (Revision != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "revision", NSPackageProperties)).InnerXml = Revision;
			if (Subject != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "subject", NSDc)).InnerXml = Subject;
			if (Title != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dc", "title", NSDc)).InnerXml = Title;
			if (Version != null)
				coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "version", NSPackageProperties)).InnerXml = Version;

			if (Modified.HasValue)
			{
				XmlAttribute att = doc.CreateAttribute("xsi", "type", NSXsi);
				att.Value = "dcterms:W3CDTF";
				
				XmlNode modified = coreProperties.AppendChild (doc.CreateNode (XmlNodeType.Element, "dcterms", "modified", NSDcTerms));
				modified.Attributes.Append (att);
				modified.InnerXml = Modified.Value.ToString ();
			}
			
			doc.WriteContentTo (writer);
		}
	}
}
