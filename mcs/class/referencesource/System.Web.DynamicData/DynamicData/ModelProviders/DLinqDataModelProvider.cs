using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using LinqMetaTable = System.Data.Linq.Mapping.MetaTable;
using LinqMetaType = System.Data.Linq.Mapping.MetaType;

namespace System.Web.DynamicData.ModelProviders {
    internal sealed class DLinqDataModelProvider : DataModelProvider {
        private ReadOnlyCollection<TableProvider> _roTables;
        private Dictionary<PropertyInfo, DLinqColumnProvider> _columnLookup = new Dictionary<PropertyInfo, DLinqColumnProvider>();

        private Func<object> ContextFactory { get; set; }

        public DLinqDataModelProvider(object contextInstance, Func<object> contextFactory) {
            ContextFactory = contextFactory;

            DataContext context = (DataContext)contextInstance ?? (DataContext)CreateContext();
            ContextType = context.GetType();

            DLinqTables = new List<TableProvider>();
            foreach (PropertyInfo prop in ContextType.GetProperties()) {
                Type entityType = GetEntityType(prop);

                if (entityType != null) {
                    LinqMetaTable table = GetLinqTable(context, entityType);
                    ProcessTable(table, table.RowType, prop.Name, prop);
                }
            }

            DLinqTables.ForEach(t => ((DLinqTableProvider)t).Initialize());

            _roTables = new ReadOnlyCollection<TableProvider>(DLinqTables);
        }

        private LinqMetaTable GetLinqTable(DataContext context, Type entityType) {
            return context.Mapping.GetTables().First(t => t.RowType.Type == entityType);
        }

        private Type GetEntityType(PropertyInfo prop) {
            // 
            if (prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof(Table<>))
                return prop.PropertyType.GetGenericArguments()[0];

            return null;
        }

        private void ProcessTable(LinqMetaTable table, LinqMetaType rowType, string name, PropertyInfo prop) {
            DLinqTables.Add(new DLinqTableProvider(this, rowType, name, prop));

            foreach (LinqMetaType derivedType in rowType.DerivedTypes)
                ProcessTable(table, derivedType, derivedType.Name, prop);
        }

        internal Dictionary<PropertyInfo, DLinqColumnProvider> ColumnLookup {
            get {
                return _columnLookup;
            }
        }

        internal List<TableProvider> DLinqTables { get; private set; }

        public override object CreateContext() {
            return ContextFactory();
        }

        public override ReadOnlyCollection<TableProvider> Tables {
            get {
                return _roTables;
            }
        }
    }
}
