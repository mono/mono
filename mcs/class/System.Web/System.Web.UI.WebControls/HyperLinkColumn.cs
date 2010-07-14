//
// System.Web.UI.WebControls.HyperLinkColumn.cs
//
// Author: Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

using System.ComponentModel;
using System.Data;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
        public class HyperLinkColumn : DataGridColumn
        {
		public HyperLinkColumn ()
		{
		}

		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string DataNavigateUrlField {
			get {
				return ViewState.GetString ("DataNavigateUrlField", String.Empty);
			}
			set { ViewState ["DataNavigateUrlField"] = value; }
		}

		[DefaultValue("")]
		[Description("The formatting applied to the value bound to the NavigateUrl property.")]
		[WebCategory ("Misc")]
		public virtual string DataNavigateUrlFormatString {
			get {
				return ViewState.GetString ("DataNavigateUrlFormatString", String.Empty);
			}
			set { ViewState ["DataNavigateUrlFormatString"] = value; }
		}

		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string DataTextField {
			get {
				return ViewState.GetString ("DataTextField", String.Empty);
			}
			set { ViewState ["DataTextField"] = value; }
		}

		[Description("The formatting applied to the value bound to the Text property.")]
		[DefaultValue("")]
		[WebCategory ("Misc")]
		public virtual string DataTextFormatString {
			get {
				return ViewState.GetString ("DataTextFormatString", String.Empty);
			}
			set { ViewState ["DataTextFormatString"] = value; }
		}

		[UrlProperty]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string NavigateUrl {
			get {
				return ViewState.GetString ("NavigateUrl", String.Empty);
			}
			set { ViewState ["NavigateUrl"] = value; }
		}

		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		[TypeConverter ("System.Web.UI.WebControls.TargetConverter")]
		public virtual string Target {
			get {
				return ViewState.GetString ("Target", String.Empty);
			}
			set { ViewState ["Target"] = value; }
		}

		[Localizable (true)]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string Text {
			get {
				return ViewState.GetString ("Text", String.Empty);
			}
			set { ViewState ["Text"] = value; }
		}

		protected virtual string FormatDataNavigateUrlValue (object value)
		{
			string format = DataNavigateUrlFormatString;
			if (format == "")
				format = null;

			return DataBinder.FormatResult (value, format);
		}

		protected virtual string FormatDataTextValue (object value)
		{
			string format = DataTextFormatString;
			if (format == "")
				format = null;

			return DataBinder.FormatResult (value, format);
		}

		public override void Initialize ()
		{
			base.Initialize ();
		}

		void ItemDataBinding (object sender, EventArgs args)
		{
			TableCell cell = (TableCell)sender;
			HyperLink ctrl = (HyperLink)cell.Controls[0];
			DataGridItem item = (DataGridItem)cell.NamingContainer;

			if (DataNavigateUrlField != "")
				ctrl.NavigateUrl = FormatDataNavigateUrlValue (DataBinder.Eval (item.DataItem, DataNavigateUrlField));
			else
				ctrl.NavigateUrl = NavigateUrl;

			if (DataTextField != "")
				ctrl.Text = FormatDataTextValue (DataBinder.Eval (item.DataItem, DataTextField));
			else
				ctrl.Text = Text;

			ctrl.Target = Target;
		}

		public override void InitializeCell (TableCell cell, int column_index, ListItemType item_type)
		{
			base.InitializeCell (cell, column_index, item_type);

			switch (item_type)
			{
			case ListItemType.Separator: 
			case ListItemType.Pager:
			case ListItemType.Footer:
			case ListItemType.Header: {
				// Base handles header and footer, dunno about the others
				return;
			}
			case ListItemType.Item:
			case ListItemType.EditItem:
			case ListItemType.AlternatingItem:
				cell.DataBinding += new EventHandler(ItemDataBinding);
				cell.Controls.Add (new HyperLink ());
				break;
			}
		}
        }
}
