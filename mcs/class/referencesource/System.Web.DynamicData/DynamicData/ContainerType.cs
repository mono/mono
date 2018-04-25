namespace System.Web.DynamicData {

    /// <summary>
    /// A data control container type
    /// </summary>
    public enum ContainerType {
        /// <summary>
        /// A list container, such as ListView, GridView, Repeater (or a control implementing IDataBoundListControl)
        /// </summary>
        List,
        /// <summary>
        /// An item container, such as DetailsView, FormView (or a control implementing IDataBoundItemControl)
        /// </summary>
        Item
    }
}
