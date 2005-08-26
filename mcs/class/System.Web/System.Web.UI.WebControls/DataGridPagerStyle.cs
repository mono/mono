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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.ComponentModel;

namespace System.Web.UI.WebControls {
	public sealed class DataGridPagerStyle : TableItemStyle {
		#region Constructors
		internal DataGridPagerStyle () {
		}
		#endregion	// Constructors

		#region Public Instance Properties
		[Bindable(true)]
		[DefaultValue(PagerMode.NextPrev)]
		[NotifyParentProperty(true)]
		public PagerMode Mode {
			get {
				if ((styles & Styles.Mode) == 0) {
					return PagerMode.NextPrev;
				}

				return (PagerMode)ViewState["Mode"];
			}

			set {
				styles |= Styles.Mode;
				ViewState["Mode"] = value;
			}
		}

		[Bindable(true)]
		[DefaultValue("&gt;")]
		[NotifyParentProperty(true)]
		public string NextPageText {
			get {
				if ((styles & Styles.NextPageText) == 0) {
					return "&gt;";
				}

				return ViewState.GetString("NextPageText", "&gt;");
			}

			set {
				styles |= Styles.NextPageText;
				ViewState["NextPageText"] = value;
			}
		}

		[Bindable(true)]
		[DefaultValue(10)]
		[NotifyParentProperty(true)]
		public int PageButtonCount {
			get {
				if ((styles & Styles.PageButtonCount) == 0) {
					return 10;
				}

				return ViewState.GetInt("PageButtonCount", 10);
			}

			set {
				if (value < 1) {
					throw new ArgumentOutOfRangeException("value", "PageButtonCount must be greater than 0");
				}

				styles |= Styles.PageButtonCount;
				ViewState["PageButtonCount"] = value;
			}
		}

		[Bindable(true)]
		[DefaultValue(PagerPosition.Bottom)]
		[NotifyParentProperty(true)]
		public PagerPosition Position {
			get {
				if ((styles & Styles.Position) == 0) {
					return PagerPosition.Bottom;
				}

				return (PagerPosition)ViewState["Position"];
			}

			set {
				styles |= Styles.Position;
				ViewState["Position"] = value;
			}
		}

		[Bindable(true)]
		[DefaultValue("&lt;")]
		[NotifyParentProperty(true)]
		public string PrevPageText {
			get {
				if ((styles & Styles.NextPageText) == 0) {
					return "&lt;";
				}

				return ViewState.GetString("PrevPageText", "&lt;");
			}

			set {
				styles |= Styles.PrevPageText;
				ViewState["PrevPageText"] = value;
			}
		}

		[Bindable(true)]
		[DefaultValue(true)]
		[NotifyParentProperty(true)]
		public bool Visible {
			get {
				if ((styles & Styles.Visible) == 0) {
					return true;
				}

				return ViewState.GetBool("Visible", true);
			}

			set {
				styles |= Styles.Visible;
				ViewState["Visible"] = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public override void CopyFrom(Style s) {
			base.CopyFrom (s);

			if (s == null || s.IsEmpty) {
				return;
			}

			if (((s.styles & Styles.Mode) != 0) && (((DataGridPagerStyle)s).Mode != PagerMode.NextPrev)) {
				this.Mode = ((DataGridPagerStyle)s).Mode;
			}

			if (((s.styles & Styles.NextPageText) != 0) && (((DataGridPagerStyle)s).NextPageText != "&gt;")) {
				this.NextPageText = ((DataGridPagerStyle)s).NextPageText;
			}

			if (((s.styles & Styles.PageButtonCount) != 0) && (((DataGridPagerStyle)s).PageButtonCount != 10)) {
				this.PageButtonCount = ((DataGridPagerStyle)s).PageButtonCount;
			}

			if (((s.styles & Styles.Position) != 0) && (((DataGridPagerStyle)s).Position != PagerPosition.Bottom)) {
				this.Position = ((DataGridPagerStyle)s).Position;
			}

			if (((s.styles & Styles.PrevPageText) != 0) && (((DataGridPagerStyle)s).PrevPageText != "&lt;")) {
				this.PrevPageText = ((DataGridPagerStyle)s).PrevPageText;
			}

			if (((s.styles & Styles.Visible) != 0) && (((DataGridPagerStyle)s).Visible != true)) {
				this.Visible = ((DataGridPagerStyle)s).Visible;
			}

		}

		public override void MergeWith(Style s) {
			base.MergeWith (s);

			if (s == null || s.IsEmpty) {
				return;
			}

			if (((styles & Styles.Mode) == 0) && ((s.styles & Styles.Mode) != 0) && (((DataGridPagerStyle)s).Mode != PagerMode.NextPrev)) {
				this.Mode = ((DataGridPagerStyle)s).Mode;
			}

			if (((styles & Styles.NextPageText) == 0) && ((s.styles & Styles.NextPageText) != 0) && (((DataGridPagerStyle)s).NextPageText != "&gt;")) {
				this.NextPageText = ((DataGridPagerStyle)s).NextPageText;
			}

			if (((styles & Styles.PageButtonCount) == 0) && ((s.styles & Styles.PageButtonCount) != 0) && (((DataGridPagerStyle)s).PageButtonCount != 10)) {
				this.PageButtonCount = ((DataGridPagerStyle)s).PageButtonCount;
			}

			if (((styles & Styles.Position) == 0) && ((s.styles & Styles.Position) != 0) && (((DataGridPagerStyle)s).Position != PagerPosition.Bottom)) {
				this.Position = ((DataGridPagerStyle)s).Position;
			}

			if (((styles & Styles.PrevPageText) == 0) && ((s.styles & Styles.PrevPageText) != 0) && (((DataGridPagerStyle)s).PrevPageText != "&lt;")) {
				this.PrevPageText = ((DataGridPagerStyle)s).PrevPageText;
			}

			if (((styles & Styles.Visible) == 0) && ((s.styles & Styles.Visible) != 0) && (((DataGridPagerStyle)s).Visible != true)) {
				this.Visible = ((DataGridPagerStyle)s).Visible;
			}
		}

		public override void Reset() {
			// We call base.Reset(), we don't need this
			//styles &= ~(Styles.Mode | Styles.NextPageText | Styles.PageButtonCount | Styles.Position | Styles.PrevPageText | Styles.Visible);

			ViewState.Remove("Mode");
			ViewState.Remove("NextPageText");
			ViewState.Remove("PageButtonCount");
			ViewState.Remove("Position");
			ViewState.Remove("PrevPageText");
			ViewState.Remove("Visible");

			base.Reset ();
		}

		internal override void LoadViewStateInternal()
		{
			if (viewstate["Mode"] != null) {
				styles |= Styles.Mode;
			}
			if (viewstate["NextPageText"] != null) {
				styles |= Styles.NextPageText;
			}
			if (viewstate["PageButtonCount"] != null) {
				styles |= Styles.PageButtonCount;
			}
			if (viewstate["Position"] != null) {
				styles |= Styles.Position;
			}
			if (viewstate["PrevPageText"] != null) {
				styles |= Styles.PrevPageText;
			}
			if (viewstate["Visible"] != null) {
				styles |= Styles.Visible;
			}

			base.LoadViewStateInternal();
		}
		#endregion	// Public Instance Methods
	}
}
