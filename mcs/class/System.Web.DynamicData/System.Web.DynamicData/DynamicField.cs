//
// DynamicField.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2009 Novell Inc. http://novell.com
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DynamicField : DataControlField, IAttributeAccessor, IFieldFormattingOptions
	{
		MetaColumn myColumn;
		Dictionary <string, string> attributes;
		
		public DynamicField ()
		{
			DataFormatString = String.Empty;
			HtmlEncode = true;
			NullDisplayText = String.Empty;
		}
		
		public bool ApplyFormatInEditMode {
			get; set;
		}

		public bool ConvertEmptyStringToNull {
			get; set;
		}

		public virtual string DataField {
			get {
				return (string) ViewState ["_DataField"] ?? String.Empty;
			}
			set {
				ViewState ["_DataField"] = value;
				OnFieldChanged ();
			}
		}		

		public string DataFormatString {
			get; set;
		}

		public override string HeaderText {
			get {
				string s = (string) ViewState ["headerText"];
				if (s != null)
					return s;

				MetaColumn column = MyColumn;
				if (column != null)
					return column.DisplayName;
				
				return DataField;
			}
			
			set { base.HeaderText = value; }
		}		

		public bool HtmlEncode {
			get; set;
		}

		MetaColumn MyColumn {
			get {
				if (myColumn != null)
					return myColumn;
				Control owner = Control;
				if (owner == null)
					return null;
				
				MetaTable table = owner.FindMetaTable ();
				if (table == null)
					return null;

				myColumn = table.GetColumn (DataField);
				return myColumn;
			}
		}
		
		public string NullDisplayText {
			get; set;
		}

		public override string SortExpression {
			get {
				string s = (string) ViewState ["sortExpression"];
				if (s != null)
					return s;

				MetaColumn column = MyColumn;
				if (column != null)
					return column.SortExpression;

				return String.Empty;
			}
			
			set { base.SortExpression = value; }
		}		

		public virtual string UIHint {
			get {
				return (string) ViewState ["uiHint"] ?? String.Empty;
			}
			
			set {
				ViewState ["uiHint"] = value;
				OnFieldChanged ();
			}
		}

		[MonoTODO]
		protected override void CopyProperties (DataControlField newField)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DataControlField CreateField ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void ExtractValuesFromCell (IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			throw new NotImplementedException ();
		}

		public string GetAttribute (string key)
		{
			if (attributes == null)
				return null;

			string ret;
			if (attributes.TryGetValue (key, out ret))
				return ret;

			return null;
		}

		public override void InitializeCell (DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			if (cellType == DataControlCellType.Header || cellType == DataControlCellType.Footer) {
				base.InitializeCell (cell, cellType, rowState, rowIndex);
				return;
			}

			DynamicControl dc = new DynamicControl ();
			dc.ApplyFormatInEditMode = ApplyFormatInEditMode;
			dc.ConvertEmptyStringToNull = ConvertEmptyStringToNull;
			dc.Column = MyColumn;
			dc.DataField = DataField;
			dc.DataFormatString = DataFormatString;
			dc.HtmlEncode = HtmlEncode;
			dc.Mode = (rowState & DataControlRowState.Edit) != 0 ? DataBoundControlMode.Edit :
				(rowState & DataControlRowState.Insert) != 0 ? DataBoundControlMode.Insert : DataBoundControlMode.ReadOnly;
			dc.NullDisplayText = NullDisplayText;
			dc.UIHint = UIHint;
			dc.InternalSetAttributes (attributes);
			
			cell.Controls.Add (dc);
		}

		public void SetAttribute (string key, string value)
		{
			if (attributes == null)
				attributes = new Dictionary <string, string> ();

			if (attributes.ContainsKey (key))
				attributes [key] = value;
			else
				attributes.Add (key, value);
		}
	}
}
