//
// Authors:
//	Ben Maurer <bmaurer@novell.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.Helpers
{
	public class RepeatInfoUser : IRepeatInfoUser
	{
		bool footer;
		bool header;
		bool separators;
		int count;
		int counter;

		public RepeatInfoUser (bool header, bool footer, bool separators, int count)
		{
			this.footer = footer;
			this.header = header;
			this.separators = separators;
			this.count = count;
		}

		static HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		public static string DoTest (int cols, int cnt, RepeatDirection d, RepeatLayout l, bool OuterTableImplied, bool hdr, bool ftr, bool sep)
		{
			HtmlTextWriter htw = GetWriter ();
			RepeatInfo ri = new RepeatInfo ();
			ri.RepeatColumns = cols;
			ri.RepeatDirection = d;
			ri.RepeatLayout = l;
			ri.OuterTableImplied = OuterTableImplied;
			// get some variation in if we use style or not
			Style s = new Style ();
			if (cols != 3)
				s.CssClass = "mainstyle";
			
			ri.RenderRepeater (htw, new RepeatInfoUser (hdr, ftr, sep, cnt), s, new DataList ());
			return htw.InnerWriter.ToString ();
		}


		public bool HasFooter {
			get { return footer; }
		}

		public bool HasHeader {
			get { return header; }
		}
		
		public bool HasSeparators {
			get { return separators; }
		}

		public int RepeatedItemCount {
			get { return count; }
		}

		public Style GetItemStyle (ListItemType itemType, int repeatIndex)
		{
			Style s = new Style ();
			s.CssClass = String.Format ("{0}{1}", itemType, repeatIndex);
			return s;
		}

		public void RenderItem (ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			writer.Write ("({0},{1},{2})", counter++, itemType, repeatIndex);
		}
	}
}
