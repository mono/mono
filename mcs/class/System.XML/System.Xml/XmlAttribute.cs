// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlAttribute
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weberusing System;

namespace System.Xml
{
	/// <summary>
	/// Summary description for XmlAttribute.
	/// </summary>
	public class XmlAttribute : XmlNode
	{
		// ============  private data structures ==============================
		private  XmlNode FOwner;
		private XmlNode FOwnerElement;
		
		string FelementName;
		string FattrName;
		string FattrValue;
		
		//==== Public Properties ====================================================



		/// <summary>
		/// Saves all children of the current node to the passed writer
		/// </summary>
		/// <param name="w"></param>
		public override void WriteContentTo(XmlWriter w)
		{
			// TODO - implement XmlAttribute.WriteContentsTo(XmlWriter)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Saves the current node to writer w
		/// </summary>
		/// <param name="w"></param>
		public override void WriteTo(XmlWriter w)
		{
			// TODO - implement XmlAttribute.WriteTo(XmlWriter)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the local name of the attribute.  For attributes, this is the same as Name
		/// </summary>
		public override string LocalName 
		{
			get
			{
				return Name;
			}
		}

		/// <summary>
		/// Get the XmlNode representing the owning document
		/// </summary>
		public override XmlDocument OwnerDocument
		{
			get
			{
				if (FOwner.NodeType == XmlNodeType.Document)
					return FOwner as XmlDocument;
				else
					return null;
			}
		}

		/// <summary>
		/// Retrieve the XmlElement owner of this attribute, or null if attribute not assigned
		/// </summary>
		public virtual XmlElement OwnerElement
		{
			get
			{
				if (FOwnerElement.NodeType == XmlNodeType.Element)
					return FOwnerElement as XmlElement;
				else
					return null;
			}
		}
		/// <summary>
		/// Get the qualified attribute name.  Attributes do not have an associated namespace.
		/// </summary>
		public override string Name 
		{ 
			get
			{
				return Name;
			}
		}

		//============== Public Methods =============================================
		/// <summary>
		/// Override.  Returns the node type.
		/// </summary>
		public override XmlNodeType NodeType 
		{
			get
			{
				return XmlNodeType.Attribute;
			}
		}

		/// <summary>
		/// Return a clone of the node
		/// </summary>
		/// <param name="deep">Make copy of all children</param>
		/// <returns>Cloned node</returns>
		public override XmlNode CloneNode( bool deep)
		{
			// TODO - implement XmlAttribute.CloneNode()
			throw new NotImplementedException();
		}

		// ============  Internal methods  ====================================
		internal void setOwnerElement( XmlElement newOwnerElement)
		{
			FOwnerElement = newOwnerElement;
		}

		// ============  Constructors =========================================
		public XmlAttribute(XmlDocument aOwnerDoc) : base(aOwnerDoc)
		{
			//
			// TODO: Add constructor logic here
			//
		}

		internal XmlAttribute ( XmlDocument aOwner,			// owner document
			string elementName,								// can be ""
			string attributeName,							// cannot be ""
			// attType,
			// defaultDecl,
			string attValue) : base(aOwner)
		{
			if (aOwner == null)
				throw new ArgumentException("Null OwnerDocument passed to XmlAttribute constructor");
			if (attributeName.Length == 0)
				throw new ArgumentException("Empty string passed to XmlAttribute constructor");

			FOwner = aOwner;
			FOwnerElement = null;
			FelementName = elementName;
			FattrName = attributeName;
			FattrValue = attValue;
		}
	

	}
}
