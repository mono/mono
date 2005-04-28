//
// System.Security.PermissionSetCollection class
//
// Authors
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Security {

	[Serializable]
	[ComVisible (true)]
	[Obsolete ("seems the *Choice actions won't survive")]
	public sealed class PermissionSetCollection : ICollection, IEnumerable {

		private static string tagName = "PermissionSetCollection";
		private IList _list;

		public PermissionSetCollection ()
		{
			_list = (IList) new ArrayList ();
		}

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public IList PermissionSets {
			get { return _list; }
		}

		public object SyncRoot {
			// this is the "real thing"
			get { throw new NotSupportedException (); }
		}

		// methods

		public void Add (PermissionSet permSet)
		{
			if (permSet == null)
				throw new ArgumentNullException ("permSet");
			_list.Add (permSet);
		}

		public PermissionSetCollection Copy ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			foreach (PermissionSet ps in _list) {
				psc._list.Add (ps.Copy ());
			}
			return psc;
		}

		public void CopyTo (PermissionSet[] array, int index)
		{
			// this is the "real thing"
			throw new NotSupportedException ();
		}

		void ICollection.CopyTo (Array array, int index)
		{
			// this is the "real thing"
			throw new NotSupportedException ();
		}

		public void Demand ()
		{
			// check all collection in a single stack walk
			PermissionSet superset = new PermissionSet (PermissionState.None);
			foreach (PermissionSet ps in _list) {
				foreach (IPermission p in ps) {
					superset.AddPermission (p);
				}
			}
			superset.Demand ();
		}

		public void FromXml (SecurityElement el) 
		{
			if (el == null)
				throw new ArgumentNullException ("el");
			if (el.Tag != tagName) {
				string msg = String.Format ("Invalid tag {0} expected {1}", el.Tag, tagName);
				throw new ArgumentException (msg, "el");
			}
			_list.Clear ();
			if (el.Children != null) {
				foreach (SecurityElement child in el.Children) {
					PermissionSet ps = new PermissionSet (PermissionState.None);
					ps.FromXml (child);
					_list.Add (ps);
				}
			}
		}

		public IEnumerator GetEnumerator ()
		{
			return _list.GetEnumerator ();
		}

		public PermissionSet GetSet (int index) 
		{
			if ((index < 0) || (index >= _list.Count))
				throw new ArgumentOutOfRangeException ("index");

			return (PermissionSet) _list [index];
		}

		public void RemoveSet (int index) 
		{
			if ((index < 0) || (index >= _list.Count))
				throw new ArgumentOutOfRangeException ("index");

			_list.RemoveAt (index);
		}

		public override string ToString ()
		{
			return ToXml ().ToString ();
		}

		public SecurityElement ToXml ()
		{
			SecurityElement se = new SecurityElement (tagName);
			foreach (PermissionSet ps in _list) {
				se.AddChild (ps.ToXml ());
			}
			return se;
		}

		// internal stuff

		internal void DemandChoice ()
		{
			SecurityException exception = null;
			bool result = false;
			foreach (PermissionSet pset in _list) {
				try {
					pset.Demand ();
					result = true;
					break;
				}
				catch (SecurityException se) {
					// keep the first failure, we may throw it if we not succeed
					if (exception == null)
						exception = se;
				}
			}

			if (!result) {
				if (exception != null)
					throw exception;
				else
					throw new SecurityException ("DemandChoice failed.");
			}
		}

		// 2.0 metadata format

		internal static PermissionSetCollection CreateFromBinaryFormat (byte[] data)
		{
			if ((data == null) || (data [0] != 0x2E) || (data.Length < 2)) {
				string msg = Locale.GetText ("Invalid data in 2.0 metadata format.");
				throw new SecurityException (msg);
			}

			int pos = 1;
			int numattr = PermissionSet.ReadEncodedInt (data, ref pos);
			PermissionSetCollection psc = new PermissionSetCollection ();
			for (int i = 0; i < numattr; i++) {
				IPermission p = PermissionSet.ProcessAttribute (data, ref pos);
				if (p == null) {
					string msg = Locale.GetText ("Unsupported data found in 2.0 metadata format.");
					throw new SecurityException (msg);
				}

				PermissionSet ps = new PermissionSet (PermissionState.None);
				ps.DeclarativeSecurity = true;
				ps.AddPermission (p); 
				psc.Add (ps);
			}
			return psc;
		}
	}
}

#endif
