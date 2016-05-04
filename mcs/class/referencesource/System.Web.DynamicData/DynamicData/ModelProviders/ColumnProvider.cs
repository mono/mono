using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web.DynamicData;
using System.Web.DynamicData.Util;

namespace System.Web.DynamicData.ModelProviders {
    /// <summary>
    /// Base provider class for columns.
    /// Each provider type (e.g. Linq To Sql, Entity Framework, 3rd party) extends this class.
    /// </summary>
    public abstract class ColumnProvider {
        private bool? _isReadOnly;
    
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="table">the table this column belongs to</param>
        protected ColumnProvider(TableProvider table) {
            if (table == null) {
                throw new ArgumentNullException("table");
            }

            Table = table;
        }

        /// <summary>
        /// readable representation
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString() {
            // To help identifying objects in debugger
            return Name ?? base.ToString();
        }

        internal virtual PropertyDescriptor PropertyDescriptor {
            get {
                return Table.GetTypeDescriptor().GetProperties().Find(Name, true/*ignoreCase*/);
            }
        }

        public virtual AttributeCollection Attributes {
            get {
                var propertyDescriptor = PropertyDescriptor;
                var attributes = propertyDescriptor != null ? propertyDescriptor.Attributes : AttributeCollection.Empty;
                return AddDefaultAttributes(this, attributes);
            }
        }

        protected static AttributeCollection AddDefaultAttributes(ColumnProvider columnProvider, AttributeCollection attributes) {
            List<Attribute> extraAttributes = new List<Attribute>();

            // If there is no required attribute and the Provider says required, add one
            var requiredAttribute = attributes.FirstOrDefault<RequiredAttribute>();
            if (requiredAttribute == null && !columnProvider.Nullable) {
                extraAttributes.Add(new RequiredAttribute());
            }

            // If there is no StringLength attribute and it's a string, add one
            var stringLengthAttribute = attributes.FirstOrDefault<StringLengthAttribute>();
            int maxLength = columnProvider.MaxLength;
            if (stringLengthAttribute == null && columnProvider.ColumnType == typeof(String) && maxLength > 0) {
                extraAttributes.Add(new StringLengthAttribute(maxLength));
            }

            // If we need any extra attributes, create a new collection
            if (extraAttributes.Count > 0) {
                attributes = AttributeCollection.FromExisting(attributes, extraAttributes.ToArray());
            }

            return attributes;
        }

        /// <summary>
        /// The name of the column
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// The CLR type of the column
        /// </summary>
        public virtual Type ColumnType { get; protected set; }

        /// <summary>
        /// Is this column a primary key in its table
        /// </summary>
        public virtual bool IsPrimaryKey { get; protected set; }

        /// <summary>
        /// Specifies if this column is read only
        /// </summary>
        public virtual bool IsReadOnly {
            get {
                if (_isReadOnly == null) {
                    var propertyDescriptor = PropertyDescriptor;
                    _isReadOnly = propertyDescriptor != null ? propertyDescriptor.IsReadOnly : false;
                }
                return _isReadOnly.Value;
            }
            protected set {
                _isReadOnly = value;
            }
        }

        /// <summary>
        /// Is it a database generated column
        /// </summary>
        public virtual bool IsGenerated { get; protected set; }

        /// <summary>
        /// Returns whether the underlying model supports sorting of the table on this column
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sortable", Justification="It's a valid word")]
        public virtual bool IsSortable { get; protected set; }

        /// <summary>
        /// The maximun length allowed for this column (applies to string columns)
        /// </summary>
        public virtual int MaxLength { get; protected set; }

        /// <summary>
        /// Does it allow null values (meaning it is not required)
        /// </summary>
        public virtual bool Nullable { get; protected set; }

        /// <summary>
        /// meant to indicate that a member is an extra property that was declared in a partial class
        /// </summary>
        public virtual bool IsCustomProperty { get; protected set; }

        /// <summary>
        /// If the column represents and association with anther table, this returns the association information.
        /// Otherwise, null is returned.
        /// </summary>
        public virtual AssociationProvider Association { get; protected set; }

        /// <summary>
        /// The table that this column belongs to
        /// </summary>
        public TableProvider Table { get; private set; }

        /// <summary>
        /// The PropertyInfo of the property that represents this column on the entity type
        /// </summary>
        public virtual PropertyInfo EntityTypeProperty { get; protected set; }

        /// <summary>
        /// This is set for columns that are part of a foreign key. Note that it is NOT set for
        /// the strongly typed entity ref columns (though those columns 'use' one or more columns
        /// where IsForeignKeyComponent is set).
        /// </summary>
        public virtual bool IsForeignKeyComponent { get; protected set; }
    }
}
