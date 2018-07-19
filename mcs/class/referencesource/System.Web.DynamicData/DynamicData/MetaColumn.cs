using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Spatial;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.DynamicData.ModelProviders;
using System.Web.DynamicData.Util;

namespace System.Web.DynamicData {
    /// <summary>
    /// Object that represents a database column used by dynamic data
    /// </summary>
    public class MetaColumn : IFieldFormattingOptions, IMetaColumn {
        private TypeCode _typeCode = TypeCode.Empty;
        private Type _type;
        private object _metadataCacheLock = new object();

        // text, ntext, varchar(max), nvarchar(max) all have different maximum lengths so this is a minimum value
        // that ensures that all of the abovementioned columns get treated as long strings.
        private static readonly int s_longStringLengthCutoff = ((Int32.MaxValue >> 1) - 4);

        // Metadata related members
        private IMetaColumnMetadata _metadata;
        private bool? _scaffoldValueManual;
        private bool? _scaffoldValueDefault;

        public MetaColumn(MetaTable table, ColumnProvider columnProvider) {
            Table = table;
            Provider = columnProvider;
        }

        /// <summary>
        /// The collection of metadata attributes that apply to this column
        /// </summary>
        public AttributeCollection Attributes {
            get {
                return Metadata.Attributes;
            }
        }

        /// <summary>
        /// The CLR type of the property/column
        /// </summary>
        public Type ColumnType {
            get {
                if (_type == null) {
                    // If it's an Nullable<T>, work with T instead
                    _type = Misc.RemoveNullableFromType(Provider.ColumnType);
                }

                return _type;
            }
        }

        /// <summary>
        ///  The DataTypeAttribute used for the column
        /// </summary>
        public DataTypeAttribute DataTypeAttribute {
            get {
                return Metadata.DataTypeAttribute;
            }
        }

        /// <summary>
        /// This column's defalut value. It is typically used to populate the field when creating a new entry.
        /// </summary>
        public object DefaultValue {
            get {
                return Metadata.DefaultValue;
            }
        }

        /// <summary>
        /// A description for this column
        /// </summary>
        public virtual string Description {
            get {
                // 
                return Metadata.Description;
            }
        }

        /// <summary>
        /// A friendly display name for this column
        /// </summary>
        public virtual string DisplayName {
            get {
                // Default to the Name if there is no DisplayName
                return Metadata.DisplayName ?? Name;
            }
        }

        /// <summary>
        /// The PropertyInfo of the property that represents this column on the entity type
        /// </summary>
        public PropertyInfo EntityTypeProperty { get { return Provider.EntityTypeProperty; } }

        /// <summary>
        /// The FilterUIHint used for the column
        /// </summary>
        public string FilterUIHint {
            get {
                return Metadata.FilterUIHint;
            }
        }

        /// <summary>
        /// Does this column contain binary data
        /// </summary>
        public bool IsBinaryData {
            get {
                return ColumnType == typeof(byte[]);
            }
        }

        /// <summary>
        /// meant to indicate that a member is an extra property that was declared in a partial class 
        /// </summary>
        public bool IsCustomProperty { get { return Provider.IsCustomProperty; } }

        /// <summary>
        /// Is this column a floating point type (float, double, decimal)
        /// </summary>
        public bool IsFloatingPoint {
            get {
                return ColumnType == typeof(float) || ColumnType == typeof(double) || ColumnType == typeof(decimal);
            }
        }

        /// <summary>
        /// This is set for columns that are part of a foreign key. Note that it is NOT set for
        /// the strongly typed entity ref columns (though those columns 'use' one or more columns
        /// where IsForeignKeyComponent is set).
        /// </summary>
        public bool IsForeignKeyComponent {
            get { return Provider.IsForeignKeyComponent; }
        }

        /// <summary>
        /// Is this column's value auto-generated in the database
        /// </summary>
        public bool IsGenerated { get { return Provider.IsGenerated; } }

