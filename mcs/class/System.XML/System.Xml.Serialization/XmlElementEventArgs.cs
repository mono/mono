//
// XmlElementEventArgs.cs: 
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
	/// Summary description for XmlElementEventArgs.
	/// </summary>
	public class XmlElementEventArgs : EventArgs
	{
		private XmlElement attr;
		private int lineNumber;
		private int linePosition;
		private object obj;

		internal XmlElementEventArgs(XmlElement attr, int lineNum, int linePos, object source)
		{
			this.attr		= attr;
			this.lineNumber = lineNum;
			this.linePosition = linePos;
			this.obj		= source;
		}

		public XmlElement Element
		{ 
			get { return attr; }
		}

		public int LineNumber 
		{
			get { return lineNumber; }
		}
		
		public int LinePosition 
		{
			get { return linePosition; }
		}
		
		public object ObjectBeingDeserialized 
		{
			get{ return obj; }
		}
	}
}
