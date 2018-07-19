using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace System.Web.DynamicData.ModelProviders {
    internal sealed class EFDataModelProvider : DataModelProvider {
        private ReadOnlyCollection<TableProvider> _tables;

        internal Dictionary<long, EFColumnProvider> RelationshipEndLookup { get; private set; }
        internal Dictionary<EntityType, EFTableProvider> TableEndLookup { get; private set; }
        private Func<object> ContextFactory { get; set; }
        private Dictionary<EdmType, Type> _entityTypeToClrType = new Dictionary<EdmType, Type>();
        private ObjectContext _context;
        private ObjectItemCollection _objectSpaceItems;

        public EFDataModelProvider(object contextInstance, Func<object> contextFactory) {
            ContextFactory = contextFactory;
            RelationshipEndLookup = new Dictionary<long, EFColumnProvider>();
            TableEndLookup = new Dictionary<EntityType, EFTableProvider>();

            _context = (ObjectContext)contextInstance ?? (ObjectContext)CreateContext();
            ContextType = _context.GetType();

            // get a "container" (a scope at the instance level)
            EntityContainer container = _context.MetadataWorkspace.GetEntityContainer(_context.DefaultContainerName, DataSpace.CSpace);
            // load object space metadata
            _context.MetadataWorkspace.LoadFromAssembly(ContextType.Assembly);
            _objectSpaceItems = (ObjectItemCollection)_context.MetadataWorkspace.GetItemCollection(DataSpace.OSpace);

            var tables = new List<TableProvider>();

            // Create a dictionary from entity type to entity set. The entity type should be at the root of any inheritance chain.
            IDictionary<EntityType, EntitySet> entitySetLookup = container.BaseEntitySets.OfType<EntitySet>().ToDictionary(e => e.ElementType);

            // Create a lookup from parent entity to entity
            ILookup<EntityType, EntityType> derivedTypesLookup = _context.MetadataWorkspace.GetItems<EntityType>(DataSpace.CSpace).ToLookup(e => (EntityType)e.BaseType);

            // Keeps track of the current entity set being processed
            EntitySet currentEntitySet = null;

            // Do a DFS to get the inheritance hierarchy in order
            // i.e. Consider the hierarchy
            // null -> Person
            // Person -> Employee, Contact
            // Employee -> SalesPerson, Programmer
            // We'll walk the children in a depth first order -> Person, Employee, SalesPerson, Programmer, Contact.
            var objectStack = new Stack<EntityType>();
            // Start will null (the root of the hierarchy)
            objectStack.Push(null);
            while (objectStack.Any()) {
                EntityType entityType = objectStack.Pop();
                if (entityType != null) {
                    // Update the entity set when we are at another root type (a type without a base type).
                    if (entityType.BaseType == null) {
                        currentEntitySet = entitySetLookup[entityType];
                    }

                    var table = CreateTableProvider(currentEntitySet, entityType);
                    tables.Add(table);
                }

                foreach (EntityType derivedEntityType in derivedTypesLookup[entityType]) {
                    // Push the derived entity types on the stack
                    objectStack.Push(derivedEntityType);
                }
            }

            _tables = tables.AsReadOnly();
        }

        public override object CreateContext() {
            return ContextFactory();
        }

        public override ReadOnlyCollection<TableProvider> Tables {
            get {
                return _tables;
            }
        }

        internal Type GetClrType(EdmType entityType) {
            var result = _entityTypeToClrType[entityType];
            Debug.Assert(result != null, String.Format(CultureInfo.CurrentCulture, "Cannot map EdmType '{0}' to matching CLR Type", entityType));
            return result;
        }

        internal Type GetClrType(EnumType enumType) {
            var objectSpaceType = (EnumType)_context.MetadataWorkspace.GetObjectSpaceType(enumType);
            return _objectSpaceItems.GetClrType(objectSpaceType);
        }

        private Type GetClrType(EntityType entityType) {
            var objectSpaceType = (EntityType)_context.MetadataWorkspace.GetObjectSpaceType(entityType);
            return _objectSpaceItems.GetClrType(objectSpaceType);
        }

        private TableProvider CreateTableProvider(EntitySet entitySet, EntityType entityType) {
            // Get the parent clr type
            Type parentClrType = null;
            EntityType parentEntityType = entityType.BaseType as EntityType;
            if (parentEntityType != null) {
                parentClrType = GetClrType(parentEntityType);
            }

            Type rootClrType = GetClrType(entitySet.ElementType);
            Type clrType = GetClrType(entityType);

            _entityTypeToClrType[entityType] = clrType;

            // Normally, use the entity set name as the table name
            string tableName = entitySet.Name;

            // But in inheritance scenarios where all types in the hierarchy share the same entity set,
            // we need to use the type name instead for the table name.
            if (parentClrType != null) {
                tableName = entityType.Name;
            }

            EFTableProvider table = new EFTableProvider(this, entitySet, entityType, clrType, parentClrType, rootClrType, tableName);
            TableEndLookup[entityType] = table;

            return table;
        }
    }
}
