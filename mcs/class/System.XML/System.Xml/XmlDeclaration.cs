// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlDeclaration
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
	public class XmlDeclaration : XmlNode
	{
		// Private data members
		private string Fencoding = "UTF-8";
		bool Fstandalone;

		// public properties
		/// <summary>
		/// Get/Set the encoding used for the document
		/// Typical values are "UTF-8", "UTF-16", "ISO-8859-nn" (where n is 0-9).
		/// If not defined, "UTF-8" is assumed.
		/// encoding is not case sensitive.
		/// </summary>
		public string Encoding 
		{
			get
			{
				return Fencoding;
			}

			set
			{
				string val = value.ToUpper();

				if ( (val == "UTF-8") | ( val == "UTF-16") )
				{
					Fencoding = value;
					return;
				}
				else
				{
					if ( ( val.StartsWith( "ISO-8859-" ) ) & (val.Length == 10) )
					{
						try
						{
							int code = System.Convert.ToInt32( val.Substring(9,1) );
							Fencoding = value;
						}
						catch
						{
							throw new NotImplementedException("Encoding " + value + " is not supported");
						}

					}

					
				}
			}
		}
		
		/// <summary>
		/// Get the local name of the declaration.  Returns "xml"
		/// </summary>
		public override string LocalName 
		{
			get
			{
				return "xml";
			}
		}

		/// <summary>
		/// Get the name of the node.  For XmlDeclaration, returns "xml".
		/// </summary>
		public override string Name 
		{
			get
			{
				return "xml";
			}
		}

		/// <summary>
		/// Return the node type.  For XmlDeclaration, returns XmlNodeType.XmlDeclaration.
		/// </summary>
		public override XmlNodeType NodeType 
		{
			get
			{
				return XmlNodeType.XmlDeclaration;
			}
		}

		/// <summary>
		/// Get/Set the value of the standalone attribute.
		/// "yes" => no external DTD required.
		/// "no" => external data sources required.
		/// Silently fails if Set to invalid value.
		/// Not case sensitive.
		/// </summary>
		public string Standalone {
			get
			{
				if (Fstandalone) 
					return "yes";
				else
					return "no";
			}

			set
			{
				if (value.ToUpper() == "YES")
					Fstandalone = true;
				if (value.ToUpper() == "NO")
					Fstandalone = false;
			}
		}

		/// <summary>
		/// Get the xml version of the file.  Returns "1.0"
		/// </summary>
		public string Version 
		{
			get
			{
				return "1.0";
			}
		}

		// Public Methods
		/// <summary>
		/// Overriden.  Returns a cloned version of this node.
		/// Serves as a copy constructor.  Duplicate node has no parent.
		/// </summary>
		/// <param name="deep">Create deep copy.  N/A for XmlDeclaration.</param>
		/// <returns>Cloned node.</returns>
		public override XmlNode CloneNode(bool deep)
		{
			// TODO - implement XmlDeclration.CloneNode()
			throw new NotImplementedException("XmlDeclration.CloneNode() not implemented");
		}

		/// <summary>
		/// Save the children of this node to the passed XmlWriter.  Since an XmlDeclaration has 
		/// no children, this call has no effect.
		/// </summary>
		/// <param name="w"></param>
		public override void WriteContentTo(XmlWriter w)
		{
			// Nothing to do - no children.
		}

		/// <summary>
		/// Saves the node to the specified XmlWriter
		/// </summary>
		/// <param name="w">XmlWriter to writ to.</param>
		public override void WriteTo(XmlWriter w)
		{
			// TODO - implement XmlDeclration.WriteTo()
			throw new NotImplementedException("XmlDeclaration.WriteTo() not implemented");
		}

		// Constructors
		internal XmlDeclaration( XmlDocument aOwnerDoc) : base(aOwnerDoc)
		{
		}
	}
}
