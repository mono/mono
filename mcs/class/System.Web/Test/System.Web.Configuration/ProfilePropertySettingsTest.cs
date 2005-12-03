//
// ProfilePropertySettingsTest.cs 
//	- unit tests for System.Web.Configuration.ProfilePropertySettings
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class ProfilePropertySettingsTest {

		[Test]
		public void Defaults ()
		{
			ProfilePropertySettings p = new ProfilePropertySettings ("Hi");

			Assert.AreEqual ("Hi", p.Name, "A1");
			Assert.IsFalse (p.AllowAnonymous, "A2");
			Assert.AreEqual ("", p.CustomProviderData, "A3");
			Assert.AreEqual ("", p.DefaultValue, "A4");
			Assert.AreEqual ("", p.Provider, "A5");
			Assert.IsFalse (p.ReadOnly, "A6");
			Assert.AreEqual (SerializationMode.ProviderSpecific, p.SerializeAs, "A7");
			Assert.AreEqual ("string", p.Type, "A8");
		}

		[Test]
		public void NameValidatorSuccess ()
		{
			ProfilePropertySettings p = new ProfilePropertySettings ("Hi");

			p.Name = "hi";
			p.Name = "hi_there";
			p.Name = "string";
			p.Name = "Type";
			p.Name = "Property";
		}

		[Test]
		public void NameValidatorFailures ()
		{
			ProfilePropertySettings p = new ProfilePropertySettings ("Hi");
			bool f;

			f = false; try { p.Name = ""; } catch (ConfigurationErrorsException e) { f = true; } Assert.IsTrue (f, "A1");
			//			f = false; try { p.Name = "1Hi"; } catch (ConfigurationErrorsException e) { f = true; } Assert.IsTrue (f, "A2");
			//			f = false; try { p.Name = "Hi$"; } catch (ConfigurationErrorsException e) { f = true; } Assert.IsTrue (f, "A3");
			//			f = false; try { p.Name = "12345"; } catch (ConfigurationErrorsException e) { f = true; } Assert.IsTrue (f, "A3");
		}
	}

}

#endif
