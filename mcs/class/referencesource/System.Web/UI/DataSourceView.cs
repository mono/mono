//------------------------------------------------------------------------------
// <copyright file="DataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.Collections;
    using System.ComponentModel;
    using System.Security.Permissions;

    public abstract class DataSourceView {

        private static readonly object EventDataSourceViewChanged = new object();

        private EventHandlerList _events;
        private string _name;


        protected DataSourceView(IDataSource owner, string viewName) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }
            if (viewName == null) {
                throw new ArgumentNullException("viewName");
            }

            _name = viewName;

            DataSourceControl dataSourceControl = owner as DataSourceControl;
            if (dataSourceControl != null) {
                dataSourceControl.DataSourceChangedInternal += new EventHandler(OnDataSourceChangedInternal);
            }
            else {
                owner.DataSourceChanged += new EventHandler(OnDataSourceChangedInternal);
            }
        }

        // CanX properties indicate whether the data source allows each
        // operation, and if so, whether it's appropriate to do so.
        // For instance, a control may allow Deletion, but if a required Delete
        // command isn't set, CanDelete should be false, because a Delete
        // operation would fail.

        public virtual bool CanDelete {
            get {
                return false;
            }
        }


        public virtual bool CanInsert {
            get {
                return false;
            }
        } 


        public virtual bool CanPage {
            get {
                return false;
            }
        }


        public virtual bool CanRetrieveTotalRowCount {
            get {
                return false;
            }
        }
        

        public virtual bool CanSort {
            get {
                return false;
            }
        }


        public virtual bool CanUpdate {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// Indicates the list of event handler delegates for the view. This property is read-only.
        /// </devdoc>
        protected EventHandlerList Events {
            get {
                if (_events == null) {
                    _events = new EventHandlerList();
                }
                return _events;
            }
        }


        public string Name {
            get {
                return _name;
            }
        }


        public event EventHandler DataSourceViewChanged {
            add {
                Events.AddHandler(EventDataSourceViewChanged, value);
            }
            remove {
                Events.RemoveHandler(EventDataSourceViewChanged, value);
            }
        }

        public virtual bool CanExecute(string commandName) {
            return false;
        }

        public virtual void Delete(IDictionary keys, IDictionary oldValues, DataSourceViewOperationCallback callback) {
            if (callback == null) {
                throw new ArgumentNullException("callback");
            }
            
            int affectedRecords = 0;
            bool performedCallback = false;

            try {
                affectedRecords = ExecuteDelete(keys, oldValues);
            }
            catch (Exception ex) {
                performedCallback = true;
                if (!callback(affectedRecords, ex)) {
                    throw;
                }
            }
            finally {
                if (!performedCallback) {
                    callback(affectedRecords, null);
                }
            }
        }

        public virtual void ExecuteCommand(string commandName, IDictionary keys, IDictionary values, DataSourceViewOperationCallback callback) {
            if (callback == null) {
                throw new ArgumentNullException("callback");
            }

            int affectedRecords = 0;
            bool performedCallback = false;

            try {
                affectedRecords = ExecuteCommand(commandName, keys, values);
            }
            catch (Exception ex) {
                performedCallback = true;
                if (!callback(affectedRecords, ex)) {
                    throw;
                }
            }
            finally {
                if (!performedCallback) {
                    callback(affectedRecords, null);
                }
            }
        }

        protected virtual int ExecuteCommand(string commandName, IDictionary keys, IDictionary values) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Performs a delete operation on the specified list. This is only
        /// supported by a DataSourceControl when CanDelete returns true.
        /// </summary>
        /// <param name="keys">
        /// The set of name/value pairs used to filter
        /// the items in the list that should be deleted.
        /// </param>
        /// <param name="oldValues">
        /// The complete set of name/value pairs used to filter
        /// the items in the list that should be deleted.
        /// </param>
        /// <returns>
        /// The number of items that were affected by the operation.
        /// </returns>
        protected virtual int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Performs an insert operation on the specified list. This is only
        /// supported by a DataControl when CanInsert is true.
        /// </summary>
        /// <param name="values">
        /// The set of name/value pairs to be used to initialize
        /// a new item in the list.
        /// </param>
        /// <returns>
        /// The number of items that were affected by the operation.
        /// </returns>
        protected virtual int ExecuteInsert(IDictionary values) {
            throw new NotSupportedException();
        }


        /// <devdoc>
        /// </devdoc>
        protected internal abstract IEnumerable ExecuteSelect(DataSourceSelectArguments arguments);
        

        /// <summary>
        /// Performs an update operation on the specified list. This is only
        /// supported by a DataControl when CanUpdate is true.
        /// </summary>
        /// <param name="keys">
        /// The set of name/value pairs used to filter
        /// the items in the list that should be updated.
        /// </param>
        /// <param name="values">
        /// The set of name/value pairs to be used to update the
        /// items in the list.
        /// </param>
        /// <param name="oldValues">
        /// The set of name/value pairs to be used to identify the
        /// item to be updated.
        /// </param>
        /// <returns>
        /// The number of items that were affected by the operation.
        /// </returns>
        protected virtual int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
            throw new NotSupportedException();
        }

        private void OnDataSourceChangedInternal(object sender, EventArgs e) {
            OnDataSourceViewChanged(e);
        }


        protected virtual void OnDataSourceViewChanged(EventArgs e) {
            EventHandler handler = Events[EventDataSourceViewChanged] as EventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        public virtual void Insert(IDictionary values, DataSourceViewOperationCallback callback) {
            if (callback == null) {
                throw new ArgumentNullException("callback");
            }
            
            int affectedRecords = 0;
            bool performedCallback = false;

            try {
                affectedRecords = ExecuteInsert(values);
            }
            catch (Exception ex) {
                performedCallback = true;
                if (!callback(affectedRecords, ex)) {
                    throw;
                }
            }
            finally {
                if (!performedCallback) {
                    callback(affectedRecords, null);
                }
            }
        }



        protected internal virtual void RaiseUnsupportedCapabilityError(DataSourceCapabilities capability) {
            if (!CanPage && ((capability & DataSourceCapabilities.Page) != 0)) {
                throw new NotSupportedException(SR.GetString(SR.DataSourceView_NoPaging));
            }
        
            if (!CanSort && ((capability & DataSourceCapabilities.Sort) != 0)) {
                throw new NotSupportedException(SR.GetString(SR.DataSourceView_NoSorting));
            }
        
            if (!CanRetrieveTotalRowCount && ((capability & DataSourceCapabilities.RetrieveTotalRowCount) != 0)) {
                throw new NotSupportedException(SR.GetString(SR.DataSourceView_NoRowCount));
            }
        }

        public virtual void Select(DataSourceSelectArguments arguments, DataSourceViewSelectCallback callback) {
            if (callback == null) {
                throw new ArgumentNullException("callback");
            }
            callback(ExecuteSelect(arguments));
        }

        public virtual void Update(IDictionary keys, IDictionary values, IDictionary oldValues, DataSourceViewOperationCallback callback) {
            if (callback == null) {
                throw new ArgumentNullException("callback");
            }
            
            int affectedRecords = 0;
            bool performedCallback = false;

            try {
                affectedRecords = ExecuteUpdate(keys, values, oldValues);
            }
            catch (Exception ex) {
                performedCallback = true;
                if (!callback(affectedRecords, ex)) {
                    throw;
                }
            }
            finally {
                if (!performedCallback) {
                    callback(affectedRecords, null);
                }
            }
        }
    }
}
