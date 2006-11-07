//
// XmlNodeEventArgs.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
