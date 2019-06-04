//
// ChannelEndpointElementTest.cs
//
// Author:
//	Eyal Alaluf <eyala@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using NUnit.Framework;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Configuration;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class ChannelEndpointElementTest
	{
		[Test]
		public void TestEmptyProps ()
		{
			ChannelEndpointElement empty = new ChannelEndpointElement ();
			Assert.AreEqual ("", empty.Name, "#01");
			Assert.AreEqual (null, empty.Contract, "#02");
			Assert.AreEqual (null, empty.Binding, "#03");
			Assert.AreEqual (null, empty.Address, "#04");
			Assert.AreEqual ("", empty.BindingConfiguration, "#05");
			Assert.AreEqual ("", empty.BehaviorConfiguration, "#06");
			Assert.IsNotNull (empty.Headers, "#07");
			Assert.IsNotNull (empty.Identity, "#08");
		}
	}
}
#endif
