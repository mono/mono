namespace System.Web.UI.WebControls {

    /// <summary>
    /// Represents data that is passed into an CallingDataMethodsEventHandler delegate.
    /// </summary>
    public class CallingDataMethodsEventArgs : EventArgs {

        /// <summary>
        /// Set this property when the data methods are static methods on a type.
        /// When this property is set, <see cref='System.Web.UI.WebControls.CallingDataMethodsEventArgs.DataMethodsObject' /> should not be set.
        /// </summary>
        public Type DataMethodsType {
            get;
            set;
        }

        /// <summary>
        /// Set this property with the actual instance where the data methods are present.
        /// When this property is set, <see cref='System.Web.UI.WebControls.CallingDataMethodsEventArgs.DataMethodsType' /> should not be set.
        /// </summary>
        public object DataMethodsObject {
            get;
            set;
        }
    }
}
