//
// SemaphoreTest.cs - Unit tests for System.Threading.Semaphore
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

using System;
using System.Security.AccessControl;
using System.Threading;

namespace MonoTests.System.Threading {

	[TestFixture]
	public class SemaphoreTest {

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_IntInt_NegativeInitialCount ()
		{
			new Semaphore (-1, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_IntInt_ZeroMaximumCount ()
		{
			new Semaphore (0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_IntInt_InitialBiggerThanMaximum ()
		{
			new Semaphore (2, 1);
		}

		[Test]
		public void Constructor_IntInt ()
		{
			new Semaphore (1, 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_IntIntString_NegativeInitialCount ()
		{
			new Semaphore (-1, 1, "mono");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_IntIntString_ZeroMaximumCount ()
		{
			new Semaphore (0, 0, "mono");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_IntIntString_InitialBiggerThanMaximum ()
		{
			new Semaphore (2, 1, "mono");
		}

		[Test]
		public void Constructor_IntIntString_NullName ()
		{
			new Semaphore (0, 1, null);
		}

		[Test]
		public void Constructor_IntIntStringBool_NegativeInitialCount ()
		{
			bool created = true;
			try {
				new Semaphore (-1, 1, "mono", out created);
			}
			catch (ArgumentOutOfRangeException) {
				Assert.IsTrue (created, "Created");
			}
		}

		[Test]
		public void Constructor_IntIntStringBool_ZeroMaximumCount ()
		{
			bool created = true;
			try {
				new Semaphore (0, 0, "mono", out created);
			}
			catch (ArgumentOutOfRangeException) {
				Assert.IsTrue (created, "Created");
			}
		}

		[Test]
		public void Constructor_IntIntStringBool_InitialBiggerThanMaximum ()
		{
			bool created = true;
			try {
				new Semaphore (2, 1, "mono", out created);
			}
			catch (ArgumentException) {
				Assert.IsTrue (created, "Created");
			}
		}

		[Test]
		public void Constructor_IntIntStringBool_NullName ()
		{
			bool created = false;
			new Semaphore (0, 1, null, out created);
			Assert.IsTrue (created, "Created");
		}

		[Test]
		public void Constructor_IntIntStringBoolSecurity_NegativeInitialCount ()
		{
			bool created = true;
			try {
				new Semaphore (-1, 1, "mono", out created, null);
			}
			catch (ArgumentOutOfRangeException) {
				Assert.IsTrue (created, "Created");
			}
		}

		[Test]
		public void Constructor_IntIntStringBoolSecurity_ZeroMaximumCount ()
		{
			bool created = true;
			try {
				new Semaphore (0, 0, "mono", out created, null);
			}
			catch (ArgumentOutOfRangeException) {
				Assert.IsTrue (created, "Created");
			}
		}

		[Test]
		public void Constructor_IntIntStringBoolSecurity_InitialBiggerThanMaximum ()
		{
			bool created = true;
			try {
				new Semaphore (2, 1, "mono", out created, null);
			}
			catch (ArgumentException) {
				Assert.IsTrue (created, "Created");
			}
		}

		[Test]
		public void Constructor_IntIntStringBoolSecurity_NullName ()
		{
			bool created = false;
			new Semaphore (0, 1, null, out created, null);
			Assert.IsTrue (created, "Created");
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void Constructor_IntIntStringBoolSecurity ()
		{
			bool created = false;
			SemaphoreSecurity ss = new SemaphoreSecurity ();
			new Semaphore (0, 1, "secure", out created, ss);
			Assert.IsTrue (created, "Created");
		}

		[Test]
		[Category ("MobileNotWorking")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void OpenExisting_NullName ()
		{
			Semaphore.OpenExisting (null);
		}

		[Test]
		[Category ("MobileNotWorking")]
		[ExpectedException (typeof (ArgumentException))]
		public void OpenExisting_EmptyName ()
		{
			Semaphore.OpenExisting (String.Empty);
		}

		[Test]
		[Category ("MobileNotWorking")]
		[ExpectedException (typeof (ArgumentException))]
		public void OpenExisting_TooLongName ()
		{
			Semaphore.OpenExisting (new String (' ', 261));
		}

		[Test]
		[Category ("MobileNotWorking")]
		[ExpectedException (typeof (WaitHandleCannotBeOpenedException))]
		public void OpenExisting_Unexisting ()
		{
			Semaphore.OpenExisting (new String ('a', 260));
		}

		[Test]
		[Category ("NotWorking")] // not implemented in Mono
		public void OpenExisting_BadRights ()
		{
			Semaphore s = new Semaphore (0, 1, "bad-rights");
			SemaphoreRights rights = (SemaphoreRights) Int32.MinValue;
			Semaphore existing = Semaphore.OpenExisting ("bad-rights", rights);
			// rights bits aren't validated
			Assert.IsNotNull (existing, "OpenExisting");
			Assert.IsFalse (Object.ReferenceEquals (s, existing), "!ref");
		}

		[Test]
		[Category ("NotWorking")] // not implemented in Mono
		public void AccessControl_Unnamed ()
		{
			Semaphore s = new Semaphore (0, 1, null);
			SemaphoreSecurity ss = s.GetAccessControl ();
			Assert.IsNotNull (ss, "GetAccessControl");
			s.SetAccessControl (ss);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotWorking")] // not implemented in Mono
		public void SetAccessControl_Null ()
		{
			Semaphore s = new Semaphore (0, 1, null);
			s.SetAccessControl (null);
		}

		[Test]
		public void Release ()
		{
			Semaphore s = new Semaphore (0, 1, null);
			s.Release ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Release_Zero ()
		{
			Semaphore s = new Semaphore (0, 1, null);
			s.Release (0);
		}
	}
}

#endif
