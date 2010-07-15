//
// System.Web.UI.WebControls.ObjectDataSourceView
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Web.Compilation;

namespace System.Web.UI.WebControls
{

	public class ObjectDataSourceView : DataSourceView, IStateManager
	{
		ObjectDataSource owner;
		HttpContext context;
		Type objectType;
		Type dataObjectType;

		bool convertNullToDBNull = false;
		bool enablePaging = false;
		string dataObjectTypeName = null;
		string filterExpression = null;
		string maximumRowsParameterName = null;
		string oldValuesParameterFormatString = null;
		string deleteMethod = null;
		string insertMethod = null;
		string selectCountMethod = null;
		string selectMethod = null;
		string sortParameterName = null;
		string startRowIndexParameterName = null;
		string typeName = null;
		string updateMethod = null;

		bool isTrackingViewState = false;
		ParameterCollection selectParameters;
		ParameterCollection updateParameters;
		ParameterCollection deleteParameters;
		ParameterCollection insertParameters;
		ParameterCollection filterParameters;
		
		static readonly object DeletedEvent = new object();
		static readonly object DeletingEvent = new object();
		static readonly object FilteringEvent = new object();
		static readonly object InsertedEvent = new object();
		static readonly object InsertingEvent = new object();
		static readonly object ObjectCreatedEvent = new object();
		static readonly object ObjectCreatingEvent = new object();
		static readonly object ObjectDisposingEvent = new object();
		//		static readonly object ResolvingMethodEvent = new object();
		static readonly object SelectedEvent = new object();
		static readonly object SelectingEvent = new object();
		static readonly object UpdatedEvent = new object();
		static readonly object UpdatingEvent = new object();
		
		public ObjectDataSourceView (ObjectDataSource owner, string name, HttpContext context): base (owner, name)
		{
			this.owner = owner;
			this.context = context;
		}
		
		public event ObjectDataSourceStatusEventHandler Deleted {
			add { Events.AddHandler (DeletedEvent, value); }
			remove { Events.RemoveHandler (DeletedEvent, value); }
		}
		
		public event ObjectDataSourceMethodEventHandler Deleting {
			add { Events.AddHandler (DeletingEvent, value); }
			remove { Events.RemoveHandler (DeletingEvent, value); }
		}
		
		public event ObjectDataSourceFilteringEventHandler Filtering {
			add { Events.AddHandler (FilteringEvent, value); }
			remove { Events.RemoveHandler (FilteringEvent, value); }
		}
		
		public event ObjectDataSourceStatusEventHandler Inserted {
			add { Events.AddHandler (InsertedEvent, value); }
			remove { Events.RemoveHandler (InsertedEvent, value); }
		}
		
		public event ObjectDataSourceMethodEventHandler Inserting {
			add { Events.AddHandler (InsertingEvent, value); }
			remove { Events.RemoveHandler (InsertingEvent, value); }
		}
		
		public event ObjectDataSourceObjectEventHandler ObjectCreated {
			add { Events.AddHandler (ObjectCreatedEvent, value); }
			remove { Events.RemoveHandler (ObjectCreatedEvent, value); }
		}
		
		public event ObjectDataSourceObjectEventHandler ObjectCreating {
			add { Events.AddHandler (ObjectCreatingEvent, value); }
			remove { Events.RemoveHandler (ObjectCreatingEvent, value); }
		}
		
		public event ObjectDataSourceDisposingEventHandler ObjectDisposing {
			add { Events.AddHandler (ObjectDisposingEvent, value); }
			remove { Events.RemoveHandler (ObjectDisposingEvent, value); }
		}
		
		/*		public event ObjectDataSourceResolvingMethodEventHandler ResolvingMethod {
				add { Events.AddHandler (ResolvingMethodEvent, value); }
				remove { Events.RemoveHandler (ResolvingMethodEvent, value); }
				}
		*/
		public event ObjectDataSourceStatusEventHandler Selected {
			add { Events.AddHandler (SelectedEvent, value); }
			remove { Events.RemoveHandler (SelectedEvent, value); }
		}
		
		public event ObjectDataSourceSelectingEventHandler Selecting {
			add { Events.AddHandler (SelectingEvent, value); }
			remove { Events.RemoveHandler (SelectingEvent, value); }
		}
		
		public event ObjectDataSourceStatusEventHandler Updated {
			add { Events.AddHandler (UpdatedEvent, value); }
			remove { Events.RemoveHandler (UpdatedEvent, value); }
		}
		
