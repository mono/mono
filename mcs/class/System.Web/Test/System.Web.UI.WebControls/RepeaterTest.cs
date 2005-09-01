//
// Tests for System.Web.UI.WebControls.Repeater.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
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

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class RepeaterTest {	
		class Poker : Repeater {
			
			public void TrackState () 
			{
				TrackViewState ();
			}
			
			public object SaveState ()
			{
				return SaveViewState ();
			}
			
			public void LoadState (object o)
			{
				LoadViewState (o);
			}

#if NET_2_0
			public DataSourceSelectArguments GetSelectArguments()
			{
				return SelectArguments;
			}

			public DataSourceSelectArguments DoCreateDataSourceSelectArguments ()
			{
				return base.CreateDataSourceSelectArguments();
			}
#endif
		}

#if NET_2_0
		[Test]
		public void Repeater_DefaultsSelectArguments ()
		{
			Poker p = new Poker ();
			DataSourceSelectArguments args, args2;

			args = p.GetSelectArguments();
			args2 = p.DoCreateDataSourceSelectArguments();

			Assert.AreEqual (args, args2, "call == prop");

			Assert.IsNotNull (args, "property null check");
			Assert.IsTrue    (args != DataSourceSelectArguments.Empty, "property != Empty check");
			Assert.IsTrue    (args.Equals (DataSourceSelectArguments.Empty), "property but they are empty check");

			Assert.IsNotNull (args2, "method null check");
			Assert.IsTrue    (args2 != DataSourceSelectArguments.Empty, "method != Empty check");
			Assert.IsTrue    (args2.Equals (DataSourceSelectArguments.Empty), "method but they are empty check");

			/* check to see whether multiple calls give us different refs */
			args = p.DoCreateDataSourceSelectArguments();
			
			Assert.AreEqual (args, args2, "multiple calls, same ref");
		}
#endif
	}
}
