// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlText
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber


using System;

namespace System.Xml
{
	/// <summary>
	/// Represents the text content of an element or attribute
	/// </summary>
	public class XmlText : XmlNode
	{
		// Private data members

		// public properties
		public override string LocalName 
		{
			get
			{
				return "#text";
			}
		}
		/// <summary>
		/// Get the name of the node.
		/// </summary>
		public override string Name 
		{
			get
			{
				return "#text";
			}
		}

		public override XmlNodeType NodeType
		{
			get
			{
				return XmlNodeType.Text;
			}
		}
		

		// Public Methods
		//===========================================================================
		public override XmlNode CloneNode( bool deep)
		{
			throw new NotImplementedException();
		}

		public override void WriteContentTo(XmlWriter w)
		{
			throw new NotImplementedException();
		}

		public override void WriteTo(XmlWriter w)
		{
			throw new NotImplementedException();
		}

		// Internal method calls
		//===========================================================================

		// Constructors
		//===========================================================================

	}
}
