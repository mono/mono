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
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using NUnit.Framework;

[assembly:XmlnsDefinition ("urn:mono-test", "MonoTests.System.Xaml.NamespaceTest")]
[assembly:XmlnsDefinition ("urn:mono-test2", "MonoTests.System.Xaml.NamespaceTest2")]
[assembly:XmlnsCompatibleWith ("urn:foo", "urn:bar")]

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class XamlSchemaContextTest
	{
		[Test]
		public void ConstructorNullAssemblies ()
		{
			// allowed.
			var ctx = new XamlSchemaContext ((Assembly []) null);
			Assert.IsFalse (ctx.FullyQualifyAssemblyNamesInClrNamespaces, "#1");
			Assert.IsFalse (ctx.SupportMarkupExtensionsWithDuplicateArity, "#2");
			Assert.IsNull (ctx.ReferenceAssemblies, "#3");
		}

		[Test]
		public void ConstructorNullSettings ()
		{
			// allowed.
			var ctx = new XamlSchemaContext ((XamlSchemaContextSettings) null);
		}

		[Test]
		public void ConstructorNoAssembly ()
		{
			var ctx = new XamlSchemaContext (new Assembly [0]);
		}

		[Test]
		public void Constructor ()
		{
			var ctx = new XamlSchemaContext (new Assembly [] {typeof (XamlSchemaContext).Assembly });
			Assert.AreEqual (1, ctx.ReferenceAssemblies.Count, "#1");
		}

		[Test]
		public void GetAllXamlNamespaces ()
		{
			var ctx = new XamlSchemaContext (null, null);
			var arr = ctx.GetAllXamlNamespaces ().ToArray ();
			Assert.AreEqual (3, arr.Length, "#1");
			Assert.IsTrue (arr.Contains (XamlLanguage.Xaml2006Namespace), "#1-2");
			Assert.IsTrue (arr.Contains ("urn:mono-test"), "#1-3");
			Assert.IsTrue (arr.Contains ("urn:mono-test2"), "#1-4");

			ctx = new XamlSchemaContext (new Assembly [] {typeof (XamlSchemaContext).Assembly });
			arr = ctx.GetAllXamlNamespaces ().ToArray ();
			Assert.AreEqual (1, arr.Length, "#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, arr [0], "#2-2");

			ctx = new XamlSchemaContext (new Assembly [] {GetType ().Assembly });
			arr = ctx.GetAllXamlNamespaces ().ToArray ();
			Assert.AreEqual (2, arr.Length, "#3");
			Assert.IsTrue (arr.Contains ("urn:mono-test"), "#3-2");
			Assert.IsTrue (arr.Contains ("urn:mono-test2"), "#3-3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetPreferredPrefixNull ()
		{
			var ctx = new XamlSchemaContext (null, null);
			ctx.GetPreferredPrefix (null);
		}

		[Test]
		public void GetPreferredPrefix ()
		{
			var ctx = new XamlSchemaContext (null, null);
			Assert.AreEqual ("x", ctx.GetPreferredPrefix (XamlLanguage.Xaml2006Namespace), "#1");
			Assert.AreEqual ("p", ctx.GetPreferredPrefix ("urn:4mbw93w89mbh"), "#2"); // ... WTF "p" ?
			Assert.AreEqual ("p", ctx.GetPreferredPrefix ("urn:etbeoesmj"), "#3"); // ... WTF "p" ?
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryGetCompatibleXamlNamespaceNull ()
		{
			var ctx = new XamlSchemaContext (null, null);
			string dummy;
			ctx.TryGetCompatibleXamlNamespace (null, out dummy);
		}

		[Test]
		[Category ("NotDotNet")] // TryGetCompatibleXamlNamespace() never worked like documented.
		public void TryGetCompatibleXamlNamespace ()
		{
			var ctx = new XamlSchemaContext (null, null);
			string dummy;
			Assert.IsFalse (ctx.TryGetCompatibleXamlNamespace (String.Empty, out dummy), "#1");
			Assert.IsNull (dummy, "#1-2"); // this shows the fact that the out result value for false case is not trustworthy.

			ctx = new XamlSchemaContext (new Assembly [] {GetType ().Assembly});
			Assert.IsFalse (ctx.TryGetCompatibleXamlNamespace (String.Empty, out dummy), "#2");
			Assert.IsFalse (ctx.TryGetCompatibleXamlNamespace ("urn:bar", out dummy), "#3");
			// why does .NET return false here?
			Assert.IsTrue (ctx.TryGetCompatibleXamlNamespace ("urn:foo", out dummy), "#4");
			Assert.AreEqual ("urn:bar", dummy, "#5");
		}

/*
			var settings = new XamlSchemaContextSettings () { FullyQualifyAssemblyNamesInClrNamespaces = true };
			ctx = new XamlSchemaContext (new Assembly [] {typeof (XamlSchemaContext).Assembly }, settings);

			ctx = new XamlSchemaContext (new Assembly [] {GetType ().Assembly }, settings);
			arr = ctx.GetAllXamlNamespaces ().ToArray ();
			Assert.AreEqual (2, arr.Length, "#5");
			Assert.IsTrue (arr.Contains ("urn:mono-test"), "#5-2");
			Assert.IsTrue (arr.Contains ("urn:mono-test2"), "#5-3");
		}
*/
	}
}
