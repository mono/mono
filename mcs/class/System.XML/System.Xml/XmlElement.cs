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


		// Implement abstract methods of XmlNode
		//=====================================================================
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
			get
			{
				// TODO - implement LocalName
				return String.Empty;
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
				// TODO - implement Name
				return String.Empty;
			}
		}

		public override XmlNodeType NodeType 
		{
			get
			{
				return XmlNodeType.Element;
			}
		}


		// ============= Internal calls =============================================


		} // class
	}  //namespace