        /// <summary>
        /// Is this column a integer
        /// </summary>
        public bool IsInteger {
            get {
                return ColumnType == typeof(byte) || ColumnType == typeof(short) || ColumnType == typeof(int) || ColumnType == typeof(long);
            }
        }

        /// <summary>
        /// Is this column a 'long' string. This is used to determine whether a textbox or textarea should be used.
        /// </summary>
        public bool IsLongString {
            get {
                return IsString && Provider.MaxLength >= s_longStringLengthCutoff;
            }
        }

        /// <summary>
        /// Is this column part if the table's primary key
        /// </summary>
        public bool IsPrimaryKey { get { return Provider.IsPrimaryKey; } }

        /// <summary>
        /// Is this a readonly column
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")]
        public virtual bool IsReadOnly {
            get {
                return Provider.IsReadOnly || Metadata.IsReadOnly ||
                    (Metadata.EditableAttribute != null && !Metadata.EditableAttribute.AllowEdit);
            }
        }

        /// <summary>
        /// Specifies if a read-only column (see IsReadOnly) allows for a value to be set on insert.
        /// The default value is false when the column is read-only; true when the column is not read-only.
        /// The default value can be override by using EditableAttribute (note that this will indicate that
        /// the column is meant to be read only).
        /// </summary>
        public bool AllowInitialValue {
            get {
                if (IsGenerated) {
                    // always return false for generated columns, since that is a stronger statement.
                    return false;
                }

                return Metadata.EditableAttribute.GetPropertyValue(a => a.AllowInitialValue, !IsReadOnly);
            }
        }

        /// <summary>
        /// Does this column require a value
        /// </summary>
        public bool IsRequired {
            get {
                return Metadata.RequiredAttribute != null;
            }
        }

        /// <summary>
        /// Is this column a string
        /// </summary>
        public bool IsString {
            get {
                return ColumnType == typeof(string);
            }
        }

        /// <summary>
        /// The maximun length allowed for this column (applies to string columns)
        /// </summary>
        public int MaxLength {
            get {
                var stringLengthAttribute = Metadata.StringLengthAttribute;
                return stringLengthAttribute != null ? stringLengthAttribute.MaximumLength : Provider.MaxLength;
            }
        }

        /// <summary>
        /// The MetaModel that this column belongs to
        /// </summary>
        public MetaModel Model { get { return Table.Model; } }

        /// <summary>
        /// The column's name
        /// </summary>
        public string Name { get { return Provider.Name; } }

        /// <summary>
        /// A value that can be used as a watermark in UI bound to value represented by this column.
        /// </summary>
        public virtual string Prompt { get { return Metadata.Prompt; } }

        /// <summary>
        /// the abstraction provider object that was used to construct this metacolumn.
        /// </summary>
        public ColumnProvider Provider { get; private set; }

        /// <summary>
        /// The error message used if this column is required and it is set to empty
        /// </summary>
        public string RequiredErrorMessage {
            get {
                var requiredAttribute = Metadata.RequiredAttribute;
                return requiredAttribute != null ? 
                    StringLocalizerUtil.GetLocalizedString(requiredAttribute, DisplayName) : String.Empty;
            }
        }

        /// <summary>
        /// Is it a column that should be displayed (e.g. in a GridView's auto generate mode) 
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")]
        public virtual bool Scaffold {
            get {
                // If the value was explicitely set, that always takes precedence
                if (_scaffoldValueManual != null) {
                    return _scaffoldValueManual.Value;
                }

                // If there is a DisplayAttribute with an explicit value, always honor it
                var displayAttribute = Metadata.DisplayAttribute;
                if (displayAttribute != null && displayAttribute.GetAutoGenerateField().HasValue) {
                    return displayAttribute.GetAutoGenerateField().Value;
                }

                // If there is an explicit Scaffold attribute, always honor it
                var scaffoldAttribute = Metadata.ScaffoldColumnAttribute;
                if (scaffoldAttribute != null) {
                    return scaffoldAttribute.Scaffold;
                }

                if (_scaffoldValueDefault == null) {
                    _scaffoldValueDefault = ScaffoldNoCache;
                }

                return _scaffoldValueDefault.Value;
            }
            set {
                _scaffoldValueManual = value;
            }
        }

