//
// System.Web.Security.MembershipUserCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Web.UI;

namespace System.Web.Security {
	public class MembershipUserCollection : ICloneable, ICollection {
		public MembershipUserCollection ()
		{
		}
		
		public void Add (MembershipUser user)
		{
			CheckNotReadOnly ();
			store.Add (user.Username, user);
		}
		
		public void Clear ()
		{
			CheckNotReadOnly ();
			store.Clear ();
		}
		
		public object Clone ()
		{
			MembershipUserCollection clone = new MembershipUserCollection ();
			foreach (MembershipUser u in this)
				clone.Add (u);
			return clone;
		}
		
		public void CopyTo (Array array, int index)
		{
			store.CopyTo (array, index);
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
				throw new InvalidOperationException ();
		}
		
		KeyedList store = new KeyedList ();
		bool readOnly = false;
	}
}
#endif

