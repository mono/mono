//
// System.Xml.XmlTextWriterOpenElement
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//
//
//	Scope support for XmlLang and XmlSpace in XmlTextWriter.
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
using System;

namespace System.Xml
{
	internal class XmlTextWriterOpenElement
	{
		#region Fields

		string prefix;
		string localName;
		string xmlLang;
		XmlSpace xmlSpace;
		bool indentingOverriden = false;

		#endregion

		#region Constructors

		public XmlTextWriterOpenElement (string prefix, string localName)
		{
			Reset (prefix, localName);
		}

		#endregion

		public void Reset (string prefix, string localName)
		{
			this.prefix = prefix;
			this.localName = localName;
			this.indentingOverriden = false;
			this.xmlLang = null;
			this.XmlSpace = XmlSpace.None;
		}

		#region Properties

		public string LocalName {
			get { return localName; }
		}

		public string Prefix {
			get { return prefix; }
		}

		public bool IndentingOverriden {
			get { return indentingOverriden; }
			set { indentingOverriden = value; }
		}

		public string XmlLang {
			get { return xmlLang; }
			set { xmlLang = value; }
		}

		public XmlSpace XmlSpace {
			get { return xmlSpace; }
			set { xmlSpace = value; }
		}

		#endregion
	}
}
