//
// System.Web.UI.WebControls.ImageField.cs
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
using System.Reflection;

namespace System.Web.UI.WebControls {

	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ImageField : DataControlField
	{
		public static readonly string ThisExpression = "!";
		
		PropertyDescriptor imageProperty;
		PropertyDescriptor textProperty;
		
		public override bool Initialize (bool sortingEnabled, Control control)
		{
			return base.Initialize (sortingEnabled, control);
		}

	    [DefaultValueAttribute ("")]
	    [WebCategoryAttribute ("Appearance")]
	    [LocalizableAttribute (true)]
		public virtual string AlternateText {
			get {
				object ob = ViewState ["AlternateText"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["AlternateText"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute (true)]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ConvertEmptyStringToNull {
			get {
				object ob = ViewState ["ConvertEmptyStringToNull"];
				if (ob != null) return (bool) ob;
				return true;
			}
			set {
				ViewState ["ConvertEmptyStringToNull"] = value;
				OnFieldChanged ();
			}
		}

		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("")]
		public virtual string DataAlternateTextField {
			get {
				object ob = ViewState ["DataAlternateTextField"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["DataAlternateTextField"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("")]
		public virtual string DataAlternateTextFormatString {
			get {
				object ob = ViewState ["DataAlternateTextFormatString"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["DataAlternateTextFormatString"] = value;
				OnFieldChanged ();
			}
		}

		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("")]
		public virtual string DataImageUrlField {
			get {
				object ob = ViewState ["DataImageUrlField"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["DataImageUrlField"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("")]
		public virtual string DataImageUrlFormatString {
			get {
				object ob = ViewState ["DataImageUrlFormatString"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["DataImageUrlFormatString"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
	    [LocalizableAttribute (true)]
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
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	    [UrlPropertyAttribute]
	    [WebCategoryAttribute ("Behavior")]
		public virtual string NullImageUrl {
			get {
				object ob = ViewState ["NullImageUrl"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["NullImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Behavior")]
		[DefaultValueAttribute (false)]
		public bool ReadOnly {
			get {
				object val = ViewState ["ReadOnly"];
				return val != null ? (bool) val : false;
			}
			set { 
				ViewState ["ReadOnly"] = value;
				OnFieldChanged ();
			}
		}

		public override void ExtractValuesFromCell (IOrderedDictionary dictionary,
			DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			if ((ReadOnly && !includeReadOnly) || cell.Controls.Count == 0) return;
			
			bool editable = (rowState & (DataControlRowState.Edit | DataControlRowState.Insert)) != 0;
			if (editable && !ReadOnly) {
				TextBox box = cell.Controls [0] as TextBox;
				dictionary [DataImageUrlField] = box.Text;
			} else if (includeReadOnly) {
				Image img = cell.Controls [0] as Image;
				dictionary [DataImageUrlField] = img.ImageUrl;
			}
		}
		
		public override void InitializeCell (DataControlFieldCell cell,
			DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			base.InitializeCell (cell, cellType, rowState, rowIndex);
			if (cellType == DataControlCellType.DataCell) {
				InitializeDataCell (cell, rowState);
				if ((rowState & DataControlRowState.Insert) == 0)
					cell.DataBinding += new EventHandler (OnDataBindField);
			}
		}
		
		public virtual void InitializeDataCell (DataControlFieldCell cell, DataControlRowState rowState)
		{
			bool editable = (rowState & (DataControlRowState.Edit | DataControlRowState.Insert)) != 0;
			if (editable && !ReadOnly) {
				TextBox box = new TextBox ();
				cell.Controls.Add (box);
			} else {
				Image img = new Image ();
				cell.Controls.Add (img);
			}
		}
		
		protected virtual string FormatImageUrlValue (object value)
		{
			if (value == null || (value.ToString().Length == 0 && ConvertEmptyStringToNull))
				return NullImageUrl;
			else if (DataImageUrlFormatString.Length > 0)
				return string.Format (DataImageUrlFormatString, value);
			else
				return value.ToString ();
		}
		
		protected virtual string GetFormattedAlternateText (Control controlContainer)
		{
			if (DataAlternateTextField.Length > 0)
			{
				if (textProperty == null)
					textProperty = GetProperty (controlContainer, DataAlternateTextField);
					
				object value = GetValue (controlContainer, DataAlternateTextField, ref textProperty);
				
				if (value == null || (value.ToString().Length == 0 && ConvertEmptyStringToNull))
					return NullDisplayText;
				else if (DataAlternateTextFormatString.Length > 0)
					return string.Format (DataAlternateTextFormatString, value);
				else
					return value.ToString ();
			}
			else
				return AlternateText;
		
		}
		
		protected virtual object GetValue (Control controlContainer, string fieldName, ref PropertyDescriptor cachedDescriptor)
		{
			if (DesignMode)
				return GetDesignTimeValue ();
			else {
				if (fieldName == ThisExpression)
					return controlContainer.ToString ();
				else {
					IDataItemContainer dic = (IDataItemContainer) controlContainer;
					if (cachedDescriptor != null) return cachedDescriptor.GetValue (dic.DataItem);
					PropertyDescriptor prop = GetProperty (controlContainer, fieldName);
					return prop.GetValue (dic.DataItem);
				}
			}
		}
		
		PropertyDescriptor GetProperty (Control controlContainer, string fieldName)
		{
			IDataItemContainer dic = (IDataItemContainer) controlContainer;
			PropertyDescriptor prop = TypeDescriptor.GetProperties (dic.DataItem) [fieldName];
			if (prop == null)
				new InvalidOperationException ("Property '" + fieldName + "' not found in object of type " + dic.DataItem.GetType());
			return prop;
		}
		
		protected virtual string GetDesignTimeValue ()
		{
			return "Databound";
		}
		
		protected virtual void OnDataBindField (object sender, EventArgs e)
		{
			DataControlFieldCell cell = (DataControlFieldCell) sender;
			
			if (imageProperty == null)
				imageProperty = GetProperty (cell.BindingContainer, DataImageUrlField);
			
			Control c = cell.Controls [0];
			if (c is TextBox) {
				object val = GetValue (cell.BindingContainer, DataImageUrlField, ref imageProperty);
				((TextBox)c).Text = val != null ? val.ToString() : "";
			}
			else if (c is Image) {
				Image img = (Image)c;
				img.ImageUrl = FormatImageUrlValue (GetValue (cell.BindingContainer, DataImageUrlField, ref imageProperty));
				img.AlternateText = GetFormattedAlternateText (cell.BindingContainer);
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
#endif
