//
// FaultReasonTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class FaultReasonTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullFaultReasonText ()
		{
			new FaultReason ((FaultReasonText) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullEnumerable ()
		{
			new FaultReason ((IEnumerable<FaultReasonText>) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CtorEmptyEnumerable ()
		{
			new FaultReason (new List<FaultReasonText> ());
		}

		[Test]
		public void Simple ()
		{
			FaultReason r = new FaultReason ("testing");
			FaultReasonText t = r.GetMatchingTranslation ();
			Assert.IsNotNull (t);
			Assert.AreEqual ("testing", t.Text, "#1");
			Assert.AreEqual (CultureInfo.CurrentCulture.Name,
				t.XmlLang, "#2");
		}

		[Test]
		public void MultipleLanguages ()
		{
			string current = CultureInfo.CurrentCulture.Name;

			FaultReason r = new FaultReason (new FaultReasonText [] {
				new FaultReasonText ("hello"),
				new FaultReasonText ("hola", "es-ES"),
				new FaultReasonText ("bonjour", "fr")});

			// CurrentCulture
			FaultReasonText t = r.GetMatchingTranslation (
				CultureInfo.CurrentCulture);
			Assert.IsNotNull (t);
			Assert.AreEqual ("hello", t.Text, "#1");
			Assert.AreEqual (current, t.XmlLang, "#2");

			// non-neutral name, get by non-neutral culture
			t = r.GetMatchingTranslation (
				new CultureInfo ("es-ES"));
			Assert.IsNotNull (t);
			Assert.AreEqual ("hola", t.Text, "#3");
			Assert.AreEqual ("es-ES", t.XmlLang, "#4");

			// .ctor(non-neutral name), get by neutral culture
			t = r.GetMatchingTranslation (new CultureInfo ("es"));
			Assert.IsNotNull (t);
			Assert.AreEqual ("hello", t.Text, "#5");
			Assert.AreEqual (current, t.XmlLang, "#6");

			// .ctor(neutral name), get by non-neutral culture
			t = r.GetMatchingTranslation (
				new CultureInfo ("fr-FR"));
			Assert.IsNotNull (t);
			Assert.AreEqual ("bonjour", t.Text, "#7");
			Assert.AreEqual ("fr", t.XmlLang, "#8");
		}

		[Test]
		public void NoCurrentCulture ()
		{
			string current = CultureInfo.CurrentCulture.Name;

			FaultReason r = new FaultReason (new FaultReasonText [] {
				new FaultReasonText ("hola", "es-ES"),
				new FaultReasonText ("bonjour", "fr")});

			// CurrentCulture
			FaultReasonText t = r.GetMatchingTranslation (
				CultureInfo.CurrentCulture);
			Assert.IsNotNull (t);
			// seems like the first item is used.
			Assert.AreEqual ("hola", t.Text, "#1");
			Assert.AreEqual ("es-ES", t.XmlLang, "#2");

			// InvariantCulture
			t = r.GetMatchingTranslation (
				CultureInfo.InvariantCulture);
			Assert.IsNotNull (t);
			// seems like the first item is used.
			Assert.AreEqual ("hola", t.Text, "#3");
			Assert.AreEqual ("es-ES", t.XmlLang, "#4");
		}
	}
}
