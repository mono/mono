// System.Xml.XmlCharacterData.cs
//
// Author: Daniel Weber (daniel-weber@austin.rr.com)
//
// Implementation of abstract Xml.XmlCharacterData class
//
// Provides text manipulation methods used by derived classes
//	abstract class

using System;

namespace System.Xml
{
	/// <summary>
	/// Abstratc class to provide text manipulation methods for derived classes
	/// </summary>
	public abstract class XmlCharacterData : XmlLinkedNode
	{
		// ============ Public Properties =====================================
		//=====================================================================
		/// <summary>
		/// Contains the nodes data
		/// </summary>
		public virtual string Data 
		{
			get
			{
				// TODO - implement Data {get;}
				throw new NotImplementedException();
			}
			
			set
			{
				// TODO - implement Data {set;}
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Get/Set the nodes value
		/// </summary>
		public override string Value 
		{
			get
			{
				// TODO - implement Value {get;}
				throw new NotImplementedException();
			}

			set
			{
				// TODO - implement Value {set;}
				throw new NotImplementedException();
			}
		}

		public override string InnerText 
		{
			get
			{
				// TODO - implement InnerText {get;}
				throw new NotImplementedException();
			}

			set
			{
				// TODO - implement InnerText {set;}
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Returns the length of data, in characters
		/// </summary>
		public virtual int Length 
		{
			get
			{
				// TODO - implement Length {get;}
				throw new NotImplementedException();
			}
		}

		// ============ Public Methods  =======================================
		//=====================================================================
		/// <summary>
		/// Appends string strData to the end of data
		/// </summary>
		/// <param name="strData"></param>
		public virtual void AppendData(string strData)
		{
			// TODO - implement AppendData(strData)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Remove a range of characters from the node
		/// </summary>
		/// <param name="offset">offset, in characters, to start delete</param>
		/// <param name="count">Number of characters to delete</param>
		public virtual void DeleteData(int offset, int count)
		{
			// TODO - implement DeleteData(offset, count)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Replaces the number of characters, starting at offset, with the passed string
		/// </summary>
		/// <param name="offset">Offset (in characters) to start replacement</param>
		/// <param name="count">Number of characters to replace</param>
		/// <param name="strData">Replacement string</param>
		public virtual void ReplaceData(int offset, int count, string strData)
		{
			// TODO - implement ReplaceData(offset, count, strData)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieves a substring of the specified range
		/// </summary>
		/// <param name="offset">Character offset to begin string</param>
		/// <param name="count">Number of characters to return</param>
		/// <returns></returns>
		public virtual string Substring(int offset, int count)
		{
			// TODO - implement Substring(offset, count)
			throw new NotImplementedException();
		}

		// ============ Protected Methods  ====================================
		//=====================================================================
		/// <summary>
		/// Listed in beta 2, but no description
		/// [to be supplied]
		/// </summary>
		/// <param name="node"></param>
		/// <param name="xnt"></param>
		/// <returns></returns>
		protected internal bool DecideXPNodeTypeForWhitespace(
			XmlNode node,
			ref XPathNodeType xnt
			)
		{
			// TODO
			throw new NotImplementedException();
		}

		// Constructors
		internal XmlCharacterData ( XmlDocument aOwnerDoc) : base(aOwnerDoc)
		{
		}
	}
}