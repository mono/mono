using System.Security.Permissions;

namespace System.Web.DynamicData {
    /// <summary>
    /// Allows for overriding the name of a table. (What previously TableOptions was used for).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TableNameAttribute : Attribute {
        /// <summary>
        /// The new name of the table
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="name">the new name override</param>
        public TableNameAttribute(string name) {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }
            Name = name;
        }
    }
}
