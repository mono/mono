// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlDocument
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber

using System;
using System.IO;

namespace System.Xml
{
	/// <summary>
	/// Abstract class XmlNodeList.
	/// </summary>
	public class XmlDocument : XmlNode
	{
		// Private data members
		XmlResolver _resolver = null;

		// public properties
		
		/// <summary>
		/// Get the base URI for this document (the location from where the document was loaded)
		/// </summary>
		/// <example>If a document was loaded with doc.Load("c:\tmp\mydoc.xml"), 
		/// then BaseURI would hold "c:\tmp\mydoc.xml"</example>
		public override string BaseURI 
		{
			get
			{
				// TODO - implement XmlDocument.BaseURI {get;}
				throw new NotImplementedException("BaseURI.get not implemented");
			}
		}

		/// <summary>
		/// Get the root element for the document.  If no root exists, null is returned.
		/// </summary>
		public XmlElement DocumentElement 
		{
			get
			{
				// TODO - implement XmlDocument.Documentelement {get;}
				throw new NotImplementedException("XmlDocument.DocumentElement not implemented");
			}
		}

		/// <summary>
		/// Gets the node containing the DOCTYPE declaration.
		/// </summary>
		public virtual XmlDocumentType DocumentType 
		{
			get
			{
				// TODO - implement XmlDocument.DocumentType
				throw new NotImplementedException("XmlDocument.DocumentType not implemented");
			}
		}


		/// <summary>
		/// Get the XmlImplemenation for the current document.
		/// </summary>
		public XmlImplementation Implementation 
		{
			get
			{
				// TODO - implement XmlDocument.Implementation
				throw new NotImplementedException("Implementation not implemented");
			}
		}


		/// <summary>
		/// Get/Set the markup representing the children of the document.
		/// </summary>
		public override string InnerXml 
		{
			get
			{
				// TODO - implement XmlDocument.InnerXml {get;}
				throw new NotImplementedException("InnerXml get not implemented");
			}
			set
			{
				// TODO - implement XmlDocument.InnerXml {set;}
				throw new NotImplementedException("InnerXml set not implemented");
			}
		}

