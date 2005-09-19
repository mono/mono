//
// System.Web.UI.WebControls.BoundColumn.cs
//
// Author:
//      Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;

namespace System.Web.UI.WebControls {
	public class BoundColumn : DataGridColumn
	{

		private string data_format_string;

		public BoundColumn ()
		{
		}

		public static readonly string thisExpr = "!";

		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string DataField 
		{
			get {
				return ViewState.GetString ("DataField", String.Empty);
			}
			set {
				ViewState ["DataField"] = value;
			}
		}
		
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string DataFormatString 
		{
			get {
				return ViewState.GetString ("DataFormatString", String.Empty);
			}
			set {
				ViewState ["DataFormatString"] = value;
			}
		}

		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual bool ReadOnly 
		{
			get {
				return ViewState.GetBool ("ReadOnly", false);
			}
			set {
				ViewState ["ReadOnly"] = value;
			}
		}
		
		public override void Initialize ()
		{
			data_format_string = DataFormatString;
		}

		public override void InitializeCell (TableCell cell, int columnIndex,
				ListItemType itemType)
		{
			base.InitializeCell (cell, columnIndex, itemType);

			switch (itemType) {
			case ListItemType.Item:
			case ListItemType.EditItem:
			case ListItemType.AlternatingItem:
				cell.DataBinding += new EventHandler (ItemDataBinding);
				break;
			}
		}

		protected virtual string FormatDataValue (object dataValue)
		{
			if (dataValue == null)
				return "";

			if (data_format_string == String.Empty)
				return dataValue.ToString ();

			return String.Format (data_format_string, dataValue);
		}

		private void ItemDataBinding (object sender, EventArgs e)
		{
			TableCell cell = (TableCell) sender;
			DataGridItem item  = (DataGridItem) cell.NamingContainer;
			object value = null;

			if (DataField != thisExpr) {
				value = DataBinder.Eval (item.DataItem, DataField);
			} else {
				value = item.DataItem;
			}

			string text = FormatDataValue (value);
			if (text == String.Empty)
				text = "&nbsp;";
			cell.Text = text;
		}
	}
}
