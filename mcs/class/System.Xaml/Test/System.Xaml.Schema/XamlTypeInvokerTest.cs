//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xaml.Schema
{
	[TestFixture]
	public class XamlTypeInvokerTest
	{
		XamlSchemaContext sctx = new XamlSchemaContext (new XamlSchemaContextSettings ());

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorTypeNull ()
		{
			new XamlTypeInvoker (null);
		}

		[Test]
		public void DefaultValues ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (object), sctx));
			Assert.IsNull (i.SetMarkupExtensionHandler, "#1");
			Assert.IsNull (i.SetTypeConverterHandler, "#2");
		}

		[XamlSetMarkupExtension ("HandleMarkupExtension")]
		public class TestClassMarkupExtension1
		{
		}
		
		// SetMarkupExtensionHandler
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetHandleMarkupExtensionInvalid ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (TestClassMarkupExtension1), sctx));
			Assert.IsNull (i.SetMarkupExtensionHandler, "#1");
		}

		[XamlSetMarkupExtension ("HandleMarkupExtension")]
		public class TestClassMarkupExtension2
		{
			// delegate type mismatch
			void HandleMarkupExtension ()
			{
			}
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetHandleMarkupExtensionInvalid2 ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (TestClassMarkupExtension2), sctx));
			Assert.IsNull (i.SetMarkupExtensionHandler, "#1");
		}

		[XamlSetMarkupExtension ("HandleMarkupExtension")]
		public class TestClassMarkupExtension3
		{
			// must be static
			public void HandleMarkupExtension (object o, XamlSetMarkupExtensionEventArgs a)
			{
			}
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetHandleMarkupExtensionInvalid3 ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (TestClassMarkupExtension3), sctx));
			Assert.IsNull (i.SetMarkupExtensionHandler, "#1");
		}

		[XamlSetMarkupExtension ("HandleMarkupExtension")]
		public class TestClassMarkupExtension4
		{
			// can be private.
			static void HandleMarkupExtension (object o, XamlSetMarkupExtensionEventArgs a)
			{
			}
		}
		
		[Test]
		public void SetHandleMarkupExtension ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (TestClassMarkupExtension4), sctx));
			Assert.IsNotNull (i.SetMarkupExtensionHandler, "#1");
		}

		// SetTypeConverterHandler
		
		[XamlSetTypeConverter ("HandleTypeConverter")]
		public class TestClassTypeConverter1
		{
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetHandleTypeConverterInvalid ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (TestClassTypeConverter1), sctx));
			Assert.IsNull (i.SetTypeConverterHandler, "#1");
		}

		[XamlSetTypeConverter ("HandleTypeConverter")]
		public class TestClassTypeConverter2
		{
			// delegate type mismatch
			void HandleTypeConverter ()
			{
			}
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetHandleTypeConverterInvalid2 ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (TestClassTypeConverter2), sctx));
			Assert.IsNull (i.SetTypeConverterHandler, "#1");
		}

		[XamlSetTypeConverter ("HandleTypeConverter")]
		public class TestClassTypeConverter3
		{
			// must be static
			public void HandleTypeConverter (object o, XamlSetTypeConverterEventArgs a)
			{
			}
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetHandleTypeConverterInvalid3 ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (TestClassTypeConverter3), sctx));
			Assert.IsNull (i.SetTypeConverterHandler, "#1");
		}

		[XamlSetTypeConverter ("HandleTypeConverter")]
		public class TestClassTypeConverter4
		{
			// can be private.
			static void HandleTypeConverter (object o, XamlSetTypeConverterEventArgs a)
			{
			}
		}
		
		[Test]
		public void SetHandleTypeConverter ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (TestClassTypeConverter4), sctx));
			Assert.IsNotNull (i.SetTypeConverterHandler, "#1");
		}

		// AddToCollection

		[Test]
		public void AddToCollectionNoUnderlyingType ()
		{
			var i = new XamlTypeInvoker (new XamlType ("urn:foo", "FooType", null, sctx));
			i.AddToCollection (new List<int> (), 5); // ... passes.
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void AddToCollectionArrayExtension ()
		{
			var i = XamlLanguage.Array.Invoker;
			var ax = new ArrayExtension ();
			i.AddToCollection (ax, 5);
		}
		
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void AddToCollectionArrayInstance ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (int []), sctx));
			var ax = new ArrayExtension ();
			i.AddToCollection (ax, 5);
		}
		
		[Test]
		public void AddToCollectionList_ObjectTypeMismatch ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (List<int>), sctx));
			try {
				i.AddToCollection (new ArrayExtension (), 5);
				Assert.Fail ("not supported operation.");
			} catch (NotSupportedException) {
			} catch (TargetException) {
				// .NET throws this, but the difference should not really matter.
			}
		}
		
		[Test]
		public void AddToCollectionList_ObjectTypeMismatch2 ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (List<int>), sctx));
			i.AddToCollection (new List<object> (), 5); // it is allowed.
		}
		
		[Test]
		public void AddToCollectionList_ObjectTypeMismatch3 ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (List<object>), sctx));
			i.AddToCollection (new List<int> (), 5); // it is allowed too.
		}
		
		[Test]
		public void AddToCollectionList_ObjectTypeMismatch4 ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (List<Uri>), sctx));
			i.AddToCollection (new List<TimeSpan> (), TimeSpan.Zero); // it is allowed too.
		}
		
		[Test]
		public void AddToCollectionList_NonCollectionType ()
		{
			// so, the source collection type is not checked at all.
			var i = new XamlTypeInvoker (new XamlType (typeof (Uri), sctx));
			i.AddToCollection (new List<TimeSpan> (), TimeSpan.Zero); // it is allowed too.
		}
		
		[Test]
		public void AddToCollectionList ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (List<int>), sctx));
			var l = new List<int> ();
			i.AddToCollection (l, 5);
			i.AddToCollection (l, 3);
			i.AddToCollection (l, -12);
			Assert.AreEqual (3, l.Count, "#1");
			Assert.AreEqual (-12, l [2], "#2");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddToCollectionTypeMismatch ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (List<int>), sctx));
			var l = new List<int> ();
			i.AddToCollection (l, "5");
		}

		// CreateInstance

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstanceNoUnderlyingType ()
		{
			var i = new XamlTypeInvoker (new XamlType ("urn:foo", "FooType", null, sctx));
			i.CreateInstance (new object [0]); // unkown type is not supported
		}

		[Test]
		public void CreateInstanceArrayExtension ()
		{
			var i = XamlLanguage.Array.Invoker;
			i.CreateInstance (new object [0]);
		}
		
		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void CreateInstanceArray ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (int []), sctx));
			i.CreateInstance (new object [0]); // no default constructor.
		}
		
		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void CreateInstanceList_ArgumentMismatch ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (List<int>), sctx));
			i.CreateInstance (new object [] {"foo"});
		}
		
		[Test]
		public void CreateInstanceList ()
		{
			var i = new XamlTypeInvoker (new XamlType (typeof (List<int>), sctx));
			i.CreateInstance (new object [0]);
		}
	}
}