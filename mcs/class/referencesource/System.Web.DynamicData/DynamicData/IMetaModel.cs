namespace System.Web.DynamicData {
    using System;
    using System.Web.DynamicData.ModelProviders;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal interface IMetaModel {
        string DynamicDataFolderVirtualPath { get; set; }
        IFieldTemplateFactory FieldTemplateFactory { get; set; }
        string GetActionPath(string tableName, string action, object row);
        IMetaTable GetTable(string tableName, Type contextType);
        IMetaTable GetTable(string uniqueTableName);
        IMetaTable GetTable(Type entityType);
        void RegisterContext(Func<object> contextFactory);
        void RegisterContext(Func<object> contextFactory, ContextConfiguration configuration);
        void RegisterContext(Type contextType);
        void RegisterContext(Type contextType, ContextConfiguration configuration);
        void RegisterContext(DataModelProvider dataModelProvider);
        void RegisterContext(DataModelProvider dataModelProvider, ContextConfiguration configuration);
        ReadOnlyCollection<IMetaTable> Tables { get; }
        bool TryGetTable(string uniqueTableName, out IMetaTable table);
        bool TryGetTable(Type entityType, out IMetaTable table);
        List<IMetaTable> VisibleTables { get; }
    }
}
