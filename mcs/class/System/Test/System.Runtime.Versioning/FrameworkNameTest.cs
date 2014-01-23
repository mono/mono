//
// System.Runtime.Versioning.FrameworkNameTest class
//
// Authors
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://novell.com)
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

#if NET_4_0 && !MOBILE

using System;
using System.Runtime.Versioning;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Versioning
{
	[TestFixture]
	public class FrameworkNameTest
	{
		void Throws<TEx> (string message, Action code)
		{
			bool failed = false;
			Exception exception = null;
			try {
				code ();
				failed = true;
			} catch (Exception ex) {
				if (ex.GetType () != typeof (TEx)) {
					failed = true;
					exception = ex;
				}
			}

			if (failed) {
				if (exception != null)
					Assert.Fail ("{0}{1}Expected exception {2}, got {3}",
							message, Environment.NewLine, typeof (TEx), exception.GetType ());
				else
					Assert.Fail ("{0}{1}Expected exception {2}",
						message, Environment.NewLine, typeof (TEx));
			}
		}
		
		[Test]
		public void Constructor_String ()
		{
			FrameworkName fn;

			Throws <ArgumentNullException> ("#A1", () => {
					fn = new FrameworkName (null);
				});

			Throws<ArgumentException> ("#A1-1", () => {
					fn = new FrameworkName (String.Empty);
				});

			Throws <ArgumentException> ("#A1-2", () => {
					fn = new FrameworkName (".NETFramework");
				});

			Throws <ArgumentException> ("#A1-3", () => {
					fn = new FrameworkName (".NETFramework,Ver=test");
				});

			Throws <ArgumentException> ("#A1-4", () => {
					fn = new FrameworkName (".NETFramework,Version=A");
				});

			Throws <ArgumentException> ("#A1-5", () => {
					fn = new FrameworkName ("Version=1.2,.NETFramework");
				});

			Throws <ArgumentException> ("#A1-6", () => {
					fn = new FrameworkName (".NETFramework,Version=vA");
				});

			Throws <ArgumentException> ("#A1-7", () => {
					fn = new FrameworkName (".NETFramework,Version=A.B");
				});

			Throws <ArgumentException> ("#A1-8", () => {
					fn = new FrameworkName (".NETFramework,Version=vA.B");
				});

			Throws <ArgumentException> ("#A1-9", () => {
					fn = new FrameworkName (".NETFramework,Version=VA.B");
				});

			Throws <ArgumentException> ("#A1-10", () => {
					fn = new FrameworkName (".NETFramework,Version=vA.B.C");
				});

			Throws <ArgumentException> ("#A1-11", () => {
					fn = new FrameworkName (".NETFramework,Version=vA.B.C.D");
				});

			Throws <ArgumentException> ("#A1-12", () => {
					fn = new FrameworkName (".NETFramework,Version=2");
				});

			Throws <ArgumentException> ("#A1-13", () => {
					fn = new FrameworkName (".NETFramework,Version=v2");
				});

			Throws <ArgumentException> ("#A1-14", () => {
					fn = new FrameworkName (".NETFramework,Version=v2.0.1.A");
				});

			Throws <ArgumentException> ("#A1-15", () => {
					fn = new FrameworkName (".NETFramework,Version=v2.0.1.0,Some=value");
				});

			Throws <ArgumentException> ("#A1-16", () => {
					fn = new FrameworkName (".NETFramework,Version=v2.0.1.0,Profile=profile name, Extra=value");
				});

			Throws <ArgumentException> ("#A1-17", () => {
					fn = new FrameworkName (".NETFramework,Profile=profile name");
				});

			Throws <ArgumentException> ("#A1-18", () => {
					fn = new FrameworkName ("Profile=profile name,.NETFramework,Version=v2.0.1.0");
				});

			Throws <ArgumentException> ("#A1-19", () => {
					var n = new FrameworkName (".NETFramework, ,Version=v2.0.1.0");
				});

			Throws <ArgumentException> ("#A1-20", () => {
					fn = new FrameworkName (".NETFramework,Version=v2..0.1.0");
				});

			Throws <ArgumentException> ("#A1-21", () => {
					fn = new FrameworkName (".NETFramework,Version=v.0.1.0");
				});

			Throws <ArgumentException> ("#A1-22", () => {
					fn = new FrameworkName (".NETFramework,Version=v1.-2.1.0");
				});

			Throws <ArgumentException> ("#A1-23", () => {
					fn = new FrameworkName (".NETFramework,Version=v0.0.0.0.0");
				});

			Throws <ArgumentException> ("#A1-24", () => {
					fn = new FrameworkName (".NETFramework,Version=vA.0.0.0,Version=v1.2.3.4");
				});

			Throws <ArgumentException> ("#A1-25", () => {
					fn = new FrameworkName (".NETFramework,Version=v0.0.0.0,Version=vA.2.3.4");
				});

			Throws <ArgumentException> ("#A1-26", () => {
					fn = new FrameworkName ("Version=1.2,profile=test profile");
				});

			Throws<ArgumentException> ("#A1-27", () => {
					fn = new FrameworkName (".NETFramework,Version=");
				});

			fn = new FrameworkName (".NETFramework=test,Version=3.5");
			Assert.AreEqual (".NETFramework=test", fn.Identifier, "#A2-1");
			Assert.IsTrue (fn.Version == new Version (3, 5), "#A2-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A2-3");
			Assert.AreEqual (".NETFramework=test,Version=v3.5", fn.FullName, "#A3-4");
			
			fn = new FrameworkName (".NETFramework,Version=2.0");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A3-1");
			Assert.IsTrue (fn.Version == new Version (2, 0), "#A3-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A3-3");
			Assert.AreEqual (".NETFramework,Version=v2.0", fn.FullName, "#A3-4");

			fn = new FrameworkName (".NETFramework,Version=v2.0");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A4-1");
			Assert.IsTrue (fn.Version == new Version (2, 0), "#A4-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A4-3");
			Assert.AreEqual (".NETFramework,Version=v2.0", fn.FullName, "#A4-4");

			fn = new FrameworkName (".NETFramework,Version=v0.1");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A5-1");
			Assert.IsTrue (fn.Version == new Version (0, 1), "#A5-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A5-3");
			Assert.AreEqual (".NETFramework,Version=v0.1", fn.FullName, "#A5-4");

			fn = new FrameworkName (".NETFramework,Version=v10.1");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A6-1");
			Assert.IsTrue (fn.Version == new Version (10, 1), "#A6-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A6-3");
			Assert.AreEqual (".NETFramework,Version=v10.1", fn.FullName, "#A6-4");

			fn = new FrameworkName (".NETFramework,Version=V2.0");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A7-1");
			Assert.IsTrue (fn.Version == new Version (2, 0), "#A7-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A7-3");
			Assert.AreEqual (".NETFramework,Version=v2.0", fn.FullName, "#A7-4");

			fn = new FrameworkName (".NETFramework,Version=v2.0.1");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A8-1");
			Assert.IsTrue (fn.Version == new Version (2, 0, 1), "#A8-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A8-3");
			Assert.AreEqual (".NETFramework,Version=v2.0.1", fn.FullName, "#A8-4");

			fn = new FrameworkName (".NETFramework,Version=v2.0.1.0");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A9-1");
			Assert.IsTrue (fn.Version == new Version (2, 0, 1, 0), "#A9-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A9-3");
			Assert.AreEqual (".NETFramework,Version=v2.0.1.0", fn.FullName, "#A9-4");

			fn = new FrameworkName (".NETFramework,Version=v2.0.1.0,Profile=profile name");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A10-1");
			Assert.IsTrue (fn.Version == new Version (2, 0, 1, 0), "#A10-2");
			Assert.AreEqual ("profile name", fn.Profile, "#A10-3");
			Assert.AreEqual (".NETFramework,Version=v2.0.1.0,Profile=profile name", fn.FullName, "#A10-4");

			fn = new FrameworkName (".NETFramework,Version=v2. 0.1.0");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A11-1");
			Assert.IsTrue (fn.Version == new Version (2, 0, 1, 0), "#A11-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A11-3");
			Assert.AreEqual (".NETFramework,Version=v2.0.1.0", fn.FullName, "#A11-4");

			fn = new FrameworkName (".NETFramework,Version=v0.0.0.0");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A12-1");
			Assert.IsTrue (fn.Version == new Version (0, 0, 0, 0), "#A12-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A12-3");
			Assert.AreEqual (".NETFramework,Version=v0.0.0.0", fn.FullName, "#A12-4");

			fn = new FrameworkName (".NETFramework,Version=v0.0.0.0,Version=v1.2.3.4");
			Assert.AreEqual (".NETFramework", fn.Identifier, "#A13-1");
			Assert.IsTrue (fn.Version == new Version (1, 2, 3, 4), "#A13-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A13-3");
			Assert.AreEqual (".NETFramework,Version=v1.2.3.4", fn.FullName, "#A13-4");

			fn = new FrameworkName ("identifier,verSION=1.2,PRofiLE=test profile");
			Assert.AreEqual ("identifier", fn.Identifier, "#A14-1");
			Assert.IsTrue (fn.Version == new Version (1, 2), "#A14-2");
			Assert.AreEqual ("test profile", fn.Profile, "#A14-3");
			Assert.AreEqual ("identifier,Version=v1.2,Profile=test profile", fn.FullName, "#A14-4");

			fn = new FrameworkName ("identifier,Version=2.0,Profile=");
			Assert.AreEqual ("identifier", fn.Identifier, "#A15-1");
			Assert.IsTrue (fn.Version == new Version (2, 0), "#A15-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A15-3");
			Assert.AreEqual ("identifier,Version=v2.0", fn.FullName, "#A15-4");
		}

		[Test]
		public void Constructor_String_Version ()
		{
			FrameworkName fn;

			Throws <ArgumentNullException> ("#A1-1", () => {
					fn = new FrameworkName (null, new Version (2, 0));
				});

			Throws <ArgumentNullException> ("#A1-2", () => {
					fn = new FrameworkName ("identifier", null);
				});

			Throws <ArgumentException> ("#A1-3", () => {
					fn = new FrameworkName (String.Empty, new Version (2, 0));
				});

			var v = new Version (1,2,3,4);
			fn = new FrameworkName ("identifier", v);
			Assert.AreEqual ("identifier", fn.Identifier, "#A2-1");
			Assert.IsTrue (fn.Version == v, "#A2-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A2-3");
			Assert.AreEqual ("identifier,Version=v1.2.3.4", fn.FullName, "#A2-4");

			fn = new FrameworkName ("identifier,v2.0", v);
			Assert.AreEqual ("identifier,v2.0", fn.Identifier, "#A3-1");
			Assert.IsTrue (fn.Version == v, "#A3-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A3-3");
			Assert.AreEqual ("identifier,v2.0,Version=v1.2.3.4", fn.FullName, "#A3-4");

			fn = new FrameworkName ("identifier,Version=v2.0", v);
			Assert.AreEqual ("identifier,Version=v2.0", fn.Identifier, "#A4-1");
			Assert.IsTrue (fn.Version == v, "#A4-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A4-3");
			Assert.AreEqual ("identifier,Version=v2.0,Version=v1.2.3.4", fn.FullName, "#A4-4");
		}

		[Test]
		public void Constructor_String_Version_String ()
		{
			FrameworkName fn;
			var v = new Version (1, 2, 3, 4);

			Throws <ArgumentNullException> ("#A1-1", () => {
					fn = new FrameworkName (null, new Version (2, 0), "profile");
				});

			Throws <ArgumentNullException> ("#A1-2", () => {
					fn = new FrameworkName ("identifier", null, "profile");
				});

			Throws <ArgumentException> ("#A1-3", () => {
					fn = new FrameworkName (String.Empty, new Version (2, 0), "profile");
				});

			fn = new FrameworkName ("identifier,Version=v2.0", v, "Profile name");
			Assert.AreEqual ("identifier,Version=v2.0", fn.Identifier, "#A2-1");
			Assert.IsTrue (fn.Version == v, "#A2-2");
			Assert.AreEqual ("Profile name", fn.Profile, "#A2-3");
			Assert.AreEqual ("identifier,Version=v2.0,Version=v1.2.3.4,Profile=Profile name", fn.FullName, "#A2-4");

			fn = new FrameworkName ("identifier,v2.0,profile=test", v, "Profile name");
			Assert.AreEqual ("identifier,v2.0,profile=test", fn.Identifier, "#A3-1");
			Assert.IsTrue (fn.Version == v, "#A3-2");
			Assert.AreEqual ("Profile name", fn.Profile, "#A3-3");
			Assert.AreEqual ("identifier,v2.0,profile=test,Version=v1.2.3.4,Profile=Profile name", fn.FullName, "#A3-4");

			fn = new FrameworkName ("identifier,v2.0,profile=test", v, null);
			Assert.AreEqual ("identifier,v2.0,profile=test", fn.Identifier, "#A4-1");
			Assert.IsTrue (fn.Version == v, "#A4-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A4-3");
			Assert.AreEqual ("identifier,v2.0,profile=test,Version=v1.2.3.4", fn.FullName, "#A4-4");

			fn = new FrameworkName ("identifier,v2.0,profile=test", v, String.Empty);
			Assert.AreEqual ("identifier,v2.0,profile=test", fn.Identifier, "#A5-1");
			Assert.IsTrue (fn.Version == v, "#A5-2");
			Assert.AreEqual (String.Empty, fn.Profile, "#A5-3");
			Assert.AreEqual ("identifier,v2.0,profile=test,Version=v1.2.3.4", fn.FullName, "#A5-4");
		}

		[Test]
		public void EqualityOperator ()
		{
			var fn = new FrameworkName ("identifier,Version=v2.0");

			Assert.IsTrue (fn == new FrameworkName ("identifier,Version=v2.0"), "#A1");

			FrameworkName fn1 = null;
			FrameworkName fn2 = null;

			Assert.IsTrue (fn1 == fn2, "#A2");
			Assert.IsFalse (fn == new FrameworkName ("identifier,Version=v2.1"), "#A3");
			Assert.IsFalse (fn == null, "#A4");
		}

		[Test]
		public void InequalityOperator ()
		{
			var fn = new FrameworkName ("identifier,Version=v2.0");

			Assert.IsFalse (fn != new FrameworkName ("identifier,Version=v2.0"), "#A1");

			FrameworkName fn1 = null;
			FrameworkName fn2 = null;

			Assert.IsFalse (fn1 != fn2, "#A2");
			Assert.IsTrue (fn != new FrameworkName ("identifier,Version=v2.1"), "#A3");
			Assert.IsTrue (fn != null, "#A4");
		}

		[Test]
		public void ToStringTest ()
		{
			var fn = new FrameworkName (".NETFramework,Version=v2.0.1");
			Assert.AreEqual (".NETFramework,Version=v2.0.1", fn.FullName, "#A1-1");
			Assert.AreEqual (fn.FullName, fn.ToString (), "#A1-2");

			fn = new FrameworkName (".NETFramework,Version=v2.0.1.0");
			Assert.AreEqual (".NETFramework,Version=v2.0.1.0", fn.FullName, "#A2-1");
			Assert.AreEqual (fn.FullName, fn.ToString (), "#A2-2");
		}
	}
}
#endif
