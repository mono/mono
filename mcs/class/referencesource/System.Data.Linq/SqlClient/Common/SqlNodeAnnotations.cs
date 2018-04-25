using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Associate annotations with SqlNodes.
    /// </summary>
    internal class SqlNodeAnnotations {
        Dictionary<SqlNode, List<SqlNodeAnnotation>> annotationMap = new Dictionary<SqlNode, List<SqlNodeAnnotation>>();
        Dictionary<Type, string> uniqueTypes = new Dictionary<Type, string>();

        /// <summary>
        /// Add an annotation to the given node.
        /// </summary>
        internal void Add(SqlNode node, SqlNodeAnnotation annotation) {
            List<SqlNodeAnnotation> list = null;
            
            if (!this.annotationMap.TryGetValue(node, out list)) {
                list = new List<SqlNodeAnnotation>();
                this.annotationMap[node]=list;
            }

            uniqueTypes[annotation.GetType()] = String.Empty;

            list.Add(annotation);
        }

        /// <summary>
        /// Gets the annotations for the given node. Null if none.
        /// </summary>
        internal List<SqlNodeAnnotation> Get(SqlNode node) {
            List<SqlNodeAnnotation> list = null;
            this.annotationMap.TryGetValue(node, out list);
            return list;
        }

        /// <summary>
        /// Whether the given node has annotations.
        /// </summary>
        internal bool NodeIsAnnotated(SqlNode node) {
            if (node == null)
                return false;
            return this.annotationMap.ContainsKey(node);
        }

        /// <summary>
        /// Checks whether there's at least one annotation of the given type.
        /// </summary>
        internal bool HasAnnotationType(Type type) {
            return this.uniqueTypes.ContainsKey(type);
        }
    }
}
