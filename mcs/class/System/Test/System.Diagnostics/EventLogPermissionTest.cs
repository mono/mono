//
// EventLogPermissionTest.cs -
//	NUnit Test Cases for EventLogPermission
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

#if !MOBILE

using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Diagnostics {

	[TestFixture]
	public class EventLogPermissionTest {

		static EventLogPermissionAccess[] AllAccess = {
			EventLogPermissionAccess.None,
			EventLogPermissionAccess.Browse,
			EventLogPermissionAccess.Instrument,
			EventLogPermissionAccess.Audit,
#if NET_2_0
			EventLogPermissionAccess.Write,
			EventLogPermissionAccess.Administer,
#endif
		};

		[Test]
		public void PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			EventLogPermission elp = new EventLogPermission (ps);
			Assert.AreEqual (0, elp.PermissionEntries.Count, "PermissionEntries");
			Assert.IsFalse (elp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = elp.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");

			EventLogPermission copy = (EventLogPermission)elp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (elp, copy), "ReferenceEquals");
			Assert.AreEqual (elp.PermissionEntries.Count, copy.PermissionEntries.Count, "copy-PermissionEntries");
			Assert.AreEqual (elp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		public void PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			EventLogPermission elp = new EventLogPermission (ps);
			Assert.AreEqual (0, elp.PermissionEntries.Count, "PermissionEntries");
			Assert.IsTrue (elp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = elp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.IsNull (se.Children, "Xml-Children");

			EventLogPermission copy = (EventLogPermission)elp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (elp, copy), "ReferenceEquals");
			Assert.AreEqual (elp.PermissionEntries.Count, copy.PermissionEntries.Count, "copy-PermissionEntries");
			Assert.AreEqual (elp.IsUnrestricted (), copy.IsUnrestricted (), "copy-IsUnrestricted ()");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void PermissionState_Bad ()
		{
			PermissionState ps = (PermissionState)77;
			EventLogPermission elp = new EventLogPermission (ps);
			Assert.IsFalse (elp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_PermissionEntries_Null ()
		{
			EventLogPermission elp = new EventLogPermission (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_MachineName_Null ()
		{
			EventLogPermission elp = new EventLogPermission (EventLogPermissionAccess.None, null);
		}

		[Test]
		public void EventLogPermissionAccesss_Bad ()
		{
			EventLogPermissionAccess elpa = (EventLogPermissionAccess)Int32.MinValue;
			EventLogPermission elp = new EventLogPermission (elpa, "localhost");
			Assert.AreEqual (1, elp.PermissionEntries.Count, "Count");
			Assert.AreEqual ((EventLogPermissionAccess)Int32.MinValue, elp.PermissionEntries [0].PermissionAccess, "PermissionAccess");
		}

		[Test]
		public void PermissionEntries ()
		{
			EventLogPermissionAccess elpa = EventLogPermissionAccess.None;
			EventLogPermission elp = new EventLogPermission (elpa, "localhost");
			EventLogPermissionEntryCollection elpec = elp.PermissionEntries;
			Assert.AreEqual (1, elpec.Count, "Count==1");

			EventLogPermissionEntry elpe = new EventLogPermissionEntry (EventLogPermissionAccess.Browse, "*");
			elp.PermissionEntries.Add (elpe);
			Assert.AreEqual (2, elpec.Count, "Count==2");

			// remove (same instance)
			elp.PermissionEntries.Remove (elpe);
			Assert.AreEqual (1, elpec.Count, "Count==1 (b)");

			// remove different instance (doesn't work)
			elpe = new EventLogPermissionEntry (EventLogPermissionAccess.None, "localhost");
			Assert.AreEqual (1, elpec.Count, "Count==1");
		}

		[Test]
		public void Copy ()
		{
			foreach (EventLogPermissionAccess elpa in AllAccess) {
				EventLogPermission elp = new EventLogPermission (PermissionState.None);
				EventLogPermissionEntry elpe = new EventLogPermissionEntry (elpa, elpa.ToString ());
				elp.PermissionEntries.Add (elpe);
				EventLogPermission copy = (EventLogPermission)elp.Copy ();
				Assert.AreEqual (1, copy.PermissionEntries.Count, "Count==1");
				Assert.AreEqual (elpa, elp.PermissionEntries [0].PermissionAccess, elpa.ToString ());
			}
		}

		[Test]
		public void Intersect_Null ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
			// No intersection with null
			Assert.IsNull (elp.Intersect (null), "None N null");
		}

		[Test]
		public void Intersect_None ()
		{
			EventLogPermission elp1 = new EventLogPermission (PermissionState.None);
			EventLogPermission elp2 = new EventLogPermission (PermissionState.None);
			// 1. None N None
			EventLogPermission result = (EventLogPermission)elp1.Intersect (elp2);
			Assert.IsNull (result, "Empty N Empty");
			// 2. None N Entry
			elp2.PermissionEntries.Add (new EventLogPermissionEntry (EventLogPermissionAccess.None, "localhost"));
			result = (EventLogPermission)elp1.Intersect (elp2);
			Assert.IsNull (result, "Empty N Entry");
			// 3. Entry N None
			result = (EventLogPermission)elp2.Intersect (elp1);
			Assert.IsNull (result, "Entry N Empty");
		}

		[Test]
		public void Intersect_Unrestricted ()
		{
			// Intersection with unrestricted == Copy
			// a. source (this) is unrestricted
			EventLogPermission elp1 = new EventLogPermission (PermissionState.Unrestricted);
			EventLogPermission elp2 = new EventLogPermission (PermissionState.None);

			// 1. Unrestricted N None
			EventLogPermission result = (EventLogPermission)elp1.Intersect (elp2);
			Assert.IsFalse (result.IsUnrestricted (), "(Unrestricted N None).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(Unrestricted N None).Count");

			// 2. None N Unrestricted
			result = (EventLogPermission)elp2.Intersect (elp1);
			Assert.IsFalse (result.IsUnrestricted (), "(None N Unrestricted).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(None N Unrestricted).Count");

			// 3. Unrestricted N Unrestricted
			result = (EventLogPermission)elp1.Intersect (elp1);
			Assert.IsTrue (result.IsUnrestricted (), "(Unrestricted N Unrestricted).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(Unrestricted N Unrestricted).Count");

			// 4. Unrestricted N Entry
			elp2.PermissionEntries.Add (new EventLogPermissionEntry (EventLogPermissionAccess.None, "localhost"));
			result = (EventLogPermission)elp1.Intersect (elp2);
			Assert.IsFalse (result.IsUnrestricted (), "(Unrestricted N Entry).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Unrestricted N Entry).Count");

			// 5. Entry N Unrestricted
			result = (EventLogPermission)elp2.Intersect (elp1);
			Assert.IsFalse (result.IsUnrestricted (), "(Entry N Unrestricted).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Entry N Unrestricted).Count");

			// 6. Unrestricted N Unrestricted
			elp1.PermissionEntries.Add (new EventLogPermissionEntry (EventLogPermissionAccess.None, "localhost"));
			result = (EventLogPermission)elp1.Intersect (elp1);
			Assert.IsTrue (result.IsUnrestricted (), "(Unrestricted N Unrestricted).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Unrestricted N Unrestricted).Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_BadPermission ()
		{
			EventLogPermission elp1 = new EventLogPermission (PermissionState.Unrestricted);
			elp1.Intersect (new SecurityPermission (SecurityPermissionFlag.Assertion));
		}

		[Test]
		public void IsSubset_Null ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
#if NET_2_0
			Assert.IsTrue (elp.IsSubsetOf (null), "null");
#else
			Assert.IsFalse (elp.IsSubsetOf (null), "null");
#endif
		}

		[Test]
		public void IsSubset_None ()
		{
			// IsSubset with none
			// a. source (this) is none -> target is never a subset
			// b. destination (target) is none -> target is always a subset
			EventLogPermission elp1 = new EventLogPermission (PermissionState.None);
			foreach (EventLogPermissionAccess elpa in AllAccess) {
				EventLogPermission elp2 = new EventLogPermission (PermissionState.None);
				elp2.PermissionEntries.Add (new EventLogPermissionEntry (elpa, elpa.ToString ()));
				Assert.IsTrue (elp1.IsSubsetOf (elp2), "target " + elpa.ToString ());
				Assert.IsFalse (elp2.IsSubsetOf (elp1), "source " + elpa.ToString ());
			}
		}

		[Test]
		public void IsSubset_Self ()
		{
			foreach (EventLogPermissionAccess elpa in AllAccess) {
				EventLogPermission elp = new EventLogPermission (PermissionState.None);
				elp.PermissionEntries.Add (new EventLogPermissionEntry (elpa, elpa.ToString ()));
				Assert.IsTrue (elp.IsSubsetOf (elp), elpa.ToString ());
			}
		}

		[Test]
		public void IsSubset_Unrestricted ()
		{
			// IsSubset with unrestricted
			// a. source (this) is unrestricted -> target is never a subset
			// b. destination (target) is unrestricted -> source is always a subset
			EventLogPermission elp1 = new EventLogPermission (PermissionState.Unrestricted);
			foreach (EventLogPermissionAccess elpa in AllAccess) {
				EventLogPermission elp2 = new EventLogPermission (PermissionState.None);
				elp2.PermissionEntries.Add (new EventLogPermissionEntry (elpa, elpa.ToString ()));
				Assert.IsFalse (elp1.IsSubsetOf (elp2), "target " + elpa.ToString ());
				Assert.IsTrue (elp2.IsSubsetOf (elp1), "source " + elpa.ToString ());
			}
			Assert.IsTrue (elp1.IsSubsetOf (elp1), "Unrestricted.IsSubsetOf(Unrestricted)");
		}

		[Test]
// "special" behavior inherited from ResourceBasePermission
//		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOf_BadPermission ()
		{
			EventLogPermission elp1 = new EventLogPermission (PermissionState.Unrestricted);
			Assert.IsFalse (elp1.IsSubsetOf (new SecurityPermission (SecurityPermissionFlag.Assertion)));
		}

		[Test]
		public void Union_Null ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
			elp.PermissionEntries.Add (new EventLogPermissionEntry (EventLogPermissionAccess.None, "localhost"));
			// Union with null is a simple copy
			EventLogPermission union = (EventLogPermission)elp.Union (null);
			Assert.IsNotNull (elp.PermissionEntries.Count, "Count");
		}

		[Test]
		public void Union_None ()
		{
			EventLogPermission elp1 = new EventLogPermission (PermissionState.None);
			foreach (EventLogPermissionAccess elpa in AllAccess) {
				EventLogPermission elp2 = new EventLogPermission (PermissionState.None);
				elp2.PermissionEntries.Add (new EventLogPermissionEntry (elpa, elpa.ToString ()));
				EventLogPermission union = (EventLogPermission)elp1.Union (elp2);
				Assert.IsFalse (union.IsUnrestricted (), "target.IsUnrestricted " + elpa.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "target.Count " + elpa.ToString ());

				union = (EventLogPermission)elp2.Union (elp1);
				Assert.IsFalse (union.IsUnrestricted (), "source.IsUnrestricted " + elpa.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "source.Count " + elpa.ToString ());
			}
		}

		[Test]
		public void Union_Self ()
		{
			foreach (EventLogPermissionAccess elpa in AllAccess)
			{
				EventLogPermission elp = new EventLogPermission (PermissionState.None);
				elp.PermissionEntries.Add (new EventLogPermissionEntry (elpa, elpa.ToString ()));
				EventLogPermission union = (EventLogPermission)elp.Union (elp);
				Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted " + elpa.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "Count " + elpa.ToString ());
			}
		}

		[Test]
		public void Union_Unrestricted ()
		{
			// Union with unrestricted is unrestricted
			EventLogPermission elp1 = new EventLogPermission (PermissionState.Unrestricted);
			foreach (EventLogPermissionAccess elpa in AllAccess) {
				EventLogPermission elp2 = new EventLogPermission (PermissionState.None);
				elp2.PermissionEntries.Add (new EventLogPermissionEntry (elpa, elpa.ToString ()));
				EventLogPermission union = (EventLogPermission)elp1.Union (elp2);
				Assert.IsTrue (union.IsUnrestricted (), "target.IsUnrestricted " + elpa.ToString ());
				Assert.AreEqual (0, union.PermissionEntries.Count, "target.Count " + elpa.ToString ());

				union = (EventLogPermission)elp2.Union (elp1);
				Assert.IsTrue (union.IsUnrestricted (), "source.IsUnrestricted " + elpa.ToString ());
				Assert.AreEqual (0, union.PermissionEntries.Count, "source.Count " + elpa.ToString ());
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_BadPermission ()
		{
			EventLogPermission elp1 = new EventLogPermission (PermissionState.Unrestricted);
			elp1.Union (new SecurityPermission (SecurityPermissionFlag.Assertion));
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		// Problem inherited from ResourcePermissionBase
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void FromXml_Null ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
			elp.FromXml (null);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void FromXml_WrongTag ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();
			se.Tag = "IMono";
			elp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void FromXml_WrongTagCase ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			elp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			elp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			elp.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			elp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			EventLogPermission elp = new EventLogPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			elp.FromXml (w);
		}
	}
}

#endif
