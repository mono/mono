//
// System.Web.UI.WebControls.CheckBoxList.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CheckBoxList : ListControl, IRepeatInfoUser, INamingContainer, IPostBackDataHandler 
	{
		CheckBox check_box;

		public CheckBoxList ()
		{
			check_box = new CheckBox ();
			Controls.Add (check_box);
		}

		[DefaultValue(-1)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int CellPadding {
			get { return TableStyle.CellPadding; }
			set { TableStyle.CellPadding = value; }
		}

		[DefaultValue(-1)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int CellSpacing {
			get { return TableStyle.CellSpacing; }
			set { TableStyle.CellSpacing = value; }
		}

		[DefaultValue(0)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int RepeatColumns {
			get { return ViewState.GetInt ("RepeatColumns", 0); }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["RepeatColumns"] = value;
			}
		}

		[DefaultValue(RepeatDirection.Vertical)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual RepeatDirection RepeatDirection {
			get { return (RepeatDirection) ViewState.GetInt ("RepeatDirection", (int) RepeatDirection.Vertical); }
			set {
				if (value < RepeatDirection.Horizontal ||
						value > RepeatDirection.Vertical)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["RepeatDirection"] = value;
			}
		}

		[DefaultValue(RepeatLayout.Table)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual RepeatLayout RepeatLayout {
			get { return (RepeatLayout) ViewState.GetInt ("RepeatLayout", (int) RepeatLayout.Table); }
			set {
				bool outOfRange;
				outOfRange = value < RepeatLayout.Table || value > RepeatLayout.OrderedList;
				if (outOfRange)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["RepeatLayout"] = value;
			}
		}

		[DefaultValue(TextAlign.Right)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual TextAlign TextAlign {
			get { return (TextAlign) ViewState.GetInt ("TextAlign", (int) TextAlign.Right); }
			set {
				if (value < TextAlign.Left || value > TextAlign.Right)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["TextAlign"] = value;
			}
		}

		TableStyle TableStyle {
			get { return (TableStyle) ControlStyle; }
		}

		protected override Style CreateControlStyle ()
		{
			return new TableStyle (ViewState);
		}

		protected override Control FindControl (string id, int pathOffset)
		{
			// Always, or in just all my tests?
			return this;
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			// Register all of the checked controls so we can
			// find out when they are unchecked.
			Page page = Page;
			for (int i = 0; i < Items.Count; i++) {
				if (Items [i].Selected) {
					check_box.ID = i.ToString (Helpers.InvariantCulture);
					if (page != null)
						page.RegisterRequiresPostBack (check_box);
				}
			}
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			if (Items.Count == 0)
				return;

			RepeatInfo ri = new RepeatInfo ();
			ri.RepeatColumns = RepeatColumns;
			ri.RepeatDirection = RepeatDirection;
			ri.RepeatLayout = RepeatLayout;

			short ti = 0;
			if (TabIndex != 0) {
				check_box.TabIndex = TabIndex;
				ti = TabIndex;
				TabIndex = 0;
			}

			string ak = AccessKey;
			check_box.AccessKey = ak;
			this.AccessKey = null;

			ri.RenderRepeater (writer, this, TableStyle, this);

			if (ti != 0)
				TabIndex = ti;
			this.AccessKey = ak;
		}

		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			if (!IsEnabled)
				return false;

			EnsureDataBound ();
			int checkbox = -1;

			try {
				string id = postDataKey.Substring (ClientID.Length + 1);
				if (Char.IsDigit (id [0]))
					checkbox = Int32.Parse (id, Helpers.InvariantCulture);
			} catch {
				return false;
			}

			if (checkbox == -1)
				return false;

			string val = postCollection [postDataKey];
			bool ischecked = val == "on";
			ListItem item = Items [checkbox];
			if (item.Enabled) {
				if (ischecked && !item.Selected) {
					item.Selected = true;
					return true;
				} else if (!ischecked && item.Selected) {
					item.Selected = false;
					return true;
				}
			}
			
			return false;
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			if (CausesValidation) {
				Page page = Page;
				if (page != null)
					page.Validate (ValidationGroup);
			}
			
			OnSelectedIndexChanged (EventArgs.Empty);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}

		protected virtual bool HasFooter  {
			get { return false; }
		}

		bool IRepeatInfoUser.HasFooter {
			get { return HasFooter; }
		}

		protected virtual bool HasHeader {
			get { return false; }
		}

		bool IRepeatInfoUser.HasHeader {
			get { return HasHeader; }
		}

		protected virtual bool HasSeparators {
			get { return false; }
		}

		bool IRepeatInfoUser.HasSeparators {
			get { return HasSeparators; }
		}

		protected virtual int RepeatedItemCount {
			get { return Items.Count; }
		}

		int IRepeatInfoUser.RepeatedItemCount {
			get { return RepeatedItemCount; }
		}

		protected virtual Style GetItemStyle (ListItemType itemType, int repeatIndex)
		{
			return null;
		}

		Style IRepeatInfoUser.GetItemStyle (ListItemType itemType, int repeatIndex)
		{
			return GetItemStyle (itemType, repeatIndex);
		}

		protected virtual void RenderItem (ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			ListItem item = Items [repeatIndex];

			string cssClass = check_box.CssClass;
			if (!String.IsNullOrEmpty (cssClass))
				check_box.CssClass = String.Empty;
			check_box.ID = repeatIndex.ToString (Helpers.InvariantCulture);
			check_box.Text = item.Text;
			check_box.AutoPostBack = AutoPostBack;
			check_box.Checked = item.Selected;
			check_box.TextAlign = TextAlign;
			if (!IsEnabled)
				check_box.Enabled = false;
			else
				check_box.Enabled = item.Enabled;

			check_box.ValidationGroup = ValidationGroup;
			check_box.CausesValidation = CausesValidation;
			if (check_box.HasAttributes)
				check_box.Attributes.Clear ();
			if (item.HasAttributes)
				check_box.Attributes.CopyFrom (item.Attributes);
			if (!RenderingCompatibilityLessThan40) {
				var attrs = check_box.InputAttributes;
			
				attrs.Clear ();
				attrs.Add ("value", item.Value);
			}
			check_box.RenderControl (writer);
		}

		void IRepeatInfoUser.RenderItem (ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			RenderItem (itemType, repeatIndex, repeatInfo, writer);
		}

		internal override bool MultiSelectOk ()
		{
			return true;
		}
	}
}
