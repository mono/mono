//
// System.Web.UI.WebControls.ButtonField.cs
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
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ButtonField : ButtonFieldBase
	{
		PropertyDescriptor boundProperty;

	    [WebCategoryAttribute ("Behavior")]
	    [DefaultValueAttribute ("")]
		public virtual string CommandName {
			get {
				object ob = ViewState ["CommandName"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["CommandName"] = value;
				OnFieldChanged ();
			}
		}
		
	    [WebCategoryAttribute ("Data")]
	    [DefaultValueAttribute ("")]
		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public virtual string DataTextField {
			get {
				object ob = ViewState ["DataTextField"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["DataTextField"] = value;
				OnFieldChanged ();
			}
		}
		
	    [WebCategoryAttribute ("Data")]
	    [DefaultValueAttribute ("")]
		public virtual string DataTextFormatString {
			get {
				object ob = ViewState ["DataTextFormatString"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["DataTextFormatString"] = value;
				OnFieldChanged ();
			}
		}
		
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	    [WebCategoryAttribute ("Appearance")]
	    [DefaultValueAttribute ("")]
	    [UrlPropertyAttribute]
		public virtual string ImageUrl {
			get {
				object ob = ViewState ["ImageUrl"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["ImageUrl"] = value;
				OnFieldChanged ();
			}
		}
		
	    [LocalizableAttribute (true)]
	    [WebCategoryAttribute ("Appearance")]
	    [DefaultValueAttribute ("")]
		public virtual string Text {
			get {
				object ob = ViewState ["Text"];
				if (ob != null) return (string) ob;
				return "";
			}
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
				return string.Format (DataTextFormatString, value);
			else if (value == null)
				return string.Empty;
			else
				return value.ToString ();
		}
		
		public override void InitializeCell (DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			string index = rowIndex.ToString ();
			
			if (cellType == DataControlCellType.DataCell) {
				DataControlButton btn = new DataControlButton (Control);
				btn.CommandName = CommandName;
				btn.CommandArgument = index;
				
				if (DataTextField != "") {
					cell.DataBinding += new EventHandler (OnDataBindField);
				}
				else {
					btn.Text = Text;
					btn.ButtonType = ButtonType;
					if (ButtonType == ButtonType.Image) btn.ImageUrl = ImageUrl;
				}
				cell.Controls.Add (btn);
			}
			else
				base.InitializeCell (cell, cellType, rowState, rowIndex);
		}
		
		void OnDataBindField (object sender, EventArgs e)
		{
			DataControlFieldCell cell = (DataControlFieldCell) sender;
			DataControlButton btn = (DataControlButton) cell.Controls [0]; 
			btn.Text = FormatDataTextValue (GetBoundValue (cell.BindingContainer));
			if (ButtonType == ButtonType.Image) btn.ImageUrl = ImageUrl;
			btn.ButtonType = ButtonType;
		}
		
		object GetBoundValue (Control controlContainer)
		{
			IDataItemContainer dic = controlContainer as IDataItemContainer;
			if (boundProperty == null) {
				boundProperty = TypeDescriptor.GetProperties (dic.DataItem) [DataTextField];
				if (boundProperty == null)
					new InvalidOperationException ("Property '" + DataTextField + "' not found in object of type " + dic.DataItem.GetType());
			}
			return boundProperty.GetValue (dic.DataItem);
		}
	}
}
#endif
