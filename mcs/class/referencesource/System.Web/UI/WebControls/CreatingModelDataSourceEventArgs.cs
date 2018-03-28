namespace System.Web.UI.WebControls {

    /// <summary>
    /// Represents data that is passed into an <see cref='System.Web.UI.WebControls.CreatingModelDataSourceEventHandler' /> delegate.
    /// </summary>
    public class CreatingModelDataSourceEventArgs : EventArgs {

        /// <summary>
        /// The <see cref='System.Web.UI.WebControls.ModelDataSource' /> used for data operations.
        /// </summary>
        public ModelDataSource ModelDataSource {
            get;
            set;
        }
    }
}
