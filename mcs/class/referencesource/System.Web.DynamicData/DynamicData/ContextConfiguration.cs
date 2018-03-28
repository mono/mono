using System.Collections.Generic;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData {
    /// <summary>
    /// Allows for providing extra config information to a context
    /// </summary>
    public class ContextConfiguration {
        /// <summary>
        /// An optional factory for obtaining a metadata source for a given entity type
        /// </summary>
        public Func<Type, TypeDescriptionProvider> MetadataProviderFactory { get; set; }

        /// <summary>
        /// scaffold all tables
        /// </summary>
        public bool ScaffoldAllTables { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This can be set by the user to support inline dictionary intializers")]
        public ContextConfiguration() {
            MetadataProviderFactory = type => new AssociatedMetadataTypeTypeDescriptionProvider(type);
        }
    }
}
