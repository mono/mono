// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlElement
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber
using System;

namespace System.Xml
{
	public class XmlElement : XmlLinkedNode
	{
		// Private/Protected internal data structures
		//===========================================================================
		private XmlAttributeCollection _attributes;

		private string prefix;
		private string localName;
		private string namespaceURI;

		// Public Properties
		//===========================================================================

		/// <summary>
		/// Return the XmlAttributeCollection on the Element
		/// </summary>
		public override XmlAttributeCollection Attributes
		{
			get
			{
				// TODO - implement Attributes
				return _attributes;
			}
		}

		/// <summary>
		/// Get/Set the value for this node
		/// </summary>
		public override string Value
		{
			get
			{
				return null;
			}

			set
			{
				// Do nothing, can't set value on XmlElement...
			}
		}

		// Implement abstract methods of XmlNode
		//=====================================================================
		/// <summary>
		/// Remove all children and attributes.  If
		/// </summary>
		public override void RemoveAll()
		{
			// Remove all child nodes
			base.RemoveAll();

			// Remove all attributes
			_attributes.RemoveAll();

			// If we have any default attributes, add them back in with the
			//	appropriate namespace, baseURI, name, localName
			// TODO - implement adding default attributes back in XmlElement.RemoveAll()
		}

		/// <summary>
		/// Return a clone of the node
		/// </summary>
		/// <param name="deep">Make copy of all children</param>
		/// <returns>Cloned node</returns>
		public override XmlNode CloneNode( bool deep)
		{
			// TODO - implement CloneNode()
			throw new NotImplementedException();
		}

		/// <summary>
		/// Saves all children of the current node to the passed writer
		/// </summary>
		/// <param name="w"></param>
		public override void WriteContentTo(XmlWriter w)
		{
			// TODO - implement WriteContentsTo(XmlWriter)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Saves the current node to writer w
		/// </summary>
		/// <param name="w"></param>
		public override void WriteTo(XmlWriter w)
		{
			// TODO - implement WriteTo(XmlWriter)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the local name of the node with qualifiers removed
		/// LocalName of ns:elementName = "elementName"
		/// </summary>
		public override string LocalName
		{
			get {
				return localName;
			}
		}


		/// <summary>
		/// Get the qualified node name
		/// derived classes must implement as behavior varies
		/// by tag type.
		/// </summary>
		public override string Name
		{
			get
			{
				return prefix != String.Empty ? prefix + ":" + localName : localName;
			}
		}

		public override string NamespaceURI
		{
			get
			{
				return namespaceURI;
			}
		}

		public override XmlNodeType NodeType
		{
			get
			{
				return XmlNodeType.Element;
			}
		}

		public override string Prefix
		{
			get
			{
				return prefix;
			}
		}


		// ============= Internal calls =============================================

		// Constructors
		// ==========================================================================
		protected internal XmlElement(string prefix, string localName, string namespaceURI, XmlDocument doc) : base(doc)
		{
			this.prefix = prefix;
			this.localName = localName;
			this.namespaceURI = namespaceURI;

			_attributes = new XmlAttributeCollection(doc, this, null);
		}


		} // class
	}  //namespace
