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
