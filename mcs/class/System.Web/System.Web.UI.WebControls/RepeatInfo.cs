//
// System.Web.UI.WebControls.RepeatInfo.cs
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

//#define DEBUG_REPEAT_INFO

using System.Diagnostics;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS - no inheritance demand required because the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class RepeatInfo {

		// What is baseControl for ?
		public void RenderRepeater (HtmlTextWriter writer, IRepeatInfoUser user, Style controlStyle, WebControl baseControl)
		{
			PrintValues (user);
			RepeatLayout layout = RepeatLayout;
			bool listLayout = layout == RepeatLayout.OrderedList || layout == RepeatLayout.UnorderedList;

			if (listLayout) {
				if (user != null) {
					if ((user.HasHeader || user.HasFooter || user.HasSeparators))
						throw new InvalidOperationException ("The UnorderedList and OrderedList layouts do not support headers, footers or separators.");
				}

				if (OuterTableImplied)
					throw new InvalidOperationException ("The UnorderedList and OrderedList layouts do not support implied outer tables.");

				int cols = RepeatColumns;
				if (cols > 1)
					throw new InvalidOperationException ("The UnorderedList and OrderedList layouts do not support multi-column layouts.");
			}
			if (RepeatDirection == RepeatDirection.Vertical) {
				if (listLayout)
					RenderList (writer, user, controlStyle, baseControl);
				else
					RenderVert (writer, user, controlStyle, baseControl);
			} else {
				if (listLayout)
						throw new InvalidOperationException ("The UnorderedList and OrderedList layouts only support vertical layout.");
				RenderHoriz (writer, user, controlStyle, baseControl);
			}
		}

		void RenderBr (HtmlTextWriter w)
		{
			w.Write ("<br />");
		}
		void RenderList (HtmlTextWriter w, IRepeatInfoUser user, Style controlStyle, WebControl baseControl)
		{
			int items = user.RepeatedItemCount;
			RenderBeginTag (w, controlStyle, baseControl);

			for (int i = 0; i < items; i++) {
				// Style s = null;
				// s = user.GetItemStyle (ListItemType.Item, i);
				// if (s != null)
				// 	s.AddAttributesToRender (w);
				w.RenderBeginTag (HtmlTextWriterTag.Li);
				user.RenderItem (ListItemType.Item, i, this, w);
				w.RenderEndTag (); // </li>
				w.WriteLine ();
			}
			
			w.RenderEndTag ();
		}
		void RenderVert (HtmlTextWriter w, IRepeatInfoUser user, Style controlStyle, WebControl baseControl) 
		{
			int itms = user.RepeatedItemCount;
			// total number of rows/columns in our table
			int cols = RepeatColumns == 0 ? 1 : RepeatColumns;
			// this gets ceil (itms / cols)
			int rows = (itms + cols - 1) / cols;
			bool sep = user.HasSeparators;
			bool oti = OuterTableImplied;
			int hdr_span = cols * ((sep && cols != 1) ? 2 : 1);
			bool table = RepeatLayout == RepeatLayout.Table && !oti;
			bool show_empty_trailing_items = true;
			bool show_empty_trailing_sep = true;
			
			if (! oti)
				RenderBeginTag (w, controlStyle, baseControl);

			if (Caption.Length > 0) {
				if (CaptionAlign != TableCaptionAlign.NotSet)
					w.AddAttribute (HtmlTextWriterAttribute.Align, CaptionAlign.ToString());

				w.RenderBeginTag (HtmlTextWriterTag.Caption);
				w.Write (Caption);
				w.RenderEndTag ();

			}

			// Render the header
			if (user.HasHeader) {
				if (oti)
					user.RenderItem (ListItemType.Header, -1, this, w);
				else if (table) {
					w.RenderBeginTag (HtmlTextWriterTag.Tr);
					// Make sure the header takes up the full width. We have two
					// columns per item if we are using separators, otherwise
					// one per item.
					if (hdr_span != 1)
						w.AddAttribute (HtmlTextWriterAttribute.Colspan, hdr_span.ToString (), false);

					if (UseAccessibleHeader)
						w.AddAttribute ("scope", "col", false);
					
					Style s = user.GetItemStyle (ListItemType.Header, -1);
					if (s != null)
						s.AddAttributesToRender (w);

					if (UseAccessibleHeader)
						w.RenderBeginTag (HtmlTextWriterTag.Th);
					else
						w.RenderBeginTag (HtmlTextWriterTag.Td);

					user.RenderItem (ListItemType.Header, -1, this, w);
					w.RenderEndTag (); // td
					w.RenderEndTag (); // tr
				} else {
					user.RenderItem (ListItemType.Header, -1, this, w);
					RenderBr (w);
				}
			}

			for (int r = 0; r < rows; r ++) {
				if (table)
					w.RenderBeginTag (HtmlTextWriterTag.Tr);
				
				for (int c = 0; c < cols; c ++) {
					// Find the item number we are in according to the repeat
					// direction.
					int item = index_vert (rows, cols, r, c, itms);

					// This item is blank because there there not enough items
					// to make a full row.
					if (!show_empty_trailing_items && item >= itms)
						continue;

					if (table) {
						Style s = null;
						if (item < itms)
							s = user.GetItemStyle (ListItemType.Item, item);
						if (s != null)
							s.AddAttributesToRender (w);
						w.RenderBeginTag (HtmlTextWriterTag.Td);
					}
					
					if (item < itms)
						user.RenderItem (ListItemType.Item, item, this, w);

					if (table)
						w.RenderEndTag (); // td

					if (sep && cols != 1) {
						if (table) {
							if (item < itms - 1) {
								Style s = user.GetItemStyle (ListItemType.Separator, item);
								if (s != null)
									s.AddAttributesToRender (w);
							}
							if (item < itms - 1 || show_empty_trailing_sep)
								w.RenderBeginTag (HtmlTextWriterTag.Td);
						}

						if (item < itms - 1)
							user.RenderItem (ListItemType.Separator, item, this, w);

						if (table && (item < itms - 1 || show_empty_trailing_sep))
							w.RenderEndTag (); // td
					}
				}
				if (oti) {
				} else if (table) {
					w.RenderEndTag (); // tr
				} else if (r != rows - 1) {
					RenderBr(w);
				}
				
				if (sep && r != rows - 1 /* no sep on last item */ && cols == 1) {
					if (table) {
						w.RenderBeginTag (HtmlTextWriterTag.Tr);
						Style s = user.GetItemStyle (ListItemType.Separator, r);
						if (s != null)
							s.AddAttributesToRender (w);
					
						w.RenderBeginTag (HtmlTextWriterTag.Td);
					}
					
					user.RenderItem (ListItemType.Separator, r, this, w);

					if (table) {
						w.RenderEndTag (); // td
						w.RenderEndTag (); // tr
					} else if (!oti) {
						RenderBr (w);
					}
				}
			}

			// Render the footer
			if (user.HasFooter) {
				if (oti)
					user.RenderItem (ListItemType.Footer, -1, this, w);
				else if (table) {
					w.RenderBeginTag (HtmlTextWriterTag.Tr);
					if (hdr_span != 1)
						w.AddAttribute (HtmlTextWriterAttribute.Colspan, hdr_span.ToString (), false);

					Style s = user.GetItemStyle (ListItemType.Footer, -1);
					if (s != null)
						s.AddAttributesToRender (w);
					
					w.RenderBeginTag (HtmlTextWriterTag.Td);
					user.RenderItem (ListItemType.Footer, -1, this, w);
					w.RenderEndTag (); // td
					w.RenderEndTag (); // tr
				} else {
					// avoid dups on 0 items
					if (itms != 0)
						RenderBr (w);
					user.RenderItem (ListItemType.Footer, -1, this, w);
				}
			}
			if (! oti)
				w.RenderEndTag (); // table/span
			
		}
		
		void RenderHoriz (HtmlTextWriter w, IRepeatInfoUser user, Style controlStyle, WebControl baseControl) 
		{
			int itms = user.RepeatedItemCount;
			// total number of rows/columns in our table
			int cols = RepeatColumns == 0 ? itms : RepeatColumns;
			// this gets ceil (itms / cols)
			int rows = cols == 0 ? 0 : (itms + cols - 1) / cols;
			bool sep = user.HasSeparators;
			//bool oti = OuterTableImplied;
			int hdr_span = cols * (sep ? 2 : 1);

			bool table = RepeatLayout == RepeatLayout.Table;
			bool show_empty_trailing_items = true;
			bool show_empty_trailing_sep = true;

			RenderBeginTag (w, controlStyle, baseControl);

			if (Caption.Length > 0) {
				if (CaptionAlign != TableCaptionAlign.NotSet)
					w.AddAttribute (HtmlTextWriterAttribute.Align, CaptionAlign.ToString());

				w.RenderBeginTag (HtmlTextWriterTag.Caption);
				w.Write (Caption);
				w.RenderEndTag ();

			}
			
			// Render the header
			if (user.HasHeader) {
				if (table) {
					w.RenderBeginTag (HtmlTextWriterTag.Tr);
					// Make sure the header takes up the full width. We have two
					// columns per item if we are using separators, otherwise
					// one per item.
					if (hdr_span != 1)
						w.AddAttribute (HtmlTextWriterAttribute.Colspan, hdr_span.ToString (), false);

					if (UseAccessibleHeader)
						w.AddAttribute ("scope", "col", false);

					Style s = user.GetItemStyle (ListItemType.Header, -1);
					if (s != null)
						s.AddAttributesToRender (w);

					if (UseAccessibleHeader)
						w.RenderBeginTag (HtmlTextWriterTag.Th);
					else
						w.RenderBeginTag (HtmlTextWriterTag.Td);

					user.RenderItem (ListItemType.Header, -1, this, w);
					w.RenderEndTag (); // td
					w.RenderEndTag (); // tr
				} else {
					user.RenderItem (ListItemType.Header, -1, this, w);
					if (!table && RepeatColumns != 0 && itms != 0)
						RenderBr (w);
				}
			}
						
			for (int r = 0; r < rows; r ++) {
				if (table)
					w.RenderBeginTag (HtmlTextWriterTag.Tr);
				
				for (int c = 0; c < cols; c ++) {
					// Find the item number we are in according to the repeat
					// direction.
					int item = r * cols + c;

					// This item is blank because there there not enough items
					// to make a full row.
					if (!show_empty_trailing_items && item >= itms)
						continue;

					if (table) {
						Style s = null;
						if (item < itms)
							s = user.GetItemStyle (ListItemType.Item, item);

						if (s != null)
							s.AddAttributesToRender (w);
						w.RenderBeginTag (HtmlTextWriterTag.Td);
					}

					if (item < itms)
						user.RenderItem (ListItemType.Item, item, this, w);

					if (table)
						w.RenderEndTag (); // td

					if (sep) {
						if (table) {
							if (item < itms - 1) {
								Style s = user.GetItemStyle (ListItemType.Separator, item);
								if (s != null)
									s.AddAttributesToRender (w);
							}
							if (item < itms - 1 || show_empty_trailing_sep)
								w.RenderBeginTag (HtmlTextWriterTag.Td);
						}

						if (item < itms - 1)
							user.RenderItem (ListItemType.Separator, item, this, w);

						if (table && (item < itms - 1 || show_empty_trailing_sep))
							w.RenderEndTag (); // td
					}
				}

				if (table) {
					//	if (!oti)
						w.RenderEndTag (); // tr
				} else if (!(r == rows -1 && RepeatColumns == 0))
					RenderBr (w);
				
			}

			// Render the footer
			if (user.HasFooter) {
				if (table) {
					w.RenderBeginTag (HtmlTextWriterTag.Tr);
					if (hdr_span != 1)
						w.AddAttribute (HtmlTextWriterAttribute.Colspan, hdr_span.ToString (), false);

					Style s = user.GetItemStyle (ListItemType.Footer, -1);
					if (s != null)
						s.AddAttributesToRender (w);
					
					w.RenderBeginTag (HtmlTextWriterTag.Td);
					user.RenderItem (ListItemType.Footer, -1, this, w);
					w.RenderEndTag (); // td
					w.RenderEndTag (); // tr
				} else {
					user.RenderItem (ListItemType.Footer, -1, this, w);
				}
			}
			if (true)
				w.RenderEndTag (); // table/span
			
		}

		int index_vert (int rows, int cols, int r, int c, int items)
		{
			int last = items % cols;

			if (last == 0)
				last = cols;
			if (r == rows - 1 && c >= last)
				return items;
			
			
			int add;
			int v;
			if (c > last){
				add = last * rows + (c-last) * (rows-1);
				v = add + r;
			} else
				v = rows * c + r;
			
			return v;
		}

		void RenderBeginTag (HtmlTextWriter w, Style s, WebControl wc)
		{
			WebControl c;
			switch (RepeatLayout) {	
				case RepeatLayout.Table:
					c = new Table ();
					break;
					
				case RepeatLayout.Flow:
					c = new Label ();
					break;
				case RepeatLayout.OrderedList:
					c = new WebControl (HtmlTextWriterTag.Ol);
					break;

				case RepeatLayout.UnorderedList:
					c = new WebControl (HtmlTextWriterTag.Ul);
					break;
				default:
					throw new InvalidOperationException (String.Format ("Unsupported RepeatLayout value '{0}'.", RepeatLayout));
			}

			c.ID = wc.ClientID;
			c.CopyBaseAttributes (wc);
			c.ApplyStyle (s);
			c.Enabled = wc.IsEnabled;
			c.RenderBeginTag (w);
		}
		
		
		bool outer_table_implied;
		public bool OuterTableImplied {
			get {
				return outer_table_implied;
			}
			set {
				outer_table_implied = value;
			}
		}

		int repeat_cols;
		public int RepeatColumns {
			get {
				return repeat_cols;
			}
			set {
				repeat_cols = value;
			}
		}

		RepeatDirection dir = RepeatDirection.Vertical;
		public RepeatDirection RepeatDirection {
			get {
				return dir;
			}
			set {
				if (value != RepeatDirection.Horizontal &&
				    value != RepeatDirection.Vertical)
					throw new ArgumentOutOfRangeException ();
				
				dir = value;
			}
		}

		RepeatLayout layout;
		public RepeatLayout RepeatLayout {
			get {
				return layout;
			}
			set {
				bool outOfRange;
				outOfRange = value < RepeatLayout.Table || value > RepeatLayout.OrderedList;
				if (outOfRange)
					throw new ArgumentOutOfRangeException ();	
				layout = value;
			}
		}

		[Conditional ("DEBUG_REPEAT_INFO")]
		internal void PrintValues (IRepeatInfoUser riu)
		{
			string s = String.Format ("Layout {0}; Direction {1}; Cols {2}; OuterTableImplied {3}\n" +
					"User: itms {4}, hdr {5}; ftr {6}; sep {7}", RepeatLayout, RepeatDirection,
					RepeatColumns, OuterTableImplied, riu.RepeatedItemCount, riu.HasSeparators, riu.HasHeader,
					riu.HasFooter, riu.HasSeparators
				);
			Console.WriteLine (s);
			if (HttpContext.Current != null)
				HttpContext.Current.Trace.Write (s);
		}

		string caption = String.Empty;
		TableCaptionAlign captionAlign = TableCaptionAlign.NotSet; 
		bool useAccessibleHeader = false; 

		[WebSysDescription ("")]
		[WebCategory ("Accessibility")]
		public string Caption {
			get {return caption;}
			set { caption = value; }
		}

		[WebSysDescription ("")]
		[WebCategory ("Accessibility")]
		public TableCaptionAlign CaptionAlign {
			get {return captionAlign;}
			set { captionAlign = value; }
		}

		[WebSysDescription ("")]
		[WebCategory ("Accessibility")]
		public bool UseAccessibleHeader {
			get {return useAccessibleHeader;}
			set { useAccessibleHeader = value; }
		}
	}
}
