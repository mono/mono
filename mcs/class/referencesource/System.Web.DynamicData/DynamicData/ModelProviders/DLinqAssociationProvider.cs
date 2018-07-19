using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections;
using System.Globalization;

namespace System.Web.DynamicData.ModelProviders {
    internal sealed class DLinqAssociationProvider : AssociationProvider {

        public DLinqAssociationProvider(DLinqColumnProvider column) {
            FromColumn = column;

            MetaAssociation association = column.Member.Association;

            SetOtherEndOfAssociation(association);

            SetDirection(association);
            Debug.Assert(Direction != AssociationDirection.ManyToMany, "Many to Many is not supported by Linq to SQL");

            SetAssociationKeyInfo(association);
        }

        private void SetAssociationKeyInfo(MetaAssociation association) {
            DLinqColumnProvider column = (DLinqColumnProvider)FromColumn;

            List<string> foreignKeyNames = new List<string>();

            int count = column.Member.Association.ThisKey.Count;
            for (int i = 0; i < count; i++) {
                MetaDataMember thisKeyMetaDataMember = column.Member.Association.ThisKey[i];
                MetaDataMember otherKeyMetaDataMember = column.Member.Association.OtherKey[i];

                DLinqColumnProvider thisEntityMemberComponent = FindColumn(column.Table, thisKeyMetaDataMember.Name);

                if (ShouldRemoveThisAssociation(association)) {
                    column.ShouldRemove = true;
                    return;
                }

                foreignKeyNames.Add(thisEntityMemberComponent.Name);

                if (thisEntityMemberComponent.IsPrimaryKey) {
                    IsPrimaryKeyInThisTable = true;
                }
                if (association.IsForeignKey) {
                    thisEntityMemberComponent.IsForeignKeyComponent = true;
                }
            }

            ForeignKeyNames = new ReadOnlyCollection<string>(foreignKeyNames);
        }

        private bool ShouldRemoveThisAssociation(MetaAssociation association) {
            if (Direction == AssociationDirection.ManyToOne && !association.OtherKeyIsPrimaryKey) {
                return true;
            }
            if (Direction == AssociationDirection.OneToMany && !association.ThisKeyIsPrimaryKey) {
                return true;
            }
            if (Direction == AssociationDirection.OneToOne) {
                if (!association.IsForeignKey && !association.ThisKeyIsPrimaryKey) {
                    return true;
                }
                if (association.IsForeignKey && !association.OtherKeyIsPrimaryKey) {
                    return true;
                }
            }

            return false;
        }

        private void SetOtherEndOfAssociation(MetaAssociation association) {
            DLinqTableProvider entityMemberParentEntity = (DLinqTableProvider)FromColumn.Table;
            DLinqDataModelProvider parentEntityDataContext = (DLinqDataModelProvider)entityMemberParentEntity.DataModel;
            if (association.OtherMember != null) {
                ToColumn = parentEntityDataContext.ColumnLookup[(PropertyInfo)association.OtherMember.Member];
            } else {
                ToTable = ((DLinqDataModelProvider)FromColumn.Table.DataModel).DLinqTables.Single(tp => tp.EntityType == association.OtherType.Type);
            }
        }

        private static DLinqColumnProvider FindColumn(TableProvider table, String columnName) {
            // 
            return (DLinqColumnProvider)table.Columns.First(member => member.Name.Equals(columnName));
        }

        private void SetDirection(MetaAssociation association) {
            if (association.IsMany) {
                Direction = AssociationDirection.OneToMany;
            } else if (association.OtherMember == null || association.OtherMember.Association.IsMany) {
                // there might not be the other member if this is a one-sided association
                Direction = AssociationDirection.ManyToOne;
            } else {
                Direction = AssociationDirection.OneToOne;
            }
        }

        public override string GetSortExpression(ColumnProvider sortColumn) {
            return GetSortExpression(sortColumn, "{0}.{1}");
        }
    }
}
