using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;

namespace System.Web.DynamicData.ModelProviders {
    /// <summary>
    /// Base provider class for tables.
    /// Each provider type (e.g. Linq To Sql, Entity Framework, 3rd party) extends this class.
    /// </summary>
    public abstract class TableProvider {
        private Type _rootEntityType;
        private string _dataContextPropertyName;

        internal TableProvider() {
            // for unit testing
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="model">the model this table belongs to</param>
        protected TableProvider(DataModelProvider model) {
            if (model == null) {
                throw new ArgumentNullException("model");
            }

            DataModel = model;
        }

        /// <summary>
        /// readable representation
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString() {
            // To help identifying objects in debugger
            return Name ?? base.ToString();
        }

        /// <summary>
        /// Provides access to attributes defined for the table represented by this provider.
        /// </summary>
        public virtual AttributeCollection Attributes {
            get {
                return GetTypeDescriptor().GetAttributes();
            }
        }

        public virtual ICustomTypeDescriptor GetTypeDescriptor() {
            return TypeDescriptor.GetProvider(EntityType).GetTypeDescriptor(EntityType);
        }

        /// <summary>
        /// The name of the table.  Typically, this is the name of the property in the data context class
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// The CLR type that represents this table
        /// </summary>
        public virtual Type EntityType { get; protected set; }

        /// <summary>
        /// The collection of columns in this table
        /// </summary>
        public abstract ReadOnlyCollection<ColumnProvider> Columns { get; }

        /// <summary>
        /// The IQueryable that returns the elements of this table
        /// </summary>
        public abstract IQueryable GetQuery(object context);

        /// <summary>
        /// The data model provider that this table is part of
        /// </summary>
        public DataModelProvider DataModel { get; internal set; }

        /// <summary>
        /// Get the value of a foreign key for a given row. By default, it just looks up a property by that name
        /// </summary>
        public virtual object EvaluateForeignKey(object row, string foreignKeyName) {
            return System.Web.UI.DataBinder.GetPropertyValue(row, foreignKeyName);
        }

        /// <summary>
        /// Return the parent type of this entity's inheritance hierarchy; if the type is at the top
        /// of an inheritance hierarchy or does not have any inheritance, will return null.
        /// </summary>
        public virtual Type ParentEntityType { get; protected set; }

        /// <summary>
        /// Return the root type of this entity's inheritance hierarchy; if the type is at the top
        /// of an inheritance hierarchy or does not have any inheritance, will return EntityType.
        /// </summary>
        public virtual Type RootEntityType {
            get {
                return _rootEntityType ?? EntityType;
            }
            protected set {
                _rootEntityType = value;
            }
        }

        /// <summary>
        /// Name of table coming from the property on the data context. E.g. the value is "Products" for
        /// a table that is part of the NorthwindDataContext.Products collection. If this value has not
        /// been set, it will return the value of the Name property.
        /// </summary>
        public virtual string DataContextPropertyName {
            get {
                return _dataContextPropertyName ?? Name;
            }
            protected set {
                _dataContextPropertyName = value;
            }
        }

        /// <summary>
        /// Returns whether the passed in user is allowed to delete items from the table
        /// </summary>
        public virtual bool CanDelete(IPrincipal principal) {
            if (principal == null) {
                throw new ArgumentNullException("principal");
            }
            return true;
        }

        /// <summary>
        /// Returns whether the passed in user is allowed to insert into the table
        /// </summary>
        public virtual bool CanInsert(IPrincipal principal) {
            if (principal == null) {
                throw new ArgumentNullException("principal");
            }
            return true;
        }

        /// <summary>
        /// Returns whether the passed in user is allowed to read from the table
        /// </summary>
        public virtual bool CanRead(IPrincipal principal) {
            if (principal == null) {
                throw new ArgumentNullException("principal");
            }
            return true;
        }

        /// <summary>
        /// Returns whether the passed in user is allowed to make changes tothe table
        /// </summary>
        public virtual bool CanUpdate(IPrincipal principal) {
            if (principal == null) {
                throw new ArgumentNullException("principal");
            }
            return true;
        }
    }
}
