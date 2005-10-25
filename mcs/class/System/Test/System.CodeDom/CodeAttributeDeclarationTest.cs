//
// CodeAttributeDeclarationTest.cs
//	- Unit tests for System.CodeDom.CodeAttributeDeclaration
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
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
using System.CodeDom;

namespace MonoTests.System.CodeDom
{
	[TestFixture]
	public class CodeAttributeDeclarationTest
	{
		[Test]
		public void NullName ()
		{
			CodeAttributeDeclaration cad = new CodeAttributeDeclaration ((string) null);
			Assert.IsNotNull (cad.Name, "#1");
			Assert.AreEqual (string.Empty, cad.Name, "#2");
#if NET_2_0
			Assert.IsNotNull (cad.AttributeType, "#3");
			Assert.AreEqual (typeof(void).FullName, cad.AttributeType.BaseType, "#4");
#endif
			Assert.IsNotNull (cad.Arguments, "#5");
			Assert.AreEqual (0, cad.Arguments.Count, "#6");

			cad.Name = null;
			Assert.IsNotNull (cad.Name, "#7");
			Assert.AreEqual (string.Empty, cad.Name, "#8");
#if NET_2_0
			Assert.IsNotNull (cad.AttributeType, "#9");
			Assert.AreEqual (typeof(void).FullName, cad.AttributeType.BaseType, "#10");
#endif
		}

		[Test]
		public void Constructor0 ()
		{
			CodeAttributeDeclaration cad = new CodeAttributeDeclaration ();
			Assert.IsNotNull (cad.Name, "#1");
			Assert.AreEqual (string.Empty, cad.Name, "#2");
#if NET_2_0
			Assert.IsNull (cad.AttributeType, "#3");
#endif
			Assert.IsNotNull (cad.Arguments, "#4");
			Assert.AreEqual (0, cad.Arguments.Count, "#5");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeAttributeDeclaration cad = new CodeAttributeDeclaration ("mono");
			Assert.AreEqual (0, cad.Arguments.Count, "#1");
			Assert.AreEqual ("mono", cad.Name, "#2");
#if NET_2_0
			Assert.IsNotNull (cad.AttributeType, "#3");
			Assert.AreEqual ("mono", cad.AttributeType.BaseType, "#4");
#endif
		}

		[Test]
		public void Constructor2 ()
		{
			CodeAttributeDeclaration cad = new CodeAttributeDeclaration ("mono", new CodeAttributeArgument ());
			Assert.AreEqual (1, cad.Arguments.Count, "#1");
			Assert.AreEqual ("mono", cad.Name, "#2");
#if NET_2_0
			Assert.IsNotNull (cad.AttributeType, "#3");
			Assert.AreEqual ("mono", cad.AttributeType.BaseType, "#4");
#endif
			cad.Name = null;
			Assert.AreEqual (1, cad.Arguments.Count, "#5");
			Assert.AreEqual (string.Empty, cad.Name, "#6");
#if NET_2_0
			Assert.IsNotNull (cad.AttributeType, "#7");
			Assert.AreEqual (typeof(void).FullName, cad.AttributeType.BaseType, "#8");
#endif
		}

#if NET_2_0
		[Test]
		public void Constructor3 ()
		{
			CodeAttributeDeclaration cad = new CodeAttributeDeclaration (new CodeTypeReference ("mono"));
			Assert.AreEqual (0, cad.Arguments.Count, "#1");
			Assert.AreEqual ("mono", cad.Name, "#2");
			Assert.IsNotNull (cad.AttributeType, "#3");
			Assert.AreEqual ("mono", cad.AttributeType.BaseType, "#4");

			cad.Name = null;
			Assert.AreEqual (0, cad.Arguments.Count, "#5");
			Assert.AreEqual (string.Empty, cad.Name, "#6");
			Assert.AreEqual (typeof(void).FullName, cad.AttributeType.BaseType, "#7");

			cad.Name = "mono";
			Assert.AreEqual (0, cad.Arguments.Count, "#8");
			Assert.AreEqual ("mono", cad.Name, "#9");
			Assert.IsNotNull (cad.AttributeType, "#10");
			Assert.AreEqual ("mono", cad.AttributeType.BaseType, "#11");

			cad = new CodeAttributeDeclaration ((CodeTypeReference) null);
			Assert.AreEqual (0, cad.Arguments.Count, "#12");
			Assert.AreEqual (string.Empty, cad.Name, "#13");
			Assert.IsNull (cad.AttributeType, "#14");
		}

		[Test]
		public void Constructor4 ()
		{
			CodeAttributeDeclaration cad = new CodeAttributeDeclaration (new CodeTypeReference ("mono"),
				new CodeAttributeArgument (), new CodeAttributeArgument ());
			Assert.AreEqual (2, cad.Arguments.Count, "#1");
			Assert.AreEqual ("mono", cad.Name, "#2");
			Assert.IsNotNull (cad.AttributeType, "#3");
			Assert.AreEqual ("mono", cad.AttributeType.BaseType, "#4");

			cad.Name = null;
			Assert.AreEqual (2, cad.Arguments.Count, "#5");
			Assert.AreEqual (string.Empty, cad.Name, "#6");
			Assert.AreEqual (typeof(void).FullName, cad.AttributeType.BaseType, "#7");

			cad.Name = "mono";
			Assert.AreEqual (2, cad.Arguments.Count, "#8");
			Assert.AreEqual ("mono", cad.Name, "#9");
			Assert.IsNotNull (cad.AttributeType, "#10");
			Assert.AreEqual ("mono", cad.AttributeType.BaseType, "#11");

			cad = new CodeAttributeDeclaration ((CodeTypeReference) null,
				new CodeAttributeArgument ());
			Assert.AreEqual (1, cad.Arguments.Count, "#12");
			Assert.AreEqual (string.Empty, cad.Name, "#13");
			Assert.IsNull (cad.AttributeType, "#14");
		}
#endif
	}
}
