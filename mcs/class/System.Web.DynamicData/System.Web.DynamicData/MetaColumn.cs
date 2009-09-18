//
// MetaColumn.cs
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class MetaColumn : IFieldFormattingOptions
	{
		// (Int32.MaxValue / 2) - 5
		const int SHORT_STRING_MAX_LENGTH = 0x3ffffffa;

		bool? scaffold;
		bool? scaffoldReflected;
		bool? applyFormatInEditMode;
		bool? convertEmptyStringToNull;
		bool dataTypeReflected;
		bool defaultValueReflected;
		bool descriptionReflected;
		bool requiredReflected;
		bool uiHintReflected;
		
		string dataFormatString;
		object defaultValue;
		string description;
		string displayName;
		bool? readOnly;
		int? maxLength;
		string nullDisplayText;
		string requiredErrorMessage;
		string uiHint;
		
		// Attributes
		AttributeCollection attributes;
		
		DisplayFormatAttribute displayFormatAttr;
		DataTypeAttribute dataTypeAttr;
		ScaffoldColumnAttribute scaffoldAttr;
		RequiredAttribute requiredAttr;
		
		internal MetaColumn (MetaTable table, ColumnProvider provider)
		{
			Table = table;
			Provider = provider;
			Model = table.Model;
			HtmlEncode = true;

			Type columnType = ColumnType;
			TypeCode code = Type.GetTypeCode (columnType);
			TypeCode = code;
			switch (code) {
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					IsFloatingPoint = true;
					break;

				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
					IsInteger = true;
					break;

				case TypeCode.String:
					IsString = true;
					break;

				case TypeCode.Object:
					// So far only byte[] seems to be treated as a binary type
					if (columnType.IsArray && columnType.GetArrayRank () == 1 && columnType.GetElementType () == typeof (byte))
						IsBinaryData = true;
					break;

				default:
					TypeCode = TypeCode.Object;
					break;
			}

			IsLongString = MaxLength > SHORT_STRING_MAX_LENGTH;
		}

		public bool ApplyFormatInEditMode {
			get {
				if (applyFormatInEditMode == null)
					applyFormatInEditMode = CheckApplyFormatInEditMode ();

				return (bool)applyFormatInEditMode;
			}
		}

		public AttributeCollection Attributes {
			get {
				if (attributes == null)
					attributes = LoadAttributes ();
				
				return attributes;
			}
			
		}

		public Type ColumnType {
			get { return Provider.ColumnType; }
		}

		public bool ConvertEmptyStringToNull {
			get {
				if (convertEmptyStringToNull == null)
					convertEmptyStringToNull = CheckConvertEmptyStringToNull ();

				return (bool)convertEmptyStringToNull;
			}
			
		}

		public string DataFormatString {
			get {
				if (dataFormatString == null)
					dataFormatString = CheckDataFormatString ();

				return dataFormatString;
			}
		}

		public DataTypeAttribute DataTypeAttribute {
			get {
				if (!dataTypeReflected && dataTypeAttr == null)
					dataTypeAttr = CheckDataTypeAttribute ();
					
				return dataTypeAttr;
			}
			
		}

		public Object DefaultValue {
			get {
				if (!defaultValueReflected && defaultValue == null) {
					DefaultValueAttribute defaultValueAttr = CheckDefaultValueAttribute ();
					if (defaultValueAttr != null)
						defaultValue = defaultValueAttr.Value;
				}
				
				return defaultValue;
			}
		}

		public string Description {
			get {
				if (!descriptionReflected && description == null) {
					DescriptionAttribute descriptionAttr = CheckDescriptionAttribute ();
					if (descriptionAttr != null)
						description = descriptionAttr.Description;
				}

				return description;
			}
		}

		public string DisplayName {
			get {
				if (displayName == null)
					displayName = CheckDisplayName ();

				return displayName;
			}
		}

		public PropertyInfo EntityTypeProperty {
			get { return Provider.EntityTypeProperty; }
		}

		public bool HtmlEncode { get; private set; }

		public bool IsBinaryData { get; private set; }

		public bool IsCustomProperty {
			get { return Provider.IsCustomProperty; }
		}

		public bool IsFloatingPoint { get; private set; }

		public bool IsForeignKeyComponent {
			get { return Provider.IsForeignKeyComponent; }
		}

		public bool IsGenerated {
			get { return Provider.IsGenerated; }
		}

		public bool IsInteger { get; private set; }

		public bool IsLongString { get; private set; }

		public bool IsPrimaryKey {
			get { return Provider.IsPrimaryKey; }
		}

		public bool IsReadOnly {
			get {
				if (readOnly == null)
					readOnly = CheckReadOnlyAttribute ();

				return (bool)readOnly;
			}
		}

		// It appears that all columns are required unless Provider.Nullable is true for
		// them. We could skip checking for the RequiredAttribute for that reason, but that
		// way we wouldn't be forward-compatible.
		// What's more, it appears that a RequiredAttribute instance is always included in
		// Attributes, whether or not the corresponding field is decorataed with it.
		public bool IsRequired {
			get {
				if (!requiredReflected && requiredAttr == null)
					requiredAttr = CheckRequiredAttribute ();

				return requiredAttr != null;
			}
		}

		public bool IsString { get; private set; }

		public int MaxLength {
			get {
				if (maxLength == null)
					maxLength = CheckMaxLength ();

				return (int)maxLength;
			}
		}

		public MetaModel Model { get; private set; }

		public string Name {
			get { return Provider.Name; }
		}

		public string NullDisplayText {
			get {
				if (nullDisplayText == null)
					nullDisplayText = CheckNullDisplayText ();

				return nullDisplayText;
			}
		}

		public ColumnProvider Provider { get; private set; }

		public string RequiredErrorMessage {
			get {
				if (requiredErrorMessage == null) {
					RequiredAttribute attr = CheckRequiredAttribute ();
					if (attr == null)
						requiredErrorMessage = String.Empty;
					else
						requiredErrorMessage = attr.ErrorMessage;
				}

				return requiredErrorMessage;
			}
		}

		public bool Scaffold {
			get {
				if (scaffold != null)
					return (bool)scaffold;
				if (scaffoldReflected != null)
					return (bool)scaffoldReflected;				

				MetaModel.GetDataFieldAttribute <ScaffoldColumnAttribute> (Attributes, ref scaffoldAttr);
				if (scaffoldAttr != null) {
					scaffoldReflected = scaffoldAttr.Scaffold;
					return (bool)scaffoldReflected;
				}

				string uiHint = UIHint;
				if (!String.IsNullOrEmpty (uiHint))
					scaffoldReflected = true;
				// LAMESPEC: IsForeignKeyComponent does NOT set Scaffold=false
				else if (IsGenerated || IsCustomProperty)
					scaffoldReflected = false;
				else if (Table.ScaffoldAllTables)
					scaffoldReflected = true;
				else
					scaffoldReflected = true;

				return (bool)scaffoldReflected;
			}
			
			set { scaffold = value; }
		}

		public string SortExpression {
			get {
				ColumnProvider provider = Provider;
				if (provider.IsSortable)
					return Name;

				return String.Empty;
			}
		}

		public MetaTable Table { get; private set; }

		public TypeCode TypeCode { get; private set; }

		// LAMESPEC: if there's no attribute, null is returned
		public string UIHint {
			get {
				if (!uiHintReflected && uiHint == null)
					uiHint = CheckUIHintAttribute ();
				
				return uiHint;
			}
		}

		string CheckUIHintAttribute ()
		{
			if (uiHintReflected)
				return uiHint;

			uiHintReflected = true;
			UIHintAttribute attr = null;
			MetaModel.GetDataFieldAttribute <UIHintAttribute> (Attributes, ref attr);

			if (attr == null)
				return null;

			return attr.UIHint;
		}
		
		bool CheckApplyFormatInEditMode ()
		{
			var displayFormat = GetDisplayFormat ();
			if (displayFormat == null)
				return false;

			return displayFormat.ApplyFormatInEditMode;
		}

		bool CheckConvertEmptyStringToNull ()
		{
			var displayFormat = GetDisplayFormat ();
			if (displayFormat == null)
				return true;

			return displayFormat.ConvertEmptyStringToNull;
		}

		string CheckDataFormatString ()
		{
			var displayFormat = GetDisplayFormat ();
			if (displayFormat == null)
				return String.Empty;

			return displayFormat.DataFormatString;
		}

		DataTypeAttribute CheckDataTypeAttribute ()
		{
			if (dataTypeReflected)
				return dataTypeAttr;

			dataTypeReflected = true;
			MetaModel.GetDataFieldAttribute <DataTypeAttribute> (Attributes, ref dataTypeAttr);
			if (dataTypeAttr == null && (ColumnType == typeof (string)))
				return new DataTypeAttribute (IsLongString ? DataType.MultilineText : DataType.Text);

			return dataTypeAttr;
		}

		DefaultValueAttribute CheckDefaultValueAttribute ()
		{
			defaultValueReflected = true;
			DefaultValueAttribute dummy = null;
			MetaModel.GetDataFieldAttribute <DefaultValueAttribute> (Attributes, ref dummy);
			if (dummy == null)
				return null;

			return dummy;
		}

		DescriptionAttribute CheckDescriptionAttribute ()
		{
			descriptionReflected = true;
			DescriptionAttribute dummy = null;
			MetaModel.GetDataFieldAttribute <DescriptionAttribute> (Attributes, ref dummy);
			if (dummy == null)
				return null;
			
			return dummy;
		}

		string CheckDisplayName ()
		{
			DisplayNameAttribute attr = null;
			MetaModel.GetDataFieldAttribute <DisplayNameAttribute> (Attributes, ref attr);
			if (attr != null)
				return attr.DisplayName;

			return Name;
		}

		RequiredAttribute CheckRequiredAttribute ()
		{
			if (requiredReflected)
				return requiredAttr;

			requiredReflected = true;
			MetaModel.GetDataFieldAttribute <RequiredAttribute> (Attributes, ref requiredAttr);

			return requiredAttr;
		}

		bool CheckReadOnlyAttribute ()
		{
			ReadOnlyAttribute attr = null;
			MetaModel.GetDataFieldAttribute <ReadOnlyAttribute> (Attributes, ref attr);

			// Apparently attr.IsReadOnly and/or comparisons to
			// ReadOnlyAttribute.{Yes,No} don't matter. The sole presence of the
			// attribute marks column as read-only
			return attr != null;
		}

		int CheckMaxLength ()
		{
			StringLengthAttribute attr = null;
			MetaModel.GetDataFieldAttribute <StringLengthAttribute> (Attributes, ref attr);

			if (attr != null)
				return attr.MaximumLength;
			
			return Provider.MaxLength;
		}

		string CheckNullDisplayText ()
		{
			DisplayFormatAttribute displayFormat = GetDisplayFormat ();

			if (displayFormat == null)
				return String.Empty;

			return displayFormat.NullDisplayText;
		}

		DisplayFormatAttribute GetDisplayFormat ()
		{
			MetaModel.GetDataFieldAttribute <DisplayFormatAttribute> (Attributes, ref displayFormatAttr);
			if (displayFormatAttr == null) {
				var dta = DataTypeAttribute;
				displayFormatAttr = dta == null ? null : dta.DisplayFormat;
			}
			
			return displayFormatAttr;
		}

		internal virtual void Init ()
		{
		}
		
		AttributeCollection LoadAttributes ()
		{
			var props = MetaModel.GetTypeDescriptor (Table.EntityType).GetProperties ();
			AttributeCollection reflected;

			int propsCount = props == null ? 0 : props.Count;
			if (propsCount == 0)
				reflected = AttributeCollection.Empty;
			else {
				var property = props.Find (Name, true);
				if (property == null)
					reflected = AttributeCollection.Empty;
				else
					reflected = property.Attributes;
			}

			if (!Provider.Nullable && reflected.OfType <RequiredAttribute> ().Count () == 0)
				reflected = AttributeCollection.FromExisting (reflected, new Attribute[] { new RequiredAttribute () });
			
			return reflected;
		}
		
 		public override string ToString ()
		{
			return Name;
		}
	}
}
