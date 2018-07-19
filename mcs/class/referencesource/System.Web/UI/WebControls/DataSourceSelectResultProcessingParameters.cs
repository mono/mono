namespace System.Web.UI.WebControls {
    /// <summary>
    /// This is used to encapsulate the options required for processing a select method result from a <see cref="System.Web.UI.WebControls.ModelDataSourceView"/> select operation.
    /// </summary>
    public class DataSourceSelectResultProcessingOptions {
        /// <summary>
        /// Indicates whether paging is done automatically by <see cref="System.Web.UI.WebControls.ModelDataSourceView"/>.
        /// </summary>
        public bool AutoPage { get; set; }

        /// <summary>
        /// Indicates whether sorting is done automatically by <see cref="System.Web.UI.WebControls.ModelDataSourceView"/>.
        /// </summary>
        public bool AutoSort { get; set; }

        /// <summary>
        /// The type which is used for auto paging and sorting. 
        /// </summary>
        public Type ModelType { get; set; }
    }
}
