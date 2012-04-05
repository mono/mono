//
// System.Xml.XmlImplementation.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
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
using System.Globalization;

namespace System.Xml
{
	public class XmlImplementation
	{
		#region Constructor
		public XmlImplementation ()
			: this (new NameTable ())
		{
		}

		public XmlImplementation (XmlNameTable nt)
		{
			InternalNameTable = nt;
		}
		#endregion

		#region Public Methods
		public virtual XmlDocument CreateDocument ()
		{
			return new XmlDocument (this);
		}

		public bool HasFeature (string strFeature, string strVersion)
		{
			if (String.Compare (strFeature, "xml", true, CultureInfo.InvariantCulture) == 0) { // not case-sensitive
				switch (strVersion) {
				case "1.0":
				case "2.0":
				case null:
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Internals
		internal XmlNameTable InternalNameTable;
		#endregion
	}
}
