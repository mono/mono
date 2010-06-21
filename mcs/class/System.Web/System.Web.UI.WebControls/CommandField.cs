//
// System.Web.UI.WebControls.CommandField.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CommandField : ButtonFieldBase
	{
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string CancelImageUrl {
			get { return ViewState.GetString ("CancelImageUrl", String.Empty); }
			set {
				ViewState ["CancelImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[LocalizableAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string CancelText {
			get { return ViewState.GetString ("CancelText", "Cancel"); }
			set {
				ViewState ["CancelText"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute (true)]
		public override bool CausesValidation {
			get { return ViewState.GetBool ("CausesValidation", true); }
			set {
				ViewState ["CausesValidation"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string DeleteImageUrl {
			get { return ViewState.GetString ("DeleteImageUrl", String.Empty); }
			set {
				ViewState ["DeleteImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[LocalizableAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string DeleteText {
			get { return ViewState.GetString ("DeleteText", "Delete"); }
			set {
				ViewState ["DeleteText"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string EditImageUrl {
			get { return ViewState.GetString ("EditImageUrl", String.Empty); }
			set {
				ViewState ["EditImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[LocalizableAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string EditText {
			get { return ViewState.GetString ("EditText", "Edit"); }
			set {
				ViewState ["EditText"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string InsertImageUrl {
			get { return ViewState.GetString ("InsertImageUrl", String.Empty); }
			set {
				ViewState ["InsertImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[LocalizableAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string InsertText {
			get { return ViewState.GetString ("InsertText", "Insert"); }
			set {
				ViewState ["InsertText"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string NewImageUrl {
			get { return ViewState.GetString ("NewImageUrl", String.Empty); }
			set {
				ViewState ["NewImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[LocalizableAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string NewText {
			get { return ViewState.GetString ("NewText", "New"); }
			set {
				ViewState ["NewText"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string SelectImageUrl {
			get { return ViewState.GetString ("SelectImageUrl", String.Empty); }
			set {
				ViewState ["SelectImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[LocalizableAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string SelectText {
			get { return ViewState.GetString ("SelectText", "Select"); }
			set {
				ViewState ["SelectText"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowCancelButton {
			get { return ViewState.GetBool ("ShowCancelButton", true); }
			set {
				ViewState ["ShowCancelButton"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowDeleteButton {
			get { return ViewState.GetBool ("ShowDeleteButton", false); }
			set {
				ViewState ["ShowDeleteButton"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowEditButton {
			get { return ViewState.GetBool ("ShowEditButton", false); }
			set {
				ViewState ["ShowEditButton"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowSelectButton {
			get { return ViewState.GetBool ("ShowSelectButton", false); }
			set {
				ViewState ["ShowSelectButton"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ShowInsertButton {
			get { return ViewState.GetBool ("ShowInsertButton", false); }
			set {
				ViewState ["ShowInsertButton"] = value;
				OnFieldChanged ();
			}
		}

		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string UpdateImageUrl {
			get { return ViewState.GetString ("UpdateImageUrl", String.Empty); }
			set {
				ViewState ["UpdateImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[LocalizableAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string UpdateText {
			get { return ViewState.GetString ("UpdateText", "Update"); }
			set {
				ViewState ["UpdateText"] = value;
				OnFieldChanged ();
			}
		}
		
		public override void InitializeCell (DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			string index = rowIndex.ToString ();
			
			if (cellType == DataControlCellType.DataCell) {
				if ((rowState & DataControlRowState.Edit) != 0) {
					if (ShowEditButton) {
						cell.Controls.Add (CreateButton (UpdateText, UpdateImageUrl, DataControlCommands.UpdateCommandName, index));
						if (ShowCancelButton) {
							AddSeparator (cell);
							cell.Controls.Add (CreateButton (CancelText, CancelImageUrl, DataControlCommands.CancelCommandName, index));
						}
					}
				} else if ((rowState & DataControlRowState.Insert) != 0) {
					if (ShowInsertButton) {
						cell.Controls.Add (CreateButton (InsertText, InsertImageUrl, DataControlCommands.InsertCommandName, index));
						if (ShowCancelButton) {
							AddSeparator (cell);
							cell.Controls.Add (CreateButton (CancelText, CancelImageUrl, DataControlCommands.CancelCommandName, index));
						}
					}
				} else {
					if (ShowEditButton) {
						AddSeparator (cell);
						cell.Controls.Add (CreateButton (EditText, EditImageUrl, DataControlCommands.EditCommandName, index));
					}
					if (ShowDeleteButton) {
						AddSeparator (cell);
						cell.Controls.Add (CreateButton (DeleteText, DeleteImageUrl, DataControlCommands.DeleteCommandName, index));
					}
					if (ShowInsertButton) {
						AddSeparator (cell);
						cell.Controls.Add (CreateButton (NewText, NewImageUrl, DataControlCommands.NewCommandName, index));
					}
					if (ShowSelectButton) {
						AddSeparator (cell);
						cell.Controls.Add (CreateButton (SelectText, SelectImageUrl, DataControlCommands.SelectCommandName, index));
					}
				}
			} else
				base.InitializeCell (cell, cellType, rowState, rowIndex);
		}
		
		Control CreateButton (string text, string image, string command, string arg)
		{
			IDataControlButton c = DataControlButton.CreateButton (ButtonType, Control, text, image, command, arg, false);
			if (CausesValidation) {
				if (command == DataControlCommands.UpdateCommandName || command == DataControlCommands.InsertCommandName) {
					c.Container = null;
					c.CausesValidation = true;
					c.ValidationGroup = ValidationGroup;
				}
			}
			return (Control)c;
		}
		
		void AddSeparator (DataControlFieldCell cell)
		{
			if (cell.Controls.Count > 0) {
				Literal lit = new Literal ();
				lit.Text = "&nbsp;";
				cell.Controls.Add (lit);
			}
		}
		
		protected override DataControlField CreateField ()
		{
			return new CommandField ();
		}
		
		protected override void CopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
			CommandField field = (CommandField) newField;
			field.CancelImageUrl = CancelImageUrl;
			field.CancelText = CancelText;
			field.DeleteImageUrl = DeleteImageUrl;
			field.DeleteText = DeleteText;
			field.EditImageUrl = EditImageUrl;
			field.EditText = EditText;
			field.InsertImageUrl = InsertImageUrl;
			field.InsertText = InsertText;
			field.NewImageUrl = NewImageUrl;
			field.NewText = NewText;
			field.SelectImageUrl = SelectImageUrl;
			field.SelectText = SelectText;
			field.ShowCancelButton = ShowCancelButton;
			field.ShowDeleteButton = ShowDeleteButton;
			field.ShowEditButton = ShowEditButton;
			field.ShowSelectButton = ShowSelectButton;
			field.ShowInsertButton = ShowInsertButton;
			field.UpdateImageUrl = UpdateImageUrl;
			field.UpdateText = UpdateText;
		}
		
		public override void ValidateSupportsCallback ()
		{
			if (ShowSelectButton)
				throw new NotSupportedException ("Callbacks are not supported on CommandField when the select button is enabled because other controls on your page that are dependent on the selected value of '" + Control.ID + "' for their rendering will not update in a callback.  Turn callbacks off on '" + Control.ID + "'.");
		}
	}
}

