namespace System.Web.DynamicData {
    using System;
    internal interface IMetaChildrenColumn : IMetaColumn {
        IMetaTable ChildTable { get; }
        IMetaColumn ColumnInOtherTable { get; }
        string GetChildrenListPath(object row);
        string GetChildrenPath(string action, object row);
        string GetChildrenPath(string action, object row, string path);
        bool IsManyToMany { get; }
    }
}
