//
// System.Security.Cryptography.Pkcs.CmsRecipientCollection class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005, 2008 Novell, Inc (http://www.novell.com)
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

#if SECURITY_DEP

using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {

	public sealed class CmsRecipientCollection : ICollection, IEnumerable {

		private ArrayList _list;

		// constructors

		public CmsRecipientCollection () 
		{
			_list = new ArrayList ();
		}

		public CmsRecipientCollection (CmsRecipient recipient)
		{
			_list.Add (recipient);
		}

		public CmsRecipientCollection (SubjectIdentifierType recipientIdentifierType, X509Certificate2Collection certificates)
		{
			// no null check, MS throws a NullReferenceException here
			foreach (X509Certificate2 x509 in certificates) {
				CmsRecipient p7r = new CmsRecipient (recipientIdentifierType, x509);
				_list.Add (p7r);
			}
		}

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return _list.IsSynchronized; }
		}

		public CmsRecipient this [int index] {
			get { return (CmsRecipient) _list [index]; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		public int Add (CmsRecipient recipient) 
		{
			return _list.Add (recipient);
		}

		public void CopyTo (Array array, int index) 
		{
			_list.CopyTo (array, index);
		}

		public void CopyTo (CmsRecipient[] array, int index) 
		{
			_list.CopyTo (array, index);
		}

		public CmsRecipientEnumerator GetEnumerator () 
		{
			return new CmsRecipientEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new CmsRecipientEnumerator (_list);
		}

		public void Remove (CmsRecipient recipient) 
		{
			_list.Remove (recipient);
		}
	}
}

#endif