        /// <summary>
        /// Look at various pieces of data on the column to determine whether it's
        /// Scaffold mode should be on.  This only gets called once per column and the result
        /// is cached
        /// </summary>
        internal virtual bool ScaffoldNoCache {
            get {
                // Any field with a UIHint should be included
                if (!String.IsNullOrEmpty(UIHint)) return true;

                // Skip columns that are part of a foreign key, since they are already 'covered' in the
                // strongly typed foreign key column
                if (IsForeignKeyComponent) return false;

                // Skip generated columns, which are not typically interesting
                if (IsGenerated) return false;

                // Always include non-generated primary keys
                if (IsPrimaryKey) return true;

                // Skip custom properties
                if (IsCustomProperty) return false;

                // Include strings and characters
                if (IsString) return true;
                if (ColumnType == typeof(char)) return true;

                // Include numbers
                if (IsInteger) return true;
                if (IsFloatingPoint) return true;

                // Include date related columns
                if (ColumnType == typeof(DateTime)) return true;
                if (ColumnType == typeof(TimeSpan)) return true;
                if (ColumnType == typeof(DateTimeOffset)) return true;


                // Include bools
                if (ColumnType == typeof(bool)) return true;

                // Include enums
                Type enumType;
                if (this.IsEnumType(out enumType)) return true;

                //Include spatial types
                if (ColumnType == typeof(DbGeography)) return true;
                if (ColumnType == typeof(DbGeometry)) return true;

                return false;
            }
        }

        /// <summary>
        /// A friendly short display name for this column. Meant to be used in GridView and similar controls where there might be
        /// limited column header space
        /// </summary>
        public virtual string ShortDisplayName {
            get {
                // Default to the DisplayName if there is no ShortDisplayName
                return Metadata.ShortDisplayName ?? DisplayName;
            }
        }

        /// <summary>
        /// The expression used to determine the sort order for this column
        /// </summary>
        public string SortExpression {
            get {
                return SortExpressionInternal;
            }
        }

        internal virtual string SortExpressionInternal {
            get {
                return Provider.IsSortable ? Name : string.Empty;
            }
        }

        /// <summary>
        /// The MetaTable that this column belongs to
        /// </summary>
        public MetaTable Table { get; private set; }

        /// <summary>
        /// The TypeCode of this column. It is derived from the ColumnType
        /// </summary>
        public TypeCode TypeCode {
            get {
                if (_typeCode == TypeCode.Empty) {
                    _typeCode = DataSourceUtil.TypeCodeFromType(ColumnType);
                }

                return _typeCode;
            }
        }

        /// <summary>
        ///  The UIHint used for the column
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")]
        public virtual string UIHint {
            get {
                return Metadata.UIHint;
            }
        }

        #region IFieldFormattingOptions Members

        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        public bool ApplyFormatInEditMode {
            get {
                return Metadata.ApplyFormatInEditMode;
            }
        }

        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        public bool ConvertEmptyStringToNull {
            get {
                return Metadata.ConvertEmptyStringToNull;
            }
        }

        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        public string DataFormatString {
            get {
                return Metadata.DataFormatString;
            }
        }

        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        public bool HtmlEncode {
            get { 
                return Metadata.HtmlEncode; 
            }
        }

        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        public string NullDisplayText {
            get {
                return Metadata.NullDisplayText;
            }
        }

        #endregion

        /// <summary>
        /// Build the attribute collection, later made available through the Attributes property
        /// </summary>
        protected virtual AttributeCollection BuildAttributeCollection() {
            return Provider.Attributes;
        }

        /// <summary>
        /// Perform initialization logic for this column
        /// </summary>
        internal protected virtual void Initialize() { }

