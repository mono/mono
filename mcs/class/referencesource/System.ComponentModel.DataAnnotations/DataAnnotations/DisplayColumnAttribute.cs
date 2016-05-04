using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// Sets the display column, the sort column, and the sort order for when a table is used as a parent table in FK relationships.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want users to be able to extend this class")]
    public class DisplayColumnAttribute : Attribute {
        public DisplayColumnAttribute(string displayColumn)
            : this(displayColumn, null) {
        }

        public DisplayColumnAttribute(string displayColumn, string sortColumn)
            : this(displayColumn, sortColumn, false) {
        }

        public DisplayColumnAttribute(string displayColumn, string sortColumn, bool sortDescending) {
            this.DisplayColumn = displayColumn;
            this.SortColumn = sortColumn;
            this.SortDescending = sortDescending;
        }

        public string DisplayColumn { get; private set; }

        public string SortColumn { get; private set; }

        public bool SortDescending { get; private set; }
    }
}
