//
// System.Web.UI.WebControls.BoundField.cs
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
	public class BoundField : DataControlField
	{
		public static readonly string ThisExpression = "!";
		
		PropertyDescriptor boundProperty;

		[MonoTODO]
		[DefaultValueAttribute (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ApplyFormatInEditMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[DefaultValueAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ConvertEmptyStringToNull {
			get { return ViewState.GetBool ("ConvertEmptyStringToNull", false); }
			set {
				ViewState ["ConvertEmptyStringToNull"] = value;
				OnFieldChanged ();
			}
		}

		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("")]
		public virtual string DataField {
			get { return ViewState.GetString ("DataField", ""); }
			set {
				ViewState ["DataField"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataFormatString {
			get { return ViewState.GetString ("DataFormatString", ""); }
			set {
				ViewState ["DataFormatString"] = value;
				OnFieldChanged ();
			}
		}

		[MonoTODO]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public override string HeaderText {
			get { return ViewState.GetString ("HeaderText", "");
			}
			set {
				ViewState["HeaderText"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string NullDisplayText {
			get { return ViewState.GetString ("NullDisplaytext", ""); }
			set {
				ViewState ["NullDisplayText"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool ReadOnly {
			get { return ViewState.GetBool ("ReadOnly", false); }
			set { 
				ViewState ["ReadOnly"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("HtmlEncode")]
		public virtual bool HtmlEncode {
			get { return ViewState.GetBool ("ReadOnly", true); }
			set { 
				ViewState ["HtmlEncode"] = true;
				OnFieldChanged ();
			}
		}

		public override void ExtractValuesFromCell (IOrderedDictionary dictionary,
			DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			bool editable = (rowState & (DataControlRowState.Edit | DataControlRowState.Insert)) != 0;
			if (editable && !ReadOnly) {
				if (cell.Controls.Count > 0) {
					TextBox box = (TextBox) cell.Controls [0];
					dictionary [DataField] = box.Text;
				}
			} else if (includeReadOnly) {
				dictionary [DataField] = cell.Text;
			}
		}

		[MonoTODO]
		public override bool Initialize (bool enableSorting, 
						 Control control)
		{
			return base.Initialize (enableSorting, control);
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
		
		protected virtual void InitializeDataCell (DataControlFieldCell cell, DataControlRowState rowState)
		{
			bool editable = (rowState & (DataControlRowState.Edit | DataControlRowState.Insert)) != 0;
			if (editable && !ReadOnly) {
				TextBox box = new TextBox ();
				cell.Controls.Add (box);
			}
		}
		
		protected virtual bool SupportsHtmlEncode {
			get { return true; }
		}
		
		protected virtual string FormatDataValue (object value, bool encode)
		{
			string res;
			if (value == null || (value.ToString().Length == 0 && ConvertEmptyStringToNull))
				res = NullDisplayText;
			else if (DataFormatString.Length > 0)
				res = string.Format (DataFormatString, value);
			else
				res = value.ToString ();
				
			if (encode) return HttpUtility.HtmlEncode (res);
			else return res;
		}
		
		protected virtual object GetValue (Control controlContainer)
		{
			if (DesignMode)
				return GetDesignTimeValue ();
			else
				return GetBoundValue (controlContainer);
		}
		
		protected virtual object GetDesignTimeValue ()
		{
			return GetBoundValue (Control);
		}
		
		object GetBoundValue (Control controlContainer)
		{
			if (DataField == ThisExpression)
				return controlContainer.ToString ();
			else {
				IDataItemContainer dic = (IDataItemContainer) controlContainer;
				if (boundProperty == null) {
					boundProperty = TypeDescriptor.GetProperties (dic.DataItem) [DataField];
					if (boundProperty == null)
						new InvalidOperationException ("Property '" + DataField + "' not found in object of type " + dic.DataItem.GetType());
				}
				return boundProperty.GetValue (dic.DataItem);
			}
		}
		
		protected virtual void OnDataBindField (object sender, EventArgs e)
		{
			DataControlFieldCell cell = (DataControlFieldCell) sender;
			if (cell.Controls.Count > 0) {
				TextBox box = (TextBox) cell.Controls [0];
				object val = GetValue (cell.BindingContainer);
				box.Text = val != null ? val.ToString() : "";
			}
			else
				cell.Text = FormatDataValue (GetValue (cell.BindingContainer), SupportsHtmlEncode && HtmlEncode);
		}
		
		protected override DataControlField CreateField ()
		{
			return new BoundField ();
		}
		
		protected override void CopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
			BoundField field = (BoundField) newField;
			field.ConvertEmptyStringToNull = ConvertEmptyStringToNull;
			field.DataField = DataField;
			field.DataFormatString = DataFormatString;
			field.NullDisplayText = NullDisplayText;
			field.ReadOnly = ReadOnly;
			field.HtmlEncode = HtmlEncode;
		}

		[MonoTODO]
		public override void ValidateSupportsCallback ()
		{
			throw new NotImplementedException ();
		}

	}
}
#endif
