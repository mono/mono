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

namespace System.Web.UI.WebControls {

	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class BoundField : DataControlField
	{
		public static readonly string ThisExpression = "!";
		
		PropertyDescriptor boundProperty;
		
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
		public virtual string DataField {
			get {
				object ob = ViewState ["DataField"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["DataField"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		public virtual string DataFormatString {
			get {
				object ob = ViewState ["DataFormatString"];
				if (ob != null) return (string) ob;
				return "";
			}
			set {
				ViewState ["DataFormatString"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
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

		public override void ExtractValuesFromCell (IOrderedDictionary dictionary,
			DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
		}
		
		public override void InitializeCell (DataControlFieldCell cell,
			DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			base.InitializeCell (cell, cellType, rowState, rowIndex);
			if (cellType == DataControlCellType.DataCell)
				InitializeDataCell (cell, rowState);
			cell.DataBinding += new EventHandler (OnDataBindField);
		}
		
		public virtual void InitializeDataCell (DataControlFieldCell cell, DataControlRowState rowState)
		{
		}
		
		string FormatValue (object value)
		{
			if (value == null || (value.ToString().Length == 0 && ConvertEmptyStringToNull))
				return NullDisplayText;
			if (DataFormatString.Length > 0)
				return string.Format (DataFormatString, value);
			else
				return value.ToString ();
		}
		
		protected virtual object GetValue (Control controlContainer)
		{
			if (DesignMode)
				return GetDesignTimeValue (controlContainer);
			else
				return GetBoundValue (controlContainer);
		}
		
		protected virtual object GetDesignTimeValue (Control controlContainer)
		{
			return GetBoundValue (controlContainer);
		}
		
		object GetBoundValue (Control controlContainer)
		{
			if (DataField == ThisExpression)
				return controlContainer.ToString ();
			else {
				IDataItemContainer dic = controlContainer as IDataItemContainer;
				if (boundProperty == null) {
					ICustomTypeDescriptor desc = dic.DataItem as ICustomTypeDescriptor;
					if (desc != null) {
						boundProperty = desc.GetProperties () [DataField];
						if (boundProperty != null)
							return boundProperty.GetValue (dic.DataItem);
					}
					throw new InvalidOperationException ("Property '" + DataField + "' not found in data bound item");
				}
				return boundProperty.GetValue (dic.DataItem);
			}
		}
		
		protected virtual void OnDataBindField (object sender, EventArgs e)
		{
			DataControlFieldCell cell = (DataControlFieldCell) sender;
			cell.Text = FormatValue (GetValue (cell.BindingContainer));
		}
	}
}
#endif
