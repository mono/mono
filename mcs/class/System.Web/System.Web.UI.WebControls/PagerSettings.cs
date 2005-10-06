//
// System.Web.UI.WebControls.PagerSettings.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[TypeConverterAttribute (typeof(ExpandableObjectConverter))]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class PagerSettings: IStateManager
	{
		StateBag ViewState = new StateBag ();
		Control ctrl;
		
		public PagerSettings ()
		{
		}
		
		internal PagerSettings (Control ctrl)
		{
			this.ctrl = ctrl;
		}

		[WebCategoryAttribute ("Appearance")]
		[NotifyParentPropertyAttribute (true)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string FirstPageImageUrl {
			get {
				object ob = ViewState ["FirstPageImageUrl"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["FirstPageImageUrl"] = value;
			}
		}
	
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute ("&lt;&lt;")]
		[NotifyParentPropertyAttribute (true)]
		public string FirstPageText {
			get {
				object ob = ViewState ["FirstPageText"];
				if (ob != null) return (string) ob;
				return "&lt;&lt;";
			}
			set {
				ViewState ["FirstPageText"] = value;
			}
		}
	
		[WebCategoryAttribute ("Appearance")]
		[NotifyParentPropertyAttribute (true)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string LastPageImageUrl {
			get {
				object ob = ViewState ["LastPageImageUrl"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["LastPageImageUrl"] = value;
			}
		}
	
		[NotifyParentPropertyAttribute (true)]
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute ("&gt;&gt;")]
		public string LastPageText {
			get {
				object ob = ViewState ["LastPageText"];
				if (ob != null) return (string) ob;
				return "&gt;&gt;";
			}
			set {
				ViewState ["LastPageText"] = value;
			}
		}
	
		[NotifyParentPropertyAttribute (true)]
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute (PagerButtons.Numeric)]
		public PagerButtons Mode {
			get {
				object ob = ViewState ["Mode"];
				if (ob != null) return (PagerButtons) ob;
				return PagerButtons.Numeric;
			}
			set {
				ViewState ["Mode"] = value;
			}
		}
	
		[WebCategoryAttribute ("Appearance")]
		[NotifyParentPropertyAttribute (true)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string NextPageImageUrl {
			get {
				object ob = ViewState ["NextPageImageUrl"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["NextPageImageUrl"] = value;
			}
		}

		[WebCategoryAttribute ("Appearance")]
		[NotifyParentPropertyAttribute (true)]
		[DefaultValueAttribute ("&gt;")]
		public string NextPageText {
			get {
				object ob = ViewState ["NextPageText"];
				if (ob != null) return (string) ob;
				return "&gt;";
			}
			set {
				ViewState ["NextPageText"] = value;
			}
		}
	
		[WebCategoryAttribute ("Behavior")]
		[NotifyParentPropertyAttribute (true)]
		[DefaultValueAttribute (10)]
		public int PageButtonCount {
			get {
				object ob = ViewState ["PageButtonCount"];
				if (ob != null) return (int) ob;
				return 10;
			}
			set {
				ViewState ["PageButtonCount"] = value;
			}
		}
	
		[WebCategoryAttribute ("Layout")]
		[DefaultValueAttribute (PagerPosition.Bottom)]
		[NotifyParentPropertyAttribute (true)]
		public PagerPosition Position {
			get {
				object ob = ViewState ["Position"];
				if (ob != null) return (PagerPosition) ob;
				return PagerPosition.Bottom;
			}
			set {
				ViewState ["Position"] = value;
			}
		}
	
		[WebCategoryAttribute ("Appearance")]
		[NotifyParentPropertyAttribute (true)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string PreviousPageImageUrl {
			get {
				object ob = ViewState ["PreviousPageImageUrl"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["PreviousPageImageUrl"] = value;
			}
		}
	
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute ("&lt;")]
		[NotifyParentPropertyAttribute (true)]
		public string PreviousPageText {
			get {
				object ob = ViewState ["PreviousPageText"];
				if (ob != null) return (string) ob;
				return "&lt;";
			}
			set {
				ViewState ["PreviousPageText"] = value;
			}
		}
	
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute (true)]
		[NotifyParentPropertyAttribute (true)]
		public bool Visible {
			get {
				object ob = ViewState ["Visible"];
				if (ob != null) return (bool) ob;
				return true;
			}
			set {
				ViewState ["Visible"] = value;
			}
		}
	
		public override string ToString ()
		{
			return string.Empty;
		}

		void IStateManager.LoadViewState (object savedState)
		{
			ViewState.LoadViewState (savedState);
		}
		
		object IStateManager.SaveViewState ()
		{
			return ViewState.SaveViewState();
		}
		
		void IStateManager.TrackViewState ()
		{
			ViewState.TrackViewState();
		}
		
		bool IStateManager.IsTrackingViewState
		{
			get { return ViewState.IsTrackingViewState; }
		}
		
		internal Table CreatePagerControl (int currentPage, int pageCount)
		{
			Table table = new Table ();
			TableRow row = new TableRow ();
			table.Rows.Add (row);

			if (Mode == PagerButtons.NextPrevious || Mode == PagerButtons.NextPreviousFirstLast)
			{
				if (currentPage > 0) {
					if (Mode == PagerButtons.NextPreviousFirstLast)
						row.Cells.Add (CreateCell (FirstPageText, FirstPageImageUrl, "Page", "First"));
					row.Cells.Add (CreateCell (PreviousPageText, PreviousPageImageUrl, "Page", "Prev"));
				}
				if (currentPage < pageCount - 1) {
					row.Cells.Add (CreateCell (NextPageText, NextPageImageUrl, "Page", "Next"));
					if (Mode == PagerButtons.NextPreviousFirstLast)
						row.Cells.Add (CreateCell (LastPageText, LastPageImageUrl, "Page", "Last"));
				}
			}
			else if (Mode == PagerButtons.Numeric || Mode == PagerButtons.NumericFirstLast)
			{
				int first = currentPage / PageButtonCount;
				int last = first + PageButtonCount;
				if (last >= pageCount) last = pageCount;
				
				if (first > 0) {
					if (Mode == PagerButtons.NumericFirstLast)
						row.Cells.Add (CreateCell (FirstPageText, FirstPageImageUrl, "Page", "First"));
					row.Cells.Add (CreateCell (PreviousPageText, PreviousPageImageUrl, "Page", "Prev"));
				}
				
				for (int n = first; n < last; n++)
					row.Cells.Add (CreateCell ((n+1).ToString(), string.Empty, (n != currentPage) ? "Page" : "", (n+1).ToString()));
				
				if (last < pageCount - 1) {
					row.Cells.Add (CreateCell (NextPageText, NextPageImageUrl, "Page", "Next"));
					if (Mode == PagerButtons.NumericFirstLast)
						row.Cells.Add (CreateCell (LastPageText, LastPageImageUrl, "Page", "Last"));
				}
			}
			return table;
		}
		
		TableCell CreateCell (string text, string image, string command, string argument)
		{
			TableCell cell = new TableCell ();
			cell.Controls.Add (new DataControlButton (ctrl, text, image, command, argument, true));
			return cell;
		}
	}
}

#endif
