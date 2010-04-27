//
// System.Web.Security.MembershipUserCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Runtime.CompilerServices;
using System.Web.UI;

namespace System.Web.Security
{
#if NET_4_0
	[TypeForwardedFrom ("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
	[Serializable]
	public sealed class MembershipUserCollection : ICollection
	{
		public MembershipUserCollection ()
		{
		}
		
		public void Add (MembershipUser user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			CheckNotReadOnly ();
			store.Add (user.UserName, user);
		}
		
		public void Clear ()
		{
			CheckNotReadOnly ();
			store.Clear ();
		}
		
		void ICollection.CopyTo (Array array, int index)
		{
			store.Values.CopyTo (array, index);
		}
		
		public void CopyTo (MembershipUser[] array, int index)
		{
			store.Values.CopyTo (array, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return ((IEnumerable) store).GetEnumerator ();
		}
		
		public void Remove (string name)
		{
			CheckNotReadOnly ();
			store.Remove (name);
		}
		
		public void SetReadOnly ()
		{
			readOnly = true;
		}
		
		public int Count {
			get { return store.Count; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public MembershipUser this [string name] {
			get { return (MembershipUser) store [name]; }
		}
		
		public object SyncRoot {
			get { return this; }
		}
		
		void CheckNotReadOnly ()
		{
			if (readOnly)
				throw new NotSupportedException ();
		}
		
		KeyedList store = new KeyedList ();
		bool readOnly = false;
	}
}


