//------------------------------------------------------------------------------
// <copyright file="ModelDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.Compilation;
    using System.Web.ModelBinding;
    using System.Web.UI;
    using System.Web.Util;

    /// <summary>
    /// Represents a single view of a ModelDataSource.
    /// </summary>
    public class ModelDataSourceView : DataSourceView, IStateManager {

        // Having the immediate caller of MethodInfo.Invoke be a dynamic method gives us two security advantages:
        //  - It forces the callee to be a public method on a public type.
        //  - It forces a CAS transparency check on the callee.
        private delegate object MethodInvokerDispatcher(MethodInfo methodInfo, object instance, object[] args);
        private static readonly MethodInvokerDispatcher _methodInvokerDispatcher = ((Expression<MethodInvokerDispatcher>)((methodInfo, instance, args) => methodInfo.Invoke(instance, args))).Compile();

        private ModelDataSource _owner;
        private MethodParametersDictionary _selectParameters;
        private bool _tracking;

        private string _modelTypeName;
        private string _deleteMethod;
        private string _insertMethod;
        private string _selectMethod;
        private string _updateMethod;
        private string _dataKeyName;

        private const string TotalRowCountParameterName = "totalRowCount";
        private const string MaximumRowsParameterName = "maximumRows";
        private const string StartRowIndexParameterName = "startRowIndex";
        private const string SortParameterName = "sortByExpression";

        private static readonly object EventCallingDataMethods = new object();

        /// <summary>
        /// Creates a new ModelDataSourceView.
        /// </summary>
        public ModelDataSourceView(ModelDataSource owner)
            : base(owner, ModelDataSource.DefaultViewName) {

            if (owner == null) {
                throw new ArgumentNullException("owner");
            }

            _owner = owner;

            if (owner.DataControl.Page != null) {
                owner.DataControl.Page.LoadComplete += OnPageLoadComplete;
            }
        }


        /// <summary>
        /// Indicates that the view can delete rows.
        /// </summary>
        public override bool CanDelete {
            get {
                return (DeleteMethod.Length != 0);
            }
        }


        /// <summary>
        /// Indicates that the view can add new rows.
        /// </summary>
        public override bool CanInsert {
            get {
                return (InsertMethod.Length != 0);
            }
        }

        /// <summary>
        /// Indicates that the view can do server paging.
        /// We allow paging by default.
        /// </summary>
        public override bool CanPage {
            get {
                return true;
            }
        }

        /// <summary>
        /// Indicates that the view can sort rows.
        /// We allow sorting by default.
        /// </summary>
        public override bool CanSort {
            get {
                return true;
            }
        }

        /// <summary>
        /// We allow retrieving total row count by default.
        /// </summary>
        public override bool CanRetrieveTotalRowCount {
            get {
                return true;
            }
        }

        /// <summary>
        /// Indicates that the view can update rows.
        /// </summary>
        public override bool CanUpdate {
            get {
                return (UpdateMethod.Length != 0);
            }
        }

        //All the property setters below are internal for unit tests.

        /// <summary>
        /// The Data Type Name for the Data Bound Control.
        /// </summary>
        public string ModelTypeName {
            get {
                return _modelTypeName ?? String.Empty;
            }
            internal set {
                if (_modelTypeName != value) {
                    _modelTypeName = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }


        /// <summary>
        /// Name of the method to execute when Delete() is called.
        /// </summary>
        public string DeleteMethod {
            get {
                return _deleteMethod ?? String.Empty;
            }
            internal set {
                _deleteMethod = value;
            }
        }

        /// <summary>
        /// Name of the method to execute when Insert() is called.
        /// </summary>
        public string InsertMethod {
            get {
                return _insertMethod ?? String.Empty;
            }
            internal set {
                _insertMethod = value;
            }
        }

        /// <summary>
        /// Name of the method to execute when Select() is called.
        /// </summary>
        public string SelectMethod {
            get {
                return _selectMethod ?? String.Empty;
            }
            internal set {
                if (_selectMethod != value) {
                    _selectMethod = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Name of the method to execute when Update() is called.
        /// </summary>
        public string UpdateMethod {
            get {
                return _updateMethod ?? String.Empty;
            }
            internal set {
                _updateMethod = value;
            }
        }

        /// <summary>
        /// First of the DataKeyNames array of the data bound control if applicable (FormView/ListView/GridView/DetailsView) and present.
        /// </summary>
        public string DataKeyName {
            get {
                return _dataKeyName ?? String.Empty;
            }
            internal set {
                if (_dataKeyName != value) {
                    _dataKeyName = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public event CallingDataMethodsEventHandler CallingDataMethods {
            add {
                Events.AddHandler(EventCallingDataMethods, value);
            }
            remove {
                Events.RemoveHandler(EventCallingDataMethods, value);
            }
        }

        public void UpdateProperties(string modelTypeName, string selectMethod, string updateMethod, string insertMethod, string deleteMethod, string dataKeyName) {
            ModelTypeName = modelTypeName;
            SelectMethod = selectMethod;
            UpdateMethod = updateMethod;
            InsertMethod = insertMethod;
            DeleteMethod = deleteMethod;
            DataKeyName = dataKeyName;
        }

        protected virtual void OnCallingDataMethods(CallingDataMethodsEventArgs e) {
            CallingDataMethodsEventHandler handler = Events[EventCallingDataMethods] as CallingDataMethodsEventHandler;
            if (handler != null) {
                handler(_owner.DataControl, e);
            }
        }

        private void OnPageLoadComplete(object sender, EventArgs e) {
            EvaluateSelectParameters();
        }

        private static bool IsAutoPagingRequired(MethodInfo selectMethod, bool isReturningQueryable) {

            bool maximumRowsFound = false;
            bool totalRowCountFound = false;
            bool startRowIndexFound = false;

            foreach (ParameterInfo parameter in selectMethod.GetParameters()) {
                string parameterName = parameter.Name;
                if (String.Equals(StartRowIndexParameterName, parameterName, StringComparison.OrdinalIgnoreCase)) {
                    if (parameter.ParameterType.IsAssignableFrom(typeof(int))) {
                        startRowIndexFound = true;
                    }
                    continue;
                }
                if (String.Equals(MaximumRowsParameterName, parameterName, StringComparison.OrdinalIgnoreCase)) {
                    if (parameter.ParameterType.IsAssignableFrom(typeof(int))) {
                        maximumRowsFound = true;
                    }
                    continue;
                }
                if (String.Equals(TotalRowCountParameterName, parameterName, StringComparison.OrdinalIgnoreCase)) {
                    if (parameter.IsOut && typeof(int).IsAssignableFrom(parameter.ParameterType.GetElementType())) {
                        totalRowCountFound = true;
                    }
                    continue;
                }
            }

            bool pagingParamsFound = maximumRowsFound && startRowIndexFound && totalRowCountFound;

            if (!isReturningQueryable && !pagingParamsFound) {
                throw new InvalidOperationException(SR.GetString(SR.ModelDataSourceView_InvalidPagingParameters));
            }

            return !pagingParamsFound;
        }

        private static bool IsAutoSortingRequired(MethodInfo selectMethod, bool isReturningQueryable) {

            bool sortExpressionFound = false;

            foreach (ParameterInfo parameter in selectMethod.GetParameters()) {
                string parameterName = parameter.Name;
                if (String.Equals(SortParameterName, parameterName, StringComparison.OrdinalIgnoreCase)) {
                    if (parameter.ParameterType.IsAssignableFrom(typeof(string))) {
                        sortExpressionFound = true;
                    }
                }
            }

            if ((!isReturningQueryable && !sortExpressionFound)) {
                throw new InvalidOperationException(SR.GetString(SR.ModelDataSourceView_InvalidSortingParameters));
            }

            return !sortExpressionFound;
        }

        /// <summary>
        /// Invokes the select method and gets the result. Also handles auto paging and sorting when required.
        /// </summary>
        /// <param name="arguments">The DataSourceSelectArguments for the select operation.
        /// When applicable, this method sets the TotalRowCount out parameter in the arguments.
        /// </param>
        /// <returns>The return value from the select method.</returns>
        protected virtual object GetSelectMethodResult(DataSourceSelectArguments arguments) {
            
            if (SelectMethod.Length == 0) {
                throw new InvalidOperationException(SR.GetString(SR.ModelDataSourceView_SelectNotSupported));
            }

            DataSourceSelectResultProcessingOptions options = null;
            ModelDataSourceMethod method = EvaluateSelectMethodParameters(arguments, out options);
            ModelDataMethodResult result = InvokeMethod(method);
            return ProcessSelectMethodResult(arguments, options, result);
        }

        /// <summary>
        /// Evaluates the select method parameters and also determines the options for processing select result like auto paging and sorting behavior.
        /// </summary>
        /// <param name="arguments">The DataSourceSelectArguments for the select operation.</param>
        /// <param name="selectResultProcessingOptions">The <see cref="System.Web.UI.WebControls.DataSourceSelectResultProcessingOptions"/> to use 
        /// for processing the select result once select operation is complete. These options are determined in this method and later used
        /// by the method <see cref="System.Web.UI.WebControls.ModelDataSourceView.ProcessSelectMethodResult"/>.
        /// </param>
        /// <returns>A <see cref="System.Web.UI.WebControls.ModelDataSourceMethod"/> with the information required to invoke the select method.</returns>
        protected virtual ModelDataSourceMethod EvaluateSelectMethodParameters(DataSourceSelectArguments arguments, out DataSourceSelectResultProcessingOptions selectResultProcessingOptions) {
            ModelDataSourceMethod method;
            IOrderedDictionary mergedParameters = MergeSelectParameters(arguments);
            // Resolve the method
            method = FindMethod(SelectMethod);

            Type selectMethodReturnType = method.MethodInfo.ReturnType;
            Type modelType = ModelType;
            if (modelType == null) {
                //When ModelType is not specified but SelectMethod returns IQueryable<T>, we treat T as model type for auto paging and sorting.
                //If the return type is something like CustomType<U,T> : IQueryable<T>, we should use T for paging and sorting, hence
                //we walk over the return type's generic arguments for a proper match.
                foreach (Type typeParameter in selectMethodReturnType.GetGenericArguments()) {
                    if (typeof(IQueryable<>).MakeGenericType(typeParameter).IsAssignableFrom(selectMethodReturnType)) {
                        modelType = typeParameter;
                    }
                }
            }
            Type queryableModelType = (modelType != null) ? typeof(IQueryable<>).MakeGenericType(modelType) : null;

            //We do auto paging or auto sorting when the select method is returning an IQueryable and does not have parameters for paging or sorting.
            bool isReturningQueryable = queryableModelType != null && queryableModelType.IsAssignableFrom(selectMethodReturnType);

            bool autoPage = false;
            bool autoSort = false;

            if (arguments.StartRowIndex >= 0 && arguments.MaximumRows > 0) {
                autoPage = IsAutoPagingRequired(method.MethodInfo, isReturningQueryable);
            }

            if (!String.IsNullOrEmpty(arguments.SortExpression)) {
                autoSort = IsAutoSortingRequired(method.MethodInfo, isReturningQueryable);
            }

            selectResultProcessingOptions = new DataSourceSelectResultProcessingOptions() { ModelType = modelType, AutoPage = autoPage, AutoSort = autoSort };
            EvaluateMethodParameters(DataSourceOperation.Select, method, mergedParameters);
            return method;
        }

        /// <summary>
        /// This method performs operations on the select method result like auto paging and sorting if applicable.
        /// </summary>
        /// <param name="arguments">The DataSourceSelectArguments for the select operation.</param>
        /// <param name="selectResultProcessingOptions">The <see cref="System.Web.UI.WebControls.DataSourceSelectResultProcessingOptions"/> to use for processing the select result.
        /// These options are determined in an earlier call to <see cref="System.Web.UI.WebControls.ModelDataSourceView.EvaluateSelectMethodParameters"/>.
        /// </param>
        /// <param name="result">The result after operations like auto paging/sorting are done.</param>
        /// <returns></returns>
        protected virtual object ProcessSelectMethodResult(DataSourceSelectArguments arguments, DataSourceSelectResultProcessingOptions selectResultProcessingOptions, ModelDataMethodResult result) {
            // If the return value is null, there is no more processing to be done
            if (result.ReturnValue == null) {
                return null;
            }

            bool autoPage = selectResultProcessingOptions.AutoPage;
            bool autoSort = selectResultProcessingOptions.AutoSort;
            Type modelType = selectResultProcessingOptions.ModelType;
            string sortExpression = arguments.SortExpression;

            if (autoPage) {
                MethodInfo countHelperMethod = typeof(QueryableHelpers).GetMethod("CountHelper").MakeGenericMethod(modelType);
                arguments.TotalRowCount = (int)countHelperMethod.Invoke(null, new object[] { result.ReturnValue });

                //Bug 180907: We would like to auto sort on DataKeyName when paging is enabled and result is not already sorted by user to overcome a limitation in EF.
                MethodInfo isOrderingMethodFoundMethod = typeof(QueryableHelpers).GetMethod("IsOrderingMethodFound").MakeGenericMethod(modelType);
                bool isOrderingMethodFound = (bool)isOrderingMethodFoundMethod.Invoke(null, new object[] { result.ReturnValue });
                if (!isOrderingMethodFound) {
                    if (String.IsNullOrEmpty(sortExpression) && !String.IsNullOrEmpty(DataKeyName)) {
                        autoSort = true;
                        selectResultProcessingOptions.AutoSort = true;
                        sortExpression = DataKeyName;
                    }
                }
            }
            else if (arguments.StartRowIndex >= 0 && arguments.MaximumRows > 0) {
                //When paging is handled by developer, we need to set the TotalRowCount parameter from the select method out parameter.
                arguments.TotalRowCount = (int)result.OutputParameters[TotalRowCountParameterName];
            }

            if (autoPage || autoSort) {
                MethodInfo sortPageHelperMethod = typeof(QueryableHelpers).GetMethod("SortandPageHelper").MakeGenericMethod(modelType);
                object returnValue = sortPageHelperMethod.Invoke(null, new object[] { result.ReturnValue, 
                                                                                            autoPage ? (int?)arguments.StartRowIndex : null,
                                                                                            autoPage ? (int?)arguments.MaximumRows : null,
                                                                                            autoSort ? sortExpression : null });
                return returnValue;
            }

            return result.ReturnValue;
        }

        private static IOrderedDictionary MergeSelectParameters(DataSourceSelectArguments arguments) {
            bool shouldPage = arguments.StartRowIndex >= 0 && arguments.MaximumRows > 0;
            bool shouldSort = !String.IsNullOrEmpty(arguments.SortExpression);
            // Copy the parameters into a case insensitive dictionary
            IOrderedDictionary mergedParameters = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

            // Add the sort expression as a parameter if necessary
            if (shouldSort) {
                mergedParameters[SortParameterName] = arguments.SortExpression;
            }

            // Add the paging arguments as parameters if necessary
            if (shouldPage) {
                // Create a new dictionary with the paging information and merge it in (so we get type conversions)
                IDictionary pagingParameters = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                pagingParameters[MaximumRowsParameterName] = arguments.MaximumRows;
                pagingParameters[StartRowIndexParameterName] = arguments.StartRowIndex;
                pagingParameters[TotalRowCountParameterName] = 0;
                MergeDictionaries(pagingParameters, mergedParameters);
            }

            return mergedParameters;
        }

        /// <summary>
        /// Returns the incoming result after wrapping it into IEnumerable if it's not already one.
        /// Also ensures that the result is properly typed when ModelTypeName property is set.
        /// </summary>
        /// <param name="result">The return value of select method.</param>
        /// <returns>Returns the value wrapping it into IEnumerable if necessary.
        /// If ItemType is set and the return value is not of proper type, throws an InvalidOperationException.
        /// </returns>
        protected virtual IEnumerable CreateSelectResult(object result) {
            if (result == null) {
                return null;
            }

            Type modelType = ModelType;

            //If it is IEnumerable<ModelType> we return as is.
            Type enumerableModelType = (modelType != null) ? typeof(IEnumerable<>).MakeGenericType(modelType) : typeof(IEnumerable);
            if (enumerableModelType.IsInstanceOfType(result)) {
                return (IEnumerable)result;
            }
            else {
                if (modelType == null || modelType.IsInstanceOfType(result)) {
                    //If it is a <ModelType> we wrap it in an array and return.
                    return new object[1] { result };
                }
                else {
                    //Sorry only the above return types are allowed!!!
                    throw new InvalidOperationException(SR.GetString(SR.ModelDataSourceView_InvalidSelectReturnType, modelType));
                }
            }
        }

        /// <summary>
        /// Invokes the Delete method and gets the result.
        /// </summary>
        protected virtual object GetDeleteMethodResult(IDictionary keys, IDictionary oldValues) {
            ModelDataSourceMethod method = EvaluateDeleteMethodParameters(keys, oldValues);
            ModelDataMethodResult result = InvokeMethod(method);

            return result.ReturnValue;
        }

        protected virtual ModelDataSourceMethod EvaluateDeleteMethodParameters(IDictionary keys, IDictionary oldValues) {
            if (!CanDelete) {
                throw new NotSupportedException(SR.GetString(SR.ModelDataSourceView_DeleteNotSupported));
            }

            IDictionary caseInsensitiveOldValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

            MergeDictionaries(keys, caseInsensitiveOldValues);
            MergeDictionaries(oldValues, caseInsensitiveOldValues);

            ModelDataSourceMethod method = FindMethod(DeleteMethod);
            EvaluateMethodParameters(DataSourceOperation.Delete, method, caseInsensitiveOldValues);
            return method;
        }

        /// <summary>
        /// Invokes the Insert method and gets the result.
        /// </summary>
        protected virtual object GetInsertMethodResult(IDictionary values) {
            ModelDataSourceMethod method = EvaluateInsertMethodParameters(values);
            ModelDataMethodResult result = InvokeMethod(method);

            return result.ReturnValue;
        }

        protected virtual ModelDataSourceMethod EvaluateInsertMethodParameters(IDictionary values) {
            if (!CanInsert) {
                throw new NotSupportedException(SR.GetString(SR.ModelDataSourceView_InsertNotSupported));
            }

            IDictionary caseInsensitiveNewValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

            MergeDictionaries(values, caseInsensitiveNewValues);

            ModelDataSourceMethod method = FindMethod(InsertMethod);
            EvaluateMethodParameters(DataSourceOperation.Insert, method, caseInsensitiveNewValues);
            return method;
        }

        /// <summary>
        /// Invokes the Update method and gets the result.
        /// </summary>
        protected virtual object GetUpdateMethodResult(IDictionary keys, IDictionary values, IDictionary oldValues) {
            ModelDataSourceMethod method = EvaluateUpdateMethodParameters(keys, values, oldValues);
            ModelDataMethodResult result = InvokeMethod(method);

            return result.ReturnValue;
        }

        protected virtual ModelDataSourceMethod EvaluateUpdateMethodParameters(IDictionary keys, IDictionary values, IDictionary oldValues) {
            if (!CanUpdate) {
                throw new NotSupportedException(SR.GetString(SR.ModelDataSourceView_UpdateNotSupported));
            }

            IDictionary caseInsensitiveNewValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

            // We start out with the old values, just to pre-populate the list with items
            // that might not have corresponding new values. For example if a GridView has
            // a read-only field, there will be an old value, but no new value. The data object
            // still has to have *some* value for a given field, so we just use the old value.
            MergeDictionaries(oldValues, caseInsensitiveNewValues);
            MergeDictionaries(keys, caseInsensitiveNewValues);
            MergeDictionaries(values, caseInsensitiveNewValues);

            ModelDataSourceMethod method = FindMethod(UpdateMethod);
            EvaluateMethodParameters(DataSourceOperation.Update, method, caseInsensitiveNewValues);
            return method;
        }

        /// <summary>
        /// This method is used by ExecuteInsert/Update/Delete methods to return the result if it's an integer or return a default value.
        /// </summary>
        /// <param name="result">The return value from one of the above operations.</param>
        /// <returns>Returns the result as is if it's integer. Otherwise returns -1.</returns>
        private static int GetIntegerReturnValue(object result) {
            return (result is int) ? (int)result : -1 ;
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
            object result = GetDeleteMethodResult(keys, oldValues);
            OnDataSourceViewChanged(EventArgs.Empty);
            return GetIntegerReturnValue(result);
        }

        protected override int ExecuteInsert(IDictionary values) {
            object result = GetInsertMethodResult(values);

            Debug.Assert(_owner.DataControl.Page != null);
            //We do not want to databind when ModelState is invaild so that user entered values are not cleared.
            if (_owner.DataControl.Page.ModelState.IsValid) {
                OnDataSourceViewChanged(EventArgs.Empty);
            }

            return GetIntegerReturnValue(result);
        }

        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
            object result = GetSelectMethodResult(arguments);
            return CreateSelectResult(result);
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
            object result = GetUpdateMethodResult(keys, values, oldValues);

            Debug.Assert(_owner.DataControl.Page != null);
            //We do not want to databind when ModelState is invaild so that user entered values are not cleared.
            if (_owner.DataControl.Page.ModelState.IsValid) {
                OnDataSourceViewChanged(EventArgs.Empty);
            }

            return GetIntegerReturnValue(result);
        }

        //For unit testing.
        internal IEnumerable Select(DataSourceSelectArguments arguments) {
            return ExecuteSelect(arguments);
        }

        //For unit testing.
        internal int Update(IDictionary keys, IDictionary values, IDictionary oldValues) {
            return ExecuteUpdate(keys, values, oldValues);
        }

        //Evaluates the select method parameters using the custom value provides. This is done after page load so that
        //we raise the DataSourceChanged event if the values of parameters change.
        private void EvaluateSelectParameters() {
            if (!String.IsNullOrEmpty(SelectMethod)) {
                ModelDataSourceMethod method = FindMethod(SelectMethod);
                EvaluateMethodParameters(DataSourceOperation.Select, method, controlValues: null, isPageLoadComplete: true);
            }
        }

        /// <summary>
        /// Evaluates the method parameters using model binding.
        /// </summary>
        /// <param name="dataSourceOperation">The datasource operation for which parameters are being evaluated.</param>
        /// <param name="modelDataSourceMethod">The ModelDataSourceMethod object for which the Parameter collection is being evaluated. The MethodInfo property should already be set on this object.</param>
        /// <param name="controlValues">The values from the data bound control.</param>
        protected virtual void EvaluateMethodParameters(DataSourceOperation dataSourceOperation, ModelDataSourceMethod modelDataSourceMethod, IDictionary controlValues) {
            EvaluateMethodParameters(dataSourceOperation, modelDataSourceMethod, controlValues, isPageLoadComplete: false);
        }

        /// <summary>
        /// Evaluates the method parameters using model binding.
        /// </summary>
        /// <param name="dataSourceOperation">The datasource operation for which parameters are being evaluated.</param>
        /// <param name="modelDataSourceMethod">The ModelDataSourceMethod object for which the Parameter collection is being evaluated. The MethodInfo property should already be set on this object.</param>
        /// <param name="controlValues">The values from the data bound control.</param>
        /// <param name="isPageLoadComplete">This must be set to true only when this method is called in Page's LoadComplete event handler
        /// to evaluate the select method parameters that use custom value providers so that we can identify any changes
        /// to those and mark the data-bound control for data binding if necessary.</param>
        protected virtual void EvaluateMethodParameters(DataSourceOperation dataSourceOperation, ModelDataSourceMethod modelDataSourceMethod, IDictionary controlValues, bool isPageLoadComplete) {

            Debug.Assert(_owner.DataControl.Page != null);
            Debug.Assert(_owner.DataControl.TemplateControl != null);

            MethodInfo actionMethod = modelDataSourceMethod.MethodInfo;
            
            IModelBinder binder = ModelBinders.Binders.DefaultBinder;

            IValueProvider dataBoundControlValueProvider = GetValueProviderFromDictionary(controlValues);
            
            ModelBindingExecutionContext modelBindingExecutionContext = _owner.DataControl.Page.ModelBindingExecutionContext;
            //This is used by ControlValueProvider later.
            modelBindingExecutionContext.PublishService<Control>(_owner.DataControl);

            //This is done for the TryUpdateModel to work inside a Data Method. 
            if (dataSourceOperation != DataSourceOperation.Select) {
                _owner.DataControl.Page.SetActiveValueProvider(dataBoundControlValueProvider);
            }

            foreach (ParameterInfo parameterInfo in actionMethod.GetParameters()) {
                object value = null;
                string modelName = parameterInfo.Name;

                if (parameterInfo.ParameterType == typeof(ModelMethodContext)) {
                    //ModelMethodContext is a special parameter we pass in for enabling developer to call
                    //TryUpdateModel when Select/Update/Delete/InsertMethods are on a custom class.
                    value = new ModelMethodContext(_owner.DataControl.Page);
                }
                //Do not attempt model binding the out parameters
                else if (!parameterInfo.IsOut) {
                    bool validateRequest;
                    IValueProvider customValueProvider = GetCustomValueProvider(modelBindingExecutionContext, parameterInfo, ref modelName, out validateRequest);

                    //When we are evaluating the parameter at the time of page load, we do not want to populate the actual ModelState
                    //because there will be another evaluation at data-binding causing duplicate errors if model validation fails.
                    ModelStateDictionary modelState = isPageLoadComplete ? new ModelStateDictionary() : _owner.DataControl.Page.ModelState;

                    ModelBindingContext bindingContext = new ModelBindingContext() {
                        ModelBinderProviders = ModelBinderProviders.Providers,
                        ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, parameterInfo.ParameterType),
                        ModelState = modelState,
                        ModelName = modelName,
                        ValueProvider = customValueProvider,
                        ValidateRequest = validateRequest
                    };

                    //Select parameters that take custom values providers are tracked by ViewState so that 
                    //we can detect any changes from previous page request and mark the data bound control for data binding if necessary.
                    if (dataSourceOperation == DataSourceOperation.Select && customValueProvider != null && parameterInfo.ParameterType.IsSerializable) {
                        if (!SelectParameters.ContainsKey(parameterInfo.Name)) {
                            SelectParameters.Add(parameterInfo.Name, new MethodParameterValue());
                        }

                        if (binder.BindModel(modelBindingExecutionContext, bindingContext)) {
                            value = bindingContext.Model;
                        }
                        SelectParameters[parameterInfo.Name].UpdateValue(value);
                    }
                    else {
                        if (isPageLoadComplete) {
                            Debug.Assert(dataSourceOperation == DataSourceOperation.Select, "Only Select Operation should have been done immediately after page load");
                            //When this method is called as part of Page's LoadComplete event handler we do not have values in defaultValueProvider 
                            //(i.e., values from DataBoundControl), so we need not evaluate the parameters values.
                            continue;
                        }

                        if (customValueProvider == null) {
                            bindingContext.ValueProvider = dataBoundControlValueProvider;
                        }

                        if (binder.BindModel(modelBindingExecutionContext, bindingContext)) {
                            value = bindingContext.Model;
                        }
                    }

                    if (!isPageLoadComplete) {
                        ValidateParameterValue(parameterInfo, value, actionMethod);
                    }
                }
                modelDataSourceMethod.Parameters.Add(parameterInfo.Name, value);
            }
        }

        private static IValueProvider GetValueProviderFromDictionary(IDictionary controlValues) {
            Dictionary<string, object> genericDictionary = new Dictionary<string, object>();

            if (controlValues != null) {
                foreach (DictionaryEntry entry in controlValues) {
                    Debug.Assert(entry.Key is string, "Some key value is not string");
                    genericDictionary.Add((string)entry.Key, entry.Value);
                }
            }

            return new DictionaryValueProvider<object>(genericDictionary, CultureInfo.CurrentCulture);
        }

        private IValueProvider GetCustomValueProvider(ModelBindingExecutionContext modelBindingExecutionContext, ParameterInfo parameterInfo, ref string modelName, out bool validateRequest) {
            validateRequest = true;
            object[] valueProviderAttributes = parameterInfo.GetCustomAttributes(typeof(IValueProviderSource), false);

            if (valueProviderAttributes.Count() > 1) {
                throw new NotSupportedException(SR.GetString(SR.ModelDataSourceView_MultipleValueProvidersNotSupported, parameterInfo.Name));
            }
            
            if (valueProviderAttributes.Count() > 0) {
                IValueProviderSource valueProviderAttribute = (IValueProviderSource)valueProviderAttributes[0];
                if (valueProviderAttribute is IModelNameProvider) {
                    string name = ((IModelNameProvider)valueProviderAttribute).GetModelName();
                    if (!String.IsNullOrEmpty(name)) {
                        modelName = name;
                    }
                }
                if (valueProviderAttribute is IUnvalidatedValueProviderSource) {
                    validateRequest = ((IUnvalidatedValueProviderSource)valueProviderAttribute).ValidateInput;
                }
                return valueProviderAttribute.GetValueProvider(modelBindingExecutionContext);
            }
            return null;
        }

        /// <summary>
        /// Finds the method to be executed. Raises the CallingDataMethods event to see if developer opted in for custom model method look-up instead of page/usercontrol code behind.
        /// Uses the TemplateControl type as a fallback.
        /// </summary>
        /// <param name="methodName">Name of the data method.</param>
        /// <returns>
        /// A ModelDataSourceMethod with the Instance and MethodInfo set. 
        /// The Parameters collection on ModelDataSourceMethod is still empty after this method.
        /// </returns>
        protected virtual ModelDataSourceMethod FindMethod(string methodName) {
            CallingDataMethodsEventArgs e = new CallingDataMethodsEventArgs();
            OnCallingDataMethods(e);
            Type type;
            BindingFlags flags;
            object instance;

            if (e.DataMethodsType != null) {
                if (e.DataMethodsObject != null) {
                    throw new InvalidOperationException(SR.GetString(SR.ModelDataSourceView_MultipleModelMethodSources, methodName));
                }
                flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
                instance = null;
                type = e.DataMethodsType;
            } 
            else if (e.DataMethodsObject != null) {
                flags = BindingFlags.Public | BindingFlags.Instance;
                instance = e.DataMethodsObject;
                type = instance.GetType();
            } 
            else {
                //The compiled page code is a child class of code behind class where usually static methods are defined.
                //We will not get those methods unless we use FlattenHierarchy.
                flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
                instance = _owner.DataControl.TemplateControl;
                type = instance.GetType();
            }
            
            MethodInfo[] allMethods = type.GetMethods(flags);
            MethodInfo[] actionMethods = Array.FindAll(allMethods, methodInfo => methodInfo.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));

            if (actionMethods.Length != 1) {
                throw new InvalidOperationException(SR.GetString(SR.ModelDataSourceView_DataMethodNotFound, methodName, type));
            }

            ValidateMethodIsCallable(actionMethods[0]);

            return new ModelDataSourceMethod(instance: instance, methodInfo: actionMethods[0]);
        }

        private void ValidateMethodIsCallable(MethodInfo methodInfo) {
            // we can't call methods with open generic type parameters
            if (methodInfo.ContainsGenericParameters) {
                throw new InvalidOperationException(SR.GetString(SR.ModelDataSourceView_CannotCallOpenGenericMethods, methodInfo, methodInfo.ReflectedType.FullName));
            }

            // we can't call methods with ref parameters
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            foreach (ParameterInfo parameterInfo in parameterInfos) {
                if (parameterInfo.ParameterType.IsByRef && !parameterInfo.Name.Equals(TotalRowCountParameterName, StringComparison.OrdinalIgnoreCase)) {
                    throw new InvalidOperationException(SR.GetString(SR.ModelDataSourceView_CannotCallMethodsWithOutOrRefParameters,
                        methodInfo, methodInfo.ReflectedType.FullName, parameterInfo));
                }
            }
        }

        /// <summary>
        /// Extracts the values of all output (out and ref) parameters given a list of parameters and their respective values.
        /// </summary>
        private OrderedDictionary GetOutputParameters(ParameterInfo[] parameters, object[] values) {
            OrderedDictionary outputParameters = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < parameters.Length; i++) {
                ParameterInfo parameter = parameters[i];
                if (parameter.ParameterType.IsByRef) {
                    outputParameters[parameter.Name] = values[i];
                }
            }
            return outputParameters;
        }

        /// <summary>
        /// Invokes the data method in a secure fashion.
        /// </summary>
        /// <param name="method">
        /// The ModelDataSouceMethod object specifying the Instance on which the method should be invoked (null for static methods), 
        /// the MethodInfo of the method to be invoked and the Parameters for invoking the method.
        /// All the above properties should be populated before this method is called.
        /// </param>
        /// <returns>
        /// A ModelDataSouceResult object containing the ReturnValue of the method and any out parameters.
        /// </returns>
        protected virtual ModelDataMethodResult InvokeMethod(ModelDataSourceMethod method) {
            object returnValue = null;

            object[] parameterValues = null;
            if (method.Parameters != null && method.Parameters.Count > 0) {
                parameterValues = new object[method.Parameters.Count];
                for (int i = 0; i < method.Parameters.Count; i++) {
                    parameterValues[i] = method.Parameters[i];
                }
            }

            returnValue = _methodInvokerDispatcher(method.MethodInfo, method.Instance, parameterValues);
            OrderedDictionary outputParameters = GetOutputParameters(method.MethodInfo.GetParameters(), parameterValues);
            method.Instance = null;

            //Data Method is done executing, turn off the TryUpdateModel
            _owner.DataControl.Page.SetActiveValueProvider(null);

            return new ModelDataMethodResult(returnValue, outputParameters);
        }

        protected virtual bool IsTrackingViewState() {
            return _tracking;
        }

        protected virtual void LoadViewState(object savedState) {
            if (savedState != null) {
                ((IStateManager)SelectParameters).LoadViewState(savedState);
            }
        }

        protected virtual object SaveViewState() {
            return _selectParameters != null ? ((IStateManager)_selectParameters).SaveViewState() : null;
        }

        protected virtual void TrackViewState() {
            _tracking = true;
            if (_selectParameters != null) {
                ((IStateManager)_selectParameters).TrackViewState();
            }
        }

        private void ValidateParameterValue(ParameterInfo parameterInfo, object value, MethodInfo methodInfo) {
            if (value == null && !TypeHelpers.TypeAllowsNullValue(parameterInfo.ParameterType)) {
                // tried to pass a null value for a non-nullable parameter type
                string message = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ModelDataSourceView_ParameterCannotBeNull),
                    parameterInfo.Name, parameterInfo.ParameterType, methodInfo, methodInfo.DeclaringType);
                throw new InvalidOperationException(message);
            }

            if (value != null && !parameterInfo.ParameterType.IsInstanceOfType(value)) {
                // value was supplied but is not of the proper type
                string message = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ModelDataSourceView_ParameterValueHasWrongType),
                    parameterInfo.Name, methodInfo, methodInfo.DeclaringType, value.GetType(), parameterInfo.ParameterType);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Merges new values in the source dictionary with old values in the destination dictionary.
        /// </summary>
        private static void MergeDictionaries(IDictionary source, IDictionary destination) {
            Debug.Assert(destination != null);

            if (source != null) {
                foreach (DictionaryEntry de in source) {
                    object value = de.Value;
                    // If the reference collection contains this parameter, we will convert its type to match it
                    string parameterName = (string)de.Key;
                    destination[parameterName] = value;
                }
            }
        }

        private Type ModelType {
            get {
                string modelTypeName = ModelTypeName;
                
                if (String.IsNullOrEmpty(modelTypeName)) {
                    return null;
                }

                // Load the data object type using BuildManager
                return BuildManager.GetType(modelTypeName, true, true);
            }
        }

        private MethodParametersDictionary SelectParameters {
            get {
                if (_selectParameters == null) {
                    _selectParameters = new MethodParametersDictionary();

                    _selectParameters.ParametersChanged += OnSelectParametersChanged;

                    if (_tracking) {
                        ((IStateManager)_selectParameters).TrackViewState();
                    }
                }
                return _selectParameters;
            }
        }

        private void OnSelectParametersChanged(object sender, EventArgs e) {
            OnDataSourceViewChanged(EventArgs.Empty);
        }

        #region Implementation of IStateManager

        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState();
            }
        }

        void IStateManager.LoadViewState(object savedState) {
            LoadViewState(savedState);
        }

        object IStateManager.SaveViewState() {
            return SaveViewState();
        }

        void IStateManager.TrackViewState() {
            TrackViewState();
        }
        #endregion
    }
}

