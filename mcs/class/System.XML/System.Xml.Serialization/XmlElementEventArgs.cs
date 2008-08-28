//
// XmlElementEventArgs.cs: 
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
	/// Summary description for XmlElementEventArgs.
	/// </summary>
	public class XmlElementEventArgs : EventArgs
	{
		private XmlElement attr;
		private int lineNumber;
		private int linePosition;
		private object obj;
#if NET_2_0		
		private string expectedElements;
#endif

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

#if NET_2_0
		public string ExpectedElements {
			get { return expectedElements; }
			internal set { expectedElements = value; }
		}
#endif
	}
}
