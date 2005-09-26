//
// System.Security.Policy.ApplicationTrustCollection class
//
// Author:
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
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Policy {

	[ComVisible (true)]
	public sealed class ApplicationTrustCollection : ICollection, IEnumerable {

		private ArrayList _list;

		internal ApplicationTrustCollection ()
		{
			_list = new ArrayList ();
		}

		// constants (from beta1 - still useful ?)

//		public const string ApplicationTrustProperty = "ApplicationTrust";
//		public const string InstallReferenceIdentifier = "{3f471841-eef2-47d6-89c0-d028f03a4ad5}";

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return false; }	// always false
		}

		public object SyncRoot {
			get { return this; }	// self
		}

		public ApplicationTrust this [int index] {
			get { return (ApplicationTrust) _list [index]; }
		}

		public ApplicationTrust this [string appFullName] {
			get {
				for (int i=0; i < _list.Count; i++) {
					ApplicationTrust at = (_list [i] as ApplicationTrust);
					if (at.ApplicationIdentity.FullName == appFullName)
						return at;
				}
				return null;
			}
		}

		// methods

		public int Add (ApplicationTrust trust)
		{
			if (trust == null)
				throw new ArgumentNullException ("trust");
			if (trust.ApplicationIdentity == null) {
				throw new ArgumentException (Locale.GetText (
					"ApplicationTrust.ApplicationIdentity can't be null."), "trust");
			}

			return _list.Add (trust);
		}

		public void AddRange (ApplicationTrust[] trusts)
		{
			if (trusts == null)
				throw new ArgumentNullException ("trusts");

			foreach (ApplicationTrust t in trusts) {
				if (t.ApplicationIdentity == null) {
					throw new ArgumentException (Locale.GetText (
						"ApplicationTrust.ApplicationIdentity can't be null."), "trust");
				}
				_list.Add (t);
			}
		}

		public void AddRange (ApplicationTrustCollection trusts)
		{
			if (trusts == null)
				throw new ArgumentNullException ("trusts");

			foreach (ApplicationTrust t in trusts) {
				if (t.ApplicationIdentity == null) {
					throw new ArgumentException (Locale.GetText (
						"ApplicationTrust.ApplicationIdentity can't be null."), "trust");
				}
				_list.Add (t);
			}
		}

		public void Clear ()
		{
			_list.Clear ();
		}

		public void CopyTo (ApplicationTrust[] array, int index)
		{
			_list.CopyTo (array, index);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			_list.CopyTo (array, index);
		}

		public ApplicationTrustCollection Find (ApplicationIdentity applicationIdentity, ApplicationVersionMatch versionMatch)
		{
			if (applicationIdentity == null)
				throw new ArgumentNullException ("applicationIdentity");

			string fullname = applicationIdentity.FullName;

			switch (versionMatch) {
			case ApplicationVersionMatch.MatchAllVersions:
				int pos = fullname.IndexOf (", Version=");
				if (pos >= 0)
					fullname = fullname.Substring (0, pos);
				break;
			case ApplicationVersionMatch.MatchExactVersion:
				break;
			default:
				throw new ArgumentException ("versionMatch");
			}

			ApplicationTrustCollection coll = new ApplicationTrustCollection ();
			foreach (ApplicationTrust t in _list) {
				if (t.ApplicationIdentity.FullName.StartsWith (fullname)) {
					coll.Add (t);
				}
			}

			return coll;
		}

		public ApplicationTrustEnumerator GetEnumerator ()
		{
			return new ApplicationTrustEnumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator) new ApplicationTrustEnumerator (this);
		}

		public void Remove (ApplicationTrust trust)
		{
			if (trust == null)
				throw new ArgumentNullException ("trust");
			if (trust.ApplicationIdentity == null) {
				throw new ArgumentException (Locale.GetText (
					"ApplicationTrust.ApplicationIdentity can't be null."), "trust");
			}

			RemoveAllInstances (trust);
		}

		public void Remove (ApplicationIdentity applicationIdentity, ApplicationVersionMatch versionMatch)
		{
			ApplicationTrustCollection coll = Find (applicationIdentity, versionMatch);
			foreach (ApplicationTrust t in coll) {
				RemoveAllInstances (t);
			}
		}

		public void RemoveRange (ApplicationTrust[] trusts)
		{
			if (trusts == null)
				throw new ArgumentNullException ("trusts");

			foreach (ApplicationTrust t in trusts) {
				RemoveAllInstances (t);
			}
		}

		public void RemoveRange (ApplicationTrustCollection trusts)
		{
			if (trusts == null)
				throw new ArgumentNullException ("trusts");

			foreach (ApplicationTrust t in trusts) {
				RemoveAllInstances (t);
			}
		}

		// helpers

		internal void RemoveAllInstances (ApplicationTrust trust)
		{
			for (int i=_list.Count - 1; i >= 0; i--) {
				if (trust.Equals (_list [i]))
					_list.RemoveAt (i);
			}
		}
	}
}

#endif
