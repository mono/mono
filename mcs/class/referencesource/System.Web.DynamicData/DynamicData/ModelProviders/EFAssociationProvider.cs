using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace System.Web.DynamicData.ModelProviders {
    internal sealed class EFAssociationProvider : AssociationProvider {
        public EFAssociationProvider(EFColumnProvider column, NavigationProperty navigationProperty) {
            FromColumn = column;

            var entityMemberParentEntity = (EFTableProvider)column.Table;
            var parentEntityModel = (EFDataModelProvider)entityMemberParentEntity.DataModel;

            EFColumnProvider columnProvider;
            EntityType otherEntityType = navigationProperty.ToEndMember.GetEntityType();

            // If we can get to the entityType of the ToMember side of the relaionship then build a relationship key and try to lookup the column provider.
            if (otherEntityType != null) {
                long key = BuildRelationshipKey(otherEntityType, navigationProperty.ToEndMember);
                if (parentEntityModel.RelationshipEndLookup.TryGetValue(key, out columnProvider)) {
                    ToColumn = columnProvider;
                }
                else {
                    // Otherwise just lookup the entityType in the table lookup
                    ToTable = parentEntityModel.TableEndLookup[otherEntityType];
                }
            }
            else {
                EntityType value = (EntityType)navigationProperty.ToEndMember.TypeUsage.EdmType.MetadataProperties.Single(prop => prop.Name == "ElementType").Value;
                ToTable = parentEntityModel.TableEndLookup[value];
            }

            if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many) {
                if (navigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many) {
                    Direction = AssociationDirection.ManyToMany;
                }
                else {
                    Direction = AssociationDirection.OneToMany;
                }
            }
            else {
                if (navigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many) {
                    Direction = AssociationDirection.ManyToOne;
                }
                else {
                    Direction = AssociationDirection.OneToOne;
                }
            }

            // If it's a foreign key reference (as opposed to a entity set), figure out the foreign keys
            if (IsForeignKeyReference) {
                var foreignKeyNames = new List<string>();
                var primaryKeyNames = FromColumn.Table.Columns.Where(c => c.IsPrimaryKey).Select(c => c.Name);

                // Add the foreign keys for this association.
                foreignKeyNames.AddRange(GetDependentPropertyNames(navigationProperty));

                if (IsZeroOrOne(navigationProperty)) {
                    // Assume this is true for 1 to 0..1 relationships on both sides
                    IsPrimaryKeyInThisTable = true;
                }
                else {
                    // If any of the foreign keys are also PKs, set the flag                    
                    IsPrimaryKeyInThisTable = foreignKeyNames.Any(fkName => primaryKeyNames.Contains(fkName, StringComparer.OrdinalIgnoreCase));
                }

                if (!foreignKeyNames.Any()) {
                    // If we couldn't find any dependent properties, we're dealing with a model that doesn't
                    // have FKs, and requires the use of flattened FK names (e.g. Category.CategoryId)
                    foreach (ColumnProvider toEntityColumn in ToTable.Columns.Where(c => c.IsPrimaryKey)) {
                        foreignKeyNames.Add(FromColumn.Name + "." + toEntityColumn.Name);
                    }
                }

                ForeignKeyNames = foreignKeyNames.AsReadOnly();
            }
        }

        private static bool IsZeroOrOne(NavigationProperty navigationProperty) {
            return (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne &&
                                navigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One) ||
                                (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One &&
                                navigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne);
        }

        private bool IsForeignKeyReference {
            get {
                return Direction == AssociationDirection.OneToOne || Direction == AssociationDirection.ManyToOne;
            }
        }

        internal static long BuildRelationshipKey(EntityType entityType, RelationshipEndMember member) {
            return Misc.CombineHashCodes(entityType.GetHashCode(), member.GetHashCode());
        }

        internal static IEnumerable<string> GetDependentPropertyNames(NavigationProperty navigationProperty) {
            return GetDependentPropertyNames(navigationProperty, true /*checkRelationshipType*/);
        }

        internal static IEnumerable<string> GetDependentPropertyNames(NavigationProperty navigationProperty, bool checkRelationshipType) {
            if (checkRelationshipType) {
                if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne &&
                    navigationProperty.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One) {
                    // Get constraint when this association is on the "parent" (aka "from") side. This means
                    // that the navProperty represents a Children association in a 1-1 relationship. For example,
                    // this could be a Item-ItemDetail scenario where the ItemDetail table has a PK that is also an FK
                    // into the Item table (thus ensuring the 1-1 cardinality). We need to special case this situation because normally we would try
                    // to build a foreign key name of the form "Item.ItemID", but we want just "ItemID".
                    AssociationType relationshipType = (AssociationType)navigationProperty.RelationshipType;
                    ReferentialConstraint constraint = relationshipType.ReferentialConstraints.FirstOrDefault(c => c.ToRole == navigationProperty.ToEndMember);
                    if (constraint != null) {
                        return constraint.FromProperties.Select(p => p.Name);
                    }

                    // Fall back on the primary keys if no constraints were found but only if we are on the parent side. i.e the 1 side Item side in an Item-ItemDetail
                    // Get the primary keys on the "from" side of the relationship. i.e Product.Category -> ProductID
                    return navigationProperty.FromEndMember.GetEntityType().KeyMembers.Select(m => m.Name);
                }
            }
            return navigationProperty.GetDependentProperties().Select(m => m.Name);
        }

        public override string GetSortExpression(ColumnProvider sortColumn) {
            return GetSortExpression(sortColumn, "{0}.{1}");
        }
    }
}
