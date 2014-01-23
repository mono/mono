// 
// SchemaImporterExtensionCollectionTests.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
//

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

using System;
using System.CodeDom;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;
using NUnit.Framework;

namespace MonoTests.System.Xml.Serialization.Advanced
{
	[TestFixture]
	public class SchemaImporterExtensionCollectionTests
	{
		class MyExtension : SchemaImporterExtension
		{
		}

		class MyExtension2 : SchemaImporterExtension
		{
		}

		abstract class MyAbstractExtension : SchemaImporterExtension
		{
		}

		[Test]
		public void Add ()
		{
			SchemaImporterExtensionCollection c =
				new SchemaImporterExtensionCollection ();
			Assert.AreEqual (0, c.Add ("foo", typeof (MyExtension)), "#1");
			Assert.IsTrue (c [0] is MyExtension, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNameNull ()
		{
			SchemaImporterExtensionCollection c =
				new SchemaImporterExtensionCollection ();
			c.Add (null, typeof (MyExtension));
		}

		[Test]
		[Category ("NotDotNet")] // NRE happens there.
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddTypeNull ()
		{
			SchemaImporterExtensionCollection c =
				new SchemaImporterExtensionCollection ();
			c.Add ("foo", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddTypeNonExtension ()
		{
			SchemaImporterExtensionCollection c =
				new SchemaImporterExtensionCollection ();
			c.Add ("foo", typeof (int));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddTypeAbstract ()
		{
			SchemaImporterExtensionCollection c =
				new SchemaImporterExtensionCollection ();
			c.Add ("foo", typeof (SchemaImporterExtension));
		}

		[Test]
		public void AddTypeAbstract2 ()
		{
			try {
				SchemaImporterExtensionCollection c =
					new SchemaImporterExtensionCollection ();
				c.Add ("foo", typeof (MyAbstractExtension));
				Assert.Fail ("Abstract type should not be accepted.");
			} catch (Exception) {
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DuplicateNames ()
		{
			SchemaImporterExtensionCollection c =
				new SchemaImporterExtensionCollection ();
			c.Add ("foo", typeof (MyExtension));
			c.Add ("foo", typeof (MyExtension2));
		}

		[Test]
		public void RemoveNonExistent ()
		{
			SchemaImporterExtensionCollection c =
				new SchemaImporterExtensionCollection ();
			c.Remove ("foo");
		}
	}
}

#endif
