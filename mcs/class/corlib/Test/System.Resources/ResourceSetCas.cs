//
// ResourceSetCas.cs - CAS unit tests for System.Resources.ResourceSet
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
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.Resources {

	[TestFixture]
	[Category ("CAS")]
	public class ResourceSetCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// we use reflection to call ResourceSet as the Stream constructor is 
		// protected by LinkDemand (which will be converted into full demand, i.e. 
		// a stack walk) when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor_Stream ()
		{
			ConstructorInfo ci = typeof (ResourceSet).GetConstructor (new Type [1] { typeof (Stream) });
			ci.Invoke (new object [1] { Stream.Null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		public void Constructor_String ()
		{
			ConstructorInfo ci = typeof (ResourceSet).GetConstructor (new Type [1] { typeof (string) });
			ci.Invoke (new object [1] { MonoTests.System.Resources.ResourceReaderTest.m_ResourceFile });
			// works - i.e. no LinkDemand
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		public void Constructor_IResourceReader ()
		{
			ResourceReader r = new ResourceReader (MonoTests.System.Resources.ResourceReaderTest.m_ResourceFile);
			ConstructorInfo ci = typeof (ResourceSet).GetConstructor (new Type [1] { typeof (IResourceReader) });
			ci.Invoke (new object [1] { r });
			// works - i.e. no LinkDemand
		}
	}
}
