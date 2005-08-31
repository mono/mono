//
// XmlSchemaExceptionCas.cs 
//	- CAS unit tests for System.Xml.Schema.XmlSchemaException
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
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Xml.Schema;

namespace MonoCasTests.System.Xml.Schema {

	[TestFixture]
	[Category ("CAS")]
	public class XmlSchemaExceptionCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[ExpectedException (typeof (SecurityException))]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		public void DenySerializationFormatter_GetObjectData ()
		{
			StreamingContext sc = new StreamingContext (StreamingContextStates.All);
			XmlSchemaException xe = new XmlSchemaException (String.Empty, null);
			xe.GetObjectData (null, sc);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		public void PermitOnlySerializationFormatter_GetObjectData ()
		{
			StreamingContext sc = new StreamingContext (StreamingContextStates.All);
			XmlSchemaException xe = new XmlSchemaException (String.Empty, null);
			xe.GetObjectData (null, sc);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void DenyUnrestricted ()
		{
			// can we call everything without a SecurityException ?
			XmlSchemaException xe = new XmlSchemaException (String.Empty, null);
			Assert.AreEqual (0, xe.LineNumber, "LineNumber");
			Assert.AreEqual (0, xe.LinePosition, "LinePosition");
			Assert.IsNotNull (xe.Message, "Message");
			Assert.IsNull (xe.SourceSchemaObject, "SourceSchemaObject");
			Assert.IsNull (xe.SourceUri, "SourceUri");
		}
	}
}
