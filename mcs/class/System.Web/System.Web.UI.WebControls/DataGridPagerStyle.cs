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
// Copyright (c) 2005-2010 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS - no inheritance demand required because the class is sealed
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class DataGridPagerStyle : TableItemStyle
	{
		[Flags]
		enum DataGridPagerStyles
		{
			Mode = 0x00100000,
			NextPageText = 0x00200000,
			PageButtonCount = 0x00400000,
			Position = 0x00800000,
			PrevPageText = 0x01000000,
			Visible = 0x02000000
		}

		#region Constructors
		internal DataGridPagerStyle ()
		{
		}
		#endregion	// Constructors

		#region Public Instance Properties
		[DefaultValue(PagerMode.NextPrev)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public PagerMode Mode {
			get {
				if (!CheckBit ((int) DataGridPagerStyles.Mode))
					return PagerMode.NextPrev;

				return (PagerMode)ViewState["Mode"];
			}

			set {
				ViewState["Mode"] = value;
				SetBit ((int) DataGridPagerStyles.Mode);
			}
		}

		[Localizable (true)]
		[DefaultValue("&gt;")]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public string NextPageText {
			get {
				if (!CheckBit ((int) DataGridPagerStyles.NextPageText))
					return "&gt;";

				return ViewState.GetString("NextPageText", "&gt;");
			}

			set {
				ViewState["NextPageText"] = value;
				SetBit ((int) DataGridPagerStyles.NextPageText);
			}
		}

		[DefaultValue(10)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public int PageButtonCount {
			get {
				if (!CheckBit ((int) DataGridPagerStyles.PageButtonCount))
					return 10;

				return ViewState.GetInt("PageButtonCount", 10);
			}

			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException("value", "PageButtonCount must be greater than 0");

				ViewState["PageButtonCount"] = value;
				SetBit ((int) DataGridPagerStyles.PageButtonCount);
			}
		}

		[DefaultValue(PagerPosition.Bottom)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public PagerPosition Position {
			get {
				if (!CheckBit ((int) DataGridPagerStyles.Position))
					return PagerPosition.Bottom;

				return (PagerPosition)ViewState["Position"];
			}

			set {
				ViewState["Position"] = value;
				SetBit ((int) DataGridPagerStyles.Position);
			}
		}

		[Localizable (true)]
		[DefaultValue("&lt;")]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public string PrevPageText {
			get {
				if (!CheckBit ((int) DataGridPagerStyles.PrevPageText))
					return "&lt;";

				return ViewState.GetString("PrevPageText", "&lt;");
			}

			set {
				ViewState["PrevPageText"] = value;
				SetBit ((int) DataGridPagerStyles.PrevPageText);
			}
		}

		[DefaultValue(true)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public bool Visible {
			get {
				if (!CheckBit ((int) DataGridPagerStyles.Visible))
					return true;

				return ViewState.GetBool("Visible", true);
			}

			set {
				ViewState["Visible"] = value;
				SetBit ((int) DataGridPagerStyles.Visible);
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public override void CopyFrom(Style s)
		{
			base.CopyFrom (s);

			if (s == null || s.IsEmpty)
				return;

			if (s.CheckBit ((int) DataGridPagerStyles.Mode) && (((DataGridPagerStyle) s).Mode != PagerMode.NextPrev))
				this.Mode = ((DataGridPagerStyle)s).Mode;

			if (s.CheckBit ((int) DataGridPagerStyles.NextPageText) && (((DataGridPagerStyle) s).NextPageText != "&gt;"))
				this.NextPageText = ((DataGridPagerStyle)s).NextPageText;

			if (s.CheckBit ((int) DataGridPagerStyles.PageButtonCount) && (((DataGridPagerStyle) s).PageButtonCount != 10))
				this.PageButtonCount = ((DataGridPagerStyle)s).PageButtonCount;

			if (s.CheckBit ((int) DataGridPagerStyles.Position) && (((DataGridPagerStyle) s).Position != PagerPosition.Bottom))
				this.Position = ((DataGridPagerStyle)s).Position;

			if (s.CheckBit ((int) DataGridPagerStyles.PrevPageText) && (((DataGridPagerStyle) s).PrevPageText != "&lt;"))
				this.PrevPageText = ((DataGridPagerStyle)s).PrevPageText;

			if (s.CheckBit ((int) DataGridPagerStyles.Visible) && (((DataGridPagerStyle) s).Visible != true))
				this.Visible = ((DataGridPagerStyle)s).Visible;
		}

		public override void MergeWith(Style s)
		{
			base.MergeWith (s);

			if (s == null || s.IsEmpty)
				return;

			if (!CheckBit ((int) DataGridPagerStyles.Mode) && s.CheckBit ((int) DataGridPagerStyles.Mode) && (((DataGridPagerStyle) s).Mode != PagerMode.NextPrev))
				this.Mode = ((DataGridPagerStyle)s).Mode;

			if (!CheckBit ((int) DataGridPagerStyles.NextPageText) && s.CheckBit ((int) DataGridPagerStyles.NextPageText) && (((DataGridPagerStyle) s).NextPageText != "&gt;"))
				this.NextPageText = ((DataGridPagerStyle)s).NextPageText;

			if (!CheckBit ((int) DataGridPagerStyles.PageButtonCount) && s.CheckBit ((int) DataGridPagerStyles.PageButtonCount) && (((DataGridPagerStyle) s).PageButtonCount != 10))
				this.PageButtonCount = ((DataGridPagerStyle)s).PageButtonCount;

			if (!CheckBit ((int) DataGridPagerStyles.Position) && s.CheckBit ((int) DataGridPagerStyles.Position) && (((DataGridPagerStyle) s).Position != PagerPosition.Bottom))
				this.Position = ((DataGridPagerStyle)s).Position;

			if (!CheckBit ((int) DataGridPagerStyles.PrevPageText) && s.CheckBit ((int) DataGridPagerStyles.PrevPageText) && (((DataGridPagerStyle) s).PrevPageText != "&lt;"))
				this.PrevPageText = ((DataGridPagerStyle)s).PrevPageText;

			if (!CheckBit ((int) DataGridPagerStyles.Visible) && s.CheckBit ((int) DataGridPagerStyles.Visible) && (((DataGridPagerStyle) s).Visible != true))
				this.Visible = ((DataGridPagerStyle)s).Visible;
		}

		public override void Reset ()
		{
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
		#endregion	// Public Instance Methods
	}
}
