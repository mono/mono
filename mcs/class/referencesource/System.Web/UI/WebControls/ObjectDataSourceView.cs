//------------------------------------------------------------------------------
// <copyright file="ObjectDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Text;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.Util;

    using ConflictOptions = System.Web.UI.ConflictOptions;


    /// <devdoc>
    /// Represents a single view of an ObjectDataSource.
    /// </devdoc>
    public class ObjectDataSourceView : DataSourceView, IStateManager {

        private static readonly object EventDeleted = new object();
        private static readonly object EventDeleting = new object();
        private static readonly object EventFiltering = new object();
        private static readonly object EventInserted = new object();
        private static readonly object EventInserting = new object();
        private static readonly object EventObjectCreated = new object();
        private static readonly object EventObjectCreating = new object();
        private static readonly object EventObjectDisposing = new object();
        private static readonly object EventSelected = new object();
        private static readonly object EventSelecting = new object();
        private static readonly object EventUpdated = new object();
        private static readonly object EventUpdating = new object();

        private HttpContext _context;
        private ObjectDataSource _owner;
        private bool _tracking;

        private ConflictOptions _conflictDetection = ConflictOptions.OverwriteChanges;
        private bool _convertNullToDBNull;
        private string _dataObjectTypeName;
        private string _deleteMethod;
        private ParameterCollection _deleteParameters;
        private bool _enablePaging;
        private string _filterExpression;
        private ParameterCollection _filterParameters;
        private string _insertMethod;
        private ParameterCollection _insertParameters;
        private string _maximumRowsParameterName;
        private string _oldValuesParameterFormatString;
        private string _selectCountMethod;
        private string _selectMethod;
        private ParameterCollection _selectParameters;
        private string _sortParameterName;
        private string _startRowIndexParameterName;
        private string _typeName;
        private string _updateMethod;
        private ParameterCollection _updateParameters;


        /// <devdoc>
        /// Creates a new ObjectDataSourceView.
        /// </devdoc>
        public ObjectDataSourceView(ObjectDataSource owner, string name, HttpContext context)
            : base(owner, name) {
            Debug.Assert(owner != null);
            _owner = owner;
            _context = context;
        }



        /// <devdoc>
        /// Indicates that the view can delete rows.
        /// </devdoc>
        public override bool CanDelete {
            get {
                return (DeleteMethod.Length != 0);
            }
        }


        /// <devdoc>
        /// Indicates that the view can add new rows.
        /// </devdoc>
        public override bool CanInsert {
            get {
                return (InsertMethod.Length != 0);
            }
        }

        /// <devdoc>
        /// Indicates that the view can do server paging.
        /// </devdoc>
        public override bool CanPage {
            get {
                return EnablePaging;
            }
        }

        /// <devdoc>
        /// Indicates that the view can return the total number of rows returned by the query.
        /// </devdoc>
        public override bool CanRetrieveTotalRowCount {
            get {
                // We don't necessarily know if the data source can get the total row count until after the SelectMethod has
                // been executed (e.g. it might return a DataSet). If the SelectCountMethod is set, we can definitely
                // get the count. However, if it is not, we can only get the row count from the Selected data if it was not
                // paged by the user's object, and it happened to be a DataView, DataSet, DataTable, ICollection, or object.
                return ((SelectCountMethod.Length > 0) || (!EnablePaging));
            }
        }


        /// <devdoc>
        /// Indicates that the view can sort rows.
        /// </devdoc>
        public override bool CanSort {
            get {
                // We don't really know if the data source can sort until after the SelectMethod has
                // been executed (e.g. it might return a DataSet), so we just assume it's true.
                return true;
                //return (SortParameterName.Length > 0);
            }
        }


        /// <devdoc>
        /// Indicates that the view can update rows.
        /// </devdoc>
        public override bool CanUpdate {
            get {
                return (UpdateMethod.Length != 0);
            }
        }

        /// <devdoc>
        /// Whether the delete command passes old values in the parameter collection.
        /// </devdoc>
        public ConflictOptions ConflictDetection {
            get {
                return _conflictDetection;
            }
            set {
                if ((value < ConflictOptions.OverwriteChanges) || (value > ConflictOptions.CompareAllValues)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _conflictDetection = value;
                OnDataSourceViewChanged(EventArgs.Empty);
            }
        }

        /// <devdoc>
        /// Whether null values passed into insert/update/delete operations
        /// will be converted to System.DbNull.
        /// </devdoc>
        public bool ConvertNullToDBNull {
            get {
                return _convertNullToDBNull;
            }
            set {
                _convertNullToDBNull = value;
            }
        }


        /// <devdoc>
        /// An optional type that is used for update, insert, and delete
        /// scenarios where the object's methods take in an aggregate object
        /// rather than one parameter for each property in the selected data.
        /// </devdoc>
        public string DataObjectTypeName {
            get {
                if (_dataObjectTypeName == null) {
                    return String.Empty;
                }
                return _dataObjectTypeName;
            }
            set {
                if (DataObjectTypeName != value) {
                    _dataObjectTypeName = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        /// The method to execute when Delete() is called.
        /// </devdoc>
        public string DeleteMethod {
            get {
                if (_deleteMethod == null) {
                    return String.Empty;
                }
                return _deleteMethod;
            }
            set {
                _deleteMethod = value;
            }
        }

        /// <devdoc>
        // Collection of parameters used when calling the DeleteMethod. These parameters are merged with the parameters provided by data-bound controls.

        /// </devdoc>
        public ParameterCollection DeleteParameters {
            get {
                if (_deleteParameters == null) {
                    _deleteParameters = new ParameterCollection();
                }
                return _deleteParameters;
            }
        }

        /// <devdoc>
        /// Indicates whether the Select method supports paging. If this is set to true, the
        /// StartRowIndexParameterName and MaximumRowsParameterName properties must be set to the
        /// names of the parameters of the Select method that accept the values for the starting
        /// record to retrieve and the number of records to retrieve.
        /// </devdoc>
        public bool EnablePaging {
            get {
                return _enablePaging;
            }
            set {
                if (EnablePaging != value) {
                    _enablePaging = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        /// Filter expression used when Select() is called. Filtering is only available when the SelectMethod returns a DataSet or a DataTable.
        /// </devdoc>
        public string FilterExpression {
            get {
                if (_filterExpression == null) {
                    return String.Empty;
                }
                return _filterExpression;
            }
            set {
                if (FilterExpression != value) {
                    _filterExpression = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        /// Collection of parameters used in the FilterExpression property. Filtering is only available when the SelectMethod returns a DataSet or a DataTable.
        /// </devdoc>
        public ParameterCollection FilterParameters {
            get {
                if (_filterParameters == null) {
                    _filterParameters = new ParameterCollection();

                    _filterParameters.ParametersChanged += new EventHandler(SelectParametersChangedEventHandler);

                    if (_tracking) {
                        ((IStateManager)_filterParameters).TrackViewState();
                    }
                }
                return _filterParameters;
            }
        }


        /// <devdoc>
        /// The method to execute when Insert() is called.
        /// </devdoc>
        public string InsertMethod {
            get {
                if (_insertMethod == null) {
                    return String.Empty;
                }
                return _insertMethod;
            }
            set {
                _insertMethod = value;
            }
        }


        /// <devdoc>
        /// Collection of values used when calling the InsertMethod. These parameters are merged with the parameters provided by data-bound controls.
        /// </devdoc>
        public ParameterCollection InsertParameters {
            get {
                if (_insertParameters == null) {
                    _insertParameters = new ParameterCollection();
                }
                return _insertParameters;
            }
        }

        /// <devdoc>
        /// Returns whether this object is tracking view state.
        /// </devdoc>
        protected bool IsTrackingViewState {
            get {
                return _tracking;
            }
        }

        /// <devdoc>
        /// When EnablePaging is set to true, this property indicates the parameter of the Select
        /// method that accepts the value for the number of records to retrieve.
        /// </devdoc>
        public string MaximumRowsParameterName {
            get {
                if (_maximumRowsParameterName == null) {
                    return "maximumRows";
                }
                return _maximumRowsParameterName;
            }
            set {
                if (MaximumRowsParameterName != value) {
                    _maximumRowsParameterName = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// The format string applied to the names of the old values parameters
        /// </devdoc>
        [
        DefaultValue("{0}"),
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_OldValuesParameterFormatString),
        ]
        public string OldValuesParameterFormatString {
            get {
                if (_oldValuesParameterFormatString == null) {
                    return "{0}";
                }
                return _oldValuesParameterFormatString;
            }
            set {
                _oldValuesParameterFormatString = value;
                OnDataSourceViewChanged(EventArgs.Empty);
            }
        }

        /// <devdoc>
        /// The method to execute when the total row count is needed. This method is only
        /// used when EnablePaging is true and is optional.
        /// </devdoc>
        public string SelectCountMethod {
            get {
                if (_selectCountMethod == null) {
                    return String.Empty;
                }
                return _selectCountMethod;
            }
            set {
                if (SelectCountMethod != value) {
                    _selectCountMethod = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// The method to execute when Select() is called.
        /// </devdoc>
        public string SelectMethod {
            get {
                if (_selectMethod == null) {
                    return String.Empty;
                }
                return _selectMethod;
            }
            set {
                if (SelectMethod != value) {
                    _selectMethod = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// Collection of parameters used when calling the SelectMethod.
        /// </devdoc>
        public ParameterCollection SelectParameters {
            get {
                if (_selectParameters == null) {
                    _selectParameters = new ParameterCollection();

                    _selectParameters.ParametersChanged += new EventHandler(SelectParametersChangedEventHandler);

                    if (_tracking) {
                        ((IStateManager)_selectParameters).TrackViewState();
                    }
                }
                return _selectParameters;
            }
        }

        /// <devdoc>
        /// The name of the parameter in the SelectMethod that specifies the
        /// sort expression. This parameter's value will be automatically set
        /// at runtime with the appropriate sort expression.
        /// </devdoc>
        public string SortParameterName {
            get {
                if (_sortParameterName == null) {
                    return String.Empty;
                }
                return _sortParameterName;
            }
            set {
                if (SortParameterName != value) {
                    _sortParameterName = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// When EnablePaging is set to true, this property indicates the parameter of the Select
        /// method that accepts the value for the number of first record to retrieve.
        /// </devdoc>
        public string StartRowIndexParameterName {
            get {
                if (_startRowIndexParameterName == null) {
                    return "startRowIndex";
                }
                return _startRowIndexParameterName;
            }
            set {
                if (StartRowIndexParameterName != value) {
                    _startRowIndexParameterName = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// The type that contains the methods specified in this control.
        /// </devdoc>
        public string TypeName {
            get {
                if (_typeName == null) {
                    return String.Empty;
                }
                return _typeName;
            }
            set {
                if (TypeName != value) {
                    _typeName = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// The method to execute when Update() is called.
        /// </devdoc>
        public string UpdateMethod {
            get {
                if (_updateMethod == null) {
                    return String.Empty;
                }
                return _updateMethod;
            }
            set {
                _updateMethod = value;
            }
        }

        /// <devdoc>
        /// Collection of parameters and values used when calling the UpdateMethod. These parameters are merged with the parameters provided by data-bound controls.
        /// </devdoc>
        public ParameterCollection UpdateParameters {
            get {
                if (_updateParameters == null) {
                    _updateParameters = new ParameterCollection();
                }
                return _updateParameters;
            }
        }

        /// <summary>
        /// Indicates which <see cref='System.Globalization.CultureInfo'/> is used by ObjectDataSourceView
        /// when converting string values to actual types of properties of data object.
        /// </summary>
        public ParsingCulture ParsingCulture {
            get;
            set;
        }


        /// <devdoc>
        /// This event is raised after the Delete operation has completed.
        /// Handle this event if you need to examine the return values of
        /// the method call, or examine an exception that may have been thrown.
        /// </devdoc>
        public event ObjectDataSourceStatusEventHandler Deleted {
            add {
                Events.AddHandler(EventDeleted, value);
            }
            remove {
                Events.RemoveHandler(EventDeleted, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the Delete operation has been executed.
        /// Handle this event if you need to validate the values of parameters or
        /// change their values.
        /// </devdoc>
        public event ObjectDataSourceMethodEventHandler Deleting {
            add {
                Events.AddHandler(EventDeleting, value);
            }
            remove {
                Events.RemoveHandler(EventDeleting, value);
            }
        }

        public event ObjectDataSourceFilteringEventHandler Filtering {
            add {
                Events.AddHandler(EventFiltering, value);
            }
            remove {
                Events.RemoveHandler(EventFiltering, value);
            }
        }

        /// <devdoc>
        /// This event is raised after the Insert operation has completed.
        /// Handle this event if you need to examine the return values of
        /// the method call, or examine an exception that may have been thrown.
        /// </devdoc>
        public event ObjectDataSourceStatusEventHandler Inserted {
            add {
                Events.AddHandler(EventInserted, value);
            }
            remove {
                Events.RemoveHandler(EventInserted, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the Insert operation has been executed.
        /// Handle this event if you need to validate the values of parameters or
        /// change their values.
        /// </devdoc>
        public event ObjectDataSourceMethodEventHandler Inserting {
            add {
                Events.AddHandler(EventInserting, value);
            }
            remove {
                Events.RemoveHandler(EventInserting, value);
            }
        }

        /// <devdoc>
        /// This event is raised after the instance of the object has been created.
        /// Handle this event if you need to set additional properties on the
        /// object before any other methods are called. This event will not be
        /// raised if a custom instance was provided in the ObjectCreating event.
        /// </devdoc>
        public event ObjectDataSourceObjectEventHandler ObjectCreated {
            add {
                Events.AddHandler(EventObjectCreated, value);
            }
            remove {
                Events.RemoveHandler(EventObjectCreated, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the instance of the object has been created.
        /// Handle this event if you need to call a non-default constructor on the
        /// object. Set the ObjectInstance property of the EventArgs with the
        /// custom instance. If this is set, the ObjectCreated event will not be
        /// raised.
        /// </devdoc>
        public event ObjectDataSourceObjectEventHandler ObjectCreating {
            add {
                Events.AddHandler(EventObjectCreating, value);
            }
            remove {
                Events.RemoveHandler(EventObjectCreating, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the instance of the object is disposed.
        /// Handle this event if you need to retrieve properties on the
        /// object before it is disposed. If the object implements the IDispoable
        /// interface, then the Dispose() method will be called automatically.
        /// Set the Cancel property of the event args to true if you do not want
        /// IDisposable.Dispose() to be called automatically.
        /// </devdoc>
        public event ObjectDataSourceDisposingEventHandler ObjectDisposing {
            add {
                Events.AddHandler(EventObjectDisposing, value);
            }
            remove {
                Events.RemoveHandler(EventObjectDisposing, value);
            }
        }

        /// <devdoc>
        /// This event is raised after the Select operation has completed.
        /// Handle this event if you need to examine the return values of
        /// the method call, or examine an exception that may have been thrown.
        /// </devdoc>
        public event ObjectDataSourceStatusEventHandler Selected {
            add {
                Events.AddHandler(EventSelected, value);
            }
            remove {
                Events.RemoveHandler(EventSelected, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the Select operation has been executed.
        /// Handle this event if you need to validate the values of parameters or
        /// change their values.
        /// </devdoc>
        public event ObjectDataSourceSelectingEventHandler Selecting {
            add {
                Events.AddHandler(EventSelecting, value);
            }
            remove {
                Events.RemoveHandler(EventSelecting, value);
            }
        }

        /// <devdoc>
        /// This event is raised after the Update operation has completed.
        /// Handle this event if you need to examine the return values of
        /// the method call, or examine an exception that may have been thrown.
        /// </devdoc>
        public event ObjectDataSourceStatusEventHandler Updated {
            add {
                Events.AddHandler(EventUpdated, value);
            }
            remove {
                Events.RemoveHandler(EventUpdated, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the Update operation has been executed.
        /// Handle this event if you need to validate the values of parameters or
        /// change their values.
        /// </devdoc>
        public event ObjectDataSourceMethodEventHandler Updating {
            add {
                Events.AddHandler(EventUpdating, value);
            }
            remove {
                Events.RemoveHandler(EventUpdating, value);
            }
        }


        /// <devdoc>
        /// Creates a data object and sets the properties specified by name/value
        /// pairs in the dictionary.
        /// </devdoc>
        private object BuildDataObject(Type dataObjectType, IDictionary inputParameters) {
            Debug.Assert(inputParameters != null, "Did not expect null parameter dictionary");
            Debug.Assert(dataObjectType != null, "Did not expect null DataObjectType");

            object dataObject = Activator.CreateInstance(dataObjectType);

            Debug.Assert(dataObject != null, "We should never get back a null instance of the DataObject if the creation succeeded.");

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(dataObject);
            foreach (DictionaryEntry de in inputParameters) {
                // 
                string propName = (de.Key == null ? String.Empty : de.Key.ToString());
                PropertyDescriptor pd = props.Find(propName, true);
                if (pd == null) {
                    throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_DataObjectPropertyNotFound, propName, _owner.ID));
                }
                if (pd.IsReadOnly) {
                    throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_DataObjectPropertyReadOnly, propName, _owner.ID));
                }
                object value = BuildObjectValue(de.Value, pd.PropertyType, propName, ParsingCulture);
                pd.SetValue(dataObject, value);
            }

            return dataObject;
        }

        /// <devdoc>
        /// Builds a strongly typed value to be passed into a method either as a parameter
        /// or as part of a data object. This will attempt to perform appropriate type
        /// conversions based on the destination type and throw exceptions on fatal errors.
        /// </devdoc>
        private static object BuildObjectValue(object value, Type destinationType, string paramName, ParsingCulture parsingCulture) {
            // Only consider converting the type if the value is non-null and the types don't match
            if (value != null && (!destinationType.IsInstanceOfType(value))) {
                Type innerDestinationType = destinationType;
                bool isNullable = false;
                if (destinationType.IsGenericType && (destinationType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                    innerDestinationType = destinationType.GetGenericArguments()[0];
                    isNullable = true;
                }
                else {
                    if (destinationType.IsByRef) {
                        innerDestinationType = destinationType.GetElementType();
                    }
                }

                // Try to convert from for example string to DateTime, so that
                // afterwards we can convert DateTime to Nullable<DateTime>

                // If the value is a string, we attempt to use a TypeConverter to convert it
                value = ConvertType(value, innerDestinationType, paramName, parsingCulture);

                // Special-case the value when the destination is Nullable<T>
                if (isNullable) {
                    Type paramValueType = value.GetType();
                    if (innerDestinationType != paramValueType) {
                        // Throw if for example, we are trying to convert from int to Nullable<bool>
                        throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_CannotConvertType, paramName, paramValueType.FullName, String.Format(CultureInfo.InvariantCulture, "Nullable<{0}>", destinationType.GetGenericArguments()[0].FullName)));
                    }
                }
            }
            return value;
        }

        /// <devdoc>
        /// Converts a type from a string representation to a strong type using
        /// an appropriate TypeConverter. If the value is not a string, the value
        /// is left unchanged.
        /// </devdoc>
        private static object ConvertType(object value, Type type, string paramName, ParsingCulture parsingCulture) {
            string s = value as string;
            if (s != null) {
                // Get the type converter for the destination type
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                Debug.Assert(converter != null);
                if (converter != null) {
                    // Perform the conversion
                    try {
                        if (parsingCulture == ParsingCulture.Current) {
                            value = converter.ConvertFromString(null, CultureInfo.CurrentCulture, s);
                        }
                        else {
                            value = converter.ConvertFromInvariantString(s);
                        }
                    }
                    catch (NotSupportedException) {
                        throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_CannotConvertType, paramName, typeof(string).FullName, type.FullName));
                    }
                    catch (FormatException) {
                        throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_CannotConvertType, paramName, typeof(string).FullName, type.FullName));
                    }
                }
            }
            return value;
        }

        /// <devdoc>
        /// Creates an IEnumerable from the given data object and validates that the
        /// DataSourceSelectArguments are valid for the conditions.
        /// </devdoc>
        private IEnumerable CreateEnumerableData(object dataObject, DataSourceSelectArguments arguments) {
            if (FilterExpression.Length > 0) {
                // Since this type is not valid for filtering, throw if there is a filter
                throw new NotSupportedException(SR.GetString(SR.ObjectDataSourceView_FilterNotSupported, _owner.ID));
            }

            if (!String.IsNullOrEmpty(arguments.SortExpression)) {
                throw new NotSupportedException(SR.GetString(SR.ObjectDataSourceView_SortNotSupportedOnIEnumerable, _owner.ID));
            }

            IEnumerable enumerable = dataObject as IEnumerable;
            if (enumerable != null) {
                // If object is an IEnumerable, return it as it is
                if (!EnablePaging && arguments.RetrieveTotalRowCount && SelectCountMethod.Length == 0) {
                    ICollection collection = enumerable as ICollection;
                    if (collection != null) {
                        arguments.TotalRowCount = collection.Count;
                    }
                }

                return enumerable;
            }
            else {
                // The result is neither a DataView, DataSet, DataTable, nor an IEnumerable, so we just wrap it in an IEnumerable
                if (arguments.RetrieveTotalRowCount && SelectCountMethod.Length == 0) {
                    arguments.TotalRowCount = 1;
                }

                return new object[1] { dataObject };
            }
        }

        /// <devdoc>
        /// Creates a filtered DataView with optional filtering.
        /// </devdoc>
        private IEnumerable CreateFilteredDataView(DataTable dataTable, string sortExpression, string filterExpression) {
            IOrderedDictionary parameterValues = FilterParameters.GetValues(_context, _owner);
            if (filterExpression.Length > 0) {
                ObjectDataSourceFilteringEventArgs filterArgs = new ObjectDataSourceFilteringEventArgs(parameterValues);
                OnFiltering(filterArgs);
                if (filterArgs.Cancel) {
                    return null;
                }
            }
            return FilteredDataSetHelper.CreateFilteredDataView(dataTable, sortExpression, filterExpression, parameterValues);
        }

        public int Delete(IDictionary keys, IDictionary oldValues) {
            return ExecuteDelete(keys, oldValues);
        }

        /// <devdoc>
        /// </devdoc>
        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
            if (!CanDelete) {
                throw new NotSupportedException(SR.GetString(SR.ObjectDataSourceView_DeleteNotSupported, _owner.ID));
            }

            Type type = GetType(TypeName);
            Debug.Assert(type != null, "Should not have a null type at this point");

            // Try to get the DataObject type. If we do get the type then we will construct
            // DataObjects representing the old values.
            Type dataObjectType = TryGetDataObjectType();

            ObjectDataSourceMethod method;

            if (dataObjectType != null) {
                // Build DataObject (Old)
                IDictionary caseInsensitiveOldValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

                MergeDictionaries(DeleteParameters, keys, caseInsensitiveOldValues);

                if (ConflictDetection == ConflictOptions.CompareAllValues) {
                    if (oldValues == null) {
                        throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_Pessimistic, SR.GetString(SR.DataSourceView_delete), _owner.ID, "oldValues"));
                    }
                    MergeDictionaries(DeleteParameters, oldValues, caseInsensitiveOldValues);
                }

                object oldDataObject = BuildDataObject(dataObjectType, caseInsensitiveOldValues);


                // Resolve and invoke the method
                method = GetResolvedMethodData(type, DeleteMethod, dataObjectType, oldDataObject, null, DataSourceOperation.Delete);

                ObjectDataSourceMethodEventArgs eventArgs = new ObjectDataSourceMethodEventArgs(method.Parameters);
                OnDeleting(eventArgs);
                if (eventArgs.Cancel) {
                    return 0;
                }
            }
            else {
                // Build parameter list that contains old values
                IOrderedDictionary caseInsensitiveAllValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

                string oldValuesParameterFormatString = OldValuesParameterFormatString;

                MergeDictionaries(DeleteParameters, DeleteParameters.GetValues(_context, _owner), caseInsensitiveAllValues);
                MergeDictionaries(DeleteParameters, keys, caseInsensitiveAllValues, oldValuesParameterFormatString);

                if (ConflictDetection == ConflictOptions.CompareAllValues) {
                    if (oldValues == null) {
                        throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_Pessimistic, SR.GetString(SR.DataSourceView_delete), _owner.ID, "oldValues"));
                    }
                    MergeDictionaries(DeleteParameters, oldValues, caseInsensitiveAllValues, oldValuesParameterFormatString);
                }

                ObjectDataSourceMethodEventArgs eventArgs = new ObjectDataSourceMethodEventArgs(caseInsensitiveAllValues);
                OnDeleting(eventArgs);
                if (eventArgs.Cancel) {
                    return 0;
                }

                // Resolve and invoke the method
                method = GetResolvedMethodData(type, DeleteMethod, caseInsensitiveAllValues, DataSourceOperation.Delete);
            }

            ObjectDataSourceResult result = InvokeMethod(method);

            // Invalidate the cache and raise the change event to notify bound controls
            if (_owner.Cache.Enabled) {
                _owner.InvalidateCacheEntry();
            }
            OnDataSourceViewChanged(EventArgs.Empty);

            return result.AffectedRows;
        }

        /// <devdoc>
        /// </devdoc>
        protected override int ExecuteInsert(IDictionary values) {
            if (!CanInsert) {
                throw new NotSupportedException(SR.GetString(SR.ObjectDataSourceView_InsertNotSupported, _owner.ID));
            }

            Type type = GetType(TypeName);
            Debug.Assert(type != null, "Should not have a null type at this point");

            // Try to get the DataObject type. If we do get the type then we will construct
            // DataObjects representing the new values.
            Type dataObjectType = TryGetDataObjectType();

            ObjectDataSourceMethod method;

            if (dataObjectType != null) {
                // Build DataObject (New)
                if (values == null || values.Count == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_InsertRequiresValues, _owner.ID));
                }

                IDictionary caseInsensitiveNewValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

                MergeDictionaries(InsertParameters, values, caseInsensitiveNewValues);

                object newDataObject = BuildDataObject(dataObjectType, caseInsensitiveNewValues);

                // Resolve and invoke the method
                method = GetResolvedMethodData(type, InsertMethod, dataObjectType, null, newDataObject, DataSourceOperation.Insert);

                ObjectDataSourceMethodEventArgs eventArgs = new ObjectDataSourceMethodEventArgs(method.Parameters);
                OnInserting(eventArgs);
                if (eventArgs.Cancel) {
                    return 0;
                }
            }
            else {
                // Build parameter list that contains new values

                IOrderedDictionary caseInsensitiveAllValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

                MergeDictionaries(InsertParameters, InsertParameters.GetValues(_context, _owner), caseInsensitiveAllValues);
                MergeDictionaries(InsertParameters, values, caseInsensitiveAllValues);

                ObjectDataSourceMethodEventArgs eventArgs = new ObjectDataSourceMethodEventArgs(caseInsensitiveAllValues);
                OnInserting(eventArgs);
                if (eventArgs.Cancel) {
                    return 0;
                }

                // Resolve and invoke the method
                method = GetResolvedMethodData(type, InsertMethod, caseInsensitiveAllValues, DataSourceOperation.Insert);
            }

            ObjectDataSourceResult result = InvokeMethod(method);

            // Invalidate the cache and raise the change event to notify bound controls
            if (_owner.Cache.Enabled) {
                _owner.InvalidateCacheEntry();
            }
            OnDataSourceViewChanged(EventArgs.Empty);

            return result.AffectedRows;
        }

        /// <devdoc>
        /// </devdoc>
        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
            if (SelectMethod.Length == 0) {
                throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_SelectNotSupported, _owner.ID));
            }

            if (CanSort) {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Sort);
            }
            if (CanPage) {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Page);
            }
            if (CanRetrieveTotalRowCount) {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.RetrieveTotalRowCount);
            }
            arguments.RaiseUnsupportedCapabilitiesError(this);


            // Copy the parameters into a case insensitive dictionary
            IOrderedDictionary mergedParameters = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            IDictionary selectParameters = SelectParameters.GetValues(_context, _owner);
            foreach (DictionaryEntry de in selectParameters) {
                mergedParameters[de.Key] = de.Value;
            }


            // If caching is enabled, load DataView, DataSet, DataTable, or IEnumerable from cache
            bool cacheEnabled = _owner.Cache.Enabled;
            if (cacheEnabled) {
                object cachedData = _owner.LoadDataFromCache(arguments.StartRowIndex, arguments.MaximumRows);
                if (cachedData != null) {
                    DataView dataView = cachedData as DataView;
                    if (dataView != null) {
                        if (arguments.RetrieveTotalRowCount && SelectCountMethod.Length == 0) {
                            arguments.TotalRowCount = dataView.Count;
                        }
                        if (FilterExpression.Length > 0) {
                            // Since this type is not valid for filtering, throw if there is a filter
                            throw new NotSupportedException(SR.GetString(SR.ObjectDataSourceView_FilterNotSupported, _owner.ID));
                        }
                        if (String.IsNullOrEmpty(arguments.SortExpression)) {
                            // If there is no sort expression, we can return the cached DataView
                            // If sorting is requested, we don't use the cached one and we go create a new one (and we'll
                            // most likely get exception, since caching+sorting+DataView=exception).
                            return dataView;
                        }

                        // Fall through as though the data wasn't in cache
                    }
                    else {
                        DataTable dataTable = FilteredDataSetHelper.GetDataTable(_owner, cachedData);
                        if (dataTable != null) {
                            // If we got back a DataTable from cache, return that
                            ProcessPagingData(arguments, mergedParameters);

                            return CreateFilteredDataView(dataTable, arguments.SortExpression, FilterExpression);
                        }
                        else {
                            // If we got back an IEnumerable from cache, return that
                            IEnumerable enumerableReturnValue = CreateEnumerableData(cachedData, arguments);

                            ProcessPagingData(arguments, mergedParameters);

                            return enumerableReturnValue;
                        }
                    }
                }
            }

            // We have to raise the Selecting event early on so that we respect
            // any changes the user has made to the DataSourceSelectArguments.
            ObjectDataSourceSelectingEventArgs eventArgs = new ObjectDataSourceSelectingEventArgs(mergedParameters, arguments, false);
            OnSelecting(eventArgs);
            if (eventArgs.Cancel) {
                return null;
            }

            // Create a copy of mergedParameters for queryRowCount that doesn't get all the Select, Sort, paging parameters
            OrderedDictionary queryRowCountParameters = new OrderedDictionary(mergedParameters.Count);
            foreach (DictionaryEntry entry in mergedParameters) {
                queryRowCountParameters.Add(entry.Key, entry.Value);
            }

            // NOTE: These special parameters are only added here so that they don't show up
            // for the SelectCount operation.

            // Add the sort expression as a parameter if necessary
            string sortParameterName = SortParameterName;
            if (sortParameterName.Length > 0) {
                mergedParameters[sortParameterName] = arguments.SortExpression;

                // We reset the sort expression here so that we pretend as
                // though we're not really sorting (since the developer is
                // worrying about it instead of us).
                arguments.SortExpression = String.Empty;
            }


            // Add the paging arguments as parameters if necessary
            if (EnablePaging) {
                string maximumRowsParameterName = MaximumRowsParameterName;
                string startRowIndexParameterName = StartRowIndexParameterName;
                if (String.IsNullOrEmpty(maximumRowsParameterName) ||
                    String.IsNullOrEmpty(startRowIndexParameterName)) {
                    throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_MissingPagingSettings, _owner.ID));
                }
                // Create a new dictionary with the paging information and merge it in (so we get type conversions)
                IDictionary pagingParameters = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                pagingParameters[maximumRowsParameterName] = arguments.MaximumRows;
                pagingParameters[startRowIndexParameterName] = arguments.StartRowIndex;
                MergeDictionaries(SelectParameters, pagingParameters, mergedParameters);
            }


            Type type = GetType(TypeName);
            Debug.Assert(type != null, "Should not have a null type at this point");

            object instance = null;
            ObjectDataSourceResult result = null;
            try {
                // Resolve and invoke the method
                ObjectDataSourceMethod method = GetResolvedMethodData(type, SelectMethod, mergedParameters, DataSourceOperation.Select);
                result = InvokeMethod(method, false, ref instance);

                // If the return value is null, there is no more processing to be done
                if (result.ReturnValue == null) {
                    return null;
                }

                // Get the total row count if it is requested
                if (arguments.RetrieveTotalRowCount && SelectCountMethod.Length > 0) {
                    int cachedTotalRowCount = -1;
                    if (cacheEnabled) {
                        cachedTotalRowCount = _owner.LoadTotalRowCountFromCache();
                        if (cachedTotalRowCount >= 0) {
                            arguments.TotalRowCount = cachedTotalRowCount;
                        }
                    }
                    if (cachedTotalRowCount < 0) {
                        cachedTotalRowCount = QueryTotalRowCount(queryRowCountParameters, arguments, true, ref instance);
                        arguments.TotalRowCount = cachedTotalRowCount;
                        if (cacheEnabled) {
                            _owner.SaveTotalRowCountToCache(cachedTotalRowCount);
                        }
                    }
                }
            }
            finally {
                if (instance != null) {
                    ReleaseInstance(instance);
                }
            }

            // Process the return value
            // Order of precedence: DataView, DataTable, DataSet, IEnumerable, <everything else>
            {
                // Check if the return value was a DataView
                DataView dataView = result.ReturnValue as DataView;
                if (dataView != null) {
                    if (arguments.RetrieveTotalRowCount && SelectCountMethod.Length == 0) {
                        arguments.TotalRowCount = dataView.Count;
                    }
                    if (FilterExpression.Length > 0) {
                        // Since this type is not valid for filtering, throw if there is a filter
                        throw new NotSupportedException(SR.GetString(SR.ObjectDataSourceView_FilterNotSupported, _owner.ID));
                    }

                    if (!String.IsNullOrEmpty(arguments.SortExpression)) {
                        if (cacheEnabled) {
                            // Since this type is not valid for caching, throw if caching is enabled
                            throw new NotSupportedException(SR.GetString(SR.ObjectDataSourceView_CacheNotSupportedOnSortedDataView, _owner.ID));
                        }
                        dataView.Sort = arguments.SortExpression;
                    }

                    // If caching is enabled, save data to cache
                    if (cacheEnabled) {
                        SaveDataAndRowCountToCache(arguments, result.ReturnValue);
                    }
                    return dataView;
                }
                else {
                    // Check if the return value was a DataSet or DataTable
                    DataTable dataTable = FilteredDataSetHelper.GetDataTable(_owner, result.ReturnValue);
                    if (dataTable != null) {
                        if (arguments.RetrieveTotalRowCount && SelectCountMethod.Length == 0) {
                            arguments.TotalRowCount = dataTable.Rows.Count;
                        }

                        // If caching is enabled, save data to cache
                        if (cacheEnabled) {
                            SaveDataAndRowCountToCache(arguments, result.ReturnValue);
                        }
                        // If we got a DataTable from the result, create a view with filtering and sorting
                        return CreateFilteredDataView(dataTable, arguments.SortExpression, FilterExpression);
                    }
                    else {
                        // CreateEnumerableData will get an appropriate IEnumerable from the data, and also
                        // validate that filtering and sorting are not used.
                        IEnumerable enumerableReturnValue = CreateEnumerableData(result.ReturnValue, arguments);

                        // If caching is enabled, save data to cache
                        if (cacheEnabled) {
                            if (enumerableReturnValue is IDataReader) {
                                // IDataReader is specifically not supported with caching since the contract
                                // is that they are forward-only (i.e. not resettable).
                                throw new NotSupportedException(SR.GetString(SR.ObjectDataSourceView_CacheNotSupportedOnIDataReader, _owner.ID));
                            }
                            SaveDataAndRowCountToCache(arguments, enumerableReturnValue);
                        }
                        return enumerableReturnValue;
                    }
                }
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
            if (!CanUpdate) {
                throw new NotSupportedException(SR.GetString(SR.ObjectDataSourceView_UpdateNotSupported, _owner.ID));
            }

            Type type = GetType(TypeName);
            Debug.Assert(type != null, "Should not have a null type at this point");

            // Try to get the DataObject type. If we do get the type then we will construct
            // DataObjects representing the new (and possibly old) values.
            Type dataObjectType = TryGetDataObjectType();

            ObjectDataSourceMethod method;

            if (dataObjectType != null) {
                if (ConflictDetection == ConflictOptions.CompareAllValues) {
                    // Build two DataObjects (Old + New)
                    if (oldValues == null) {
                        throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_Pessimistic, SR.GetString(SR.DataSourceView_update), _owner.ID, "oldValues"));
                    }

                    IDictionary caseInsensitiveNewValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                    IDictionary caseInsensitiveOldValues = null;

                    // We start out with the old values, just to pre-populate the list with items
                    // that might not have corresponding new values. For example if a GridView has
                    // a read-only field, there will be an old value, but no new value. The data object
                    // still has to have *some* value for a given field, so we just use the old value.
                    MergeDictionaries(UpdateParameters, oldValues, caseInsensitiveNewValues);
                    MergeDictionaries(UpdateParameters, keys, caseInsensitiveNewValues);
                    MergeDictionaries(UpdateParameters, values, caseInsensitiveNewValues);

                    // For optimistic updates we require that there be old values, and then
                    // we move them into a case-insensitive dictionary for merging.
                    if (oldValues == null) {
                        throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_Pessimistic, SR.GetString(SR.DataSourceView_update), _owner.ID, "oldValues"));
                    }
                    caseInsensitiveOldValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                    MergeDictionaries(UpdateParameters, oldValues, caseInsensitiveOldValues);
                    MergeDictionaries(UpdateParameters, keys, caseInsensitiveOldValues);

                    object newDataObject = BuildDataObject(dataObjectType, caseInsensitiveNewValues);
                    object oldDataObject = BuildDataObject(dataObjectType, caseInsensitiveOldValues);

                    // Resolve and invoke the method
                    method = GetResolvedMethodData(type, UpdateMethod, dataObjectType, oldDataObject, newDataObject, DataSourceOperation.Update);
                }
                else {
                    // Build one DataObject (New)

                    IDictionary caseInsensitiveNewValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

                    // We start out with the old values, just to pre-populate the list with items
                    // that might not have corresponding new values. For example if a GridView has
                    // a read-only field, there will be an old value, but no new value. The data object
                    // still has to have *some* value for a given field, so we just use the old value.
                    MergeDictionaries(UpdateParameters, oldValues, caseInsensitiveNewValues);
                    MergeDictionaries(UpdateParameters, keys, caseInsensitiveNewValues);
                    MergeDictionaries(UpdateParameters, values, caseInsensitiveNewValues);

                    object newDataObject = BuildDataObject(dataObjectType, caseInsensitiveNewValues);

                    // Resolve and invoke the method
                    method = GetResolvedMethodData(type, UpdateMethod, dataObjectType, null, newDataObject, DataSourceOperation.Update);
                }

                ObjectDataSourceMethodEventArgs eventArgs = new ObjectDataSourceMethodEventArgs(method.Parameters);
                OnUpdating(eventArgs);
                if (eventArgs.Cancel) {
                    return 0;
                }
            }
            else {
                // Build parameter list that contains old values (optionally), new values, and keys

                IOrderedDictionary caseInsensitiveAllValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

                string oldValuesParameterFormatString = OldValuesParameterFormatString;
                // Add UpdateParameters, but exclude params that are keys (they will be added later)
                IDictionary updateParams = UpdateParameters.GetValues(_context, _owner);
                if (keys != null) {
                    foreach (DictionaryEntry de in keys) {
                        if (updateParams.Contains(de.Key)) {
                            updateParams.Remove(de.Key);
                        }
                    }
                }
                MergeDictionaries(UpdateParameters, updateParams, caseInsensitiveAllValues);
                MergeDictionaries(UpdateParameters, values, caseInsensitiveAllValues);
                if (ConflictDetection == ConflictOptions.CompareAllValues) {
                    MergeDictionaries(UpdateParameters, oldValues, caseInsensitiveAllValues, oldValuesParameterFormatString);
                }
                MergeDictionaries(UpdateParameters, keys, caseInsensitiveAllValues, oldValuesParameterFormatString);

                ObjectDataSourceMethodEventArgs eventArgs = new ObjectDataSourceMethodEventArgs(caseInsensitiveAllValues);
                OnUpdating(eventArgs);
                if (eventArgs.Cancel) {
                    return 0;
                }

                // Resolve and invoke the method
                method = GetResolvedMethodData(type, UpdateMethod, caseInsensitiveAllValues, DataSourceOperation.Update);
            }

            ObjectDataSourceResult result = InvokeMethod(method);

            // Invalidate the cache and raise the change event to notify bound controls
            if (_owner.Cache.Enabled) {
                _owner.InvalidateCacheEntry();
            }
            OnDataSourceViewChanged(EventArgs.Empty);

            return result.AffectedRows;
        }

        /// <devdoc>
        /// Returns true if the two parameters represent the same type of operation.
        /// </devdoc>
        private static DataObjectMethodType GetMethodTypeFromOperation(DataSourceOperation operation) {
            switch (operation) {
                case DataSourceOperation.Delete:
                    return DataObjectMethodType.Delete;
                case DataSourceOperation.Insert:
                    return DataObjectMethodType.Insert;
                case DataSourceOperation.Select:
                    return DataObjectMethodType.Select;
                case DataSourceOperation.Update:
                    return DataObjectMethodType.Update;
            }
            throw new ArgumentOutOfRangeException("operation");
        }

        /// <devdoc>
        /// Extracts the values of all output (out and ref) parameters given a list of parameters and their respective values.
        /// </devdoc>
        private IDictionary GetOutputParameters(ParameterInfo[] parameters, object[] values) {
            IDictionary outputParameters = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < parameters.Length; i++) {
                ParameterInfo parameter = parameters[i];
                if (parameter.ParameterType.IsByRef) {
                    outputParameters[parameter.Name] = values[i];
                }
            }
            return outputParameters;
        }

        private ObjectDataSourceMethod GetResolvedMethodData(Type type, string methodName, Type dataObjectType, object oldDataObject, object newDataObject, DataSourceOperation operation) {
            Debug.Assert(dataObjectType != null, "This overload of GetResolvedMethodData should only be called when using a DataObject");
            Debug.Assert(oldDataObject != null || newDataObject != null, "Did not expect both oldDataObject and newDataObject to be null");

            // Get a list of all the overloads of the requested method
            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy);

            MethodInfo matchedMethod = null;
            ParameterInfo[] matchedMethodParameters = null;


            int requiredParameterCount;
            if (oldDataObject == null) {
                requiredParameterCount = 1;
            }
            else {
                if (newDataObject == null) {
                    requiredParameterCount = 1;
                }
                else {
                    requiredParameterCount = 2;
                }
            }


            foreach (MethodInfo mi in methods) {
                if (String.Equals(methodName, mi.Name, StringComparison.OrdinalIgnoreCase)) {
                    if (mi.IsGenericMethodDefinition) {
                        // We do not support binding to generic methods, e.g. public void DoSomething<T>(T t)
                        continue;
                    }

                    ParameterInfo[] methodParameters = mi.GetParameters();

                    int methodParametersCount = methodParameters.Length;

                    if (methodParametersCount == requiredParameterCount) {
                        if (requiredParameterCount == 1 &&
                            methodParameters[0].ParameterType == dataObjectType) {
                            // Only one parameter, of proper type
                            // This is only valid for insert, delete, and non-optimistic update
                            matchedMethod = mi;
                            matchedMethodParameters = methodParameters;
                            break;
                        }
                        if (requiredParameterCount == 2 &&
                            methodParameters[0].ParameterType == dataObjectType &&
                            methodParameters[1].ParameterType == dataObjectType) {
                            // Two parameters of proper type in Update
                            // This is only valid for optimistic update
                            matchedMethod = mi;
                            matchedMethodParameters = methodParameters;
                            break;
                        }
                    }
                }
            }

            if (matchedMethod == null) {
                throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_MethodNotFoundForDataObject, _owner.ID, methodName, dataObjectType.FullName));
            }

            Debug.Assert(matchedMethodParameters != null, "Method parameters should not be null if a method was found");

            // Set up parameter array for method call
            OrderedDictionary parameters = new OrderedDictionary(2, StringComparer.OrdinalIgnoreCase);

            if (oldDataObject == null) {
                parameters.Add(matchedMethodParameters[0].Name, newDataObject);
            }
            else {
                if (newDataObject == null) {
                    parameters.Add(matchedMethodParameters[0].Name, oldDataObject);
                }
                else {
                    // We know that we matched on 2 objects for a optimistic update.
                    // Match the parameters based on the format string so we know which one is the old
                    // object and which is the new, then pass objects into the method in the correct order.
                    string param0Name = matchedMethodParameters[0].Name;
                    string param1Name = matchedMethodParameters[1].Name;
                    string formattedParamName = String.Format(CultureInfo.InvariantCulture, OldValuesParameterFormatString, param0Name);
                    if (String.Equals(param1Name, formattedParamName, StringComparison.OrdinalIgnoreCase)) {
                        parameters.Add(param0Name, newDataObject);
                        parameters.Add(param1Name, oldDataObject);
                    }
                    else {
                        formattedParamName = String.Format(CultureInfo.InvariantCulture, OldValuesParameterFormatString, param1Name);
                        if (String.Equals(param0Name, formattedParamName, StringComparison.OrdinalIgnoreCase)) {
                            parameters.Add(param0Name, oldDataObject);
                            parameters.Add(param1Name, newDataObject);
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_NoOldValuesParams, _owner.ID));
                        }
                    }
                }
            }

            // The parameters collection is always readonly in this case since we
            // do not want the user adding/removing the known objects.
            return new ObjectDataSourceMethod(operation, type, matchedMethod, parameters.AsReadOnly());
        }

        /// <devdoc>
        /// Resolves a method based on a type and the name of the method. Overload resolution is
        /// performed primarily based on the names of parameters passed in the two dictionary
        /// parameters. Conflict resolution is done based on a rating scale of confidence levels
        /// (see comments in this method).
        /// The return value is the MethodInfo that was found along with an array of parameter
        /// values to be passed in for the invocation.
        /// </devdoc>
        private ObjectDataSourceMethod GetResolvedMethodData(Type type, string methodName, IDictionary allParameters, DataSourceOperation operation) {
            Debug.Assert(allParameters != null, "The 'allParameters' dictionary should never be null");

            // Since there is no method type for SelectCount, we special case it
            bool isSelectCount = (operation == DataSourceOperation.SelectCount);
            DataObjectMethodType methodType = DataObjectMethodType.Select;
            if (!isSelectCount) {
                methodType = GetMethodTypeFromOperation(operation);
            }

            // Get a list of all the overloads of the requested method
            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy);
            MethodInfo matchedMethod = null;
            ParameterInfo[] matchedMethodParameters = null;

            // Indicates how confident we are that a method overload is a good match
            // -1 - indicates no confidence - no appropriate methods have been found at all
            // 0 - indicates low confidence - only parameter names match
            // 1 - indicates medium confidence - parameter names match, method is DataObjectMethod
            // 2 - indicates high confidence - parameter names match, method is DataObjectMethod, is default method
            int highestConfidence = -1;
            bool confidenceConflict = false; // Indicates that there is more than one method at the current highest confidence level

            int allParameterCount = allParameters.Count;

            foreach (MethodInfo mi in methods) {
                if (String.Equals(methodName, mi.Name, StringComparison.OrdinalIgnoreCase)) {
                    if (mi.IsGenericMethodDefinition) {
                        // We do not support binding to generic methods, e.g. public void DoSomething<T>(T t)
                        continue;
                    }

                    ParameterInfo[] methodParameters = mi.GetParameters();

                    int methodParametersCount = methodParameters.Length;

                    // We are not using DataObject. There is either a Select operation, or it is an
                    // Update/Insert/Delete operation that does not use DataObjects.

                    // First check if the parameter counts match
                    if (methodParametersCount != allParameterCount) {
                        continue;
                    }

                    // Check if all the parameter names match
                    bool parameterMismatch = false;
                    foreach (ParameterInfo pi in methodParameters) {
                        if (!allParameters.Contains(pi.Name)) {
                            parameterMismatch = true;
                            break;
                        }
                    }
                    if (parameterMismatch) {
                        continue;
                    }

                    int confidence = 0; // See comment above regarding confidence

                    if (!isSelectCount) {
                        DataObjectMethodAttribute attr = Attribute.GetCustomAttribute(mi, typeof(DataObjectMethodAttribute), true) as DataObjectMethodAttribute;
                        if (attr != null) {
                            if (attr.MethodType == methodType) {
                                if (attr.IsDefault) {
                                    // Method is decorated and is default
                                    confidence = 2;
                                }
                                else {
                                    // Method is decorated but not default
                                    confidence = 1;
                                }
                            }
                        }
                    }

                    // If we found another method
                    if (confidence == highestConfidence) {
                        confidenceConflict = true;
                    }
                    else {
                        // If method looks like it's the best match so far, store it
                        if (confidence > highestConfidence) {
                            highestConfidence = confidence;
                            confidenceConflict = false;
                            matchedMethod = mi;
                            matchedMethodParameters = methodParameters;
                        }
                    }
                }
            }

            if (confidenceConflict) {
                // There was more than one method that looked like a good match, but none had
                // a higher confidence level than the others, so we fail. See comment above
                // regarding confidence levels.
                throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_MultipleOverloads, _owner.ID));
            }

            if (matchedMethod == null) {
                if (allParameterCount == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_MethodNotFoundNoParams, _owner.ID, methodName));
                }
                else {
                    string[] paramNames = new string[allParameterCount];
                    allParameters.Keys.CopyTo(paramNames, 0);
                    string paramString = String.Join(", ", paramNames);
                    throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_MethodNotFoundWithParams, _owner.ID, methodName, paramString));
                }
            }

            Debug.Assert(matchedMethodParameters != null, "Method parameters should not be null if a method was found");

            OrderedDictionary parameters = null;

            // Create the actual parameter list to be passed to the method
            int methodParameterCount = matchedMethodParameters.Length;
            if (methodParameterCount > 0) {
                parameters = new OrderedDictionary(methodParameterCount, StringComparer.OrdinalIgnoreCase);
                bool convertNullToDBNull = ConvertNullToDBNull;
                for (int i = 0; i < matchedMethodParameters.Length; i++) {
                    ParameterInfo methodParameter = matchedMethodParameters[i];
                    string paramName = methodParameter.Name;
                    // Check if the required parameter exists in the input parameters
                    Debug.Assert(allParameters.Contains(paramName));

                    object parameterValue = allParameters[paramName];
                    if (convertNullToDBNull && (parameterValue == null)) {
                        parameterValue = DBNull.Value;
                    }
                    else {
                        parameterValue = BuildObjectValue(parameterValue, methodParameter.ParameterType, paramName, ParsingCulture);
                    }

                    parameters.Add(paramName, parameterValue);
                }
            }

            return new ObjectDataSourceMethod(operation, type, matchedMethod, parameters);
        }

        private Type GetType(string typeName) {
            if (TypeName.Length == 0) {
                throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_TypeNotSpecified, _owner.ID));
            }

            // Load the type using BuildManager (do not throw on fail, ignore case)
            Type type = BuildManager.GetType(TypeName, false, true);

            // If the type was not found, throw
            if (type == null) {
                throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_TypeNotFound, _owner.ID));
            }

            return type;
        }

        public int Insert(IDictionary values) {
            return ExecuteInsert(values);
        }

        private ObjectDataSourceResult InvokeMethod(ObjectDataSourceMethod method) {
            object instance = null;
            return InvokeMethod(method, true, ref instance);
        }

        /// <devdoc>
        /// Invokes a given method with the specified parameters. This will also raise
        /// the appropriate post event after execution.
        /// </devdoc>
        private ObjectDataSourceResult InvokeMethod(ObjectDataSourceMethod method, bool disposeInstance, ref object instance) {
            // If the method is not static, we need to create an instance of the type

            if (method.MethodInfo.IsStatic) {
                if (instance != null) {
                    ReleaseInstance(instance);
                }
                instance = null;
            }
            else {
                if (instance == null) {
                    // Raise event to allow page developer to supply a custom instance of the type
                    ObjectDataSourceEventArgs objectEventArgs = new ObjectDataSourceEventArgs(null);
                    OnObjectCreating(objectEventArgs);
    
                    // If page developer did not create a custom instance, we attempt to instantiate
                    // the object ourselves, and raise the ObjectCreated event
                    if (objectEventArgs.ObjectInstance == null) {
                        objectEventArgs.ObjectInstance = Activator.CreateInstance(method.Type);
    
                        // Raise ObjectCreated event with the same event args
                        OnObjectCreated(objectEventArgs);
                    }
                    instance = objectEventArgs.ObjectInstance;
                }
            }


            // Call the method and if an exception is thrown, hold on to it to let the page developer attempt to handle it
            object returnValue = null;
            int affectedRows = -1;

            bool eventFired = false;
            object[] parameterValues = null;
            if (method.Parameters != null && method.Parameters.Count > 0) {
                parameterValues = new object[method.Parameters.Count];
                for (int i = 0; i < method.Parameters.Count; i++) {
                    parameterValues[i] = method.Parameters[i];
                }
            }
            try {
                returnValue = method.MethodInfo.Invoke(instance, parameterValues);
            }
            catch (Exception ex) {
                // Collect output parameters
                IDictionary outputParameters = GetOutputParameters(method.MethodInfo.GetParameters(), parameterValues);
                ObjectDataSourceStatusEventArgs statusEventArgs = new ObjectDataSourceStatusEventArgs(returnValue, outputParameters, ex);
                eventFired = true;
                switch (method.Operation) {
                    case DataSourceOperation.Delete:
                        OnDeleted(statusEventArgs);
                        break;
                    case DataSourceOperation.Insert:
                        OnInserted(statusEventArgs);
                        break;
                    case DataSourceOperation.Select:
                        OnSelected(statusEventArgs);
                        break;
                    case DataSourceOperation.SelectCount:
                        OnSelected(statusEventArgs);
                        break;
                    case DataSourceOperation.Update:
                        OnUpdated(statusEventArgs);
                        break;
                }
                affectedRows = statusEventArgs.AffectedRows;

                if (!statusEventArgs.ExceptionHandled) {
                    throw;
                }
            }
            finally {
                // This block is to ensure that we always at least try to raise
                // the Disposing event, even if the other event handlers threw
                // exceptions.
                try {
                    if (eventFired == false) {
                        // Collect output parameters
                        IDictionary outputParameters = GetOutputParameters(method.MethodInfo.GetParameters(), parameterValues);
                        ObjectDataSourceStatusEventArgs statusEventArgs = new ObjectDataSourceStatusEventArgs(returnValue, outputParameters);
                        switch (method.Operation) {
                            case DataSourceOperation.Delete:
                                OnDeleted(statusEventArgs);
                                break;
                            case DataSourceOperation.Insert:
                                OnInserted(statusEventArgs);
                                break;
                            case DataSourceOperation.Select:
                                OnSelected(statusEventArgs);
                                break;
                            case DataSourceOperation.SelectCount:
                                OnSelected(statusEventArgs);
                                break;
                            case DataSourceOperation.Update:
                                OnUpdated(statusEventArgs);
                                break;
                        }
                        affectedRows = statusEventArgs.AffectedRows;
                    }
                }
                finally {
                    // Raise ObjectDisposing event
                    if (instance != null && disposeInstance) {
                        ReleaseInstance(instance);
                        instance = null;
                    }
                }
            }

            return new ObjectDataSourceResult(returnValue, affectedRows);
        }

        /// <devdoc>
        /// Loads view state.
        /// </devdoc>
        protected virtual void LoadViewState(object savedState) {
            if (savedState == null)
                return;

            Pair myState = (Pair)savedState;

            if (myState.First != null)
                ((IStateManager)SelectParameters).LoadViewState(myState.First);

            if (myState.Second != null)
                ((IStateManager)FilterParameters).LoadViewState(myState.Second);
        }

        private static void MergeDictionaries(ParameterCollection reference, IDictionary source, IDictionary destination) {
            MergeDictionaries(reference, source, destination, null);
        }

        /// <devdoc>
        /// Merges new values in the source dictionary with old values in the destination dictionary.
        /// The reference parameter is used to assign types to values that got merged.
        /// If a format string is specified, it will be applied to the parameter name when it is
        /// added to the destination dictionary.
        /// </devdoc>
        private static void MergeDictionaries(ParameterCollection reference, IDictionary source, IDictionary destination, string parameterNameFormatString) {
            Debug.Assert(destination != null);
            Debug.Assert(reference != null);

            if (source != null) {
                foreach (DictionaryEntry de in source) {
                    object value = de.Value;
                    // If the reference collection contains this parameter, we will convert its type to match it
                    Parameter referenceParameter = null;
                    string parameterName = (string)de.Key;
                    if (parameterNameFormatString != null) {
                        parameterName = String.Format(CultureInfo.InvariantCulture, parameterNameFormatString, parameterName);
                    }
                    foreach (Parameter p in reference) {
                        if (String.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase)) {
                            referenceParameter = p;
                            break;
                        }
                    }
                    if (referenceParameter != null) {
                        value = referenceParameter.GetValue(value, true);
                    }
                    destination[parameterName] = value;
                }
            }
        }

        /// <devdoc>
        /// Raises the Deleted event.
        /// </devdoc>
        protected virtual void OnDeleted(ObjectDataSourceStatusEventArgs e) {
            ObjectDataSourceStatusEventHandler handler = Events[EventDeleted] as ObjectDataSourceStatusEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the Deleting event.
        /// </devdoc>
        protected virtual void OnDeleting(ObjectDataSourceMethodEventArgs e) {
            ObjectDataSourceMethodEventHandler handler = Events[EventDeleting] as ObjectDataSourceMethodEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnFiltering(ObjectDataSourceFilteringEventArgs e) {
            ObjectDataSourceFilteringEventHandler handler = Events[EventFiltering] as ObjectDataSourceFilteringEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the Inserted event.
        /// </devdoc>
        protected virtual void OnInserted(ObjectDataSourceStatusEventArgs e) {
            ObjectDataSourceStatusEventHandler handler = Events[EventInserted] as ObjectDataSourceStatusEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the Inserting event.
        /// </devdoc>
        protected virtual void OnInserting(ObjectDataSourceMethodEventArgs e) {
            ObjectDataSourceMethodEventHandler handler = Events[EventInserting] as ObjectDataSourceMethodEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the ObjectCreated event.
        /// </devdoc>
        protected virtual void OnObjectCreated(ObjectDataSourceEventArgs e) {
            ObjectDataSourceObjectEventHandler handler = Events[EventObjectCreated] as ObjectDataSourceObjectEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the ObjectCreating event.
        /// </devdoc>
        protected virtual void OnObjectCreating(ObjectDataSourceEventArgs e) {
            ObjectDataSourceObjectEventHandler handler = Events[EventObjectCreating] as ObjectDataSourceObjectEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the ObjectDisposing event.
        /// </devdoc>
        protected virtual void OnObjectDisposing(ObjectDataSourceDisposingEventArgs e) {
            ObjectDataSourceDisposingEventHandler handler = Events[EventObjectDisposing] as ObjectDataSourceDisposingEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the Selected event.
        /// </devdoc>
        protected virtual void OnSelected(ObjectDataSourceStatusEventArgs e) {
            ObjectDataSourceStatusEventHandler handler = Events[EventSelected] as ObjectDataSourceStatusEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the Selecting event.
        /// </devdoc>
        protected virtual void OnSelecting(ObjectDataSourceSelectingEventArgs e) {
            ObjectDataSourceSelectingEventHandler handler = Events[EventSelecting] as ObjectDataSourceSelectingEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the Updated event.
        /// </devdoc>
        protected virtual void OnUpdated(ObjectDataSourceStatusEventArgs e) {
            ObjectDataSourceStatusEventHandler handler = Events[EventUpdated] as ObjectDataSourceStatusEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Raises the Updating event.
        /// </devdoc>
        protected virtual void OnUpdating(ObjectDataSourceMethodEventArgs e) {
            ObjectDataSourceMethodEventHandler handler = Events[EventUpdating] as ObjectDataSourceMethodEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Populates the paging information in the DataSourceSelectArguments and
        /// saves the data to cache if necessary.
        /// </devdoc>
        private void ProcessPagingData(DataSourceSelectArguments arguments, IOrderedDictionary parameters) {
            if (arguments.RetrieveTotalRowCount) {
                int cachedTotalRowCount = _owner.LoadTotalRowCountFromCache();
                if (cachedTotalRowCount >= 0) {
                    arguments.TotalRowCount = cachedTotalRowCount;
                }
                else {
                    // query for row count and then save it in cache
                    object dummyInstance = null;
                    cachedTotalRowCount = QueryTotalRowCount(parameters, arguments, true, ref dummyInstance);
                    arguments.TotalRowCount = cachedTotalRowCount;
                    _owner.SaveTotalRowCountToCache(cachedTotalRowCount);
                }
            }
        }

        /// <devdoc>
        /// Executes the SelectCountMethod to retrieve the total row count.
        /// </devdoc>
        private int QueryTotalRowCount(IOrderedDictionary mergedParameters, DataSourceSelectArguments arguments, bool disposeInstance, ref object instance) {
            if (SelectCountMethod.Length > 0) {
                ObjectDataSourceSelectingEventArgs eventArgs = new ObjectDataSourceSelectingEventArgs(mergedParameters, arguments, true);
                OnSelecting(eventArgs);
                if (eventArgs.Cancel) {
                    return -1;
                }

                Type type = GetType(TypeName);
                Debug.Assert(type != null, "Should not have a null type at this point");

                ObjectDataSourceMethod method = GetResolvedMethodData(type, SelectCountMethod, mergedParameters, DataSourceOperation.SelectCount);
                ObjectDataSourceResult result = InvokeMethod(method, disposeInstance, ref instance);
                if (result.ReturnValue != null && result.ReturnValue is int) {
                    return (int)result.ReturnValue;
                }
            }

            // Unknown total row count
            return -1;
        }

        private void ReleaseInstance(object instance) {
            Debug.Assert(instance != null, "ReleaseInstance: Instance shouldn't be null");
            ObjectDataSourceDisposingEventArgs disposingEventArgs = new ObjectDataSourceDisposingEventArgs(instance);
            OnObjectDisposing(disposingEventArgs);

            // Only call IDisposable.Dispose() if the page developer did not cancel
            if (!disposingEventArgs.Cancel) {
                // If the object implement IDisposable, call Dispose()
                IDisposable disposableObject = instance as IDisposable;
                if (disposableObject != null) {
                    disposableObject.Dispose();
                }
            }
        }

        private void SaveDataAndRowCountToCache(DataSourceSelectArguments arguments, object data) {
            // Make sure total row count is saved first, since this is the parent key.
            // If it's saved after the data, saving the total row count will invalidate
            // the data cache.
            if (arguments.RetrieveTotalRowCount) {
                int totalRowCount = _owner.LoadTotalRowCountFromCache();
                if (totalRowCount != arguments.TotalRowCount) {
                    _owner.SaveTotalRowCountToCache(arguments.TotalRowCount);
                }
            }
            _owner.SaveDataToCache(arguments.StartRowIndex, arguments.MaximumRows, data);
        }

        /// <devdoc>
        /// Saves view state.
        /// </devdoc>
        protected virtual object SaveViewState() {
            Pair myState = new Pair();

            myState.First = (_selectParameters != null) ? ((IStateManager)_selectParameters).SaveViewState() : null;
            myState.Second = (_filterParameters != null) ? ((IStateManager)_filterParameters).SaveViewState() : null;

            if ((myState.First == null) &&
                (myState.Second == null)) {
                return null;
            }
            return myState;
        }

        public IEnumerable Select(DataSourceSelectArguments arguments) {
            return ExecuteSelect(arguments);
        }

        /// <devdoc>
        /// Event handler for SelectParametersChanged event.
        /// </devdoc>
        private void SelectParametersChangedEventHandler(object o, EventArgs e) {
            OnDataSourceViewChanged(EventArgs.Empty);
        }


        /// <devdoc>
        /// Starts tracking view state.
        /// </devdoc>
        protected virtual void TrackViewState() {
            _tracking = true;

            if (_selectParameters != null) {
                ((IStateManager)_selectParameters).TrackViewState();
            }
            if (_filterParameters != null) {
                ((IStateManager)_filterParameters).TrackViewState();
            }
        }

        private Type TryGetDataObjectType() {
            // Load the data object type using BuildManager (do not throw on fail, ignore case)
            // We only care about it for Update/Insert/Delete operations since it's not supported for Select
            string dataObjectTypeName = DataObjectTypeName;
            if (dataObjectTypeName.Length == 0) {
                return null;
            }
            Type dataObjectType = BuildManager.GetType(dataObjectTypeName, false, true);
            if (dataObjectType == null) {
                // If the type was not found, throw
                throw new InvalidOperationException(SR.GetString(SR.ObjectDataSourceView_DataObjectTypeNotFound, _owner.ID));
            }
            return dataObjectType;
        }

        public int Update(IDictionary keys, IDictionary values, IDictionary oldValues) {
            return ExecuteUpdate(keys, values, oldValues);
        }

        #region IStateManager implementation
        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        /// <internalonly/>
        void IStateManager.LoadViewState(object savedState) {
            LoadViewState(savedState);
        }


        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }
        #endregion


        private struct ObjectDataSourceMethod {
            internal DataSourceOperation Operation;
            internal Type Type;
            internal OrderedDictionary Parameters;
            internal MethodInfo MethodInfo;

            internal ObjectDataSourceMethod(DataSourceOperation operation, Type type, MethodInfo methodInfo, OrderedDictionary parameters) {
                Operation = operation;
                Type = type;
                Parameters = parameters;
                MethodInfo = methodInfo;
            }
        }

        private class ObjectDataSourceResult {
            internal object ReturnValue;
            internal int AffectedRows;

            internal ObjectDataSourceResult(object returnValue, int affectedRows) {
                ReturnValue = returnValue;
                AffectedRows = affectedRows;
            }
        }
    }
}

