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
		
		StateBag viewState = new StateBag ();
		ParameterCollection selectParameters;
		ParameterCollection updateParameters;
		
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
		
		public override bool CanPage {
			get { return EnablePaging; }
		}
		
		public override bool CanRetrieveTotalRowCount {
			get { return SelectCountMethod.Length > 0; }
		}
		
		public override bool CanUpdate {
			get { return UpdateMethod.Length > 0; }
		}
		
		public virtual bool EnablePaging {
			get {
				object ret = ViewState ["EnablePaging"];
				return ret != null ? (bool)ret : false;
			}
			set {
				ViewState ["EnablePaging"] = value;
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
		
		public virtual string SelectCountMethod {
			get {
				object ret = ViewState ["SelectCountMethod"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["SelectCountMethod"] = value;
			}
		}
		
		public virtual string SelectMethod {
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
					if (((IStateManager)this).IsTrackingViewState)
						((IStateManager)selectParameters).TrackViewState ();
				}
				return selectParameters;
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
		
		public virtual string TypeName {
			get {
				object ret = ViewState ["TypeName"];
				return ret != null ? (string)ret : string.Empty;
			}
			set {
				ViewState ["TypeName"] = value;
				objectType = null;
			}
		}
		
		public virtual string UpdateMethod {
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
		
		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			return ExecuteSelect (arguments);
		}
		
		protected override int ExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			Hashtable allValues = new Hashtable ();
			foreach (DictionaryEntry de in keys)
				allValues [de.Key] = de.Value;
			foreach (DictionaryEntry de in values)
				allValues [de.Key] = de.Value;

			IOrderedDictionary paramValues = MergeParameterValues (UpdateParameters, allValues);
			
			ObjectDataSourceMethodEventArgs args = new ObjectDataSourceMethodEventArgs (paramValues);
			OnUpdating (args);
			if (args.Cancel)
				return -1;
			
			ObjectDataSourceStatusEventArgs rargs = InvokeMethod (UpdateMethod, paramValues);
			OnUpdated (rargs);
			
			if (rargs.Exception != null && !rargs.ExceptionHandled)
				throw rargs.Exception;

			return -1;
		}
		
		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			arguments.RaiseUnsupportedCapabilitiesError (this);

			IOrderedDictionary paramValues = MergeParameterValues (SelectParameters, null);
			
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
			
			object result = InvokeSelect (SelectMethod, paramValues);
			
			if (result is DataSet) {
				DataSet dset = (DataSet) result;
				if (dset.Tables.Count == 0)
					throw new InvalidOperationException ("The select method returnet a DataSet which doesn't contain any table.");
				result = dset.Tables [0];
			}
			
			if (result is DataTable) {
				DataView dview = new DataView ((DataTable)result);
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
			ObjectDataSourceStatusEventArgs rargs = InvokeMethod (methodName, paramValues);
			OnSelected (rargs);
			
			if (rargs.Exception != null && !rargs.ExceptionHandled)
				throw rargs.Exception;

			return rargs.ReturnValue;
		}
		
		ObjectDataSourceStatusEventArgs InvokeMethod (string methodName, IOrderedDictionary paramValues)
		{
			object instance = null;
			MethodInfo method = GetObjectMethod (methodName, paramValues);
			if (!method.IsStatic)
				instance = CreateObjectInstance ();

			ParameterInfo[] pars = method.GetParameters ();
			
			ArrayList outParamInfos;
			object[] methodArgs = GetParameterArray (pars, paramValues, out outParamInfos); 
			
			if (methodArgs == null)
				throw CreateMethodException (methodName, paramValues);
					
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
		
		Exception CreateMethodException (string methodName, IOrderedDictionary parameters)
		{
			string s = "";
			foreach (string p in parameters.Keys) {
				s += p + ", ";
			}
			return new InvalidOperationException ("ObjectDataSource " + owner.ID + " could not find a method named '" + methodName + "' with parameters " + s + "in type '" + ObjectType + "'.");
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
		
		IOrderedDictionary MergeParameterValues (ParameterCollection viewParams, IDictionary values)
		{
			OrderedDictionary mergedValues = new OrderedDictionary ();
			foreach (Parameter p in viewParams) {
				object val = values != null ? values [p.Name] : null;
				if (val != null)
					val = Convert.ChangeType (val, p.Type);
				else
					val = p.GetValue (context, owner);
				
				mergedValues [p.Name] = val;
			}
			
			if (values != null) {
				foreach (DictionaryEntry de in values)
					if (!mergedValues.Contains (de.Key))
						mergedValues [de.Key] = de.Value;
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
			return Convert.ChangeType (value, targetType);
		}
		
		protected virtual void LoadViewState (object savedState)
		{
			object[] state = (savedState == null) ? new object [3] : (object[]) savedState;
			viewState.LoadViewState (state[0]);
			((IStateManager)SelectParameters).LoadViewState (state[1]); 
			((IStateManager)UpdateParameters).LoadViewState (state[1]); 
		}

		protected virtual object SaveViewState()
		{
			object[] state = new object [3];
			state [0] = viewState.SaveViewState ();
			
			if (selectParameters != null)
				state [1] = ((IStateManager)selectParameters).SaveViewState ();
			if (updateParameters != null)
				state [2] = ((IStateManager)updateParameters).SaveViewState ();
			
			foreach (object ob in state)
				if (ob != null) return state;
			
			return null;
		}
		
		protected virtual void TrackViewState()
		{
			viewState.TrackViewState ();
			if (selectParameters != null) ((IStateManager)selectParameters).TrackViewState ();
			if (updateParameters != null) ((IStateManager)updateParameters).TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState
		{
			get { return viewState.IsTrackingViewState; }
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

