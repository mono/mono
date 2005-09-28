//
// Tests for Microsoft.Web.Services.JavaScriptConverter
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
using System.IO;
using Microsoft.Web;
using Microsoft.Web.Services;

namespace MonoTests.Microsoft.Web.Services
{
	[TestFixture]
	public class JavaScriptConverterTest
	{
		class Poker : JavaScriptConverter
		{
			public Type[] GetSupportedTypes ()
			{
				return base.SupportedTypes;
			}
		}

		[Test]
		public void SupportedTypes ()
		{
			Poker p = new Poker ();

			Type[] ts = p.GetSupportedTypes();

			Assert.IsNull (ts, "A1");
		}

		[Test]
		public void SerializeObject ()
		{
			Poker p = new Poker ();
			string s = p.Serialize ("hi");

			Assert.IsNull (s, "A1");
		}
	}

}

#endif
