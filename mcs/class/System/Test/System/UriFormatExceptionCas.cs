//
// UriFormatExceptionCas.cs - CAS unit tests for System.UriFormatException
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

using NUnit.Framework;

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System {

	[TestFixture]
	[Category ("CAS")]
	public class UriFormatExceptionCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		public void DenySerializationFormatter_GetObjectData ()
		{
			StreamingContext sc = new StreamingContext (StreamingContextStates.All);
			UriFormatException ufe = new UriFormatException ();
			ufe.GetObjectData (null, sc);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void DenyUnrestricted ()
		{
			// can we call everything without a SecurityException ?
			UriFormatException ufe = new UriFormatException ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (UriFormatException).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_GetObjectData_Deny_DenySerializationFormatter ()
		{
			StreamingContext sc = new StreamingContext (StreamingContextStates.All);
			UriFormatException ufe = new UriFormatException ();
			MethodInfo mi = ufe.GetType ().GetMethod ("GetObjectData");
			Assert.IsNotNull (mi, "GetObjectData");
			Assert.IsNotNull (mi.Invoke (ufe, new object[2] { null, sc }), "invoke");
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		[ExpectedException (typeof (TargetInvocationException))]
		// note: the inner exception is the ArgumentNullException
		public void LinkDemand_GetObjectData_PermitOnly_DenySerializationFormatter ()
		{
			StreamingContext sc = new StreamingContext (StreamingContextStates.All);
			UriFormatException ufe = new UriFormatException ();
			MethodInfo mi = ufe.GetType ().GetMethod ("GetObjectData");
			Assert.IsNotNull (mi, "GetObjectData");
			Assert.IsNotNull (mi.Invoke (ufe, new object[2] { null, sc }), "invoke");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (TargetInvocationException))]
		// note: the inner exception is the ArgumentNullException
		public void LinkDemand_ISerializableGetObjectData_Deny_DenySerializationFormatter ()
		{
			StreamingContext sc = new StreamingContext (StreamingContextStates.All);
			UriFormatException ufe = new UriFormatException ();
			MethodInfo mi = ufe.GetType ().GetMethod ("System.Runtime.Serialization.ISerializable.GetObjectData",
				BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.IsNotNull (mi, "ISerializable.GetObjectData");
			Assert.IsNotNull (mi.Invoke (ufe, new object[2] { null, sc }), "invoke");
		}
	}
}
