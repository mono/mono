namespace System.Web.UI {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Web.DynamicData;

    public static class DataControlExtensions {
        private readonly static ConcurrentDictionary<Type, MetaTable> s_MetaTableCache = new ConcurrentDictionary<Type, MetaTable>();

        public static void EnableDynamicData(this INamingContainer control, Type entityType, object defaults) {
            MetaTable table = GetTableFromCache(entityType);
            control.SetMetaTable(table, defaults);
            DynamicDataExtensions.ApplyFieldGenerator(control, table);
        }

        public static void EnableDynamicData(this INamingContainer control, Type entityType, IDictionary<string, object> defaultValues) {
            MetaTable table = GetTableFromCache(entityType);
            control.SetMetaTable(table, defaultValues);
            DynamicDataExtensions.ApplyFieldGenerator(control, table);
        }

        public static void EnableDynamicData(this INamingContainer control, Type entityType) {
            MetaTable table = GetTableFromCache(entityType);
            control.SetMetaTable(table);
            DynamicDataExtensions.ApplyFieldGenerator(control, table);
        }

        private static MetaTable GetTableFromCache(Type entityType) {
            MetaTable table;
            if (!s_MetaTableCache.TryGetValue(entityType, out table)) {
                table = MetaTable.CreateTable(entityType);
                table.Model.FieldTemplateFactory = new SimpleFieldTemplateFactory();
                s_MetaTableCache.TryAdd(entityType, table);
            }
            return table;
        }
    }
}
