using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.DynamicData.ModelProviders {
    internal sealed class DLinqTableProvider : TableProvider {
        private ReadOnlyCollection<ColumnProvider> _roColumns;
        private List<ColumnProvider> _columns;
        private MetaType _rowType;
        private PropertyInfo _prop;

        public DLinqTableProvider(DLinqDataModelProvider dataModel, MetaType rowType, string name, PropertyInfo prop)
            : base(dataModel) {
            _prop = prop;
            _rowType = rowType;
            Name = name;
            DataContextPropertyName = prop.Name;
            EntityType = rowType.Type;
            ParentEntityType = rowType.InheritanceBase != null ? rowType.InheritanceBase.Type : null;
            RootEntityType = rowType.Table.RowType.Type;

            _columns = new List<ColumnProvider>();
            var members = new List<MetaDataMember>(rowType.DataMembers);

            // Add in base-class-first order (not the typical derived-class-first order)
            foreach (PropertyInfo propInfo in GetOrderedProperties(rowType.Type)) {
                MetaDataMember member = members.FirstOrDefault(m => m.Member.Name == propInfo.Name);
                if (member != null) {
                    AddColumn(dataModel, member, propInfo);
                    members.Remove(member);
                }
            }

            // Anything we might've missed, tack it onto the end
            foreach (MetaDataMember member in members) {
                AddColumn(dataModel, member, (PropertyInfo)member.Member);
            }

            _roColumns = new ReadOnlyCollection<ColumnProvider>(_columns);
        }

        private void AddColumn(DLinqDataModelProvider dataModel, MetaDataMember member, PropertyInfo propInfo) {
            var publicGetAccessor = propInfo.GetGetMethod();
            if (publicGetAccessor == null) {
                // the property at least needs to have a public getter, otherwise databinding will not work
                return;
            }

            DLinqColumnProvider column = new DLinqColumnProvider(this, member);
            _columns.Add(column);

            if (!dataModel.ColumnLookup.ContainsKey(propInfo))
                dataModel.ColumnLookup[propInfo] = column;
        }

        private IEnumerable<PropertyInfo> GetOrderedProperties(Type type) {
            if (type == null)
                return new PropertyInfo[0];
            PropertyInfo[] props = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            return GetOrderedProperties(type.BaseType).Concat(props);
        }

        internal void Initialize() {
            _columns.ForEach(c => ((DLinqColumnProvider)c).Initialize());
            _columns.RemoveAll(c => ((DLinqColumnProvider)c).ShouldRemove);
        }

        #region IEntity Members

        public override IQueryable GetQuery(object context) {
            return (IQueryable)_prop.GetValue(context, null);
        }

        public override ReadOnlyCollection<ColumnProvider> Columns {
            get {
                return _roColumns;
            }
        }

        #endregion
    }
}