		public event ObjectDataSourceMethodEventHandler Updating {
			add { Events.AddHandler (UpdatingEvent, value); }
			remove { Events.RemoveHandler (UpdatingEvent, value); }
		}
		
		protected virtual void OnDeleted (ObjectDataSourceStatusEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceStatusEventHandler eh = (ObjectDataSourceStatusEventHandler) Events [DeletedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnDeleting (ObjectDataSourceMethodEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceMethodEventHandler eh = (ObjectDataSourceMethodEventHandler) Events [DeletingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnFiltering (ObjectDataSourceFilteringEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceFilteringEventHandler eh = (ObjectDataSourceFilteringEventHandler) Events [FilteringEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnInserted (ObjectDataSourceStatusEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceStatusEventHandler eh = (ObjectDataSourceStatusEventHandler) Events [InsertedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnInserting (ObjectDataSourceMethodEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceMethodEventHandler eh = (ObjectDataSourceMethodEventHandler) Events [InsertingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnObjectCreated (ObjectDataSourceEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceObjectEventHandler eh = (ObjectDataSourceObjectEventHandler) Events [ObjectCreatedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnObjectCreating (ObjectDataSourceEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceObjectEventHandler eh = (ObjectDataSourceObjectEventHandler) Events [ObjectCreatingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnObjectDisposing (ObjectDataSourceDisposingEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceDisposingEventHandler eh = (ObjectDataSourceDisposingEventHandler) Events [ObjectDisposingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		/*		protected virtual void OnResolvingMethod (ObjectDataSourceResolvingMethodEventArgs e)
				{
				if (Events != null) {
				ObjectDataSourceResolvingMethodEventHandler eh = (ObjectDataSourceResolvingMethodEventHandler) Events [ResolvingMethodEvent];
				if (eh != null) eh (this, e);
				}
				}
		*/
		
		protected virtual void OnSelected (ObjectDataSourceStatusEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceStatusEventHandler eh = (ObjectDataSourceStatusEventHandler) Events [SelectedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnSelecting (ObjectDataSourceSelectingEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceSelectingEventHandler eh = (ObjectDataSourceSelectingEventHandler) Events [SelectingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnUpdated (ObjectDataSourceStatusEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceStatusEventHandler eh = (ObjectDataSourceStatusEventHandler) Events [UpdatedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnUpdating (ObjectDataSourceMethodEventArgs e)
		{
			if (Events != null) {
				ObjectDataSourceMethodEventHandler eh = (ObjectDataSourceMethodEventHandler) Events [UpdatingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		public override bool CanDelete {
			get { return DeleteMethod.Length > 0; }
		}
		
		public override bool CanInsert {
			get { return InsertMethod.Length > 0; }
		}
		
		public override bool CanPage {
			get { return EnablePaging; }
		}
		
		public override bool CanRetrieveTotalRowCount {
			get {
				if( SelectCountMethod.Length > 0)
					return true;

				return !EnablePaging;
			}
		}
		
		public override bool CanSort {
			get { return true; }
		}
		
		public override bool CanUpdate {
			get { return UpdateMethod.Length > 0; }
		}

		// LAME SPEC: MSDN says value should be stored in ViewState but tests show otherwise.
		ConflictOptions conflictDetection = ConflictOptions.OverwriteChanges;
		public ConflictOptions ConflictDetection {
			get { return conflictDetection; }
			set {
				if (ConflictDetection == value)
					return;
				conflictDetection = value;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		public bool ConvertNullToDBNull	{
			get { return convertNullToDBNull; }
			set { convertNullToDBNull = value; }
		}

		public string DataObjectTypeName {
			get {
				return dataObjectTypeName != null ? dataObjectTypeName : string.Empty;
			}
			set {
				if (DataObjectTypeName == value)
					return;
				dataObjectTypeName = value;
				dataObjectType = null;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		public string DeleteMethod {
			get {
				return deleteMethod != null ? deleteMethod : string.Empty;
			}
			set {
				deleteMethod = value;
			}
		}
		
		public ParameterCollection DeleteParameters {
			get {
				if (deleteParameters == null) {
					deleteParameters = new ParameterCollection ();
				}
				return deleteParameters;
			}
		}

		public bool EnablePaging {
			get {
				return enablePaging;
			}
			set {
				if (EnablePaging == value)
					return;
				enablePaging = value;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		public string FilterExpression {
			get {
				return filterExpression != null ? filterExpression : string.Empty;
			}
			set {
				if (FilterExpression == value)
					return;
				filterExpression = value;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		public ParameterCollection FilterParameters {
			get {
				if (filterParameters == null) {
					filterParameters = new ParameterCollection ();
					filterParameters.ParametersChanged += new EventHandler (OnParametersChanged); 
					if (IsTrackingViewState)
						((IStateManager)filterParameters).TrackViewState ();
				}
				return filterParameters;
			}
		}

		public string InsertMethod {
			get {
				return insertMethod != null ? insertMethod : string.Empty;
			}
			set {
				insertMethod = value;
			}
		}

		public ParameterCollection InsertParameters {
			get {
				if (insertParameters == null) {
					insertParameters = new ParameterCollection ();
				}
				return insertParameters;
			}
		}

		public string MaximumRowsParameterName {
			get {
				return maximumRowsParameterName != null ? maximumRowsParameterName : "maximumRows";
			}
			set {
				if (MaximumRowsParameterName == value)
					return;
				maximumRowsParameterName = value;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		[DefaultValue ("{0}")]
		public string OldValuesParameterFormatString {
			get {
				return oldValuesParameterFormatString != null ? oldValuesParameterFormatString : "{0}";
			}
			set {
				if (OldValuesParameterFormatString == value)
					return;
				oldValuesParameterFormatString = value;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		public string SelectCountMethod {
			get {
				return selectCountMethod != null ? selectCountMethod : string.Empty;
			}
			set {
				if (SelectCountMethod == value)
					return;
				selectCountMethod = value;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		public string SelectMethod {
			get {
				return selectMethod != null ? selectMethod : string.Empty;
			}
			set {
				if (SelectMethod == value)
					return;
				selectMethod = value;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		public ParameterCollection SelectParameters {
			get {
				if (selectParameters == null) {
					selectParameters = new ParameterCollection ();
					selectParameters.ParametersChanged += new EventHandler (OnParametersChanged); 
					if (IsTrackingViewState)
						((IStateManager)selectParameters).TrackViewState ();
				}
				return selectParameters;
			}
		}

		public string SortParameterName {
			get {
				return sortParameterName != null ? sortParameterName : string.Empty;
			}
			set {
				sortParameterName = value;
			}
		}

		public string StartRowIndexParameterName {
			get {
				return startRowIndexParameterName != null ? startRowIndexParameterName : "startRowIndex";
			}
			set {
				if (StartRowIndexParameterName == value)
					return;
				startRowIndexParameterName = value;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		public string TypeName {
			get {
				return typeName != null ? typeName : string.Empty;
			}
			set {
				if (TypeName == value)
					return;
				typeName = value;
				objectType = null;
				OnDataSourceViewChanged (EventArgs.Empty);
			}
		}

		public string UpdateMethod {
			get {
				return updateMethod != null ? updateMethod : string.Empty;
			}
			set {
				updateMethod = value;
			}
		}

		public ParameterCollection UpdateParameters {
			get {
				if (updateParameters == null) {
					updateParameters = new ParameterCollection ();
				}
				return updateParameters;
			}
		}
    
		Type ObjectType {
			get {
				if (objectType == null) {
					objectType = HttpApplication.LoadType (TypeName);
					if (objectType == null)
						throw new InvalidOperationException ("Type not found: " + TypeName);
				}
				return objectType;
			}
		}
		
		Type DataObjectType {
			get {
				if (dataObjectType == null) {
					dataObjectType = HttpApplication.LoadType (DataObjectTypeName);
					if (dataObjectType == null)
						throw new InvalidOperationException ("Type not found: " + DataObjectTypeName);
				}
				return dataObjectType;
			}
		}
		
		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			return ExecuteSelect (arguments);
		}
		
		public int Update (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return ExecuteUpdate (keys, values, oldValues);
		}
		
		public int Delete (IDictionary keys, IDictionary oldValues)
		{
			return ExecuteDelete (keys, oldValues);
		}
		
		public int Insert (IDictionary values)
		{
			return ExecuteInsert (values);
		}
		
		protected override int ExecuteInsert (IDictionary values)
		{
			if (!CanInsert)
				throw new NotSupportedException ("Insert operation not supported.");
				
			IOrderedDictionary paramValues;
			MethodInfo method;
			
			if (DataObjectTypeName.Length == 0) {
				paramValues = MergeParameterValues (InsertParameters, values, null);
				method = GetObjectMethod (InsertMethod, paramValues, DataObjectMethodType.Insert);
			} else {
				method = ResolveDataObjectMethod (InsertMethod, values, null, out paramValues);
			}
			
			ObjectDataSourceMethodEventArgs args = new ObjectDataSourceMethodEventArgs (paramValues);
			OnInserting (args);
			if (args.Cancel)
				return -1;
			
			ObjectDataSourceStatusEventArgs rargs = InvokeMethod (method, paramValues);
			OnInserted (rargs);
			
			if (rargs.Exception != null && !rargs.ExceptionHandled)
				throw rargs.Exception;

			if (owner.EnableCaching)
				owner.Cache.Expire ();
			
			OnDataSourceViewChanged (EventArgs.Empty);

			return -1;
		}

		protected override int ExecuteDelete (IDictionary keys, IDictionary oldValues)
		{
			if (!CanDelete)
				throw new NotSupportedException ("Delete operation not supported.");
				
			if (ConflictDetection == ConflictOptions.CompareAllValues && (oldValues == null || oldValues.Count == 0))
				throw new InvalidOperationException ("ConflictDetection is set to CompareAllValues and oldValues collection is null or empty.");

			IDictionary oldDataValues;
			oldDataValues = BuildOldValuesList (keys, oldValues, false);
					
			IOrderedDictionary paramValues;
			MethodInfo method;
			
			if (DataObjectTypeName.Length == 0) {
				paramValues = MergeParameterValues (DeleteParameters, null, oldDataValues);
				method = GetObjectMethod (DeleteMethod, paramValues, DataObjectMethodType.Delete);
			} else {
				method = ResolveDataObjectMethod (DeleteMethod, oldDataValues, null, out paramValues);
			}
			
			ObjectDataSourceMethodEventArgs args = new ObjectDataSourceMethodEventArgs (paramValues);
			OnDeleting (args);
			if (args.Cancel)
				return -1;
			
			ObjectDataSourceStatusEventArgs rargs = InvokeMethod (method, paramValues);
			
			OnDeleted (rargs);
			
			if (rargs.Exception != null && !rargs.ExceptionHandled)
				throw rargs.Exception;

			if (owner.EnableCaching)
				owner.Cache.Expire ();
			
			OnDataSourceViewChanged (EventArgs.Empty);

			return -1;
		}

		protected override int ExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			IOrderedDictionary paramValues;
			MethodInfo method;

			IDictionary oldDataValues;
			oldDataValues = BuildOldValuesList (keys, oldValues, true);

			if (DataObjectTypeName.Length == 0)
			{
				IDictionary dataValues;
				dataValues = values;
				paramValues = MergeParameterValues (UpdateParameters, dataValues, oldDataValues);
				method = GetObjectMethod (UpdateMethod, paramValues, DataObjectMethodType.Update);
			}
			else
			{
				if (ConflictDetection != ConflictOptions.CompareAllValues) {
					oldDataValues = null;
				}
				IDictionary dataValues = new Hashtable ();
				if (keys != null) {
					foreach (DictionaryEntry de in keys)
						dataValues [de.Key] = de.Value;
				}
				if (values != null) {
					foreach (DictionaryEntry de in values)
						dataValues [de.Key] = de.Value;
				}

				method = ResolveDataObjectMethod (UpdateMethod, dataValues, oldDataValues, out paramValues);
			}			

			ObjectDataSourceMethodEventArgs args = new ObjectDataSourceMethodEventArgs (paramValues);
			OnUpdating (args);
			if (args.Cancel)
				return -1;
			
			ObjectDataSourceStatusEventArgs rargs = InvokeMethod (method, paramValues);
			OnUpdated (rargs);
			
			if (rargs.Exception != null && !rargs.ExceptionHandled)
				throw rargs.Exception;

			if (owner.EnableCaching)
				owner.Cache.Expire ();
			
			OnDataSourceViewChanged (EventArgs.Empty);

			return -1;
		}

		IDictionary BuildOldValuesList (IDictionary keys, IDictionary oldValues, bool keysWin) 
		{
			IDictionary oldDataValues;
			if (ConflictDetection == ConflictOptions.CompareAllValues) {
				oldDataValues = new Hashtable ();
				if (keys != null && !keysWin) {
					foreach (DictionaryEntry de in keys)
						oldDataValues [de.Key] = de.Value;
				}
				if (oldValues != null) {
					foreach (DictionaryEntry de in oldValues)
						oldDataValues [de.Key] = de.Value;
				}
				if (keys != null && keysWin) {
					foreach (DictionaryEntry de in keys)
						oldDataValues [de.Key] = de.Value;
				}
			}
			else {
				oldDataValues = keys;
			}

			return oldDataValues;
		}

		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			arguments.RaiseUnsupportedCapabilitiesError (this);

			IOrderedDictionary paramValues = MergeParameterValues (SelectParameters, null, null);
			ObjectDataSourceSelectingEventArgs args = new ObjectDataSourceSelectingEventArgs (paramValues, arguments, false);

			object result = null;

			if (owner.EnableCaching)
				result = owner.Cache.GetCachedObject (SelectMethod, SelectParameters);

			if (result == null) {
				OnSelecting (args);
				if (args.Cancel)
					return new ArrayList ();
				
				if (CanPage) {
					if (StartRowIndexParameterName.Length == 0)
						throw new InvalidOperationException ("Paging is enabled, but the StartRowIndexParameterName property is not set.");
					if (MaximumRowsParameterName.Length == 0)
						throw new InvalidOperationException ("Paging is enabled, but the MaximumRowsParameterName property is not set.");
					paramValues [StartRowIndexParameterName] = arguments.StartRowIndex;
					paramValues [MaximumRowsParameterName] = arguments.MaximumRows;
				}

				if (SortParameterName.Length > 0)
					paramValues [SortParameterName] = arguments.SortExpression;

				result = InvokeSelect (SelectMethod, paramValues);

				if (CanRetrieveTotalRowCount && arguments.RetrieveTotalRowCount)
					arguments.TotalRowCount = QueryTotalRowCount (MergeParameterValues (SelectParameters, null, null), arguments);
				
				if (owner.EnableCaching)
					owner.Cache.SetCachedObject (SelectMethod, SelectParameters, result);
			}

			if (FilterExpression.Length > 0 && !(result is DataGrid || result is DataView || result is DataTable))
				throw new NotSupportedException ("The FilterExpression property was set and the Select method does not return a DataSet, DataTable, or DataView.");

			if (owner.EnableCaching && result is IDataReader)
				throw new NotSupportedException ("Data source does not support caching objects that implement IDataReader");
			
			if (result is DataSet) {
				DataSet dset = (DataSet) result;
				if (dset.Tables.Count == 0)
					throw new InvalidOperationException ("The select method returnet a DataSet which doesn't contain any table.");
				result = dset.Tables [0];
			}
			
			if (result is DataTable) {
				DataView dview = new DataView ((DataTable)result);
				if (arguments.SortExpression != null && arguments.SortExpression.Length > 0) {
					dview.Sort = arguments.SortExpression;
				}
				if (FilterExpression.Length > 0) {
					IOrderedDictionary fparams = FilterParameters.GetValues (context, owner);
					ObjectDataSourceFilteringEventArgs fargs = new ObjectDataSourceFilteringEventArgs (fparams);
					OnFiltering (fargs);
					if (!fargs.Cancel) {
						object[] formatValues = new object [fparams.Count];
						for (int n=0; n<formatValues.Length; n++) {
							formatValues [n] = fparams [n];
							if (formatValues [n] == null) return dview;
						}
						dview.RowFilter = string.Format	(FilterExpression, formatValues);
					}
				}
				return dview;
			}
			
			if (result is IEnumerable)
				return (IEnumerable) result;
			else
				return new object[] {result};
		}
		
		int QueryTotalRowCount (IOrderedDictionary mergedParameters, DataSourceSelectArguments arguments)
		{
			ObjectDataSourceSelectingEventArgs countArgs = new ObjectDataSourceSelectingEventArgs (mergedParameters, arguments, true);
			OnSelecting (countArgs);
			if (countArgs.Cancel)
				return 0;
			
			object count = InvokeSelect (SelectCountMethod, mergedParameters);
			return (int) Convert.ChangeType (count, typeof(int));
		}
		
		object InvokeSelect (string methodName, IOrderedDictionary paramValues)
		{
			MethodInfo method = GetObjectMethod (methodName, paramValues, DataObjectMethodType.Select);
			ObjectDataSourceStatusEventArgs rargs = InvokeMethod (method, paramValues);
			OnSelected (rargs);
			
			if (rargs.Exception != null && !rargs.ExceptionHandled)
				throw rargs.Exception;

			return rargs.ReturnValue;
		}
		
		ObjectDataSourceStatusEventArgs InvokeMethod (MethodInfo method, IOrderedDictionary paramValues)
		{
			object instance = null;

			if (!method.IsStatic)
				instance = CreateObjectInstance ();

			ParameterInfo[] pars = method.GetParameters ();
			
			ArrayList outParamInfos;
			object[] methodArgs = GetParameterArray (pars, paramValues, out outParamInfos); 
			
			if (methodArgs == null)
				throw CreateMethodException (method.Name, paramValues);
					
			object result = null;
			Hashtable outParams = null;
			
			try {
				result = method.Invoke (instance, methodArgs);
				if (outParamInfos != null) {
					outParams = new Hashtable ();
					foreach (ParameterInfo op in outParamInfos)
						outParams [op.Name] = methodArgs [op.Position];
				}
				return new ObjectDataSourceStatusEventArgs (result, outParams, null);
			}
			catch (Exception ex) {
				return new ObjectDataSourceStatusEventArgs (result, outParams, ex);
			}
			finally {
				if (instance != null)
					DisposeObjectInstance (instance);
			}
		}
		
		MethodInfo GetObjectMethod (string methodName, IOrderedDictionary parameters, DataObjectMethodType methodType)
		{
			MemberInfo[] methods = ObjectType.GetMember (methodName, MemberTypes.Method, BindingFlags.Instance | 
										 BindingFlags.Static | 
										 BindingFlags.Public | 
										 BindingFlags.IgnoreCase |
										 BindingFlags.FlattenHierarchy);
			if (methods.Length > 1) {
				// MSDN: The ObjectDataSource resolves method overloads by method name and number
				// of parameters; the names and types of the parameters are not considered.
				// LAMESPEC: the tests show otherwise
				DataObjectMethodAttribute methodAttribute = null;
				MethodInfo methodInfo = null;
				bool hasConflict = false;
				foreach (MethodInfo me in methods) { // we look for methods only
					ParameterInfo [] pinfos = me.GetParameters ();
					if (pinfos.Length == parameters.Count) {
						object [] attrs = me.GetCustomAttributes (typeof (DataObjectMethodAttribute), true);
						DataObjectMethodAttribute domAttr = (attrs != null && attrs.Length > 0) ? (DataObjectMethodAttribute) attrs [0] : null;
						if (domAttr != null && domAttr.MethodType != methodType)
							continue;

						bool paramsMatch = true;
						foreach (ParameterInfo pinfo in pinfos) {
							if (!parameters.Contains (pinfo.Name)) {
								paramsMatch = false;
								break;
							}
						}

						if (!paramsMatch)
							continue;

						if (domAttr != null) {
							if (methodAttribute != null) {
								if (methodAttribute.IsDefault) {
									if (domAttr.IsDefault) {
										methodInfo = null;
										break; //fail due to a conflict
									}
									else
										continue; //existing matches better
								}
								else {
									methodInfo = null; //we override
									hasConflict = !domAttr.IsDefault;
								}
							}
							else
								methodInfo = null; //we override
						}

						if (methodInfo == null) {
							methodAttribute = domAttr;
							methodInfo = me;
							continue;
						}

						hasConflict = true;
					}
				}

				if (!hasConflict && methodInfo != null)
					return methodInfo;
			}
			else if (methods.Length == 1) {
				MethodInfo me = methods[0] as MethodInfo;
				if (me != null && me.GetParameters().Length == parameters.Count)
					return me;
			}
			
			throw CreateMethodException (methodName, parameters);
		}
		
		MethodInfo ResolveDataObjectMethod (string methodName, IDictionary values, IDictionary oldValues, out IOrderedDictionary paramValues)
		{
			MethodInfo method;
			if (oldValues != null)
				method = ObjectType.GetMethod (methodName, new Type[] { DataObjectType, DataObjectType });
			else
				method = ObjectType.GetMethod (methodName, new Type[] { DataObjectType });
			
			if (method == null)
				throw new InvalidOperationException ("ObjectDataSource " + owner.ID + " could not find a method named '" + methodName + "' with parameters of type '" + DataObjectType + "' in '" + ObjectType + "'.");

			paramValues = new OrderedDictionary (StringComparer.InvariantCultureIgnoreCase);
			ParameterInfo[] ps = method.GetParameters ();
			
			if (oldValues != null) {
				if (FormatOldParameter (ps[0].Name) == ps[1].Name) {
					paramValues [ps[0].Name] = CreateDataObject (values);
					paramValues [ps[1].Name] = CreateDataObject (oldValues);
				} else if (FormatOldParameter (ps[1].Name) == ps[0].Name) {
					paramValues [ps[0].Name] = CreateDataObject (oldValues);
					paramValues [ps[1].Name] = CreateDataObject (values);
				} else
					throw new InvalidOperationException ("Method '" + methodName + "' does not have any parameter that fits the value of OldValuesParameterFormatString.");  
			} else {
				paramValues [ps[0].Name] = CreateDataObject (values);
			}
			return method;
		}
		
		Exception CreateMethodException (string methodName, IOrderedDictionary parameters)
		{
			string s = "";
			foreach (string p in parameters.Keys) {
				s += p + ", ";
			}
			return new InvalidOperationException ("ObjectDataSource " + owner.ID + " could not find a method named '" + methodName + "' with parameters " + s + "in type '" + ObjectType + "'.");
		}
		
		object CreateDataObject (IDictionary values)
		{
			object ob = Activator.CreateInstance (DataObjectType);
			foreach (DictionaryEntry de in values) {
				PropertyInfo p = DataObjectType.GetProperty ((string)de.Key);
				if (p == null) throw new InvalidOperationException ("Property " + de.Key + " not found in type '" +DataObjectType + "'.");
				object[] attributes = p.GetCustomAttributes (typeof (System.ComponentModel.TypeConverterAttribute),
									     true);
				Type propertyType = p.PropertyType;
				object value = de.Value;
				object converted = ConvertParameterWithTypeConverter (attributes, propertyType, value);
				if (converted == null)
					converted = ConvertParameter (propertyType, value);
						   
				p.SetValue (ob, converted, null);
			}
			return ob;
		}
		
		object CreateObjectInstance ()
		{
			ObjectDataSourceEventArgs args = new ObjectDataSourceEventArgs (null);
			OnObjectCreating (args);
			
			if (args.ObjectInstance != null)
				return args.ObjectInstance;
				
			object ob = Activator.CreateInstance (ObjectType);
			
			args.ObjectInstance = ob;
			OnObjectCreated (args);
			
			return args.ObjectInstance;
		}
		
		void DisposeObjectInstance (object obj)
		{
			ObjectDataSourceDisposingEventArgs args = new ObjectDataSourceDisposingEventArgs (obj);
			OnObjectDisposing (args);
			
			if (!args.Cancel) {
				IDisposable disp = obj as IDisposable;
				if (disp != null) disp.Dispose ();
			}
		}

		object FindValueByName (string name, IDictionary values, bool format)
		{
			if (values == null)
				return null;

			foreach (DictionaryEntry de in values) {
				string valueName = format == true ? FormatOldParameter (de.Key.ToString ()) : de.Key.ToString ();
				if (String.Compare (name, valueName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return values [de.Key];
			}
			return null;
		}

		/// <summary>
		/// Merge the current data item fields with view parameter default values
		/// </summary>
		/// <param name="viewParams">default parameters</param>
		/// <param name="values">new parameters for update and insert</param>
		/// <param name="oldValues">old parameters for update and delete</param>
		/// <param name="allwaysAddNewValues">true for insert, as current item is
		/// irrelevant for insert</param>
		/// <returns>merged values</returns>
		IOrderedDictionary MergeParameterValues (ParameterCollection viewParams, IDictionary values, IDictionary oldValues)
		{
			IOrderedDictionary parametersValues = viewParams.GetValues (context, owner);
			OrderedDictionary mergedValues = new OrderedDictionary (StringComparer.InvariantCultureIgnoreCase);
			foreach (string parameterName in parametersValues.Keys) {
				mergedValues [parameterName] = parametersValues [parameterName];
				if (oldValues != null) {
					object value = FindValueByName (parameterName, oldValues, true);
					if (value != null) {
						object dataValue = viewParams [parameterName].ConvertValue (value);
						mergedValues [parameterName] = dataValue;
					}
				}

				if (values != null) {
					object value = FindValueByName (parameterName, values, false);
					if (value != null) {
						object dataValue = viewParams [parameterName].ConvertValue (value);
						mergedValues [parameterName] = dataValue;
					}
				}
			}

			if (values != null) {
				foreach (DictionaryEntry de in values)
					if (FindValueByName ((string) de.Key, mergedValues, false) == null)
						mergedValues [de.Key] = de.Value;
			}

			if (oldValues != null) {
				foreach (DictionaryEntry de in oldValues) {
					string oldValueKey = FormatOldParameter ((string) de.Key);
					if (FindValueByName (oldValueKey, mergedValues, false) == null)
						mergedValues [oldValueKey] = de.Value;
				}
			}

			return mergedValues;
		}
		
		object[] GetParameterArray (ParameterInfo[] methodParams, IOrderedDictionary viewParams, out ArrayList outParamInfos)
		{
			// FIXME: make this case insensitive

			outParamInfos = null;
			object[] values = new object [methodParams.Length];
			
			foreach (ParameterInfo mp in methodParams) {
			
				// Parameter names must match
				if (!viewParams.Contains (mp.Name)) return null;
				
				values [mp.Position] = ConvertParameter (mp.ParameterType, viewParams [mp.Name]);
				if (mp.ParameterType.IsByRef) {
					if (outParamInfos == null) outParamInfos = new ArrayList ();
					outParamInfos.Add (mp);
				}
			}
			return values;
		}

		object ConvertParameterWithTypeConverter (object[] attributes, Type targetType, object value)
		{
			if (attributes == null || attributes.Length == 0 || value == null)
				return null;
			TypeConverterAttribute tca;
			Type converterType;
			TypeConverter converter;
			
			foreach (object a in attributes) {
				tca = a as TypeConverterAttribute;
				if (tca == null)
					continue;
				converterType = HttpApplication.LoadType (tca.ConverterTypeName, false);
				if (converterType == null)
					continue;
				converter = Activator.CreateInstance (converterType, new object[] {targetType}) as TypeConverter;
				if (converter == null)
					continue;
				if (converter.CanConvertFrom (value.GetType ()))
					return converter.ConvertFrom (value);
				else if (converter.CanConvertFrom (typeof (string)))
					return converter.ConvertFrom (value.ToString ());
			}
			
			return null;
		}
		
		object ConvertParameter (Type targetType, object value)
		{
			return ConvertParameter (Type.GetTypeCode (targetType), value);
		}
		
		object ConvertParameter (TypeCode targetType, object value)
		{
			if (value == null) {
				if (targetType != TypeCode.Object && targetType != TypeCode.String)
					value = 0;
				else if (ConvertNullToDBNull)
					return DBNull.Value;
			}
			if (targetType == TypeCode.Object || targetType == TypeCode.Empty)
				return value;
			else
				return Convert.ChangeType (value, targetType);
		}
		
		string FormatOldParameter (string name)
		{
			string f = OldValuesParameterFormatString;
			if (f.Length > 0)
				return String.Format (f, name);
			else
				return name;
		}
		
		void OnParametersChanged (object sender, EventArgs args)
		{
			OnDataSourceViewChanged (EventArgs.Empty);
		}
		
		protected virtual void LoadViewState (object savedState)
		{
			object[] state = (savedState == null) ? new object [5] : (object[]) savedState;
			((IStateManager)SelectParameters).LoadViewState (state[0]); 
			((IStateManager)UpdateParameters).LoadViewState (state[1]); 
			((IStateManager)DeleteParameters).LoadViewState (state[2]); 
			((IStateManager)InsertParameters).LoadViewState (state[3]); 
			((IStateManager)FilterParameters).LoadViewState (state[4]); 
		}

		protected virtual object SaveViewState()
		{
			object[] state = new object [5];
			
			if (selectParameters != null)
				state [0] = ((IStateManager)selectParameters).SaveViewState ();
			if (updateParameters != null)
				state [1] = ((IStateManager)updateParameters).SaveViewState ();
			if (deleteParameters != null)
				state [2] = ((IStateManager)deleteParameters).SaveViewState ();
			if (insertParameters != null)
				state [3] = ((IStateManager)insertParameters).SaveViewState ();
			if (filterParameters != null)
				state [4] = ((IStateManager)filterParameters).SaveViewState ();
			
			foreach (object ob in state)
				if (ob != null) return state;
			
			return null;
		}
		
		protected virtual void TrackViewState()
		{
			isTrackingViewState = true;

			if (selectParameters != null) ((IStateManager)selectParameters).TrackViewState ();
			if (filterParameters != null) ((IStateManager)filterParameters).TrackViewState ();
		}
		
		protected bool IsTrackingViewState
		{
			get { return isTrackingViewState; }
		}
		
		
		bool IStateManager.IsTrackingViewState
		{
			get { return IsTrackingViewState; }
		}
		
		void IStateManager.TrackViewState()
		{
			TrackViewState ();
		}
		
		void IStateManager.LoadViewState (object savedState)
		{
			LoadViewState (savedState);
		}

		object IStateManager.SaveViewState()
		{
			return SaveViewState ();
		}
	}
}
#endif





