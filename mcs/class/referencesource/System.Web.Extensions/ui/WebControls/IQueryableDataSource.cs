namespace System.Web.UI.WebControls {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    public interface IQueryableDataSource : IDataSource {
        /// <summary>
        /// Raises OnDataSourceViewChanged
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification="An event exists already and it it protected")]
        void RaiseViewChanged();

        event EventHandler<QueryCreatedEventArgs> QueryCreated;
    }
}
