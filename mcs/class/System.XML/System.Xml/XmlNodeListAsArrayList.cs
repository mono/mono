using System;
using System.Collections;

namespace System.Xml
{
	/// <summary>
	/// Internal implementation of XmlNodeList for XmlNode
	/// </summary>
	internal class XmlNodeListAsArrayList : XmlNodeList
	{
		// Private data members
		ArrayList _items;

		// Public Methods
		//===========================================================================
		public override int Count
		{
			get
			{
				return _items.Count;
			}
		}

		public IList data
		{
			get
			{
				return _items;
			}
		}
		
		// Public Methods
		//===========================================================================
		public override IEnumerator GetEnumerator()
		{
			return _items.GetEnumerator();
		}


		public override XmlNode Item(int index)
		{
			if ((index > 0) & ( index < _items.Count - 1))
				return _items[index] as XmlNode;
			else
				return null;
		}

		public XmlNodeListAsArrayList() : base()
		{
			_items = new ArrayList();
		}
	}
}
