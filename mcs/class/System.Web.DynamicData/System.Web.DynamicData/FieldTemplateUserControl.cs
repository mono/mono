//
// FieldTemplateUserControl.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Marek Habersack <mhabersack@novell.com>
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
	public class FieldTemplateUserControl : UserControl, IBindableControl, IFieldTemplate
	{
		public MetaChildrenColumn ChildrenColumn {
			get {
				MetaColumn column = Column;
				var ret = column as MetaChildrenColumn;
				if (ret == null) {
					string name = column == null ? null : column.Name;
					throw new Exception ("'" + name + "' is not a children column and cannot be used here.");
				}
				
				return ret;
			}
		}		

		[MonoTODO]
		protected string ChildrenPath {
			get { return ChildrenColumn.GetChildrenListPath (Row); }
			
		}

		public MetaColumn Column {
			get {
				IFieldTemplateHost host = Host;
				if (host != null)
					return host.Column;

				return null;
			}
		}
		
		[MonoTODO]
		public virtual Control DataControl { get; private set; }
		[MonoTODO]
		public virtual object FieldValue { get; set; }
		[MonoTODO]
		public virtual string FieldValueEditString { get; private set; }
		[MonoTODO]
		public virtual string FieldValueString { get; private set; }

		[MonoTODO]
		public MetaForeignKeyColumn ForeignKeyColumn {
			get {
				MetaColumn column = Column;
				var ret = column as MetaForeignKeyColumn;
				if (ret == null) {
					string name = column == null ? null : column.Name;
					throw new Exception ("'" + name + "' is not a foreign key column and cannot be used here.");
				}
				
				return ret;
			}
		}
		
		[MonoTODO]
		protected string ForeignKeyPath {
			get { return ForeignKeyColumn.GetForeignKeyDetailsPath (Row); }
		}
		
		[MonoTODO]
		public IFieldFormattingOptions FormattingOptions { get; private set; }

		public IFieldTemplateHost Host { get; private set; }

		[MonoTODO]
		public System.ComponentModel.AttributeCollection MetadataAttributes {
			get {
				MetaColumn column = Column;
				if (column == null)
					return null;

				return column.Attributes;
			}
		}
		
		[MonoTODO]
		public DataBoundControlMode Mode {
			get {
				IFieldTemplateHost host = Host;			
				return host == null ? DataBoundControlMode.ReadOnly : host.Mode;
			}
		}
		
		[MonoTODO]
		public virtual object Row {
			get {
				Page page = Page;
				return page == null ? null : page.GetDataItem ();
			}
		}
		
		[MonoTODO]
		public MetaTable Table {
			get {
				MetaColumn column = Column;
				return column == null ? null : column.Table;
			}
		}

		[MonoTODO]
		protected string BuildChildrenPath (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected string BuildForeignKeyPath (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual object ConvertEditedValue (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void ExtractForeignKey (IDictionary dictionary, string selectedValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void ExtractValues (IOrderedDictionary dictionary)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected FieldTemplateUserControl FindOtherFieldTemplate (string columnName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string FormatFieldValue (object fieldValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual object GetColumnValue (MetaColumn column)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IBindableControl.ExtractValues (IOrderedDictionary dictionary)
		{
			ExtractValues (dictionary);
		}

		void IFieldTemplate.SetHost (IFieldTemplateHost host)
		{
			Host = host;
		}

		[MonoTODO]
		protected void IgnoreModelValidationAttribute (Type attributeType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void PopulateListControl (ListControl listControl)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void SetUpValidator (BaseValidator validator)
		{
		}

		[MonoTODO]
		protected virtual void SetUpValidator (BaseValidator validator, MetaColumn column)
		{
			throw new NotImplementedException ();
		}
	}
}
