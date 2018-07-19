//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Compilation;
    using System.Web.Query.Dynamic;
    using System.Web.Resources;
    using System.Security;
    using System.Security.Permissions;
    using DynamicValidatorEventArgs = System.Web.DynamicData.DynamicValidatorEventArgs;
    using DynamicDataSourceOperation = System.Web.DynamicData.DynamicDataSourceOperation;

    public partial class LinqDataSourceView : ContextDataSourceView {                
        private static readonly object EventDeleted = new object();
        private static readonly object EventDeleting = new object();
        private static readonly object EventException = new object();
        private static readonly object EventInserted = new object();
        private static readonly object EventInserting = new object();
        private static readonly object EventUpdated = new object();
        private static readonly object EventUpdating = new object();

        private HttpContext _context;
        private Type _contextType;
        private string _contextTypeName;
        private LinqDataSource _owner;
        private List<ContextDataSourceContextData> _selectContexts;
        private bool _enableDelete;
        private bool _enableInsert;
        private bool _enableObjectTracking = true;
        private bool _enableUpdate;
        private bool _isNewContext;
        private ILinqToSql _linqToSql;
        private bool _reuseSelectContext;
        private bool _storeOriginalValuesInViewState = true;
        private bool _storeOriginalValues;
        private object _selectResult;

        public LinqDataSourceView(LinqDataSource owner, string name, HttpContext context)
            : this(owner, name, context, new DynamicQueryableWrapper(), new LinqToSqlWrapper()) {
        }

        // internal constructor that takes mocks for unit tests.
        internal LinqDataSourceView(LinqDataSource owner, string name, HttpContext context,
                                    IDynamicQueryable dynamicQueryable, ILinqToSql linqToSql)
            : base(owner, name, context, dynamicQueryable) {
            _context = context;
            _owner = owner;
            _linqToSql = linqToSql;
        }

        public override bool CanDelete {
            get {
                return EnableDelete;
            }
        }

        public override bool CanInsert {
            get {
                return EnableInsert;
            }
        }

        // When AutoPage is false the user should manually page in the Selecting event.
        public override bool CanPage {
            get {
                return true;
            }
        }

        // When AutoPage is false the user must set the total row count in the Selecting event.
        public override bool CanRetrieveTotalRowCount {
            get {
                return true;
            }
        }

        // When AutoSort is false the user should manually sort in the Selecting event.
        public override bool CanSort {
            get {
                return true;
            }
        }

        public override bool CanUpdate {
            get {
                return EnableUpdate;
            }
        }

        public override Type ContextType {
            [SecuritySafeCritical]
            get {
                if (_contextType == null) {
                    string typeName = ContextTypeName;
                    if (String.IsNullOrEmpty(typeName)) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.LinqDataSourceView_ContextTypeNameNotSpecified, _owner.ID));
                    }
                    try {
                        _contextType = BuildManager.GetType(typeName, /*throwOnFail*/true, /*ignoreCase*/true);
                    }
                    catch (Exception e) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.LinqDataSourceView_ContextTypeNameNotFound, _owner.ID), e);
                    }
                }
                return _contextType;
            }
        }

        public override string ContextTypeName {
            get {
                return _contextTypeName ?? String.Empty;
            }
            set {
                if (_contextTypeName != value) {
                    if (_reuseSelectContext) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.LinqDataSourceView_ContextTypeNameChanged, _owner.ID));
                    }
                    _contextTypeName = value;
                    _contextType = null;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public bool EnableDelete {
            get {
                return _enableDelete;
            }
            set {
                if (_enableDelete != value) {
                    _enableDelete = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public bool EnableInsert {
            get {
                return _enableInsert;
            }
            set {
                if (_enableInsert != value) {
                    _enableInsert = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public bool EnableObjectTracking {
            get {
                return _enableObjectTracking;
            }
            set {
                if (_enableObjectTracking != value) {
                    if (_reuseSelectContext) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.LinqDataSourceView_EnableObjectTrackingChanged, _owner.ID));
                    }
                    _enableObjectTracking = value;
                }
            }
        }

        public bool EnableUpdate {
            get {
                return _enableUpdate;
            }
            set {
                if (_enableUpdate != value) {
                    _enableUpdate = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public bool StoreOriginalValuesInViewState {
            get {
                return _storeOriginalValuesInViewState;
            }
            set {
                if (_storeOriginalValuesInViewState != value) {
                    _storeOriginalValuesInViewState = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public string TableName {
            get {
                return EntitySetName;
            }
            set {
                if (EntitySetName != value) {
                    if (_reuseSelectContext) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.LinqDataSourceView_TableNameChanged, _owner.ID));
                    }                    
                    EntitySetName = value;                    
                }
            }
        }

        public event EventHandler<LinqDataSourceStatusEventArgs> ContextCreated {
            add {
                Events.AddHandler(EventContextCreated, value);
            }
            remove {
                Events.RemoveHandler(EventContextCreated, value);
            }
        }

        public event EventHandler<LinqDataSourceContextEventArgs> ContextCreating {
            add {
                Events.AddHandler(EventContextCreating, value);
            }
            remove {
                Events.RemoveHandler(EventContextCreating, value);
            }
        }

        public event EventHandler<LinqDataSourceDisposeEventArgs> ContextDisposing {
            add {
                Events.AddHandler(EventContextDisposing, value);
            }
            remove {
                Events.RemoveHandler(EventContextDisposing, value);
            }
        }

        public event EventHandler<LinqDataSourceStatusEventArgs> Deleted {
            add {
                Events.AddHandler(EventDeleted, value);
            }
            remove {
                Events.RemoveHandler(EventDeleted, value);
            }
        }

        public event EventHandler<LinqDataSourceDeleteEventArgs> Deleting {
            add {
                Events.AddHandler(EventDeleting, value);
            }
            remove {
                Events.RemoveHandler(EventDeleting, value);
            }
        }

        internal event EventHandler<DynamicValidatorEventArgs> Exception {
            add {
                Events.AddHandler(EventException, value);
            }
            remove {
                Events.RemoveHandler(EventException, value);
            }
        }

        public event EventHandler<LinqDataSourceStatusEventArgs> Inserted {
            add {
                Events.AddHandler(EventInserted, value);
            }
            remove {
                Events.RemoveHandler(EventInserted, value);
            }
        }

        public event EventHandler<LinqDataSourceInsertEventArgs> Inserting {
            add {
                Events.AddHandler(EventInserting, value);
            }
            remove {
                Events.RemoveHandler(EventInserting, value);
            }
        }

        public event EventHandler<LinqDataSourceStatusEventArgs> Selected {
            add {
                Events.AddHandler(EventSelected, value);
            }
            remove {
                Events.RemoveHandler(EventSelected, value);
            }
        }

        public event EventHandler<LinqDataSourceSelectEventArgs> Selecting {
            add {
                Events.AddHandler(EventSelecting, value);
            }
            remove {
                Events.RemoveHandler(EventSelecting, value);
            }
        }

        public event EventHandler<LinqDataSourceStatusEventArgs> Updated {
            add {
                Events.AddHandler(EventUpdated, value);
            }
            remove {
                Events.RemoveHandler(EventUpdated, value);
            }
        }

        public event EventHandler<LinqDataSourceUpdateEventArgs> Updating {
            add {
                Events.AddHandler(EventUpdating, value);
            }
            remove {
                Events.RemoveHandler(EventUpdating, value);
            }
        }

        protected virtual object CreateContext(Type contextType) {
            return DataSourceHelper.CreateObjectInstance(contextType);
        }

        protected override ContextDataSourceContextData CreateContext(DataSourceOperation operation) {
            if (operation == DataSourceOperation.Select) {
                return CreateContextAndTableForSelect();
            }
            return CreateContextAndTableForEdit(operation);            
        }        

        private ContextDataSourceContextData CreateContextAndTable(DataSourceOperation operation) {
            ContextDataSourceContextData contextData = null;            
            bool eventFired = false;
            try {
                LinqDataSourceContextEventArgs contextEventArgs = new LinqDataSourceContextEventArgs(operation);
                OnContextCreating(contextEventArgs);

                contextData = new ContextDataSourceContextData(contextEventArgs.ObjectInstance);
                Type contextType = null;
                MemberInfo tableMemberInfo = null;
                if (contextData.Context == null) {
                    // construct the context unless accessing a static table for Select.
                    contextType = ContextType;
                    tableMemberInfo = GetTableMemberInfo(contextType);
                    if (tableMemberInfo != null) {
                        if (MemberIsStatic(tableMemberInfo)) {
                            if (operation != DataSourceOperation.Select) {
                                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                                    AtlasWeb.LinqDataSourceView_TableCannotBeStatic, TableName, contextType.Name, _owner.ID));
                            }
                        }
                        else {
                            contextData.Context = CreateContext(contextType);
                            _isNewContext = true;                            
                        }
                    }
                }
                else {
                    // use the manually constructed context.
                    tableMemberInfo = GetTableMemberInfo(contextData.Context.GetType());
                }

                // fetch the table from the context.
                if (tableMemberInfo != null) {
                    FieldInfo field = tableMemberInfo as FieldInfo;
                    if (field != null) {
                        contextData.EntitySet = field.GetValue(contextData.Context);
                    }
                    PropertyInfo property = tableMemberInfo as PropertyInfo;
                    if (property != null) {
                        contextData.EntitySet = property.GetValue(contextData.Context, null);
                    }
                }
                if (contextData.EntitySet == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        AtlasWeb.LinqDataSourceView_TableNameNotFound, TableName, contextType.Name, _owner.ID));
                }
            }
            catch (Exception e) {
                eventFired = true;
                LinqDataSourceStatusEventArgs createdEventArgs = new LinqDataSourceStatusEventArgs(e);
                OnContextCreated(createdEventArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.ContextCreate));
                // CreateContextAndTable will return null if this exception is handled.
                if (!createdEventArgs.ExceptionHandled) {
                    throw;
                }
            }
            finally {
                if (!eventFired) {
                    // contextData can be null if exception thrown from ContextCreating handler.
                    object context = (contextData == null) ? null : contextData.Context;
                    LinqDataSourceStatusEventArgs createdEventArgs = new LinqDataSourceStatusEventArgs(context);
                    OnContextCreated(createdEventArgs);
                }
            }
            return contextData;
        }

        private ContextDataSourceContextData CreateContextAndTableForEdit(DataSourceOperation operation) {
            ContextDataSourceContextData contextData = CreateContextAndTable(operation);
            // context data may be null or incomplete if an exception was handled
            if (contextData != null) {
                if (contextData.Context == null) {
                    return null;
                }
                if (contextData.EntitySet == null) {
                    DisposeContext(contextData.Context);
                    return null;
                }
                ValidateContextType(contextData.Context.GetType(), false);
                ValidateTableType(contextData.EntitySet.GetType(), false);
            }
            return contextData;
        }

        private ContextDataSourceContextData CreateContextAndTableForSelect() {
            _isNewContext = false;

            if (_selectContexts == null) {
                _selectContexts = new List<ContextDataSourceContextData>();
            }
            else if (_reuseSelectContext && _selectContexts.Count > 0) {
                return _selectContexts[_selectContexts.Count - 1];
            }

            // context data may be null if an exception was handled
            ContextDataSourceContextData contextData = CreateContextAndTable(DataSourceOperation.Select);
            if (contextData != null) {
                if (contextData.Context != null) {
                    ValidateContextType(contextData.Context.GetType(), true);
                }
                if (contextData.EntitySet != null) {
                    ValidateTableType(contextData.EntitySet.GetType(), true);
                }

                _selectContexts.Add(contextData);

                // context may not be dlinq context or may be null if table was static.
                DataContext dlinqContext = contextData.Context as DataContext;
                if ((dlinqContext != null) && _isNewContext) {
                    dlinqContext.ObjectTrackingEnabled = EnableObjectTracking;
                }
                // don't reuse dlinq contexts that cache data or exterior changes will not be reflected.
                _reuseSelectContext = (dlinqContext == null) || !EnableObjectTracking;
            }
            return contextData;
        }        

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object",
            Justification = "Names are consistent with those used in the ObjectDataSource classes")]
        protected virtual void DeleteDataObject(object dataContext, object table, object oldDataObject) {
            _linqToSql.Attach((ITable)table, oldDataObject);
            _linqToSql.Remove((ITable)table, oldDataObject);
            _linqToSql.SubmitChanges((DataContext)dataContext);
        }

        protected override int DeleteObject(object oldEntity) {
            LinqDataSourceDeleteEventArgs deleteEventArgs = new LinqDataSourceDeleteEventArgs(oldEntity);
            OnDeleting(deleteEventArgs);
            if (deleteEventArgs.Cancel) {
                return -1;
            }

            LinqDataSourceStatusEventArgs deletedEventArgs = null;
            try {
                DeleteDataObject(Context, EntitySet, deleteEventArgs.OriginalObject);
            }
            catch (Exception e) {
                // allow user to handle dlinq exceptions including OnValidate validation.
                deletedEventArgs = new LinqDataSourceStatusEventArgs(e);
                OnDeleted(deletedEventArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Delete));
                if (deletedEventArgs.ExceptionHandled) {
                    return -1;
                }
                throw;
            }
            deletedEventArgs = new LinqDataSourceStatusEventArgs(deleteEventArgs.OriginalObject);
            OnDeleted(deletedEventArgs);

            return 1;
        }

        protected override void DisposeContext(object dataContext) {
            if (dataContext != null) {
                LinqDataSourceDisposeEventArgs disposingEventArgs = new LinqDataSourceDisposeEventArgs(dataContext);
                OnContextDisposing(disposingEventArgs);
                if (!disposingEventArgs.Cancel) {
                    base.DisposeContext(dataContext);
                }
            }
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
            ValidateDeleteSupported(keys, oldValues);
            return base.ExecuteDelete(keys, oldValues);
        }

        protected override int ExecuteInsert(IDictionary values) {
            ValidateInsertSupported(values);
            return base.ExecuteInsert(values);
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
            ValidateUpdateSupported(keys, values, oldValues);
            return base.ExecuteUpdate(keys, values, oldValues);
        }
        
        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
            ClearOriginalValues();

            QueryContext queryContext = CreateQueryContext(arguments);
            object table = GetSource(queryContext);
            IList result = null;

            if (_selectResult != null) {
                try {
                    IQueryable query = QueryableDataSourceHelper.AsQueryable(_selectResult);
                    query = ExecuteQuery(query, queryContext);

                    Type dataObjectType = GetDataObjectType(query.GetType());
                    result = query.ToList(dataObjectType);

                    if (_storeOriginalValues) {
                        ITable dlinqTable = table as ITable;
                        // We can store original values if the type is exact or derived
                        if ((dlinqTable != null) && dataObjectType.IsAssignableFrom(EntityType)) {
                            StoreOriginalValues(result);
                        }
                    }
                }
                catch (Exception e) {
                    result = null;
                    LinqDataSourceStatusEventArgs selectedEventArgs = new LinqDataSourceStatusEventArgs(e);
                    OnSelected(selectedEventArgs);
                    OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Select));
                    if (!selectedEventArgs.ExceptionHandled) {
                        throw;
                    }
                }
                finally {
                    if (result != null) {
                        int totalRowCount = -1; // paging performed, but row count not available.
                        if (arguments.RetrieveTotalRowCount) {
                            totalRowCount = arguments.TotalRowCount;
                        }
                        else if (!AutoPage) {
                            totalRowCount = result.Count;
                        }
                        LinqDataSourceStatusEventArgs selectedEventArgs = new LinqDataSourceStatusEventArgs(result, totalRowCount);
                        OnSelected(selectedEventArgs);
                    }
                }
                // Null out the select context
                Context = null;
            }
            return result;
        }        

        protected override object GetSource(QueryContext context) {
            LinqDataSourceSelectEventArgs selectEventArgs = new LinqDataSourceSelectEventArgs(
                context.Arguments,
                context.WhereParameters,
                context.OrderByParameters,
                context.GroupByParameters,
                context.OrderGroupsByParameters,
                context.SelectParameters);

            OnSelecting(selectEventArgs);
            if (selectEventArgs.Cancel) {
                return null;
            }

            _selectResult = selectEventArgs.Result;
            object table = _selectResult;

            // Original values should only be stored for valid delete and update scenarios.
            _storeOriginalValues = StoreOriginalValuesInViewState && (CanDelete || CanUpdate) &&
                String.IsNullOrEmpty(GroupBy) && String.IsNullOrEmpty(SelectNew);

            if (_selectResult == null) {
                table = base.GetSource(context);
                _selectResult = table;
            }
            // If the provided select result was not a DLinq table and we need to store
            // original values then we must get the table and create a new data context
            // instance so that we can access the column metadata.
            else if (!(table is ITable) && _storeOriginalValues) {
                table = base.GetSource(context);
            }

            return table;
        }

        protected virtual MemberInfo GetTableMemberInfo(Type contextType) {
            string tableName = TableName;
            if (String.IsNullOrEmpty(tableName)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.LinqDataSourceView_TableNameNotSpecified, _owner.ID));
            }

            MemberInfo[] members = contextType.FindMembers(MemberTypes.Field | MemberTypes.Property,
                                                           BindingFlags.Public | BindingFlags.Instance |
                                                           BindingFlags.Static, /*filter*/null, /*filterCriteria*/null);

            for (int i = 0; i < members.Length; i++) {
                if (String.Equals(members[i].Name, tableName, StringComparison.OrdinalIgnoreCase)) {
                    return members[i];
                }
            }
            return null;
        }                                                            

        private ReadOnlyCollection<MetaDataMember> GetTableMetaDataMembers(ITable table, Type dataObjectType) {
            DataContext context = ((ITable)table).Context;
            MetaModel contextMetaData = context.Mapping;
            MetaTable tableMetaData = contextMetaData.GetTable(dataObjectType);
            MetaType rowMetaData = tableMetaData.Model.GetMetaType(dataObjectType);
            return rowMetaData.DataMembers;
        }

        protected override void HandleValidationErrors(IDictionary<string, Exception> errors, DataSourceOperation operation) {
            LinqDataSourceValidationException exception = new LinqDataSourceValidationException(String.Format(CultureInfo.InvariantCulture,
                AtlasWeb.LinqDataSourceView_ValidationFailed,
                EntityType, errors.Values.First().Message),
                errors);

            bool exceptionHandled = false;

            switch (operation) {
                case DataSourceOperation.Delete:
                    LinqDataSourceDeleteEventArgs deleteEventArgs = new LinqDataSourceDeleteEventArgs(exception);
                    OnDeleting(deleteEventArgs);
                    OnException(new DynamicValidatorEventArgs(exception, DynamicDataSourceOperation.Delete));
                    exceptionHandled = deleteEventArgs.ExceptionHandled;
                    break;

                case DataSourceOperation.Insert:
                    LinqDataSourceInsertEventArgs insertEventArgs = new LinqDataSourceInsertEventArgs(exception);
                    OnInserting(insertEventArgs);
                    OnException(new DynamicValidatorEventArgs(exception, DynamicDataSourceOperation.Insert));
                    exceptionHandled = insertEventArgs.ExceptionHandled;
                    break;
                case DataSourceOperation.Update:
                    // allow user to handle conversion or dlinq property validation exceptions.
                    LinqDataSourceUpdateEventArgs updateEventArgs = new LinqDataSourceUpdateEventArgs(exception);
                    OnUpdating(updateEventArgs);
                    OnException(new DynamicValidatorEventArgs(exception, DynamicDataSourceOperation.Update));
                    exceptionHandled = updateEventArgs.ExceptionHandled;
                    break;
            }

            if (!exceptionHandled) {
                throw exception;
            }
        } 

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object",
            Justification = "Names are consistent with those used in the ObjectDataSource classes")]
        protected virtual void InsertDataObject(object dataContext, object table, object newDataObject) {
            _linqToSql.Add((ITable)table, newDataObject);
            _linqToSql.SubmitChanges((DataContext)dataContext);
        }

        protected override int InsertObject(object newEntity) {
            LinqDataSourceInsertEventArgs insertEventArgs = new LinqDataSourceInsertEventArgs(newEntity);
            OnInserting(insertEventArgs);
            if (insertEventArgs.Cancel) {
                return -1;
            }

            LinqDataSourceStatusEventArgs insertedEventArgs = null;
            try {
                InsertDataObject(Context, EntitySet, insertEventArgs.NewObject);
            }
            catch (Exception e) {
                // allow user to handle dlinq exceptions including OnValidate validation.
                insertedEventArgs = new LinqDataSourceStatusEventArgs(e);
                OnInserted(insertedEventArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Insert));
                if (insertedEventArgs.ExceptionHandled) {
                    return -1;
                }
                throw;
            }
            insertedEventArgs = new LinqDataSourceStatusEventArgs(insertEventArgs.NewObject);
            OnInserted(insertedEventArgs);

            return 1;
        }

        private static bool MemberIsStatic(MemberInfo member) {
            FieldInfo field = member as FieldInfo;
            if (field != null) {
                return field.IsStatic;
            }
            PropertyInfo property = member as PropertyInfo;
            if (property != null) {
                MethodInfo propertyGetter = property.GetGetMethod();
                return ((propertyGetter != null) && propertyGetter.IsStatic);
            }
            return false;
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnContextCreated(LinqDataSourceStatusEventArgs e) {
            EventHandler<LinqDataSourceStatusEventArgs> handler = (EventHandler<LinqDataSourceStatusEventArgs>)Events[EventContextCreated];
            if (handler != null) {
                handler(this, e);
            }
        }        

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnContextCreating(LinqDataSourceContextEventArgs e) {
            EventHandler<LinqDataSourceContextEventArgs> handler = (EventHandler<LinqDataSourceContextEventArgs>)Events[EventContextCreating];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnContextDisposing(LinqDataSourceDisposeEventArgs e) {
            EventHandler<LinqDataSourceDisposeEventArgs> handler = (EventHandler<LinqDataSourceDisposeEventArgs>)Events[EventContextDisposing];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnDeleted(LinqDataSourceStatusEventArgs e) {
            EventHandler<LinqDataSourceStatusEventArgs> handler =
                (EventHandler<LinqDataSourceStatusEventArgs>)Events[EventDeleted];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnDeleting(LinqDataSourceDeleteEventArgs e) {
            EventHandler<LinqDataSourceDeleteEventArgs> handler =
                (EventHandler<LinqDataSourceDeleteEventArgs>)Events[EventDeleting];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnException(DynamicValidatorEventArgs e) {
            EventHandler<DynamicValidatorEventArgs> handler = (EventHandler<DynamicValidatorEventArgs>)Events[EventException];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnInserted(LinqDataSourceStatusEventArgs e) {
            EventHandler<LinqDataSourceStatusEventArgs> handler =
                (EventHandler<LinqDataSourceStatusEventArgs>)Events[EventInserted];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnInserting(LinqDataSourceInsertEventArgs e) {
            EventHandler<LinqDataSourceInsertEventArgs> handler =
                (EventHandler<LinqDataSourceInsertEventArgs>)Events[EventInserting];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnSelected(LinqDataSourceStatusEventArgs e) {
            EventHandler<LinqDataSourceStatusEventArgs> handler =
                (EventHandler<LinqDataSourceStatusEventArgs>)Events[EventSelected];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnSelecting(LinqDataSourceSelectEventArgs e) {
            EventHandler<LinqDataSourceSelectEventArgs> handler =
                (EventHandler<LinqDataSourceSelectEventArgs>)Events[EventSelecting];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnUpdated(LinqDataSourceStatusEventArgs e) {
            EventHandler<LinqDataSourceStatusEventArgs> handler =
                (EventHandler<LinqDataSourceStatusEventArgs>)Events[EventUpdated];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnUpdating(LinqDataSourceUpdateEventArgs e) {
            EventHandler<LinqDataSourceUpdateEventArgs> handler =
                (EventHandler<LinqDataSourceUpdateEventArgs>)Events[EventUpdating];
            if (handler != null) {
                handler(this, e);
            }
        }        

        internal void ReleaseSelectContexts() {
            if (_selectContexts != null) {
                foreach (ContextDataSourceContextData contextData in _selectContexts) {
                    DisposeContext(contextData.Context);
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters",
            Justification = "Names are consistent with those used in the ObjectDataSource classes")]
        protected virtual void ResetDataObject(object table, object dataObject) {
            // DevDiv Bugs 187705, and 114508: Resetting is no longer necessary because
            // select has it's own context, but this method is kept for compatibility purposes.

            // no-op
        }

        public IEnumerable Select(DataSourceSelectArguments arguments) {
            return ExecuteSelect(arguments);
        }

        private Dictionary<string, Exception> SetDataObjectProperties(object oldDataObject, object newDataObject) {
            Dictionary<string, Exception> validateExceptions = null;

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(oldDataObject);
            foreach (PropertyDescriptor property in properties) {
                if (property.PropertyType.IsSerializable && !property.IsReadOnly) {
                    object newValue = property.GetValue(newDataObject);
                    try {
                        property.SetValue(oldDataObject, newValue);
                    }
                    catch (Exception e) {
                        if (validateExceptions == null) {
                            validateExceptions = new Dictionary<string, Exception>(StringComparer.OrdinalIgnoreCase);
                        }
                        validateExceptions[property.Name] = e;
                    }
                }
            }

            return validateExceptions;
        }

        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods",
                         Justification = "System.Data.Linq assembly will be changing to support partial trust.")]
        protected override void StoreOriginalValues(IList results) {
            Type entityType = EntityType;
            IDictionary<string, MetaDataMember> columns = GetTableMetaDataMembers((ITable)EntitySet, entityType).ToDictionary(c => c.Member.Name);

            StoreOriginalValues(results, p => columns.ContainsKey(p.Name) && 
                                                (columns[p.Name].IsPrimaryKey || 
                                                columns[p.Name].IsVersion || 
                                                (columns[p.Name].UpdateCheck != UpdateCheck.Never)));
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters",
            Justification = "Names are consistent with those used in the ObjectDataSource classes")]
        protected virtual void UpdateDataObject(object dataContext, object table,
                                                object oldDataObject, object newDataObject) {
            _linqToSql.Attach((ITable)table, oldDataObject);
            Dictionary<string, Exception> validateExceptions = SetDataObjectProperties(oldDataObject, newDataObject);

            // package up dlinq validation exceptions into single exception.
            if (validateExceptions != null) {
                throw new LinqDataSourceValidationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.LinqDataSourceView_ValidationFailed, oldDataObject.GetType(), validateExceptions.Values.First().Message), validateExceptions);
            }

            _linqToSql.SubmitChanges((DataContext)dataContext);
        }

        protected override int UpdateObject(object oldEntity, object newEntity) {
            LinqDataSourceUpdateEventArgs updateEventArgs = new LinqDataSourceUpdateEventArgs(oldEntity, newEntity);
            OnUpdating(updateEventArgs);
            if (updateEventArgs.Cancel) {
                return -1;
            }

            LinqDataSourceStatusEventArgs updatedEventArgs = null;
            try {
                UpdateDataObject(Context, EntitySet, updateEventArgs.OriginalObject, updateEventArgs.NewObject);
            }
            catch (Exception e) {
                ResetDataObject(EntitySet, updateEventArgs.OriginalObject);
                // allow user to handle dlinq exceptions including OnValidate validation.
                updatedEventArgs = new LinqDataSourceStatusEventArgs(e);
                OnUpdated(updatedEventArgs);
                OnException(new DynamicValidatorEventArgs(e, DynamicDataSourceOperation.Update));
                if (updatedEventArgs.ExceptionHandled) {
                    return -1;
                }
                throw;
            }
            updatedEventArgs = new LinqDataSourceStatusEventArgs(updateEventArgs.NewObject);
            OnUpdated(updatedEventArgs);

            return 1;
        }

        protected virtual void ValidateContextType(Type contextType, bool selecting) {
            if (!selecting && !typeof(DataContext).IsAssignableFrom(contextType)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.LinqDataSourceView_InvalidContextType, _owner.ID));
            }
        }

        protected virtual void ValidateDeleteSupported(IDictionary keys, IDictionary oldValues) {
            if (!CanDelete) {
                throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture,
                AtlasWeb.LinqDataSourceView_DeleteNotSupported, _owner.ID));
            }
            ValidateEditSupported();
        }

        protected virtual void ValidateEditSupported() {
            if (!String.IsNullOrEmpty(GroupBy)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.LinqDataSourceView_GroupByNotSupportedOnEdit, _owner.ID));
            }
            if (!String.IsNullOrEmpty(SelectNew)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.LinqDataSourceView_SelectNewNotSupportedOnEdit, _owner.ID));
            }
        }

        protected virtual void ValidateInsertSupported(IDictionary values) {
            if (!CanInsert) {
                throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture,
                AtlasWeb.LinqDataSourceView_InsertNotSupported, _owner.ID));
            }
            ValidateEditSupported();
            if ((values == null) || (values.Count == 0)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.LinqDataSourceView_InsertRequiresValues, _owner.ID));
            }
        }

        protected virtual void ValidateTableType(Type tableType, bool selecting) {
            if (!selecting) {
                if (!(tableType.IsGenericType && tableType.GetGenericArguments().Length == 1 && typeof(ITable).IsAssignableFrom(tableType))) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        AtlasWeb.LinqDataSourceView_InvalidTablePropertyType, _owner.ID));
                }
            }
        }

        protected virtual void ValidateUpdateSupported(IDictionary keys, IDictionary values, IDictionary oldValues) {
            if (!CanUpdate) {
                throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture,
                AtlasWeb.LinqDataSourceView_UpdateNotSupported, _owner.ID));
            }
            ValidateEditSupported();
        }
    }

}

