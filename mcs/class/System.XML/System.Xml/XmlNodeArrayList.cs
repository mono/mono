//
// System.Xml.XmlNodeArrayList
//
// Author:
//   Piers Haken <piersh@friskit.com>
//
// (C) 2002 Piers Haken
//

using System;
using System.Collections;

namespace System.Xml
{
	internal class XmlNodeArrayList : XmlNodeList
	{
		ArrayList _rgNodes;

		public XmlNodeArrayList (ArrayList rgNodes)
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
