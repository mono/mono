//
// Tests for Microsoft.Web.GenericScriptComponent
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//

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
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Microsoft.Web;

namespace MonoTests.Microsoft.Web
{
	[TestFixture]
	public class GenericScriptComponentTest
	{
		[Test]
		public void ValueProperty ()
		{
			GenericScriptComponent c = new GenericScriptComponent ("generic");
			StringWriter sw = new StringWriter();
			ScriptTextWriter w = new ScriptTextWriter (sw);

			c.AddValueProperty ("key1", "value1");
			c.AddValueProperty ("key2", "value2");

			((IScriptComponent)c).RenderScript (w);

			Assert.AreEqual ("<generic key1=\"value1\" key2=\"value2\" />", sw.ToString (), "A1");
		}

		[Test]
		public void ValueProperty_duplicate ()
		{
			GenericScriptComponent c = new GenericScriptComponent ("generic");
			StringWriter sw = new StringWriter();
			ScriptTextWriter w = new ScriptTextWriter (sw);

			c.AddValueProperty ("key1", "value1");
			c.AddValueProperty ("key1", "value2");

			((IScriptComponent)c).RenderScript (w);

			Assert.AreEqual ("<generic key1=\"value2\" />", sw.ToString (), "A1");
		}

		[Test]
		public void CollectionItem ()
		{
			GenericScriptComponent c = new GenericScriptComponent ("generic");
			StringWriter sw = new StringWriter();
			ScriptTextWriter w = new ScriptTextWriter (sw);
			GenericScriptComponent foo = new GenericScriptComponent ("foo");

			c.AddCollectionItem ("behaviors", foo);

			((IScriptComponent)c).RenderScript (w);

			Assert.AreEqual ("<generic>\n  <behaviors>\n    <foo />\n  </behaviors>\n</generic>", sw.ToString ().Replace ("\r\n", "\n"), "A1");
		}

		[Test]
		public void CollectionItem_duplicate ()
		{
			GenericScriptComponent c = new GenericScriptComponent ("generic");
			StringWriter sw = new StringWriter();
			ScriptTextWriter w = new ScriptTextWriter (sw);
			GenericScriptComponent foo = new GenericScriptComponent ("foo");

			c.AddCollectionItem ("behaviors", foo);
			c.AddCollectionItem ("behaviors", foo);

			c.AddCollectionItem ("behaviors2", foo);

			((IScriptComponent)c).RenderScript (w);

			Assert.AreEqual ("<generic>\n  <behaviors>\n    <foo />\n    <foo />\n  </behaviors>\n  <behaviors2>\n    <foo />\n  </behaviors2>\n</generic>", sw.ToString ().Replace ("\r\n", "\n"), "A1");
		}
	}
}
#endif
