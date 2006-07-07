//
// Tests for System.Web.UI.WebControls.GridViewRowTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
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


#if NET_2_0

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;


namespace MonoTests.System.Web.UI.WebControls
{
	class PokerGridViewRow : GridViewRow
	{
		// View state Stuff
		public PokerGridViewRow(int rowIndex,int dataItemIndex,DataControlRowType rowType,DataControlRowState rowState)
		       :base(rowIndex,dataItemIndex,rowType,rowState)
		{
			TrackViewState ();
		}

		public bool DoOnBubbleEvent (object source, EventArgs e)
		{
			return base.OnBubbleEvent (source, e);
		}
	}
	
	[TestFixture]
	public class GridViewRowTest
	{
		
		[Test]
		public void GridViewRow_DefaultProperty ()
		{
			GridViewRow row = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
			Assert.AreEqual (null, row.DataItem, "DataItem");
			Assert.AreEqual (0, row.DataItemIndex, "DataItemIndex"); //This value assigned into constractor
			Assert.AreEqual (0, row.RowIndex, "RowIndex");		 //This value assigned into constractor
			Assert.AreEqual (DataControlRowType.DataRow, row.RowType, "RowType");   //This value assigned into constractor
			Assert.AreEqual (DataControlRowState.Normal, row.RowState, "RowState"); //This value assigned into constractor
		}

		[Test]
		public void GridViewRow_AssignProperty ()
		{
			// All public or protected property are assigned into constractor
			// and was tested into default property test
		}

		[Test]
		public void GridViewRow_BubbleEvent ()
		{
			PokerGridViewRow row = new PokerGridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
			bool result = row.DoOnBubbleEvent (this, new CommandEventArgs ("", null));
			Assert.AreEqual (true, result, "OnBubbleEventWithCommandEventArgs");
			result = row.DoOnBubbleEvent (this, new EventArgs());
			Assert.AreEqual (false, result, "OnBubbleEventWithEventArgs");
		}


	}
}
#endif