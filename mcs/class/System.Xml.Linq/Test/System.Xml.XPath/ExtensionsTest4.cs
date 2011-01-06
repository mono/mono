//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.
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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class ExtensionsTest4
	{
		[Test]
		public void Bug654433 ()
		{
			string test = "<rt guid=\"ee964002-519f-468e-accb-3784ccca366e\" class=\"StTxtPara\" ownerguid=\"b890ba7a-5841-46a7-b0cd-78266c95eb26\">  <Contents>    <Str>      <Run ws=\"en\">CmPossibility.Discussion is an StTxtPara  StTxtPara.Contents is a   String-big   </Run>      <Run ws=\"en\" externalLink=\"AudioVisual\\Untitled5.WMV\" namedStyle=\"Hyperlink\"> link here</Run> <Run ws=\"en\">      more text</Run>    </Str>  </Contents>  <ParseIsCurrent val=\"False\" />  <Segments>  </Segments></rt>";
			var nav = XElement.Parse (test).CreateNavigator ();
			Assert.AreEqual (3, nav.Select ("//Run").Count, "#1");
		}
	}
}
