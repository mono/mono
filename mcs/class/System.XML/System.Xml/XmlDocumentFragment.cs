// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlDocumentFragment
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber
using System;

namespace System.Xml
{
	/// <summary>
	/// 
	/// </summary>
	public class XmlDocumentFragment : XmlNode
	{
		// Private data members

		// public properties
		//===========================================================================
		/// <summary>
		/// Returns the local name of the node with.  For document fragments, it returns "#document-fragment"
		/// </summary>
		public override string LocalName 
		{
			get
			{
				return "#document-fragment";
			}
		}


		/// <summary>
		/// Get the node name.  Document fragments return "#document-fragment".
		/// </summary>
		public override string Name 
		{ 
			get
			{
				return "#document-fragment";
			}
		}

		/// <summary>
		/// Overridden.  Returns XmlNodeType.DocumentFragment.
		/// </summary>
		public override XmlNodeType NodeType 
		{
			get
			{
				return XmlNodeType.DocumentFragment;
			}
		}
		
		// Public Methods
		//===========================================================================
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

		// Constructors
		//===========================================================================
		internal XmlDocumentFragment ( XmlDocument aOwner ) : base (aOwner)
		{
		}

	}
}
