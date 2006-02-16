//
// System.Xml.XmlNodeList
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
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

namespace System.Xml
{
	internal class XmlNodeListChildren : XmlNodeList
	{
		#region Enumerator

		private class Enumerator : IEnumerator
		{
			IHasXmlChildNode parent;
			XmlLinkedNode currentChild;
			bool passedLastNode;

			internal Enumerator (IHasXmlChildNode parent)
			{
				currentChild = null;
				this.parent = parent;
				passedLastNode = false;
			}

			public virtual object Current {
				get {
					if ((currentChild == null) ||
						(parent.LastLinkedChild == null) ||
						(passedLastNode == true))
						throw new InvalidOperationException();

					return currentChild;
				}
			}

			public virtual bool MoveNext()
			{
				bool movedNext = true;

				if (parent.LastLinkedChild == null) {
					movedNext = false;
				}
				else if (currentChild == null) {
					currentChild = parent.LastLinkedChild.NextLinkedSibling;
				}
				else {
					if (Object.ReferenceEquals(currentChild, parent.LastLinkedChild)) {
						movedNext = false;
						passedLastNode = true;
					}
					else {
						currentChild = currentChild.NextLinkedSibling;
					}
				}

				return movedNext;
			}

			public virtual void Reset()
			{
				currentChild = null;
			}
		}

		#endregion

		#region Fields

		IHasXmlChildNode parent;

		#endregion

		#region Constructors
		public XmlNodeListChildren(IHasXmlChildNode parent)
		{
			this.parent = parent;
		}

		#endregion

		#region Properties

		public override int Count {
			get {
				int count = 0;

				if (parent.LastLinkedChild != null) {
					XmlLinkedNode currentChild = parent.LastLinkedChild.NextLinkedSibling;
					
					count = 1;
					while (!Object.ReferenceEquals(currentChild, parent.LastLinkedChild)) {
						currentChild = currentChild.NextLinkedSibling;
						count++;
					}
				}

				return count;
			}
		}

		#endregion

		#region Methods

		public override IEnumerator GetEnumerator ()
		{
			return new Enumerator(parent);
		}

		public override XmlNode Item (int index)
		{
			XmlNode requestedNode = null;

			// Return null if index is out of range. by  DOM design.
			if (Count <= index)
				return null;

			// Instead of checking for && index < Count which has to walk
			// the whole list to get a count, we'll just keep a count since
			// we have to walk the list anyways to get to index.
			if ((index >= 0) && (parent.LastLinkedChild != null)) {
				XmlLinkedNode currentChild = parent.LastLinkedChild.NextLinkedSibling;
				int count = 0;

				while ((count < index) && !Object.ReferenceEquals(currentChild, parent.LastLinkedChild)) 
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
