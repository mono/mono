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

	public delegate void XmlNodeChangedEventHandler (XmlNodeChangedEventArgs args);

	/// <summary>
	/// Abstract class XmlNodeList.
	/// </summary>
	public class XmlDocument : XmlNode
	{
		// Private data members
		XmlResolver _resolver = null;

		// Public events
		//===========================================================================
		public event XmlNodeChangedEventHandler NodeChanged;

		public event XmlNodeChangedEventHandler NodeChanging;

		public event XmlNodeChangedEventHandler NodeInserted;

		public event XmlNodeChangedEventHandler NodeInserting;

		public event XmlNodeChangedEventHandler NodeRemoved;

		public event XmlNodeChangedEventHandler NodeRemoving;

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
				XmlNode node = FirstChild;

				while (node != null) {
					if (node is XmlElement)
						break;
					node = node.NextSibling;
				}

				return node != null ? node as XmlElement : null;
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
				return false;
			}
		}

		/// <summary>
		/// Get the local name of the node.  For documents, returns "#document"
		/// </summary>
		public override string LocalName {
			get
			{
				return "#document";
			}
		}

		/// <summary>
		/// Get the qualified name of the node.  For documents, returns "#document"
		/// </summary>
		public override string Name
		{
			get
			{
				return "#document";
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
			return new XmlAttribute(this, name, "");
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
			return new XmlElement(prefix, localName, namespaceURI, this);
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
			XmlReader	xmlReader = new XmlTextReader(new StringReader(xml));
			XmlNode		currentNode = this;
			XmlNode		newNode;

			// Reset our document
			// For now this just means removing all our children but later this
			// may turn out o need to call a private method that resets other things
			// like properties we have, etc.
			RemoveAll();

			// Wrapping in try/catch for now until XmlTextReader starts throwing XmlException
			try 
			{
				while (xmlReader.Read())
				{
					switch(xmlReader.NodeType)
					{
						case XmlNodeType.Element:
							newNode = CreateElement(xmlReader.Name, xmlReader.LocalName, xmlReader.NamespaceURI);
							currentNode.AppendChild(newNode);
							if (!xmlReader.IsEmptyElement)
							{
								currentNode = newNode;
							}
							break;
						
						case XmlNodeType.Text:
							newNode = CreateTextNode(xmlReader.Value);
							currentNode.AppendChild(newNode);
							break;

						case XmlNodeType.EndElement:
							currentNode = currentNode.ParentNode;
							break;
					}
				}
			}
			catch(Exception e)
			{
				throw new XmlException(e.Message, e);
			}
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


		// Internal functions
		//===========================================================================
		internal void onNodeChanging(XmlNode node, XmlNode Parent)
		{
			if (NodeInserting != null)
				NodeChanging( new XmlNodeChangedEventArgs(XmlNodeChangedAction.Change,
					node, Parent, Parent));
		}

		internal void onNodeChanged(XmlNode node, XmlNode Parent)
		{
			if (NodeChanged != null)
				NodeInserted( new XmlNodeChangedEventArgs(XmlNodeChangedAction.Change,
					node, Parent, Parent));
		}

		internal void onNodeInserting(XmlNode node, XmlNode newParent)
		{
			if (NodeInserting != null)
				NodeInserting( new XmlNodeChangedEventArgs(XmlNodeChangedAction.Insert,
					node, null, newParent));
		}

		internal void onNodeInserted(XmlNode node, XmlNode newParent)
		{
			if (NodeInserted != null)
				NodeInserted( new XmlNodeChangedEventArgs(XmlNodeChangedAction.Insert,
					node, null, newParent));
		}

		internal void onNodeRemoving(XmlNode node, XmlNode oldParent)
		{
			if (NodeRemoving != null)
				NodeRemoving(new XmlNodeChangedEventArgs(XmlNodeChangedAction.Remove,
					node, oldParent, null));
		}

		internal void onNodeRemoved(XmlNode node, XmlNode oldParent)
		{
			if (NodeRemoved != null)
				NodeRemoved(new XmlNodeChangedEventArgs(XmlNodeChangedAction.Remove,
					node, oldParent, null));

		}

		// Constructors
		//===========================================================================
		public XmlDocument() : base(null)
		{
			FOwnerDocument = this;
		}


	}
}