        /// <summary>
        /// Resets cached column metadata (i.e. information coming from attributes). The metadata cache will be rebuilt
        /// the next time any metadata-derived information gets requested.
        /// </summary>
        public void ResetMetadata() {
            _metadata = null;
        }

        /// <summary>
        /// Shows the column name. Mostly for debugging purpose.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString() {
            return GetType().Name + " " + Name;
        }

        internal IMetaColumnMetadata Metadata {
            get {
                // Use a local to avoid returning null if ResetMetadata is called
                IMetaColumnMetadata metadata = _metadata;
                if (metadata == null) {
                    metadata = new MetaColumnMetadata(this);
                    _metadata = metadata;
                }
                return metadata;
            }
            set {
                // settable for unit testing
                _metadata = value;
            }
        }

        #region Metadata abstraction

        internal interface IMetaColumnMetadata {

            AttributeCollection Attributes { get; }

            DisplayAttribute DisplayAttribute { get; }

            bool ApplyFormatInEditMode { get; }

            bool ConvertEmptyStringToNull { get; }

            bool HtmlEncode { get; }

            string DataFormatString { get; }

            DataTypeAttribute DataTypeAttribute { get; }

            object DefaultValue { get; }

            string Description { get; }

            string DisplayName { get; }

            string FilterUIHint { get; }

            string ShortDisplayName { get; }

            string NullDisplayText { get; }

            string Prompt { get; }

            RequiredAttribute RequiredAttribute { get; }

            ScaffoldColumnAttribute ScaffoldColumnAttribute { get; }

            StringLengthAttribute StringLengthAttribute { get; }

            string UIHint { get; }

            bool IsReadOnly { get; }

            EditableAttribute EditableAttribute { get; }
        }

        internal class MetaColumnMetadata : IMetaColumnMetadata {
            private MetaColumn Column { get; set; }

            public AttributeCollection Attributes { get; private set; }

            public MetaColumnMetadata(MetaColumn column) {
                Debug.Assert(column != null);
                Column = column;

                Attributes = Column.BuildAttributeCollection();

                DisplayAttribute = Attributes.FirstOrDefault<DisplayAttribute>();
                DataTypeAttribute = Attributes.FirstOrDefault<DataTypeAttribute>() ?? GetDefaultDataTypeAttribute();
                DescriptionAttribute = Attributes.FirstOrDefault<DescriptionAttribute>();
                DefaultValueAttribute = Attributes.FirstOrDefault<DefaultValueAttribute>();
                DisplayNameAttribute = Attributes.FirstOrDefault<DisplayNameAttribute>();
                RequiredAttribute = Attributes.FirstOrDefault<RequiredAttribute>();
                ScaffoldColumnAttribute = Attributes.FirstOrDefault<ScaffoldColumnAttribute>();
                StringLengthAttribute = Attributes.FirstOrDefault<StringLengthAttribute>();

                UIHint = GetHint<UIHintAttribute>(a => a.PresentationLayer, a => a.UIHint);
                FilterUIHint = GetHint<FilterUIHintAttribute>(a => a.PresentationLayer, a => a.FilterUIHint);

                EditableAttribute = Attributes.FirstOrDefault<EditableAttribute>();
                IsReadOnly = Attributes.GetAttributePropertyValue<ReadOnlyAttribute, bool>(a => a.IsReadOnly, false);

                var displayFormatAttribute = Attributes.FirstOrDefault<DisplayFormatAttribute>() ??
                    (DataTypeAttribute != null ? DataTypeAttribute.DisplayFormat : null);

                ApplyFormatInEditMode = displayFormatAttribute.GetPropertyValue(a => a.ApplyFormatInEditMode, false);
                ConvertEmptyStringToNull = displayFormatAttribute.GetPropertyValue(a => a.ConvertEmptyStringToNull, true);
                DataFormatString = displayFormatAttribute.GetPropertyValue(a => a.DataFormatString, String.Empty);
                NullDisplayText = displayFormatAttribute.GetPropertyValue(a => a.NullDisplayText, String.Empty);
                HtmlEncode = displayFormatAttribute.GetPropertyValue(a => a.HtmlEncode, true);
            }

