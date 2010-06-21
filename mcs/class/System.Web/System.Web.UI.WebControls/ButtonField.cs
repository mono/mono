//
// System.Web.UI.WebControls.ButtonField.cs
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

using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ButtonField : ButtonFieldBase
	{
		PropertyDescriptor boundProperty;

		[DefaultValueAttribute ("")]
		[WebSysDescription ("Raised when a Button Command is executed.")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string CommandName {
			get { return ViewState.GetString ("CommandName", String.Empty); }
			set {
				ViewState ["CommandName"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute ("")]
		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataTextField {
			get { return ViewState.GetString ("DataTextField", String.Empty); }
			set {
				ViewState ["DataTextField"] = value;
				OnFieldChanged ();
			}
		}
		
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataTextFormatString {
			get { return ViewState.GetString ("DataTextFormatString", String.Empty); }
			set {
				ViewState ["DataTextFormatString"] = value;
				OnFieldChanged ();
			}
		}
		
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute ("")]
		[UrlPropertyAttribute]
		public virtual string ImageUrl {
			get { return ViewState.GetString ("ImageUrl", String.Empty); }
			set {
				ViewState ["ImageUrl"] = value;
				OnFieldChanged ();
			}
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
		
		public override bool Initialize (bool sortingEnabled, Control control)
		{
			return base.Initialize (sortingEnabled, control);
		}
		
		protected virtual string FormatDataTextValue (object value)
		{
			if (DataTextFormatString.Length > 0)
				return String.Format (DataTextFormatString, value);
			else if (value == null)
				return String.Empty;
			else
				return value.ToString ();
		}
		
		public override void InitializeCell (DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			string index = rowIndex.ToString ();
			
			if (cellType == DataControlCellType.DataCell) {
				
				IDataControlButton btn = DataControlButton.CreateButton (ButtonType, Control, Text, ImageUrl, CommandName, index, false);

				if (CausesValidation) {
					btn.Container = null;
					btn.CausesValidation = true;
					btn.ValidationGroup = ValidationGroup;
				}
				
				if (!String.IsNullOrEmpty (DataTextField)) {
					if ((rowState & DataControlRowState.Insert) == 0)
						cell.DataBinding += new EventHandler (OnDataBindField);
				}
				cell.Controls.Add ((Control) btn);
			} else
				base.InitializeCell (cell, cellType, rowState, rowIndex);
		}
		
		void OnDataBindField (object sender, EventArgs e)
		{
			DataControlFieldCell cell = (DataControlFieldCell) sender;
			IDataControlButton btn = (IDataControlButton) cell.Controls [0]; 
			btn.Text = FormatDataTextValue (GetBoundValue (cell.BindingContainer));
		}
		
		object GetBoundValue (Control controlContainer)
		{
			IDataItemContainer dic = controlContainer as IDataItemContainer;
			if (boundProperty == null) {
				boundProperty = TypeDescriptor.GetProperties (dic.DataItem) [DataTextField];
				if (boundProperty == null)
					throw new InvalidOperationException ("Property '" + DataTextField + "' not found in object of type " + dic.DataItem.GetType());
			}
			return boundProperty.GetValue (dic.DataItem);
		}
		
		protected override DataControlField CreateField ()
		{
			return new ButtonField ();
		}
		
		protected override void CopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
			ButtonField field = (ButtonField) newField;
			field.CommandName = CommandName;
			field.DataTextField = DataTextField;
			field.DataTextFormatString = DataTextFormatString;
			field.ImageUrl = ImageUrl;
			field.Text = Text;
		}

		// MSDN: The ValidateSupportsCallback method is a helper method used to determine 
		// whether the controls contained in a BoundField object support callbacks. 
		// This method has been implemented as an empty method (a method that does not 
		// contain any code) to indicate that callbacks are supported.
		public override void ValidateSupportsCallback ()
		{
		}
	}
}
