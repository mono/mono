namespace System.Web.DynamicData {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.DynamicData.ModelProviders;
    using System.Web.Routing;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Principal;

    internal interface IMetaTable {
        System.ComponentModel.AttributeCollection Attributes { get; }
        ReadOnlyCollection<IMetaColumn> Columns { get; }
        bool CanDelete(IPrincipal principal);
        bool CanInsert(IPrincipal principal);
        bool CanRead(IPrincipal principal);
        bool CanUpdate(IPrincipal principal);
        object CreateContext();
        string DataContextPropertyName { get; }
        Type DataContextType { get; }
        IMetaColumn DisplayColumn { get; }
        string DisplayName { get; }
        Type EntityType { get; }
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This interface will be made internal")]
        string[] PrimaryKeyNames { get; }
        string ForeignKeyColumnsNames { get; }
        string GetActionPath(string action);
        string GetActionPath(string action, IList<object> primaryKeyValues);
        string GetActionPath(string action, IList<object> primaryKeyValues, string path);
        string GetActionPath(string action, object row);
        string GetActionPath(string action, object row, string path);
        string GetActionPath(string action, RouteValueDictionary routeValues);
        IMetaColumn GetColumn(string columnName);
        DataKey GetDataKeyFromRoute();
        string GetDisplayString(object row);
        IEnumerable<IMetaColumn> GetFilteredColumns();
        IDictionary<string, object> GetPrimaryKeyDictionary(object row);
        string GetPrimaryKeyString(IList<object> primaryKeyValues);
        string GetPrimaryKeyString(object row);
        IList<object> GetPrimaryKeyValues(object row);
        IQueryable GetQuery();
        IQueryable GetQuery(object context);
        IEnumerable<IMetaColumn> GetScaffoldColumns(DataBoundControlMode mode, ContainerType containerType);
        bool HasPrimaryKey { get; }
        bool IsReadOnly { get; }
        string ListActionPath { get; }
        IMetaModel Model { get; }
        string Name { get; }
        ReadOnlyCollection<IMetaColumn> PrimaryKeyColumns { get; }
        TableProvider Provider { get; }
        Type RootEntityType { get; }
        bool Scaffold { get; }
        IMetaColumn SortColumn { get; }
        bool SortDescending { get; }
        bool TryGetColumn(string columnName, out IMetaColumn column);
    }
}
