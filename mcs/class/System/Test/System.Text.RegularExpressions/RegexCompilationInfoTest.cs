//
// RegexCompilationInfoTest.cs 
//	- Unit tests for System.Text.RegularExpressions.RegexCompilationInfo
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
using System.Text.RegularExpressions;

namespace MonoTests.System.Text.RegularExpressions {

	[TestFixture]
	public class RegexCompilationInfoTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_NullPattern ()
		{
			new RegexCompilationInfo (null, RegexOptions.None, "name", "fullnamespace", false);
		}

		[Test]
		public void Constructor_InvalidRegexOptions ()
		{
			RegexOptions options = (RegexOptions) Int32.MinValue;
			RegexCompilationInfo info = new RegexCompilationInfo ("pattern", options, "name", "fullnamespace", true);
			Assert.AreEqual ("pattern", info.Pattern, "Pattern");
			Assert.AreEqual (options, info.Options, "Options");
			Assert.AreEqual ("name", info.Name, "Name");
			Assert.AreEqual ("fullnamespace", info.Namespace, "Namespace");
			Assert.IsTrue (info.IsPublic, "IsPublic");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_NullName ()
		{
			new RegexCompilationInfo ("pattern", RegexOptions.None, null, "fullnamespace", false);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void Constructor_EmptyName ()
		{
			new RegexCompilationInfo ("pattern", RegexOptions.None, String.Empty, "fullnamespace", false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_NullNamespace ()
		{
			new RegexCompilationInfo ("pattern", RegexOptions.None, "name", null, false);
		}

		[Test]
		public void Constructor ()
		{
			RegexCompilationInfo info =  new RegexCompilationInfo (String.Empty, RegexOptions.None, "name", String.Empty, false);
			Assert.AreEqual (String.Empty, info.Pattern, "Pattern");
			Assert.AreEqual (RegexOptions.None, info.Options, "Options");
			Assert.AreEqual ("name", info.Name, "Name");
			Assert.AreEqual (String.Empty, info.Namespace, "Namespace");
			Assert.IsFalse (info.IsPublic, "IsPublic");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Pattern_Null ()
		{
			RegexCompilationInfo info = new RegexCompilationInfo ("pattern", RegexOptions.None, "name", "fullnamespace", true);
			info.Pattern = null;
		}

		[Test]
		public void Options_Invalid ()
		{
			RegexCompilationInfo info = new RegexCompilationInfo ("pattern", RegexOptions.None, "name", "fullnamespace", true);
			info.Options = (RegexOptions) Int32.MinValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Name_Null ()
		{
			RegexCompilationInfo info = new RegexCompilationInfo ("pattern", RegexOptions.None, "name", "fullnamespace", true);
			info.Name = null;
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void Name_Empty ()
		{
			RegexCompilationInfo info = new RegexCompilationInfo ("pattern", RegexOptions.None, "name", "fullnamespace", true);
			info.Name = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Namespace_Null ()
		{
			RegexCompilationInfo info = new RegexCompilationInfo ("pattern", RegexOptions.None, "name", "fullnamespace", true);
			info.Namespace = null;
		}
	}
}
