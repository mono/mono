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
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class XamlReaderSettingsTest
	{
		[Test]
		public void DefaultValues ()
		{
			var s = new XamlReaderSettings ();
			Assert.IsFalse (s.AllowProtectedMembersOnRoot, "#1");
			Assert.IsNull (s.BaseUri, "#2");
			Assert.IsFalse (s.IgnoreUidsOnPropertyElements, "#3");
			Assert.IsNull (s.LocalAssembly, "#4");
			Assert.IsFalse (s.ProvideLineInfo, "#5");
			Assert.IsFalse (s.ValuesMustBeString, "#6");
		}

		[Test]
		public void CopyConstructorNull ()
		{
			new XamlReaderSettings (null);
		}

		[Test]
		public void CopyConstructor ()
		{
			var s = new XamlReaderSettings ();
			s.AllowProtectedMembersOnRoot = true;
			s.IgnoreUidsOnPropertyElements = true;
			s.ProvideLineInfo = true;
			s.ValuesMustBeString = true;
			s.BaseUri = new Uri ("urn:foo");
			s.LocalAssembly = typeof (object).Assembly;

			s = new XamlReaderSettings (s);

			Assert.IsTrue (s.AllowProtectedMembersOnRoot, "#1");
			Assert.IsTrue (s.BaseUri.Equals (new Uri ("urn:foo")), "#2");
			Assert.IsTrue (s.IgnoreUidsOnPropertyElements, "#3");
			Assert.AreEqual (typeof (int).Assembly, s.LocalAssembly, "#4");
			Assert.IsTrue (s.ProvideLineInfo, "#5");
			Assert.IsTrue (s.ValuesMustBeString, "#6");
		}
	}
}
