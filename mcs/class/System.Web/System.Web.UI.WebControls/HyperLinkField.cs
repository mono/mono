//
// System.Web.UI.WebControls.HyperLinkField.cs
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

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HyperLinkField : DataControlField
	{
		PropertyDescriptor textProperty;
		PropertyDescriptor[] urlProperties;
		static string[] emptyFields;
		
		public override bool Initialize (bool sortingEnabled, Control control)
		{
			return base.Initialize (sortingEnabled, control);
		}
		
		[EditorAttribute ("System.Web.UI.Design.WebControls.DataFieldEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[TypeConverterAttribute (typeof(StringArrayConverter))]
		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute (null)]
		public virtual string[] DataNavigateUrlFields {
			get {
				object ob = ViewState ["DataNavigateUrlFields"];
				if (ob != null)
					return (string[]) ob;
				if (emptyFields == null)
					emptyFields = new string[0];
				return emptyFields;
			}
			set {
				ViewState ["DataNavigateUrlFields"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataNavigateUrlFormatString {
			get {
				object ob = ViewState ["DataNavigateUrlFormatString"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["DataNavigateUrlFormatString"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("")]
		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		public virtual string DataTextField {
			get {
				object ob = ViewState ["DataTextField"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["DataTextField"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataTextFormatString {
			get {
				object ob = ViewState ["DataTextFormatString"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["DataTextFormatString"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		[WebCategoryAttribute ("Behavior")]
		public virtual string NavigateUrl {
			get {
				object ob = ViewState ["NavigateUrl"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["NavigateUrl"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		[TypeConverterAttribute (typeof(TargetConverter))]
		public virtual string Target {
			get {
				object ob = ViewState ["Target"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["Target"] = value;
				OnFieldChanged ();
			}
		}

		[LocalizableAttribute (true)]
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string Text {
			get {
				object ob = ViewState ["Text"];
				if (ob != null)
					return (string) ob;
				return String.Empty;
			}
			set {
				ViewState ["Text"] = value;
				OnFieldChanged ();
			}
		}
		
		public override void InitializeCell (DataControlFieldCell cell,DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			base.InitializeCell (cell, cellType, rowState, rowIndex);
			if (cellType == DataControlCellType.DataCell) {
				HyperLink link = new HyperLink ();
				bool bind = false;

				if (Target.Length > 0)
					link.Target = Target;

				if (DataTextField.Length > 0)
					bind = true;
				else
					link.Text = Text;

				string [] fields = DataNavigateUrlFields;
				if (fields.Length > 0)
					bind = true;
				else
					link.NavigateUrl = NavigateUrl;

				if (bind && cellType == DataControlCellType.DataCell && (rowState & DataControlRowState.Insert) == 0)
					cell.DataBinding += new EventHandler (OnDataBindField);

				link.ControlStyle.CopyFrom (ControlStyle);

				cell.Controls.Add (link);
			}
		}
		
		protected virtual string FormatDataNavigateUrlValue (object[] dataUrlValues)
		{
			if (dataUrlValues == null || dataUrlValues.Length == 0)
				return String.Empty;
			else if (DataNavigateUrlFormatString.Length > 0)
				return string.Format (DataNavigateUrlFormatString, dataUrlValues);
			else
				return dataUrlValues[0].ToString ();
		}
		
		protected virtual string FormatDataTextValue (object dataTextValue)
		{
			if (DataTextFormatString.Length > 0)
				return string.Format (DataTextFormatString, dataTextValue);
			else if (dataTextValue == null)
				return String.Empty;
			else
				return dataTextValue.ToString ();
		}
		
		void OnDataBindField (object sender, EventArgs e)
		{
			DataControlFieldCell cell = (DataControlFieldCell) sender;
			HyperLink link = (HyperLink) cell.Controls [0];
			object controlContainer = cell.BindingContainer;
			object item = DataBinder.GetDataItem (controlContainer);
			
			if (DataTextField.Length > 0) {
				if (textProperty == null) SetupProperties (controlContainer);
				link.Text = FormatDataTextValue (textProperty.GetValue (item));
			}
			
			string[] urlFields = DataNavigateUrlFields;
			if (urlFields.Length > 0) {
				if (urlProperties == null) SetupProperties (controlContainer);
				object[] dataUrlValues = new object [urlFields.Length];
				for (int n=0; n<dataUrlValues.Length; n++)
					dataUrlValues [n] = urlProperties [n].GetValue (item);
				link.NavigateUrl = FormatDataNavigateUrlValue (dataUrlValues);
			}
		}
		
		void SetupProperties (object controlContainer)
		{
			object item = DataBinder.GetDataItem (controlContainer);
			PropertyDescriptorCollection props = TypeDescriptor.GetProperties (item); 
			
			if (DataTextField.Length > 0) {
				textProperty = props.Find (DataTextField, true);
				if (textProperty == null)
					throw new InvalidOperationException ("Property '" + DataTextField + "' not found in object of type " + item.GetType());
			}
			
			string[] urlFields = DataNavigateUrlFields;
			if (urlFields.Length > 0) {
				urlProperties = new PropertyDescriptor [urlFields.Length];
				for (int n=0; n<urlFields.Length; n++) {
					PropertyDescriptor prop = props.Find (urlFields [n], true);
					if (prop == null)
						throw new InvalidOperationException ("Property '" + urlFields [n] + "' not found in object of type " + item.GetType());
					urlProperties [n] = prop;
				}
			}
		}
		
		protected override DataControlField CreateField ()
		{
			return new HyperLinkField ();
		}
		
		protected override void CopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
			HyperLinkField field = (HyperLinkField) newField;
			field.DataNavigateUrlFields = DataNavigateUrlFields;
			field.DataNavigateUrlFormatString = DataNavigateUrlFormatString;
			field.DataTextField = DataTextField;
			field.DataTextFormatString = DataTextFormatString;
			field.NavigateUrl = NavigateUrl;
			field.Target = Target;
			field.Text = Text;
		}
		
		public override void ValidateSupportsCallback ()
		{
		}
	}
}

