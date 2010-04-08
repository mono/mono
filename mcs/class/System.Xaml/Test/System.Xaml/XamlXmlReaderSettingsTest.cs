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
	public class XamlXmlReaderSettingsTest
	{
		[Test]
		public void DefaultValues ()
		{
			var s = new XamlXmlReaderSettings ();
			Assert.IsFalse (s.CloseInput, "#1");
			Assert.IsFalse (s.SkipXmlCompatibilityProcessing, "#2");
			Assert.IsNull (s.XmlLang, "#3");
			Assert.IsFalse (s.XmlSpacePreserve, "#4");
		}

		[Test]
		public void CopyConstructorNull ()
		{
			new XamlXmlReaderSettings (null);
		}

		[Test]
		public void CopyConstructor ()
		{
			var s = new XamlXmlReaderSettings ();
			s.CloseInput = true;
			s.SkipXmlCompatibilityProcessing = true;
			s.XmlLang = "ja-JP";
			s.XmlSpacePreserve = true;

			s = new XamlXmlReaderSettings (s);

			// .NET fails to copy this value.
			//Assert.IsTrue (s.CloseInput, "#1");
			Assert.IsTrue (s.SkipXmlCompatibilityProcessing, "#2");
			Assert.AreEqual ("ja-JP", s.XmlLang, "#3");
			Assert.IsTrue (s.XmlSpacePreserve, "#4");
		}
	}
}
