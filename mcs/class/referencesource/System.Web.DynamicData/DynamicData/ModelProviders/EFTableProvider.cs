using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Linq;
using System.Reflection;

namespace System.Web.DynamicData.ModelProviders {
    internal sealed class EFTableProvider : TableProvider {
        private ReadOnlyCollection<ColumnProvider> _roColumns;

        public EFTableProvider(EFDataModelProvider dataModel, EntitySet entitySet, EntityType entityType,
            Type entityClrType, Type parentEntityClrType, Type rootEntityClrType, string name)
            : base(dataModel) {

            EntityType = entityClrType;
            Name = name;
            DataContextPropertyName = entitySet.Name;
            ParentEntityType = parentEntityClrType;
            RootEntityType = rootEntityClrType;

            var genericMethod = DataModel.ContextType.GetMethod("CreateQuery");
            CreateQueryMethod = genericMethod.MakeGenericMethod(EntityType);
            CreateQueryString = CreateEntitySqlQueryString(entitySet);

            var keyMembers = entityType.KeyMembers;

            // columns (entity properties)
            // note 1: keys are also available through es.ElementType.KeyMembers
            // note 2: this includes "nav properties", kind of fancy, two-way relationship objects
            var columns = new List<ColumnProvider>();
            foreach (EdmMember m in entityType.Members) {
                if (EFColumnProvider.IsSupportedEdmMemberType(m) && IsPublicProperty(entityClrType, m.Name)) {
                    EFColumnProvider entityMember = new EFColumnProvider(entityType, this, m, keyMembers.Contains(m));
                    columns.Add(entityMember);
                }
            }

            _roColumns = new ReadOnlyCollection<ColumnProvider>(columns);
        }

        private static bool IsPublicProperty(Type entityClrType, string propertyName) {
            var property = entityClrType.GetProperty(propertyName);
            return property != null && property.GetGetMethod() != null;
        }

        private MethodInfo CreateQueryMethod { get; set; }

        private string CreateQueryString { get; set; }

        private static string CreateEntitySqlQueryString(EntitySet entitySet) {
            // Qualify the entity set name with the container name (in case the ObjectContext's default
            // container name is not set or has an unexpected value)
            return QuoteEntitySqlIdentifier(entitySet.EntityContainer.Name) + "." + QuoteEntitySqlIdentifier(entitySet.Name);
        }

        private static string QuoteEntitySqlIdentifier(string identifier) {
            // Enclose in square brackets and escape the one reserved character (])
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        public override ReadOnlyCollection<ColumnProvider> Columns {
            get {
                return _roColumns;
            }
        }

        public override IQueryable GetQuery(object context) {
            return (IQueryable)CreateQueryMethod.Invoke(context,
                new object[] { CreateQueryString, new ObjectParameter[0] });
        }
    }
}
