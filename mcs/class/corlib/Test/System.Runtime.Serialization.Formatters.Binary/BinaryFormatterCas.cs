//
// BinaryFormatterCas.cs - CAS unit tests for 
//	System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
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

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;

using MonoTests.System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoCasTests.System.Runtime.Serialization.Formatters.Binary {

	[TestFixture]
	[Category ("CAS")]
	public class BinaryFormatterCas {

		private BinaryFormatterTest unit;
		private Stream stream;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// executes at full trust
			unit = new BinaryFormatterTest ();
			stream = unit.GetSerializedStream ();
		}

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
			stream.Position = 0;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ReuseUnitTest ()
		{
			unit.Constructor_Default ();
			unit.Constructor ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Serialization_Deny_Unrestricted ()
		{
			unit.GetSerializedStream ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		public void Serialization_PermitOnly_SerializationFormatter ()
		{
			Assert.IsNotNull (unit.GetSerializedStream ());
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deserialization_Deny_Unrestricted ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			SerializationTest clone = (SerializationTest) bf.Deserialize (stream);
			Assert.AreEqual (Int32.MinValue, clone.Integer, "Integer");
			Assert.IsFalse (clone.Boolean, "Boolean");
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		public void Deserialization_PermitOnly_SerializationFormatter ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			SerializationTest clone = (SerializationTest) bf.Deserialize (stream);
			Assert.AreEqual (Int32.MinValue, clone.Integer, "Integer");
			Assert.IsFalse (clone.Boolean, "Boolean");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		// no SecurityException here because a LinkDemand is used
		public void UnsafeDeserialization_Deny_Unrestricted ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			SerializationTest clone = (SerializationTest) bf.UnsafeDeserialize (stream, null);
			Assert.AreEqual (Int32.MinValue, clone.Integer, "Integer");
			Assert.IsFalse (clone.Boolean, "Boolean");
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		public void UnsafeDeserialization_PermitOnly_SerializationFormatter ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			SerializationTest clone = (SerializationTest) bf.UnsafeDeserialize (stream, null);
			Assert.AreEqual (Int32.MinValue, clone.Integer, "Integer");
			Assert.IsFalse (clone.Boolean, "Boolean");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (BinaryFormatter).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor()");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
