//
// System.Web.UI.WebControls.DataGridColumn.cs
//
// Author:
//      Dick Porter  <dick@ximian.com>
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

using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[TypeConverter (typeof (System.ComponentModel.ExpandableObjectConverter))]
	public abstract class DataGridColumn : IStateManager
	{
		DataGrid owner;
		StateBag viewstate;
		bool tracking_viewstate;
		bool design;

		TableItemStyle footer_style;
		TableItemStyle header_style;
		TableItemStyle item_style;
		
		protected DataGridColumn ()
		{
			viewstate = new StateBag ();
		}
		
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual TableItemStyle FooterStyle {
			get {
				if (footer_style == null) {
					footer_style = new TableItemStyle ();

					if (tracking_viewstate)
						footer_style.TrackViewState ();
				}

				return (footer_style);
			}
		}
		
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string FooterText {
			get { return (viewstate.GetString ("FooterText", String.Empty)); }
			set { viewstate["FooterText"] = value; }
		}

		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		[UrlProperty]
		public virtual string HeaderImageUrl {
			get { return (viewstate.GetString ("HeaderImageUrl", String.Empty)); }
			set { viewstate["HeaderImageUrl"] = value; }
		}
		
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual TableItemStyle HeaderStyle {
			get {
				if (header_style == null) {
					header_style = new TableItemStyle ();

					if (tracking_viewstate)
						header_style.TrackViewState ();
				}

				return (header_style);
			}
		}

		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string HeaderText {
			get { return (viewstate.GetString ("HeaderText", String.Empty)); }
			set { viewstate["HeaderText"] = value; }
		}
		
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual TableItemStyle ItemStyle {
			get {
				if (item_style == null) {
					item_style = new TableItemStyle ();

					if (tracking_viewstate)
						item_style.TrackViewState ();
				}

				return (item_style);
			}
		}
		
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string SortExpression {
			get { return (viewstate.GetString ("SortExpression", String.Empty)); }
			set { viewstate["SortExpression"] = value; }
		}

		[DefaultValue (true)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public bool Visible  {
			get { return (viewstate.GetBool ("Visible", true)); }
			set { viewstate["Visible"] = value; }
		}

		public virtual void Initialize ()
		{
			if (owner != null && owner.Site != null)
				design = owner.Site.DesignMode;
		}

		internal class ForeColorLinkButton : LinkButton
		{
			Color GetForeColor (WebControl control)
			{
				if (control == null)
					return Color.Empty;

				// don't go beyond the container table.
				if (control is Table)
					return control.ControlStyle.ForeColor;
				
				Color color = control.ControlStyle.ForeColor;
				if (color != Color.Empty)
					return color;

				return GetForeColor ((WebControl) control.Parent);
			}

			protected internal override void Render (HtmlTextWriter writer)
			{
				Color color = GetForeColor (this);
				if (color != Color.Empty)
					ForeColor = color;
				base.Render (writer);
			}
		}

		public virtual void InitializeCell (TableCell cell, int columnIndex, ListItemType itemType)
		{
			switch (itemType) {
				case ListItemType.Header: 
				{
					/* If sorting is enabled, add a
					 * LinkButton or an ImageButton
					 * (depending on HeaderImageUrl).
					 *
					 * If sorting is disabled, the
					 * HeaderText or an Image is displayed
					 * (depending on HeaderImageUrl).
					 *
					 * If neither HeaderText nor
					 * HeaderImageUrl is set, use &nbsp;
					 */
					bool sort = false;
					string sort_ex = SortExpression;
				
					if (owner != null && sort_ex.Length > 0)
						sort = owner.AllowSorting;
				
					string image_url = HeaderImageUrl;
					if (image_url.Length > 0) {
						if (sort) {
							ImageButton butt = new ImageButton ();

							/* Don't need to
							 * resolve this, Image
							 * does that when it
							 * renders
							 */
							butt.ImageUrl = image_url;
							butt.CommandName = "Sort";
							butt.CommandArgument = sort_ex;

							cell.Controls.Add (butt);
						} else {
							Image image = new Image ();

							image.ImageUrl = image_url;

							cell.Controls.Add (image);
						}
					} else {
						if (sort) {
							// This one always gets the forecolor of the header_style
							// from one of the parents, but we can't look it up at this
							// point, as it can change afterwards.
							LinkButton link = new ForeColorLinkButton ();

							link.Text = HeaderText;
							link.CommandName = "Sort";
							link.CommandArgument = sort_ex;

							cell.Controls.Add (link);
						} else {
							string text = HeaderText;
							if (text.Length > 0)
								cell.Text = text;
							else
								cell.Text = "&nbsp;";
						}
					}
				}
				break;

				case ListItemType.Footer:
				{
					/* Display FooterText or &nbsp; */
					string text = FooterText;

					if (text.Length > 0)
						cell.Text = text;
					else
						cell.Text = "&nbsp;";
				}
				break;

				default:
					break;
			}
		}

		public override string ToString ()
		{
			return (String.Empty);
		}

		protected bool DesignMode {
			get {return (design); }
		}
		
		protected DataGrid Owner {
			get { return (owner); }
		}

		internal TableItemStyle GetStyle (ListItemType type)
		{
			if (type == ListItemType.Header)
				return header_style;

			if (type == ListItemType.Footer)
				return footer_style;

			return item_style;
		}

		internal void Set_Owner (DataGrid value) 
		{
			owner = value;
		}
		
		protected StateBag ViewState {
			get { return (viewstate); }
		}

		/* There are no events defined for DataGridColumn, so no
		 * idea what this method is supposed to do
		 */
		protected virtual void OnColumnChanged ()
		{
		}
		
		void IStateManager.LoadViewState (object savedState)
		{
			LoadViewState (savedState);
		}

		object IStateManager.SaveViewState ()
		{
			return (SaveViewState ());
		}

		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}

		bool IStateManager.IsTrackingViewState {
			get { return (IsTrackingViewState); }
		}

		protected virtual void LoadViewState (object savedState)
		{
			object[] pieces = savedState as object[];

			if (pieces == null)
				return;

			if (pieces[0] != null)
				viewstate.LoadViewState (pieces[0]);
			
			if (pieces[1] != null)
				FooterStyle.LoadViewState (pieces[1]);
			
			if (pieces[2] != null)
				HeaderStyle.LoadViewState (pieces[2]);
			
			if (pieces[3] != null)
				ItemStyle.LoadViewState (pieces[3]);
		}

		protected virtual object SaveViewState ()
		{
			object[] res = new object[4];

			res[0] = viewstate.SaveViewState ();

			if (footer_style != null)
				res[1] = footer_style.SaveViewState ();
			
			if (header_style != null)
				res[2] = header_style.SaveViewState ();
			
			if (item_style != null)
				res[3] = item_style.SaveViewState ();

			return (res);
		}

		protected virtual void TrackViewState ()
		{
			tracking_viewstate = true;
			
			viewstate.TrackViewState ();
			if (footer_style != null)
				footer_style.TrackViewState ();
			
			if (header_style != null)
				header_style.TrackViewState ();
			
			if (item_style != null)
				item_style.TrackViewState ();
		}

		protected bool IsTrackingViewState {
			get { return (tracking_viewstate); }
		}
	}
}
