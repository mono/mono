//
// System.Web.UI.WebControls.CheckBoxField.cs
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
	public class CheckBoxField : BoundField
	{
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override bool ApplyFormatInEditMode {
			get { throw GetNotSupportedPropException ("ApplyFormatInEditMode"); }
			set { throw GetNotSupportedPropException ("ApplyFormatInEditMode"); }
		}

		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override bool ConvertEmptyStringToNull {
			get { throw GetNotSupportedPropException ("ConvertEmptyStringToNull"); } 
			set { throw GetNotSupportedPropException ("ConvertEmptyStringToNull"); } 
		}
		
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override string DataFormatString {
			get { throw GetNotSupportedPropException ("DataFormatString"); } 
			set { throw GetNotSupportedPropException ("DataFormatString"); } 
		}
		
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override bool HtmlEncode {
			get { throw GetNotSupportedPropException ("HtmlEncode"); } 
			set { throw GetNotSupportedPropException ("HtmlEncode"); } 
		}
		
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override string NullDisplayText {
			get { throw GetNotSupportedPropException ("NullDisplayText"); } 
			set { throw GetNotSupportedPropException ("NullDisplayText"); } 
		}
		
		protected override bool SupportsHtmlEncode {
			get { return false; }
		}
		
		[LocalizableAttribute (true)]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string Text {
			get { return ViewState.GetString ("Text", String.Empty); }
			set {
				ViewState ["Text"] = value;
				OnFieldChanged ();
			}
		}
		
		protected override void InitializeDataCell (DataControlFieldCell cell, DataControlRowState rowState)
		{
			bool editable = IsEditable (rowState);
			CheckBox box = new CheckBox ();
			box.Enabled = editable;
			if (editable)
				box.ToolTip = HeaderText;
			box.Text = Text;
			cell.Controls.Add (box);
		}
		
		public override void ExtractValuesFromCell (IOrderedDictionary dictionary,
			DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			bool editable = IsEditable (rowState);
			if (editable || includeReadOnly) {
				CheckBox box = (CheckBox) cell.Controls [0];
				dictionary [DataField] = box.Checked;
			}
		}
		
		protected override void OnDataBindField (object sender, EventArgs e)
		{
			try {
				Control container = (Control) sender;
				object val = GetValue (container.NamingContainer);
				CheckBox box = sender as CheckBox;
				if (box == null) {
					DataControlFieldCell cell = sender as DataControlFieldCell;
					if (cell != null) {
						ControlCollection controls = cell.Controls;
						int ccount = controls != null ? controls.Count : 0;
						if (ccount == 1)
							box = controls [0] as CheckBox;
						if (box == null)
							return;
					}
				}
				
				if (box == null)
					throw new HttpException ("CheckBox field '" + DataField + "' contains a control that isn't a CheckBox.  Override OnDataBindField to inherit from CheckBoxField and add different controls.");
				
				if (val != null && val != DBNull.Value)
					box.Checked = (bool) val;
				else {
					if (string.IsNullOrEmpty (DataField)) {
						box.Visible = false;
						return;
					}
				}
				
				if (!box.Visible)
					box.Visible = true;
			} catch (HttpException) {
				throw;
			} catch (Exception ex) {
				throw new HttpException (ex.Message, ex);
			}
		}
		
		protected override object GetDesignTimeValue ()
		{
			return true;
		}
		
		protected override DataControlField CreateField ()
		{
			return new CheckBoxField ();
		}
		
		protected override void CopyProperties (DataControlField newField)
		{
			CheckBoxField field = (CheckBoxField) newField;
			field.DataField = DataField;
			field.ReadOnly = ReadOnly;
			field.Text = Text;
		}
	}
}

