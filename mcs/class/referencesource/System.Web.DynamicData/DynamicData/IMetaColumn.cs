namespace System.Web.DynamicData {
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using System.Web.DynamicData.ModelProviders;

    internal interface IMetaColumn {
        bool ApplyFormatInEditMode { get; }
        bool AllowInitialValue { get; }
        AttributeCollection Attributes { get; }
        Type ColumnType { get; }
        bool ConvertEmptyStringToNull { get; }
        string DataFormatString { get; }
        DataTypeAttribute DataTypeAttribute { get; }
        object DefaultValue { get; }
        string Description { get; }
        string DisplayName { get; }
        PropertyInfo EntityTypeProperty { get; }
        bool HtmlEncode { get; }
        bool IsBinaryData { get; }
        bool IsCustomProperty { get; }
        bool IsFloatingPoint { get; }
        bool IsForeignKeyComponent { get; }
        bool IsGenerated { get; }
        bool IsInteger { get; }
        bool IsLongString { get; }
        bool IsPrimaryKey { get; }
        bool IsReadOnly { get; }
        bool IsRequired { get; }
        bool IsString { get; }
        int MaxLength { get; }
        IMetaModel Model { get; }
        string Name { get; }
        string NullDisplayText { get; }
        string Prompt { get; }
        ColumnProvider Provider { get; }
        string RequiredErrorMessage { get; }
        void ResetMetadata();
        bool Scaffold { get; set; }
        string ShortDisplayName { get; }
        string SortExpression { get; }
        IMetaTable Table { get; }
        TypeCode TypeCode { get; }
        string UIHint { get; }
        string FilterUIHint { get; }
    }
}
