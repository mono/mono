//
// System.Security.Policy.ApplicationTrustCollection class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Policy {

	public sealed class ApplicationTrustCollection : CollectionBase {

		internal ApplicationTrustCollection ()
		{
		}

		// properties

		public ApplicationTrust this [int index] {
			get { return (ApplicationTrust) InnerList [index]; }
			set { InnerList [index] = value; }
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

			return InnerList.Add (trust);
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
				InnerList.Add (t);
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
				InnerList.Add (t);
			}
		}

		public bool Contains (ApplicationTrust trust)
		{
			return (IndexOf (trust) >= 0);
		}

		public void CopyTo (ApplicationTrust[] array, int index)
		{
			InnerList.CopyTo (array, index);
		}

		[MonoTODO ("missing MatchExactVersion")]
		public ApplicationTrustCollection Find (ApplicationIdentity applicationIdentity, ApplicationVersionMatch versionMatch)
		{
			ApplicationTrustCollection coll = new ApplicationTrustCollection ();
			foreach (ApplicationTrust t in InnerList) {
				if (t.ApplicationIdentity.Equals (applicationIdentity)) {
					switch (versionMatch) {
					case ApplicationVersionMatch.MatchAllVersions:
						coll.Add (t);
						break;
					case ApplicationVersionMatch.MatchExactVersion:
						// TODO: version is encoded in a fullname ?
						break;
					}
				}
			}
			return coll;
		}

		public new ApplicationTrustEnumerator GetEnumerator ()
		{
			return new ApplicationTrustEnumerator (this);
		}

		public int IndexOf (ApplicationTrust trust)
		{
			if (trust == null)
				throw new ArgumentNullException ("trust");

			for (int i=0; i < InnerList.Count; i++) {
				if (trust.Equals (InnerList [i]))
					return i;
			}
			return -1;
		}

		public void Insert (int index, ApplicationTrust trust)
		{
			if (trust == null)
				throw new ArgumentNullException ("trust");

			InnerList.Insert (index, trust);
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

		[MonoTODO ("missing MatchExactVersion (relies on Find)")]
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
			for (int i=InnerList.Count - 1; i >= 0; i--) {
				if (trust.Equals (InnerList [i]))
					InnerList.RemoveAt (i);
			}
		}
	}
}

#endif
