using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Annotation which indicates that the given node will cause a compatibility problem
    /// for the indicated set of providers.
    /// </summary>
    internal class SqlServerCompatibilityAnnotation : SqlNodeAnnotation {
        SqlProvider.ProviderMode[] providers;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The compatibility message.</param>
        /// <param name="providers">The set of providers this compatibility issue applies to.</param>
        internal SqlServerCompatibilityAnnotation(string message, params SqlProvider.ProviderMode[] providers)
            : base(message) {
            this.providers = providers;
        }

        /// <summary>
        /// Returns true if this annotation applies to the specified provider.
        /// </summary>
        internal bool AppliesTo(SqlProvider.ProviderMode provider) {
            foreach (SqlProvider.ProviderMode p in providers) {
                if (p == provider) {
                    return true;
                }
            }
            return false;
        }
    }
}
