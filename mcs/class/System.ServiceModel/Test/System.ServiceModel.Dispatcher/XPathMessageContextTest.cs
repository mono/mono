//
// XPathMessageContextTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class XPathMessageContextTest
	{
		XPathMessageContext ctx = new XPathMessageContext ();

		[Test]
		public void PredefinedNamespaces ()
		{
			Assert.AreEqual (Constants.Soap11, ctx.LookupNamespace ("s11"), "#1");
			Assert.AreEqual (Constants.Soap12, ctx.LookupNamespace ("s12"), "#2");
			// ... only them?

			foreach (char c in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")
				Assert.IsNull (ctx.LookupNamespace (c + ""), "char:" + c);

			Assert.IsNull (ctx.LookupNamespace ("wsa"), "#3");
			Assert.IsNull (ctx.LookupNamespace ("wsu"), "#4");
		}
	}
}
#endif
