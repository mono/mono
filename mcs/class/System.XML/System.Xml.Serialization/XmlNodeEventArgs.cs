//
// XmlNodeEventArgs.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Xml;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlNodeEventArgs.
	/// </summary>
	public class XmlNodeEventArgs : EventArgs
	{
		private int linenumber;
		private int lineposition ;
		private string localname;
		private string name;
		private string nsuri;
		private XmlNodeType nodetype;
		private object source;
		private string text;

		internal XmlNodeEventArgs(int linenumber, int lineposition, string localname, string name, string nsuri,
			XmlNodeType nodetype, object source, string text)
		{
			this.linenumber = linenumber;
			this.lineposition = lineposition;
			this.localname = localname;
			this.name = name;
			this.nsuri = nsuri;
			this.nodetype = nodetype;
			this.source = source;
			this.text = text;
		}

		public int LineNumber 
		{
			get { return linenumber; } 
		}
		
		public int LinePosition 
		{
			get { return lineposition; } 
		}

		public string LocalName
		{
			get { return localname; } 
		}

		public string Name 
		{
			get { return name; } 
		}

		public string NamespaceURI
		{
			get { return nsuri; } 
		}

		public XmlNodeType NodeType
		{
			get { return nodetype; } 
		}

		public object ObjectBeingDeserialized 
		{
			get { return source; } 
		}

		public string Text
		{
			get { return text; } 
		}
	}
}
