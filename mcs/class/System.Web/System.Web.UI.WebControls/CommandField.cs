//
// System.Web.UI.WebControls.CommandField.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//

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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CommandField : ButtonFieldBase
	{
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Appearance")]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		public virtual string CancelImageUrl {
			get {
				object ob = ViewState ["CancelImageUrl"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["CancelImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Appearance")]
		[LocalizableAttribute (true)]
		public virtual string CancelText {
			get {
				object ob = ViewState ["CancelText"];
				if (ob != null) return (string) ob;
				return "Cancel";
			}
			set {
				ViewState ["CancelText"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Appearance")]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		public virtual string DeleteImageUrl {
			get {
				object ob = ViewState ["DeleteImageUrl"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["DeleteImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Appearance")]
		[LocalizableAttribute (true)]
		public virtual string DeleteText {
			get {
				object ob = ViewState ["DeleteText"];
				if (ob != null) return (string) ob;
				return "Delete";
			}
			set {
				ViewState ["DeleteText"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Appearance")]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		public virtual string EditImageUrl {
			get {
				object ob = ViewState ["EditImageUrl"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["EditImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Appearance")]
		[LocalizableAttribute (true)]
		public virtual string EditText {
			get {
				object ob = ViewState ["EditText"];
				if (ob != null) return (string) ob;
				return "Edit";
			}
			set {
				ViewState ["EditText"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Appearance")]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		public virtual string InsertImageUrl {
			get {
				object ob = ViewState ["InsertImageUrl"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["InsertImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Appearance")]
		[LocalizableAttribute (true)]
		public virtual string InsertText {
			get {
				object ob = ViewState ["InsertText"];
				if (ob != null) return (string) ob;
				return "Insert";
			}
			set {
				ViewState ["InsertText"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Appearance")]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		public virtual string NewImageUrl {
			get {
				object ob = ViewState ["NewImageUrl"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["NewImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Appearance")]
		[LocalizableAttribute (true)]
		public virtual string NewText {
			get {
				object ob = ViewState ["NewText"];
				if (ob != null) return (string) ob;
				return "New";
			}
			set {
				ViewState ["NewText"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Appearance")]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		public virtual string SelectImageUrl {
			get {
				object ob = ViewState ["SelectImageUrl"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["SelectImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Appearance")]
		[LocalizableAttribute (true)]
		public virtual string SelectText {
			get {
				object ob = ViewState ["SelectText"];
				if (ob != null) return (string) ob;
				return "Select";
			}
			set {
				ViewState ["SelectText"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (true)]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowCancelButton {
			get {
				object ob = ViewState ["ShowCancelButton"];
				if (ob != null) return (bool) ob;
				return true;
			}
			set {
				ViewState ["ShowCancelButton"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowDeleteButton {
			get {
				object ob = ViewState ["ShowDeleteButton"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["ShowDeleteButton"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowEditButton {
			get {
				object ob = ViewState ["ShowEditButton"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["ShowEditButton"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowSelectButton {
			get {
				object ob = ViewState ["ShowSelectButton"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["ShowSelectButton"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowInsertButton {
			get {
				object ob = ViewState ["ShowInsertButton"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["ShowInsertButton"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Appearance")]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		public virtual string UpdateImageUrl {
			get {
				object ob = ViewState ["UpdateImageUrl"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["UpdateImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Appearance")]
		[LocalizableAttribute (true)]
		public virtual string UpdateText {
			get {
				object ob = ViewState ["UpdateText"];
				if (ob != null) return (string) ob;
				return "Update";
			}
			set {
				ViewState ["UpdateText"] = value;
				OnFieldChanged ();
			}
		}
		
		public override void InitializeCell (DataControlFieldCell cell,
			DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			string index = rowIndex.ToString ();
			
			if (cellType == DataControlCellType.DataCell)
			{
				if ((rowState & DataControlRowState.Edit) != 0) {
					cell.Controls.Add (new DataControlButton (Control, UpdateText, UpdateImageUrl, "Update", index, false));
					if (ShowCancelButton) {
						AddSeparator (cell);
						cell.Controls.Add (new DataControlButton (Control, CancelText, CancelImageUrl, "Cancel", index, false));
					}
				} else if ((rowState & DataControlRowState.Insert) != 0) {
					cell.Controls.Add (new DataControlButton (Control, InsertText, InsertImageUrl, "Insert", index, false));
					if (ShowCancelButton) {
						AddSeparator (cell);
						cell.Controls.Add (new DataControlButton (Control, CancelText, CancelImageUrl, "Cancel", index, false));
					}
				} else {
					if (ShowEditButton) {
						AddSeparator (cell);
						cell.Controls.Add (new DataControlButton (Control, EditText, EditImageUrl, "Edit", index, false));
					}
					if (ShowDeleteButton) {
						AddSeparator (cell);
						cell.Controls.Add (new DataControlButton (Control, DeleteText, DeleteImageUrl, "Delete", index, false));
					}
					if (ShowSelectButton) {
						AddSeparator (cell);
						cell.Controls.Add (new DataControlButton (Control, SelectText, SelectImageUrl, "Select", index, false));
					}
					if (ShowInsertButton) {
						AddSeparator (cell);
						cell.Controls.Add (new DataControlButton (Control, NewText, NewImageUrl, "New", index, false));
					}
				}
			} else
				base.InitializeCell (cell, cellType, rowState, rowIndex);
		}
		
		void AddSeparator (DataControlFieldCell cell)
		{
			if (cell.Controls.Count > 0) {
				Literal lit = new Literal ();
				lit.Text = "&nbsp;";
				cell.Controls.Add (lit);
			}
		}
	}
}
#endif
