//
// WindowsIdentityTest.cs - NUnit Test Cases for WindowsIdentity
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Runtime.Serialization;
using System.Security.Principal;

namespace MonoTests.System.Security.Principal {

	[TestFixture]
	public class WindowsIdentityTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorIntPtrZero () 
		{
			WindowsIdentity id = new WindowsIdentity (IntPtr.Zero);
		}

		[Test]
		//[ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorW2KS1_Null () 
		{
			WindowsIdentity id = new WindowsIdentity (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorW2KS1 () 
		{
			WindowsIdentity id = new WindowsIdentity (@"FARSCAPE\spouliot");
		}

		[Test]
		//[ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorW2KS2_NullType () 
		{
			WindowsIdentity id = new WindowsIdentity (null, "NTLM");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorW2KS2_UserNull() 
		{
			WindowsIdentity id = new WindowsIdentity (@"FARSCAPE\spouliot", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorW2KS2 () 
		{
			WindowsIdentity id = new WindowsIdentity (@"FARSCAPE\spouliot", "NTLM");
		}

		[Test]
		public void Anonymous () 
		{
			WindowsIdentity id = WindowsIdentity.GetAnonymous ();
			AssertEquals ("AuthenticationType", String.Empty, id.AuthenticationType);
			Assert ("IsAnonymous", id.IsAnonymous);
			Assert ("IsAuthenticated", !id.IsAuthenticated);
			Assert ("IsGuest", !id.IsGuest);
			Assert ("IsSystem", !id.IsSystem);
			AssertEquals ("Token", IntPtr.Zero, id.Token);
			AssertEquals ("Name", String.Empty, id.Name);
		}

		[Test]
		[Ignore ("not currently supported on mono")]
		public void Current () 
		{
			WindowsIdentity id = WindowsIdentity.GetCurrent ();
			AssertEquals ("AuthenticationType", "NTLM", id.AuthenticationType);
			Assert ("IsAnonymous", !id.IsAnonymous);
			Assert ("IsAuthenticated", id.IsAuthenticated);
			Assert ("IsGuest", !id.IsGuest);
			Assert ("IsSystem", !id.IsSystem);
			Assert ("Token", (IntPtr.Zero != id.Token));
			// e.g. FARSCAPE\spouliot
			Assert ("Name", (id.Name.IndexOf (@"\") > 0));
		}

		[Test]
		public void Interface () 
		{
			WindowsIdentity id = WindowsIdentity.GetAnonymous ();

			IIdentity i = (id as IIdentity);
			AssertNotNull ("IIdentity", i);

			IDeserializationCallback dc = (id as IDeserializationCallback);
			AssertNotNull ("IDeserializationCallback", dc);
#if NET_1_1
			ISerializable s = (id as ISerializable);
			AssertNotNull ("ISerializable", s);
#endif
		}
	}
}
