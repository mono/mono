//
// XmlDsigNodeList.cs - derived node list class for dsig
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// This class is mostly copied from System.Xml/XmlNodeArrayList.cs
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
using System.Collections;
using System.Xml;

namespace System.Security.Cryptography.Xml
{
	// Copied from XmlNodeArrayList.cs
	internal class XmlDsigNodeList : XmlNodeList
	{
		ArrayList _rgNodes;

		public XmlDsigNodeList (ArrayList rgNodes)
		{
			_rgNodes = rgNodes;
		}

		public override int Count { get { return _rgNodes.Count; } }

		public override IEnumerator GetEnumerator ()
		{
			return _rgNodes.GetEnumerator ();
		}

		public override XmlNode Item (int index)
		{
			// Return null if index is out of range. by  DOM design.
			if (index < 0 || _rgNodes.Count <= index)
				return null;

			return (XmlNode) _rgNodes [index];
		}
	}
}
