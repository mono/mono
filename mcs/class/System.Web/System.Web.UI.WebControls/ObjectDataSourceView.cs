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

namespace System.Web.UI.WebControls
{

	public class ObjectDataSourceView : DataSourceView, IStateManager
	{
		ObjectDataSource owner;
		HttpContext context;
		Type objectType;
		Type dataObjectType;
		
		StateBag viewState = new StateBag ();
		ParameterCollection selectParameters;
		ParameterCollection updateParameters;
		ParameterCollection deleteParameters;
		ParameterCollection insertParameters;
		ParameterCollection filterParameters;
		
		private static readonly object DeletedEvent = new object();
		private static readonly object DeletingEvent = new object();
		private static readonly object FilteringEvent = new object();
		private static readonly object InsertedEvent = new object();
		private static readonly object InsertingEvent = new object();
		private static readonly object ObjectCreatedEvent = new object();
		private static readonly object ObjectCreatingEvent = new object();
		private static readonly object ObjectDisposingEvent = new object();
//		private static readonly object ResolvingMethodEvent = new object();
		private static readonly object SelectedEvent = new object();
		private static readonly object SelectingEvent = new object();
		private static readonly object UpdatedEvent = new object();
		private static readonly object UpdatingEvent = new object();
		
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
		
		protected StateBag ViewState {
			get { return viewState; }
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
			get { return SelectCountMethod.Length > 0; }
		}
		
		public override bool CanSort {
			get { return true; }
		}
		
		public override bool CanUpdate {
			get { return UpdateMethod.Length > 0; }
		}
		
		public ConflictOptions ConflictDetection {
			get {
				object ret = ViewState ["ConflictDetection"];
				return ret != null ? (ConflictOptions)ret : ConflictOptions.OverwriteChanges;
			}
			set {
				ViewState ["ConflictDetection"] = value;
			}
		}
		
		public bool ConvertNullToDBNull {
			get {
				object ret = ViewState ["ConvertNullToDBNull"];
				return ret != null ? (bool)ret : false;
			}
			set {
				ViewState ["ConvertNullToDBNull"] = value;
			}
		}
		
