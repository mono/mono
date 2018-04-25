using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using System.Reflection;

namespace System.Web.DynamicData.ModelProviders {
    internal sealed class EFColumnProvider : ColumnProvider {
        private EFTableProvider _table;
        private EFAssociationProvider _association;
        private bool _isAssociation;
        private bool _isSortableProcessed;
        private const string StoreGeneratedMetadata = "http://schemas.microsoft.com/ado/2009/02/edm/annotation:StoreGeneratedPattern";

        public EFColumnProvider(EntityType entityType, EFTableProvider table, EdmMember m, bool isPrimaryKey)
            : base(table) {
            EdmMember = m;
            IsPrimaryKey = isPrimaryKey;
            _table = table;
            MaxLength = 0;
            Name = EdmMember.Name;
            // 
            IsCustomProperty = false;

            // 
            var property = EdmMember as EdmProperty;

            if (property != null) {
                IsForeignKeyComponent = DetermineIsForeignKeyComponent(property);
                IsGenerated = IsServerGenerated(property);
            }

            ProcessFacets();

            var navProp = m as NavigationProperty;
            if (navProp != null) {
                _isAssociation = true;
                long key = EFAssociationProvider.BuildRelationshipKey(entityType, navProp.FromEndMember);
                ((EFDataModelProvider)table.DataModel).RelationshipEndLookup[key] = this;
            }
        }

        private bool DetermineIsForeignKeyComponent(EdmProperty property) {
            var navigationProperties = property.DeclaringType.Members.OfType<NavigationProperty>();

            // Look at all NavigationProperties (i.e. strongly-type relationship columns) of the table this column belong to and
            // see if there is a foreign key that matches this property
            // If this is a 1 to 0..1 relationship and we are processing the more primary side. i.e in the Student in Student-StudentDetail
            // and this is the primary key we don't want to check the relationship type since if there are no constraints we will treat the primary key as a foreign key.
            return navigationProperties.Any(n => EFAssociationProvider.GetDependentPropertyNames(n, !IsPrimaryKey /* checkRelationshipType */).Contains(property.Name));
        }

        private static bool IsServerGenerated(EdmProperty property) {
            MetadataProperty generated;
            if (property.MetadataProperties.TryGetValue(StoreGeneratedMetadata, false, out generated)) {
                return "Identity" == (string)generated.Value || "Computed" == (string)generated.Value;
            }
            return false;
        }

        private void ProcessFacets() {
            foreach (Facet facet in EdmMember.TypeUsage.Facets) {
                switch (facet.Name) {
                    case "MaxLength":
                        if (facet.IsUnbounded) {
                            // If it's marked as unbounded, treat it as max int
                            MaxLength = Int32.MaxValue;
                        }
                        else if (facet.Value != null && facet.Value is int) {
                            MaxLength = (int)facet.Value;
                        }
                        break;
                    case "Nullable":
                        Nullable = (bool)facet.Value;
                        break;
                }
            }
        }

        internal EdmMember EdmMember {
            get;
            private set;
        }

        #region IEntityMember Members

        public override PropertyInfo EntityTypeProperty {
            // 
            get { return _table.EntityType.GetProperty(Name); }
        }

        public override Type ColumnType {
            get {
                if (base.ColumnType == null) {
                    // 


                    var edmType = EdmMember.TypeUsage.EdmType;
                    if (edmType is EntityType) {
                        base.ColumnType = ((EFDataModelProvider)this.Table.DataModel).GetClrType(edmType);
                    }
                    else if (edmType is CollectionType) {
                        // get the EdmType that this CollectionType is wrapping
                        base.ColumnType = ((EFDataModelProvider)this.Table.DataModel).GetClrType(((CollectionType)edmType).TypeUsage.EdmType);
                    }
                    else if (edmType is PrimitiveType) {
                        base.ColumnType = ((PrimitiveType)edmType).ClrEquivalentType;
                    }
                    else if (edmType is EnumType) {
                        base.ColumnType = ((EFDataModelProvider)this.Table.DataModel).GetClrType((EnumType)edmType);
                    }
                    else {
                        Debug.Assert(false, String.Format(CultureInfo.CurrentCulture, "Unknown EdmType {0}.", edmType.GetType().FullName));
                    }
                }
                return base.ColumnType;
            }
        }

        public override bool IsSortable {
            get {
                if (!_isSortableProcessed) {
                    base.IsSortable = (ColumnType != typeof(byte[]));
                    _isSortableProcessed = true;
                }
                return base.IsSortable;
            }
        }

        public override AssociationProvider Association {
            get {
                if (!_isAssociation) {
                    return null;
                }

                if (_association == null) {
                    _association = new EFAssociationProvider(this, (NavigationProperty)EdmMember);
                }
                return _association;
            }
        }

        #endregion

        internal static bool IsSupportedEdmMemberType(EdmMember member) {
            var edmType = member.TypeUsage.EdmType;
            return edmType is EntityType || edmType is CollectionType || edmType is PrimitiveType || edmType is EnumType;
        }
    }
}
