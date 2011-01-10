//
// ScriptReferenceBaseTest.cs
//
// Author:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Web.UI;

namespace MonoTests.System.Web.UI
{
	class MyScriptReference : ScriptReferenceBase
	{
		protected internal override string GetUrl (ScriptManager scriptManager, bool zip)
		{
			return null;
		}
	
		protected internal override bool IsFromSystemWebExtensions ()
		{
			return false;
		}

		public static string DoReplaceExtension (string path)
		{
			return ReplaceExtension (path);
		}
	}
	
	[TestFixture]
	public class ScriptReferenceBaseTest
	{
		[Test (Description="No checks are performed by .NET")]
		[ExpectedException (typeof (NullReferenceException))]
		public void ReplaceExtensionNullPath ()
		{
			MyScriptReference.DoReplaceExtension (null);
		}

		[Test (Description="No checks are performed by .NET")]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ReplaceExtensionShortPath ()
		{
			MyScriptReference.DoReplaceExtension (String.Empty);
		}

		[Test]
		public void ReplaceExtension ()
		{
			string ext = MyScriptReference.DoReplaceExtension ("js");
			Assert.AreEqual ("debug.js", ext, "#1");

			ext = MyScriptReference.DoReplaceExtension ("testjs");
			Assert.AreEqual ("testdebug.js", ext, "#2");
			
			ext = MyScriptReference.DoReplaceExtension ("test.js");
			Assert.AreEqual ("test.debug.js", ext, "#3");
		}
	}
}