		public string DataObjectTypeName {
			get {
				object ret = ViewState ["DataObjectTypeName"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["DataObjectTypeName"] = value;
			}
		}
		
		public string DeleteMethod {
			get {
				object ret = ViewState ["DeleteMethod"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["DeleteMethod"] = value;
			}
		}
		
		public ParameterCollection DeleteParameters {
			get {
				if (deleteParameters == null) {
					deleteParameters = new ParameterCollection ();
					deleteParameters.ParametersChanged += new EventHandler (OnParametersChanged); 
					if (((IStateManager)this).IsTrackingViewState)
						((IStateManager)deleteParameters).TrackViewState ();
				}
				return deleteParameters;
			}
		}
		
		public bool EnablePaging {
			get {
				object ret = ViewState ["EnablePaging"];
				return ret != null ? (bool)ret : false;
			}
			set {
				ViewState ["EnablePaging"] = value;
			}
		}
		
		public string FilterExpression {
			get {
				object ret = ViewState ["FilterExpression"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["FilterExpression"] = value;
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
				object ret = ViewState ["InsertMethod"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["InsertMethod"] = value;
			}
		}
		
		public ParameterCollection InsertParameters {
			get {
				if (insertParameters == null) {
					insertParameters = new ParameterCollection ();
					insertParameters.ParametersChanged += new EventHandler (OnParametersChanged); 
					if (IsTrackingViewState)
						((IStateManager)insertParameters).TrackViewState ();
				}
				return insertParameters;
			}
		}
		
		public string MaximumRowsParameterName {
			get {
				object ret = ViewState ["MaximumRowsParameterName"];
				return ret != null ? (string)ret : "maximumRows";
			}
			set {
				ViewState ["MaximumRowsParameterName"] = value;
			}
		}
		
	    [DefaultValueAttribute ("original_{0}")]
		public string OldValuesParameterFormatString {
			get {
				object ret = ViewState ["OldValuesParameterFormatString"];
				return ret != null ? (string)ret : "original_{0}";
			}
			set {
				ViewState ["OldValuesParameterFormatString"] = value;
			}
		}
				
		public string SelectCountMethod {
			get {
				object ret = ViewState ["SelectCountMethod"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["SelectCountMethod"] = value;
			}
		}
		
		public string SelectMethod {
			get {
				object ret = ViewState ["SelectMethod"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["SelectMethod"] = value;
			}
		}
		
		public ParameterCollection SelectParameters {
			get {
				if (selectParameters == null) {
					selectParameters = new ParameterCollection ();
					selectParameters.ParametersChanged += new EventHandler (OnParametersChanged); 
					if (((IStateManager)this).IsTrackingViewState)
						((IStateManager)selectParameters).TrackViewState ();
				}
				return selectParameters;
			}
		}
		
		public string SortParameterName {
			get {
				object ret = ViewState ["SortParameterName"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["SortParameterName"] = value;
			}
		}
		
		public string StartRowIndexParameterName {
			get {
				object ret = ViewState ["StartRowIndexParameterName"];
				return ret != null ? (string)ret : "startRowIndex";
			}
			set {
				ViewState ["StartRowIndexParameterName"] = value;
			}
		}
		
		public string TypeName {
			get {
				object ret = ViewState ["TypeName"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["TypeName"] = value;
				objectType = null;
			}
		}
		
		public string UpdateMethod {
			get {
				object ret = ViewState ["UpdateMethod"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["UpdateMethod"] = value;
			}
		}
		
		public ParameterCollection UpdateParameters {
			get {
				if (updateParameters == null) {
					updateParameters = new ParameterCollection ();
					updateParameters.ParametersChanged += new EventHandler (OnParametersChanged); 
					if (((IStateManager)this).IsTrackingViewState)
						((IStateManager)updateParameters).TrackViewState ();
				}
				return updateParameters;
			}
		}
		
		Type ObjectType {
			get {
				if (objectType == null) {
					objectType = Type.GetType (TypeName);
					if (objectType == null)
						throw new InvalidOperationException ("Type not found: " + TypeName);
				}
				return objectType;
			}
		}
		
		Type DataObjectType {
			get {
				if (dataObjectType == null) {
					dataObjectType = Type.GetType (DataObjectTypeName);
					if (objectType == null)
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
				paramValues = MergeParameterValues (InsertParameters, values, null, true);
				method = GetObjectMethod (InsertMethod, paramValues);
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

			return -1;
		}

		protected override int ExecuteDelete (IDictionary keys, IDictionary oldValues)
		{
			if (!CanDelete)
				throw new NotSupportedException ("Delete operation not supported.");
				
			if (ConflictDetection == ConflictOptions.CompareAllValues && (oldValues == null || oldValues.Count == 0))
				throw new InvalidOperationException ("ConflictDetection is set to CompareAllValues and oldValues collection is null or empty.");

			IDictionary oldDataValues;
			if (ConflictDetection == ConflictOptions.CompareAllValues) {
				oldDataValues = new Hashtable ();
				foreach (DictionaryEntry de in keys)
					oldDataValues [de.Key] = de.Value;
				foreach (DictionaryEntry de in oldValues)
					oldDataValues [de.Key] = de.Value;
			} else
				oldDataValues = keys;
					
			IOrderedDictionary paramValues;
			MethodInfo method;
			
			if (DataObjectTypeName.Length == 0) {
				paramValues = MergeParameterValues (DeleteParameters, null, oldDataValues, true);
				method = GetObjectMethod (DeleteMethod, paramValues);
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

			return -1;
		}
		
		protected override int ExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			IOrderedDictionary paramValues;
			MethodInfo method;
			
			if (DataObjectTypeName.Length == 0)
			{
				IDictionary dataValues;
				IDictionary oldDataValues;
				if (ConflictDetection == ConflictOptions.CompareAllValues) {
					oldDataValues = new Hashtable ();
					dataValues = values;
					foreach (DictionaryEntry de in keys)
						oldDataValues [de.Key] = de.Value;
					foreach (DictionaryEntry de in oldValues)
						oldDataValues [de.Key] = de.Value;
				} else {
					oldDataValues = keys;
					dataValues = values;
				}
				paramValues = MergeParameterValues (UpdateParameters, dataValues, oldDataValues, false);
				method = GetObjectMethod (UpdateMethod, paramValues);
			}
			else
			{
				IDictionary dataValues = new Hashtable ();
				IDictionary oldDataValues;
				
				foreach (DictionaryEntry de in values)
					dataValues [de.Key] = de.Value;
					
				if (ConflictDetection == ConflictOptions.CompareAllValues) {
					oldDataValues = new Hashtable ();
					foreach (DictionaryEntry de in keys) {
						oldDataValues [de.Key] = de.Value;
						dataValues [de.Key] = de.Value;
					}
					foreach (DictionaryEntry de in oldValues)
						oldDataValues [de.Key] = de.Value;
				} else {
					oldDataValues = null;
					foreach (DictionaryEntry de in keys)
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

			return -1;
		}
		
		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			arguments.RaiseUnsupportedCapabilitiesError (this);

			IOrderedDictionary paramValues = MergeParameterValues (SelectParameters, null, null, true);
			ObjectDataSourceSelectingEventArgs args = new ObjectDataSourceSelectingEventArgs (paramValues, arguments, false);
			OnSelecting (args);
			if (args.Cancel)
				return new ArrayList ();
			
			if (CanRetrieveTotalRowCount && arguments.RetrieveTotalRowCount)
				arguments.TotalRowCount = QueryTotalRowCount (paramValues, arguments);
			
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
			
			object result = InvokeSelect (SelectMethod, paramValues);
			
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
				return new object[] { result };
		}
		
		protected virtual int QueryTotalRowCount (IOrderedDictionary mergedParameters, DataSourceSelectArguments arguments)
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
			MethodInfo method = GetObjectMethod (methodName, paramValues);
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
						outParams [op.Name] = methodArgs [op.Position - 1];
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
		
		MethodInfo GetObjectMethod (string methodName, IOrderedDictionary parameters)
		{
			MemberInfo[] methods = ObjectType.GetMember (methodName);
			if (methods.Length > 1) {
				// MSDN: The ObjectDataSource resolves method overloads by method name and number
				// of parameters; the names and types of the parameters are not considered.
				foreach (MemberInfo mi in methods) {
					MethodInfo me = mi as MethodInfo;
					if (me != null && me.GetParameters().Length == parameters.Count)
						return me;
				}
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
			
			paramValues = new OrderedDictionary ();
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
				p.SetValue (ob, ConvertParameter (p.PropertyType, de.Value), null);
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
		
		IOrderedDictionary MergeParameterValues (ParameterCollection viewParams, IDictionary values, IDictionary oldValues, bool allwaysAddNewValues)
		{
			OrderedDictionary mergedValues = new OrderedDictionary ();
			foreach (Parameter p in viewParams) {
				bool oldAdded = false;
				if (oldValues != null && oldValues.Contains (p.Name)) {
					object val = Convert.ChangeType (oldValues [p.Name], p.Type);
					mergedValues [FormatOldParameter (p.Name)] = val;
					oldAdded = true;
				}
				
				if (values != null && values.Contains (p.Name)) {
					object val = Convert.ChangeType (values [p.Name], p.Type);
					mergedValues [p.Name] = val;
				} else if (!oldAdded || allwaysAddNewValues) {
					object val = p.GetValue (context, owner);
					mergedValues [p.Name] = val;
				}
			}
			
			if (values != null) {
				foreach (DictionaryEntry de in values)
					if (!mergedValues.Contains (de.Key))
						mergedValues [de.Key] = de.Value;
			}
			
			if (oldValues != null) {
				foreach (DictionaryEntry de in oldValues)
					if (!mergedValues.Contains (FormatOldParameter ((string)de.Key)))
						mergedValues [FormatOldParameter ((string)de.Key)] = de.Value;
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
		
		object ConvertParameter (Type targetType, object value)
		{
			return ConvertParameter (Type.GetTypeCode (targetType), value);
		}
		
		object ConvertParameter (TypeCode targetType, object value)
		{
			if (value == null) {
				if (targetType != TypeCode.Object && targetType != TypeCode.String)
					value = 0;
				else if (targetType == TypeCode.Object && ConvertNullToDBNull)
					return DBNull.Value;
			}
			if (targetType == TypeCode.Object)
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
			object[] state = (savedState == null) ? new object [6] : (object[]) savedState;
			viewState.LoadViewState (state[0]);
			((IStateManager)SelectParameters).LoadViewState (state[1]); 
			((IStateManager)UpdateParameters).LoadViewState (state[2]); 
			((IStateManager)DeleteParameters).LoadViewState (state[3]); 
			((IStateManager)InsertParameters).LoadViewState (state[4]); 
			((IStateManager)FilterParameters).LoadViewState (state[5]); 
		}

		protected virtual object SaveViewState()
		{
			object[] state = new object [6];
			state [0] = viewState.SaveViewState ();
			
			if (selectParameters != null)
				state [1] = ((IStateManager)selectParameters).SaveViewState ();
			if (updateParameters != null)
				state [2] = ((IStateManager)updateParameters).SaveViewState ();
			if (deleteParameters != null)
				state [3] = ((IStateManager)deleteParameters).SaveViewState ();
			if (insertParameters != null)
				state [4] = ((IStateManager)insertParameters).SaveViewState ();
			if (filterParameters != null)
				state [5] = ((IStateManager)filterParameters).SaveViewState ();
			
			foreach (object ob in state)
				if (ob != null) return state;
			
			return null;
		}
		
		protected virtual void TrackViewState()
		{
			viewState.TrackViewState ();
			if (selectParameters != null) ((IStateManager)selectParameters).TrackViewState ();
			if (updateParameters != null) ((IStateManager)updateParameters).TrackViewState ();
			if (deleteParameters != null) ((IStateManager)deleteParameters).TrackViewState ();
			if (insertParameters != null) ((IStateManager)insertParameters).TrackViewState ();
			if (filterParameters != null) ((IStateManager)filterParameters).TrackViewState ();
		}
		
		protected virtual bool IsTrackingViewState
		{
			get { return viewState.IsTrackingViewState; }
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

