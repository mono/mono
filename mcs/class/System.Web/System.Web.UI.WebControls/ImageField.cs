//
// System.Web.UI.WebControls.ImageField.cs
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
using System.Reflection;

namespace System.Web.UI.WebControls {

	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ImageField : DataControlField
	{
		public static readonly string ThisExpression = "!";
		
		PropertyDescriptor imageProperty;
		PropertyDescriptor textProperty;
		
		public override bool Initialize (bool enableSorting, Control control)
		{
			return base.Initialize (enableSorting, control);
		}

		[DefaultValueAttribute ("")]
		[LocalizableAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string AlternateText {
			get {
				object ob = ViewState ["AlternateText"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["AlternateText"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ConvertEmptyStringToNull {
			get {
				object ob = ViewState ["ConvertEmptyStringToNull"];
				if (ob != null)
					return (bool) ob;
				return true;
			}
			set {
				ViewState ["ConvertEmptyStringToNull"] = value;
				OnFieldChanged ();
			}
		}

		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataAlternateTextField {
			get {
				object ob = ViewState ["DataAlternateTextField"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["DataAlternateTextField"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataAlternateTextFormatString {
			get {
				object ob = ViewState ["DataAlternateTextFormatString"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["DataAlternateTextFormatString"] = value;
				OnFieldChanged ();
			}
		}

		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataImageUrlField {
			get {
				object ob = ViewState ["DataImageUrlField"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["DataImageUrlField"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataImageUrlFormatString {
			get {
				object ob = ViewState ["DataImageUrlFormatString"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["DataImageUrlFormatString"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[LocalizableAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string NullDisplayText {
			get {
				object ob = ViewState ["NullDisplayText"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["NullDisplayText"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string NullImageUrl {
			get {
				object ob = ViewState ["NullImageUrl"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["NullImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ReadOnly {
			get {
				object val = ViewState ["ReadOnly"];
				return val != null ? (bool) val : false;
			}
			set { 
				ViewState ["ReadOnly"] = value;
				OnFieldChanged ();
			}
		}

		public override void ExtractValuesFromCell (IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			if ((ReadOnly && !includeReadOnly) || cell.Controls.Count == 0)
				return;
			
			bool editable = (rowState & (DataControlRowState.Edit | DataControlRowState.Insert)) != 0;
			if (includeReadOnly || editable) {
				Control control = cell.Controls [0];
				//TODO: other controls?
				if (control is Image)
					dictionary [DataImageUrlField] = ((Image)control).ImageUrl;
				else if (control is TextBox)
					dictionary [DataImageUrlField] = ((TextBox) control).Text;
			}
		}
		
		public override void InitializeCell (DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			base.InitializeCell (cell, cellType, rowState, rowIndex);
			if (cellType == DataControlCellType.DataCell) {
				InitializeDataCell (cell, rowState);
				if ((rowState & DataControlRowState.Insert) == 0)
					cell.DataBinding += new EventHandler (OnDataBindField);
			}
		}
		
		protected virtual void InitializeDataCell (DataControlFieldCell cell, DataControlRowState rowState)
		{
			bool editable = (rowState & (DataControlRowState.Edit | DataControlRowState.Insert)) != 0;
			if (editable && !ReadOnly) {
				TextBox box = new TextBox ();
				cell.Controls.Add (box);
			} else if (DataImageUrlField.Length > 0) {
				Image img = new Image ();
				img.ControlStyle.CopyFrom (ControlStyle);
				cell.Controls.Add (img);
			}
		}
		
		protected virtual string FormatImageUrlValue (object dataValue)
		{
			if (dataValue == null)
				return null;
			else if (DataImageUrlFormatString.Length > 0)
				return string.Format (DataImageUrlFormatString, dataValue);
			else
				return dataValue.ToString ();
		}
		
		protected virtual string GetFormattedAlternateText (Control controlContainer)
		{
			if (DataAlternateTextField.Length > 0) {
				if (textProperty == null)
					textProperty = GetProperty (controlContainer, DataAlternateTextField);
					
				object value = GetValue (controlContainer, DataAlternateTextField, ref textProperty);
				
				if (value == null || (value.ToString().Length == 0 && ConvertEmptyStringToNull))
					return NullDisplayText;
				else if (DataAlternateTextFormatString.Length > 0)
					return string.Format (DataAlternateTextFormatString, value);
				else
					return value.ToString ();
			} else
				return AlternateText;
		
		}
		
		protected virtual object GetValue (Control controlContainer, string fieldName, ref PropertyDescriptor cachedDescriptor)
		{
			if (DesignMode)
				return GetDesignTimeValue ();
			else {
				object dataItem = DataBinder.GetDataItem (controlContainer);
				if (dataItem == null)
					throw new HttpException ("A data item was not found in the container. The container must either implement IDataItemContainer, or have a property named DataItem.");
				if (fieldName == ThisExpression)
					return dataItem;
				else {
					if (cachedDescriptor != null) return cachedDescriptor.GetValue (dataItem);
					PropertyDescriptor prop = GetProperty (controlContainer, fieldName);
					return prop.GetValue (dataItem);
				}
			}
		}
		
		PropertyDescriptor GetProperty (Control controlContainer, string fieldName)
		{
			if (fieldName == ThisExpression)
				return null;
			
			IDataItemContainer dic = (IDataItemContainer) controlContainer;
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (dic.DataItem);
			PropertyDescriptor prop = properties != null ? properties [fieldName] : null;
			if (prop == null)
				throw new InvalidOperationException ("Property '" + fieldName + "' not found in object of type " + dic.DataItem.GetType());
			
			return prop;
		}
		
		protected virtual string GetDesignTimeValue ()
		{
			return "Databound";
		}
		
		protected virtual void OnDataBindField (object sender, EventArgs e)
		{
			Control control = (Control) sender;
			ControlCollection controls = control != null ? control.Controls : null;
			Control namingContainer = control.NamingContainer;
			Control c;
			if (sender is DataControlFieldCell) {
				if (controls.Count == 0)
					return;
				c = controls [0];
			} else if (sender is Image || sender is TextBox)
				c = control;
			else
				return;

			if (imageProperty == null)
				imageProperty = GetProperty (namingContainer, DataImageUrlField);
			
			if (c is TextBox) {
				object val = GetValue (namingContainer, DataImageUrlField, ref imageProperty);
				((TextBox)c).Text = val != null ? val.ToString() : String.Empty;
			} else if (c is Image) {
				Image img = (Image)c;
				string value =  FormatImageUrlValue (GetValue (namingContainer, DataImageUrlField, ref imageProperty));
				if (value == null || (ConvertEmptyStringToNull && value.Length == 0)) {
					if (NullImageUrl == null || NullImageUrl.Length == 0) {
						c.Visible = false;
						Label label = new Label ();
						label.Text = NullDisplayText;
						controls.Add (label);
					} else
						value = NullImageUrl;
				}
				img.ImageUrl = value;
				img.AlternateText = GetFormattedAlternateText (namingContainer);
			}
		}
		
		public override void ValidateSupportsCallback ()
		{
		}
		
		protected override DataControlField CreateField ()
		{
			return new ImageField ();
		}
		
		protected override void CopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
			ImageField field = (ImageField) newField;
			field.AlternateText = AlternateText;
			field.ConvertEmptyStringToNull = ConvertEmptyStringToNull;
			field.DataAlternateTextField = DataAlternateTextField;
			field.DataAlternateTextFormatString = DataAlternateTextFormatString;
			field.DataImageUrlField = DataImageUrlField;
			field.DataImageUrlFormatString = DataImageUrlFormatString;
			field.NullDisplayText = NullDisplayText;
			field.NullImageUrl = NullImageUrl;
			field.ReadOnly = ReadOnly;
		}
	}
}
