//
// System.Xml.XmlNodeList
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.Collections;

namespace System.Xml
{
	public class XmlNodeListChildren : XmlNodeList
	{
		#region Enumerator
		///////////////////////////////////////////////////////////////////////
		//
		//	Enumerator
		//
		///////////////////////////////////////////////////////////////////////
		private class Enumerator : IEnumerator
		{
			XmlLinkedNode currentChild;
			XmlLinkedNode lastChild;

			internal Enumerator (XmlLinkedNode lastChild)
			{
				currentChild = null;
				this.lastChild = lastChild;
			}

			public virtual object Current {
				get {
					return currentChild;
				}
			}

			public virtual bool MoveNext()
			{
				bool passedEndOfCollection = Object.ReferenceEquals(currentChild, lastChild);

				if (currentChild == null) {
					currentChild = lastChild;
				}

				currentChild = currentChild.NextLinkedSibling;

				return passedEndOfCollection;
			}

			public virtual void Reset()
			{
				currentChild = null;
			}
		}

		#endregion

		#region Fields
		///////////////////////////////////////////////////////////////////////
		//
		//	Fields
		//
		///////////////////////////////////////////////////////////////////////

		XmlLinkedNode lastChild;

		#endregion

		#region Constructors
		///////////////////////////////////////////////////////////////////////
		//
		//	Constructors
		//
		///////////////////////////////////////////////////////////////////////

		public XmlNodeListChildren(XmlLinkedNode lastChild)
		{
			this.lastChild = lastChild;
		}

		#endregion

		#region Properties
		///////////////////////////////////////////////////////////////////////
		//
		//	Properties
		//
		///////////////////////////////////////////////////////////////////////

		public override int Count {
			get {
				int count = 0;

				if (lastChild != null) {
					XmlLinkedNode currentChild = lastChild.NextLinkedSibling;
					
					count = 1;
					while (!Object.ReferenceEquals(currentChild, lastChild)) {
						currentChild = currentChild.NextLinkedSibling;
						count++;
					}
				}

				return count;
			}
		}

		#endregion

		#region Methods
		///////////////////////////////////////////////////////////////////////
		//
		//	Methods
		//
		///////////////////////////////////////////////////////////////////////

		public override IEnumerator GetEnumerator ()
		{
			return new Enumerator(lastChild);
		}

		public override XmlNode Item (int index)
		{
			XmlNode requestedNode = null;

			// Instead of checking for && index < Count which has to walk
			// the whole list to get a count, we'll just keep a count since
			// we have to walk the list anyways to get to index.
			if ((index >= 0) && (lastChild != null)) {
				XmlLinkedNode currentChild = lastChild.NextLinkedSibling;
				int count = 0;

				while ((count < index) && !Object.ReferenceEquals(currentChild, lastChild)) 
				{
					currentChild = currentChild.NextLinkedSibling;
					count++;
				}

				if (count == index) {
					requestedNode = currentChild;
				}
			}

			return requestedNode;
		}

		#endregion
	}

	
}
