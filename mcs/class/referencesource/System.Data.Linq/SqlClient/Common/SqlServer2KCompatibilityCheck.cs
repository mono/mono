using System;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Methods for checking whethe a query was compatible with the
    /// server it will be sent to.
    /// </summary>
    static internal class SqlServerCompatibilityCheck {

        /// <summary>
        /// Private visitor class checks each node for compatibility annotations.
        /// </summary>
        private class Visitor : SqlVisitor {

            private SqlProvider.ProviderMode provider;
            internal SqlNodeAnnotations annotations;

            internal Visitor(SqlProvider.ProviderMode provider) {
                this.provider = provider;
            }

            /// <summary>
            /// The reasons why this query is not 2K compatible.
            /// </summary>
            internal Collection<string> reasons = new Collection<string>();

            internal override SqlNode Visit(SqlNode node) {
                if (annotations.NodeIsAnnotated(node)) {
                    foreach (SqlNodeAnnotation annotation in annotations.Get(node)) {
                        SqlServerCompatibilityAnnotation ssca = annotation as SqlServerCompatibilityAnnotation;
                        if (ssca != null && ssca.AppliesTo(provider)) {
                            reasons.Add(annotation.Message);
                        }
                    }
                }
                return base.Visit(node);
            }
        }

        /// <summary>
        /// Checks whether the given node is supported on the given server.
        /// </summary>
        internal static void ThrowIfUnsupported(SqlNode node, SqlNodeAnnotations annotations, SqlProvider.ProviderMode provider) {
            // Check to see whether there's at least one SqlServerCompatibilityAnnotation.
            if (annotations.HasAnnotationType(typeof(SqlServerCompatibilityAnnotation))) {
                Visitor visitor = new Visitor(provider);
                visitor.annotations = annotations;
                visitor.Visit(node);

                // If any messages were recorded, then throw an exception.
                if (visitor.reasons.Count > 0) {
                    throw Error.ExpressionNotSupportedForSqlServerVersion(visitor.reasons);
                }
            }
        }
    }
}
