using System.Collections.ObjectModel;
using System.Globalization;
using System.Web.Resources;

namespace System.Web.DynamicData.ModelProviders {
    /// <summary>
    /// Specifies the association cardinality
    /// </summary>
    public enum AssociationDirection {
        /// <summary>
        /// 1-1
        /// </summary>
        OneToOne,
        /// <summary>
        /// one to many
        /// </summary>
        OneToMany,
        /// <summary>
        /// many to one
        /// </summary>
        ManyToOne,
        /// <summary>
        /// many to many
        /// </summary>
        ManyToMany
    }

    /// <summary>
    /// Base provider class for associations between columns
    /// Each provider type (e.g. Linq To Sql, Entity Framework, 3rd party) extends this class.
    /// </summary>
    public abstract class AssociationProvider {
        private TableProvider _toTable;

        /// <summary>
        /// The type of association
        /// </summary>
        public virtual AssociationDirection Direction { get; protected set; }

        /// <summary>
        /// The source column of the association
        /// </summary>
        public virtual ColumnProvider FromColumn { get; protected set; }

        /// <summary>
        /// The destination table of the association
        /// </summary>
        public virtual TableProvider ToTable {
            get {
                if (_toTable != null) {
                    return _toTable;
                }

                if (ToColumn != null) {
                    return ToColumn.Table;
                }

                return null;
            }
            protected set {
                _toTable = value;
            }
        }

        /// <summary>
        /// The destination column of the association
        /// </summary>
        public virtual ColumnProvider ToColumn { get; protected set; }

        /// <summary>
        /// Returns true if the From Column part of the primary key of its table
        /// e.g. Order and Product are PKs in the Order_Details table
        /// </summary>
        public virtual bool IsPrimaryKeyInThisTable { get; protected set; }

        /// <summary>
        /// The names of the underlying foreign keys that make up this association
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification="It's a readonly collection, so the warning is incorrect")]
        public virtual ReadOnlyCollection<string> ForeignKeyNames { get; protected set; }

        /// <summary>
        /// Returns a string representing the sort expression that would be used for
        /// sorting the column represented by this association. The parameter is the
        /// property of the strongly typed entity used as the sort key for that entity.
        /// For example, assume that this association represents the Category column
        /// in the Products table. The sortColumn paramater is "CategoryName",
        /// meaning that this method is being asked to return the sort expression for
        /// sorting the Category column by the CategoryName property of the Category entity.
        /// The result sort expression would be "Category.CategoryName".
        /// The result of this method should be affected by whether the underlying data
        /// model is capable of sorting the entity by the given sort column (see
        /// ColumnProvider.IsSortable). The method can return a null value to indicate
        /// that sorting is not supported.
        /// </summary>
        /// <param name="sortColumn">the column to sort the entity by</param>
        /// <returns>the sort expression string, or null if sort is not supported for the
        /// given sort column</returns>
        public virtual string GetSortExpression(ColumnProvider sortColumn) {
            return null;
        }

        internal string GetSortExpression(ColumnProvider sortColumn, string format) {
            if (Direction == AssociationDirection.OneToMany || Direction == AssociationDirection.ManyToMany) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.AssociationProvider_DirectionDoesNotSupportSorting,
                    Direction));
            }

            if (sortColumn == null) {
                throw new ArgumentNullException("sortColumn");
            }

            if (!ToTable.Columns.Contains(sortColumn)) {
                throw new ArgumentException(DynamicDataResources.AssociationProvider_SortColumnDoesNotBelongToEndTable, "sortColumn");
            }

            if (sortColumn.IsSortable) {
                return String.Format(CultureInfo.InvariantCulture, format, FromColumn.Name, sortColumn.Name);
            } else {
                return null;
            }
        }
    }
}
