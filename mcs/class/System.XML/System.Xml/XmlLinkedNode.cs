// System.Xml.XmlLinkedNode.cs
//
// Author: Daniel Weber (daniel-weber@austin.rr.com)
//
// Implementation of abstract Xml.XmlLinkedNode class

using System;

namespace System.Xml
{
	public abstract class XmlLinkedNode : XmlNode
	{
		private XmlNode _nextSibling;
		private XmlNode _previousSibling;

		// ============ Properties ============================================
		//=====================================================================
		/// <summary>
		/// Get the node immediately following this node
		/// </summary>
		public override XmlNode NextSibling
		{
			get
			{
				return _nextSibling;
			}
		}

		/// <summary>
		/// Get the node immediately previous to this node
		/// </summary>
		public override XmlNode PreviousSibling 
		{
			get
			{
				return _previousSibling;
			}
		}

		// Internal accessor methods
		//===========================================================================
		internal void setPreviousNode ( XmlNode previous )
		{
			_previousSibling = previous;
		}

		internal void setNextSibling ( XmlNode next )
		{
			_nextSibling = next;
		}

		// Constructors
		//===========================================================================
		internal XmlLinkedNode( XmlDocument aOwner ) : base(aOwner)
		{
		}
	}
}