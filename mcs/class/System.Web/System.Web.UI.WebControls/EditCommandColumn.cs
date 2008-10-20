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
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class EditCommandColumn : DataGridColumn {
		#region Public Constructors
		public EditCommandColumn() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public virtual ButtonColumnType ButtonType {
			get {
				object obj;

				obj = ViewState["ButtonType"];
				if (obj != null) {
					return (ButtonColumnType)obj;
				}
				return ButtonColumnType.LinkButton;
			}

			set {
				ViewState["ButtonType"] = value;
			}
		}

#if NET_2_0
		[DefaultValue ("")]
		[Localizable (true)]
#endif
		public virtual string CancelText {
			get {
				return ViewState.GetString("CancelText", string.Empty);
			}

			set {
				ViewState["CancelText"] = value;
			}
		}

#if NET_2_0
		[DefaultValue(true)]
		public virtual bool CausesValidation {
			get { return ViewState.GetBool ("CausesValidation", true); }
			set { ViewState ["CausesValidation"] = value; } 
		}

		[DefaultValue("")]
		public virtual string ValidationGroup {
			get { return ViewState.GetString ("ValidationGroup", ""); }
			set { ViewState ["ValidationGroup"] = value; } 
		}

#endif

#if NET_2_0
		[DefaultValue ("")]
		[Localizable (true)]
#endif
		public virtual string EditText {
			get {
				return ViewState.GetString("EditText", string.Empty);
			}

			set {
				ViewState["EditText"] = value;
			}
		}

#if NET_2_0
		[DefaultValue ("")]
		[Localizable (true)]
#endif
		public virtual string UpdateText {
			get {
				return ViewState.GetString("UpdateText", string.Empty);
			}

			set {
				ViewState["UpdateText"] = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods

		// Modeled after Ben's CommandField.InitializeCell. Saved me a lot of figuring-out time :-)
		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType) {
			base.InitializeCell (cell, columnIndex, itemType);

			switch(itemType) {
				case ListItemType.Separator: 
				case ListItemType.Pager:
				case ListItemType.Footer:
				case ListItemType.Header: {
					// Base handles header and footer, dunno about the others
					return;
				}

				case ListItemType.Item:
				case ListItemType.SelectedItem:
				case ListItemType.AlternatingItem:{
					cell.Controls.Add(CreateButton(ButtonType, EditText, "Edit", false));
					break;
				}

				case ListItemType.EditItem: {
#if NET_2_0
					cell.Controls.Add (CreateButton (ButtonType, UpdateText, "Update", CausesValidation));
#else
					cell.Controls.Add(CreateButton(ButtonType, UpdateText, "Update", true));
#endif
					cell.Controls.Add(new LiteralControl("&nbsp;"));
					cell.Controls.Add(CreateButton(ButtonType, CancelText, "Cancel", false));
					break;
				}
			}
		}
		#endregion	// Public Instance Methods

		#region Private Methods
		Control CreateButton(ButtonColumnType type, string text, string command, bool valid) {
			Button b;
			LinkButton d;

			if (type == ButtonColumnType.LinkButton) {
				d = new ForeColorLinkButton();
				d.Text = text;
				d.CommandName = command;
				d.CausesValidation = valid;
#if NET_2_0
				if (valid) {
					d.ValidationGroup = ValidationGroup;
				}
#endif
				return d;
			}

			b = new Button();
			b.Text = text;
			b.CommandName = command;
			b.CausesValidation = valid;
#if NET_2_0
			if (valid) {
				b.ValidationGroup = ValidationGroup;
			}
#endif
			return b;
		}
		#endregion	// Private Methods
	}
}