		/// <summary>
		/// Get a value indicating if the document is read-only.
		/// </summary>
		public override bool IsReadOnly 
		{
			get
			{
				// TODO - implement XmlDocument.IsReadOnly {get;}
				throw new NotImplementedException("IsReadOnly get not implemented");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override string LocalName {
			get
			{
				// TODO - implement XmlDocument.LocalName {get;}
				throw new NotImplementedException("LocalName get not implemented");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override string Name 
		{
			get
			{
				// TODO - implement XmlDocument.Name {get;}
				throw new NotImplementedException("Name get not implemented");
			}
		}

		public XmlNameTable NameTable 
		{
			get
			{
				// TODO - implement XmlDocument.NameTable {get;}
				throw new NotImplementedException("NameTable get not implemented");
			}
		}


		public override XmlNodeType NodeType 
		{
			get
			{
				return XmlNodeType.Document;
			}
		}

		/// <summary>
		/// Returns OwnerDocument.  For an XmlDocument, this property is always null.
		/// </summary>
		public override XmlDocument OwnerDocument 
		{
			get
			{
				return null;
			}
		}

		public bool PreserveWhitespace 
		{
			get
			{
				// TODO - implement XmlDocument.PreserveWhitespace {get;}
				throw new NotImplementedException("PreserveWhitespace get not implemented");
			}
			set
			{
				// TODO - implement XmlDocument.PreserveWhitespace {set;}
				throw new NotImplementedException("PreserveWhitespace set not implemented");
			}
		}

		public XmlResolver XmlResolver 
		{
			set
			{
				// TODO - Finish/test XmlDocument.XmlResolver {set;}
				_resolver = value;
			}
		}

		// Public Methods
		//===========================================================================
		public override XmlNode CloneNode(bool deep)
		{
			// TODO - implement XmlDocument.CloneNode(bool)
			throw new NotImplementedException("CloneNode(bool) not implemented");
		}

		public XmlAttribute CreateAttribute(string name)
		{
			// TODO - implement XmlDocument.CreateAttribute(string name)
			throw new NotImplementedException("CreateAttribute(string name) not implemented");
		}

		public XmlAttribute CreateAttribute(string qualifiedName,string namespaceURI)
		{
			// TODO - implement XmlDocument.CreateAttribute(string, string)
			throw new NotImplementedException("CreateAttribute(string, string) not implemented");
		}

		public virtual XmlAttribute CreateAttribute(
			string prefix,
			string localName,
			string namespaceURI
			)
		{
			// TODO - implement XmlDocument.CreateAttribute(prefix, localName, namespaceURI)
			throw new NotImplementedException("CreateAttribute(prefix, localName, namespaceURI) not implemented");
		}

		public virtual XmlCDataSection CreateCDataSection(string data)
		{
			// TODO - implement XmlDocument.CreateCDataSection(string data)
			throw new NotImplementedException("CreateCDataSection(string data) not implemented");
		}


		public virtual XmlComment CreateComment(string data)
		{
			// TODO - implement XmlDocument.CreateComment(string data)
			throw new NotImplementedException("CreateComment(string data) not implemented");
		}

		public virtual XmlDocumentFragment CreateDocumentFragment()
		{
			// TODO - implement XmlDocument.CreateDocumentFragment
			throw new NotImplementedException("CreateDocumentFragment not implemented");
		}

		public virtual XmlDocumentType CreateDocumentType(
			string name,
			string publicId,
			string systemId,
			string internalSubset
			)
		{
			// TODO - implement XmlDocument.CreateDocumentType
			throw new NotImplementedException("CreateDocumentType not implemented");
		}

		public XmlElement CreateElement(string name)
		{
			// TODO - implement XmlDocument.CreateElement(string name)
			throw new NotImplementedException("CreateElement(string name) not implemented");
		}

		public XmlElement CreateElement(
			string qualifiedName,
			string namespaceURI
			)
		{
			// TODO - implement XmlDocument.CreateElement(string qualifiedName,	string namespaceURI)
			throw new NotImplementedException("CreateElement(string qualifiedName,	string namespaceURI) not implemented");
		}

		public virtual XmlElement CreateElement(
			string prefix,
			string localName,
			string namespaceURI
			)
		{
			// TODO - implement XmlDocument.CreateElement(prefix, localName, namespaceURI)
			throw new NotImplementedException("XmlDocument.CreateElement(prefix, localName, namespaceURI) not implemented.");
		}


		public virtual XmlEntityReference CreateEntityReference(string name)
		{
			// TODO - implement XmlDocument.CreateEntityReference
			throw new NotImplementedException("XmlDocument.CreateEntityReference not implemented.");
		}

		public virtual XmlNode CreateNode(
			string nodeTypeString,
			string name,
			string namespaceURI
			)
		{
			// TODO - implement XmlDocument.CreateNode(string, string, string)
			throw new NotImplementedException("XmlDocument.CreateNode not implemented.");
		}

		public virtual XmlNode CreateNode(
			XmlNodeType type,
			string name,
			string namespaceURI
			)
		{
			// TODO - implement XmlDocument.CreateNode(XmlNodeType, string, string)
			throw new NotImplementedException("XmlDocument.CreateNode not implemented.");
		}

		public virtual XmlNode CreateNode(
			XmlNodeType type,
			string prefix,
			string name,
			string namespaceURI
			)
		{
			// TODO - implement XmlDocument.CreateNode(XmlNodeType, string, string, string)
			throw new NotImplementedException("XmlDocument.CreateNode not implemented.");
		}

		public virtual XmlProcessingInstruction CreateProcessingInstruction(
			string target,
			string data
			)
		{
			// TODO - implement XmlDocument.CreateProcessingInstruction
			throw new NotImplementedException("XmlDocument.CreateProcessingInstruction not implemented.");
		}

		public virtual XmlSignificantWhitespace CreateSignificantWhitespace(string text	)
		{
			// TODO - implement XmlDocument.CreateSignificantWhitespace
			throw new NotImplementedException("XmlDocument.CreateSignificantWhitespace not implemented.");
		}

		public virtual XmlText CreateTextNode(string text)
		{
			// TODO - implement XmlDocument.CreateTextNode
			throw new NotImplementedException("XmlDocument.CreateTextNode not implemented.");
		}

		public virtual XmlWhitespace CreateWhitespace(string text)
		{
			// TODO - implement XmlDocument.CreateWhitespace
			throw new NotImplementedException("XmlDocument.CreateWhitespace not implemented.");
		}

		public virtual XmlDeclaration CreateXmlDeclaration(
			string version,
			string encoding,
			string standalone
			)
		{
			// TODO - implement XmlDocument.CreateXmlDeclaration
			throw new NotImplementedException("XmlDocument.CreateXmlDeclaration not implemented.");
		}

		public override bool Equals(object obj)
		{
			// TODO - implement XmlDocument.Equals(object obj)
			throw new NotImplementedException("XmlDocument.Equals(object obj) not implemented.");
		}

		public static bool Equals(
			object objA,
			object objB
			)
		{
			// TODO - implement XmlDocument.Equals(object objA, objB)
			throw new NotImplementedException("XmlDocument.Equals(object objA, objB) not implemented.");
		}

		public virtual XmlElement GetElementById(string elementId)
		{
			// TODO - implement XmlDocument.GetElementById
			throw new NotImplementedException("XmlDocument.GetElementById not implemented.");
		}

		public virtual XmlNodeList GetElementsByTagName(string name)
		{
			// TODO - implement XmlDocument.GetElementsByTagName(name)
			throw new NotImplementedException("XmlDocument.GetElementsByTagName not implemented.");
		}

		public virtual XmlNodeList GetElementsByTagName(
			string localName,
			string namespaceURI
			)
		{
			// TODO - implement XmlDocument.GetElementsByTagName(localName, namespaceURI)
			throw new NotImplementedException("XmlDocument.GetElementsByTagName not implemented.");
		}

		public virtual XmlNode ImportNode(
			XmlNode node,
			bool deep
			)
		{
			// TODO - implement XmlDocument.ImportNode
			throw new NotImplementedException("XmlDocument.ImportNode not implemented.");
		}

		public virtual void Load(Stream inStream)
		{
			// TODO - implement XmlDocument.Load(Stream)
			throw new NotImplementedException("XmlDocument.Load(Stream) not implemented.");
		}

		public virtual void Load(string filename)
		{
			// TODO - implement XmlDocument.Load(string)
			throw new NotImplementedException("XmlDocument.Load(string) not implemented.");
		}

		public virtual void Load(TextReader txtReader)
		{
			// TODO - implement XmlDocument.Load(TextReader)
			throw new NotImplementedException("XmlDocument.Load(TextReader) not implemented.");
		}

		public virtual void Load(XmlReader reader)
		{
			// TODO - implement XmlDocument.Load(XmlReader)
			throw new NotImplementedException("XmlDocument.Load(XmlReader) not implemented.");
		}

		public virtual void LoadXml(string xml)
		{
			// TODO - implement XmlDocument.LoadXml
			throw new NotImplementedException("XmlDocument.LoadXml not implemented.");
		}

		public virtual void Save(Stream outStream)
		{
			// TODO - implement XmlDocument.Save(Stream)
			throw new NotImplementedException("XmlDocument.Save(Stream) not implemented.");
		}

		public virtual void Save(string filename)
		{
			// TODO - implement XmlDocument.Save(string)
			throw new NotImplementedException("XmlDocument.Save(string) not implemented.");
		}

		public virtual void Save(TextWriter writer)
		{
			// TODO - implement XmlDocument.Save(TextWriter)
			throw new NotImplementedException("XmlDocument.Save(TextWriter) not implemented.");
		}

		public virtual void Save(XmlWriter writer)
		{
			// TODO - implement XmlDocument.Save(XmlWriter)
			throw new NotImplementedException("XmlDocument.Save(XmlWriter) not implemented.");
		}

		public override void WriteContentTo(XmlWriter w)
		{
			// TODO - implement XmlDocument.WriteContentTo
			throw new NotImplementedException("XmlDocument.WriteContentTo not implemented.");
		}

		public override void WriteTo(XmlWriter w)
		{
			// TODO - implement XmlDocument.WriteTo
			throw new NotImplementedException("XmlDocument.WriteTo not implemented.");
		}

		// Public events
		//===========================================================================
		public delegate void XmlNodeChangedEventHandler (XmlNodeChangedEventArgs args);

		public event XmlNodeChangedEventHandler NodeChanged;

		public event XmlNodeChangedEventHandler NodeChanging;

		public event XmlNodeChangedEventHandler NodeInserted;

		public event XmlNodeChangedEventHandler NodeInserting;

		public event XmlNodeChangedEventHandler NodeRemoved;

		public event XmlNodeChangedEventHandler NodeRemoving;

		// Constructors
		//===========================================================================


	}
}