            public DisplayAttribute DisplayAttribute { get; private set; }

            public bool ApplyFormatInEditMode { get; private set; }

            public bool ConvertEmptyStringToNull { get; private set; }

            public string DataFormatString { get; private set; }

            public DataTypeAttribute DataTypeAttribute { get; private set; }

            public object DefaultValue {
                get {
                    return DefaultValueAttribute.GetPropertyValue(a => a.Value, null);
                }
            }

            private DefaultValueAttribute DefaultValueAttribute { get; set; }

            public string Description {
                get {
                    return DisplayAttribute.GetLocalizedDescription() ??
                        DescriptionAttribute.GetPropertyValue(a => a.Description, null);
                }
            }

            private DescriptionAttribute DescriptionAttribute { get; set; }

            public string DisplayName {
                get {
                    return DisplayAttribute.GetLocalizedName() ??
                        DisplayNameAttribute.GetPropertyValue(a => a.DisplayName, null);
                }
            }

            public string ShortDisplayName {
                get {
                    return DisplayAttribute.GetLocalizedShortName();
                }
            }

            private DisplayNameAttribute DisplayNameAttribute { get; set; }

            public string FilterUIHint { get; private set; }

            public EditableAttribute EditableAttribute { get; private set; }

            public bool IsReadOnly { get; private set; }

            public string NullDisplayText { get; private set; }

            public string Prompt {
                get {
                    return DisplayAttribute.GetLocalizedPrompt();
                }
            }

            public RequiredAttribute RequiredAttribute { get; private set; }

            public ScaffoldColumnAttribute ScaffoldColumnAttribute { get; private set; }

            public StringLengthAttribute StringLengthAttribute { get; private set; }

            public string UIHint { get; private set; }

            private DataTypeAttribute GetDefaultDataTypeAttribute() {
                if (Column.IsString) {
                    if (Column.IsLongString) {
                        return new DataTypeAttribute(DataType.MultilineText);
                    }
                    else {
                        return new DataTypeAttribute(DataType.Text);
                    }
                }

                return null;
            }

            private string GetHint<T>(Func<T, string> presentationLayerPropertyAccessor, Func<T, string> hintPropertyAccessor) where T : Attribute {
                var uiHints = Attributes.OfType<T>();
                var presentationLayerNotSpecified = uiHints.Where(a => String.IsNullOrEmpty(presentationLayerPropertyAccessor(a)));
                var presentationLayerSpecified = uiHints.Where(a => !String.IsNullOrEmpty(presentationLayerPropertyAccessor(a)));

                T uiHintAttribute = presentationLayerSpecified.FirstOrDefault(a => presentationLayerPropertyAccessor(a).ToLower(CultureInfo.InvariantCulture) == "webforms" ||
                                                                                   presentationLayerPropertyAccessor(a).ToLower(CultureInfo.InvariantCulture) == "mvc") ??
                                                  presentationLayerNotSpecified.FirstOrDefault();

                return uiHintAttribute.GetPropertyValue(hintPropertyAccessor);
            }


            public bool HtmlEncode {
                get; set;
            }
        }

        #endregion

        string IMetaColumn.Description {
            get {
                return Description;
            }
        }

        string IMetaColumn.DisplayName {
            get {
                return DisplayName;
            }
        }

        string IMetaColumn.Prompt {
            get {
                return Prompt;
            }
        }

        string IMetaColumn.ShortDisplayName {
            get {
                return ShortDisplayName;
            }
        }

        IMetaTable IMetaColumn.Table {
            get {
                return Table;
            }
        }

        IMetaModel IMetaColumn.Model {
            get {
                return Model;
            }
        }
    }
}
