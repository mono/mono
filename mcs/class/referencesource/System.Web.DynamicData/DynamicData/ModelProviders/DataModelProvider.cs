using System.Collections.ObjectModel;

namespace System.Web.DynamicData.ModelProviders {
    /// <summary>
    /// Base data model provider class 
    /// Each provider type (e.g. Linq To Sql, Entity Framework, 3rd party) extends this class.
    /// </summary>
    public abstract class DataModelProvider {
        /// <summary>
        ///  The list of tables exposed by this data model
        /// </summary>
        public abstract ReadOnlyCollection<TableProvider> Tables { get; }

        /// <summary>
        ///  The type of the data context
        /// </summary>
        public virtual Type ContextType { get; protected set; }

        /// <summary>
        /// Create an instance of the data context
        /// </summary>
        public abstract object CreateContext();
    }
}
