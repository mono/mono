//
// System.Web.UI.Page.jvm.cs
//
// Authors:
//   Eyal Alaluf (eyala@mainsoft.com)
//
// (C) 2006 Mainsoft Co. (http://www.mainsoft.com)
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

using javax.servlet.http;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Hosting;
using System.Web.J2EE;
using System.ComponentModel;
using System.IO;
using javax.faces.context;
using javax.faces.render;
using javax.servlet;
using javax.faces;
using javax.faces.application;
using javax.faces.@event;
using javax.faces.el;
using javax.faces.component;
using System.Threading;
using System.Web.Configuration;
using Mainsoft.Web.Hosting;

namespace System.Web.UI
{
	public partial class Page
	{
		string _namespace = null;
		StateManager.SerializedView _facesSerializedView;
		MethodBinding _action;
		MethodBinding _actionListener;
		bool _immediate;
		bool [] _validatorsState;
		ICallbackEventHandler _callbackTarget;
		string _callbackEventError = String.Empty;
		static readonly object CrossPagePostBack = new object ();
		FacesContext _facesContext;
		const string RenderBodyContentOnlyKey = "mainsoft.render.body.content.only";

		static readonly java.util.List emptyList = java.util.Collections.unmodifiableList (new java.util.ArrayList ());

		bool _isMultiForm = false;
		bool _isMultiFormInited = false;

		internal string Namespace
		{
			get {
				if (_namespace == null) {

					if (getFacesContext () != null) {
						_namespace = getFacesContext ().getExternalContext ().encodeNamespace (String.Empty);
					}

					_namespace = _namespace ?? String.Empty;
				}
				return _namespace;
			}
		}

		internal Pair PageState { get; set; }

		internal string theForm {
			get {
				return "theForm" + Namespace;
			}
		}

		internal bool IsMultiForm {
			get {
				if (!_isMultiFormInited) {
					string isMultiForm = WebConfigurationManager.AppSettings ["mainsoft.use.portlet.namespace"];
					_isMultiForm = isMultiForm != null ? Boolean.Parse(isMultiForm) : false;

					_isMultiFormInited = true;
				}
				return _isMultiForm;
			}
		}

		IHttpHandler EnterThread () {

			IHttpHandler jsfHandler = _context.CurrentHandler;
			_context.PopHandler ();
			_context.PushHandler (this);
			if (jsfHandler == _context.Handler)
				_context.Handler = this;
			
			_context.CurrentHandlerInternal = this;

			return jsfHandler;
		}

		void ExitThread (IHttpHandler jsfHandler) {
			// TODO
			//if (context.getResponseComplete ())
			//    Response.End ();

			_context.PopHandler ();
			_context.PushHandler (jsfHandler);
			if (this == _context.Handler)
				_context.Handler = jsfHandler;

		}

		public override void encodeBegin (FacesContext context) {
			// do nothing
		}

		public override void encodeChildren (FacesContext context) {
			System.Diagnostics.Trace.WriteLine ("encodeChildren");

			// reset _facesContext if changed between action and render phases (such portal).
			_facesContext = null;

			IHttpHandler jsfHandler = EnterThread ();
			bool wasException = false;
			try {
				if (!context.getResponseComplete ()) {

					if (IsCallback) {
						string result = ProcessGetCallbackResult (_callbackTarget, _callbackEventError);
						HtmlTextWriter callbackOutput = new HtmlTextWriter (Response.Output);
						callbackOutput.Write (result);
						callbackOutput.Flush ();
						return;
					}

					// ensure lifecycle complete.
					if (!IsLoaded) {
						ProcessLoad ();
						RestoreValidatorsState (_validatorsState);
					}
					if (!IsPrerendered)
						ProcessLoadComplete ();

					RenderPage ();
				}
			}
			catch (Exception ex) {
				wasException = true;
				HandleException (ex);
			}
			finally {
				try {
					if (!wasException)
						ProcessUnload ();
				}
				finally {
					ExitThread (jsfHandler);
				}
			}
		}

		public override void encodeEnd (FacesContext context) {
			// do nothing
		}

		// BUGBUG: must return correct value. Currently returns 0 as performance optimization.
		public override int getChildCount ()
		{
			return 0;
		}

		// BUGBUG: must return correct value. Currently returns empty list as performance optimization.
		public override java.util.List getChildren ()
		{
			return emptyList;
		}

		public override UIComponent getParent () {
			return null;
		}

		public override void setParent (UIComponent parent) {
			//ignore: parent is root
		}

		// TODO: consider validators state
		public override object processSaveState (FacesContext context) {
			System.Diagnostics.Trace.WriteLine ("processSaveState");

			object state = new Pair (PageState, GetValidatorsState ());
			return new StateSerializer (state);
		}

		public override void processRestoreState (FacesContext context, object state) {
			System.Diagnostics.Trace.WriteLine ("processRestoreState");

			if (state == null) {
				Console.WriteLine ("WARNING: processRestoreState was called with null state.");
				return; //throw new ArgumentNullException ("state");
			}
			IHttpHandler jsfHandler = EnterThread ();
			try {
				state = ((StateSerializer) state).State;
				PageState = (Pair) ((Pair) state).First;
				_validatorsState = (bool []) ((Pair) state).Second;
				RestorePageState ();
			}
			catch (Exception ex) {
				HandleException (ex);
			}
			finally {
				ExitThread (jsfHandler);
			}
		}

		public override void processDecodes (FacesContext context) {
			System.Diagnostics.Trace.WriteLine ("processDecodes");

			IHttpHandler jsfHandler = EnterThread ();
			try {
				ProcessPostData ();

				EventRaiserFacesEvent facesEvent = new EventRaiserFacesEvent (this);
				facesEvent.setPhaseId (PhaseId.INVOKE_APPLICATION);
				context.getViewRoot ().queueEvent (facesEvent);

				base.processDecodes (context);
			}
			catch (Exception ex) {
				HandleException (ex);
			}
			finally {
				ExitThread (jsfHandler);
			}
		}

		public override void processValidators (FacesContext context) {
			System.Diagnostics.Trace.WriteLine ("processValidators");

			IHttpHandler jsfHandler = EnterThread ();
			try {
				base.processValidators (context);
			}
			catch (Exception ex) {
				HandleException (ex);
			}
			finally {
				ExitThread (jsfHandler);
			}
		}

		public override void processUpdates (FacesContext context) {
			System.Diagnostics.Trace.WriteLine ("processUpdates");

			IHttpHandler jsfHandler = EnterThread ();
			try {
				base.processUpdates (context);
			}
			catch (Exception ex) {
				HandleException (ex);
			}
			finally {
				ExitThread (jsfHandler);
			}
		}

		public override void broadcast (FacesEvent e) {
			System.Diagnostics.Trace.WriteLine ("broadcast");

			if (!(e is EventRaiserFacesEvent))
				throw new NotSupportedException ("FacesEvent of class " + e.GetType ().Name + " not supported by Page");

			IHttpHandler jsfHandler = EnterThread ();
			bool doUnload = false;
			try {
				ProcessRaiseEvents ();
				doUnload = (ProcessLoadComplete () && IsCrossPagePostBack);
			}
			catch (Exception ex) {
				doUnload = false;
				HandleException (ex);
			}
			finally {
				try {
					if (doUnload) {
						getFacesContext ().responseComplete ();
						ProcessUnload ();
					}
				}
				finally {
					ExitThread (jsfHandler);
				}
			}
		}

		void HandleException (Exception ex) {
			try {
				if (ex is ThreadAbortException) {
					if (FlagEnd.Value == ((ThreadAbortException) ex).ExceptionState) {
						Thread.ResetAbort ();
						return;
					}
					vmw.common.TypeUtils.Throw (ex);
				}
				else
					ProcessException (ex);
			}
			finally {
				if (getFacesContext () != null)
					getFacesContext ().responseComplete ();
				ProcessUnload ();
			}
		}

		bool [] GetValidatorsState () {
			if (is_validated && Validators.Count > 0) {
				bool [] validatorsState = new bool [Validators.Count];
				bool isValid = true;
				for (int i = 0; i < Validators.Count; i++) {
					IValidator val = Validators [i];
					if (!val.IsValid)
						isValid = false;
					else
						validatorsState [i] = true;
				}
				return validatorsState;
			}
			return null;
		}

		void RestoreValidatorsState (bool [] validatorsState) {
			if (validatorsState == null)
				return;

			is_validated = true;
			for (int i = 0; i < Math.Min (validatorsState.Length, Validators.Count); i++) {
				IValidator val = Validators [i];
				val.IsValid = validatorsState [i];
			}
		}

		ResponseWriter SetupResponseWriter (TextWriter httpWriter) { //TODO
			FacesContext facesContext = getFacesContext ();

			ResponseWriter oldWriter = facesContext.getResponseWriter ();
			if (oldWriter == null)
				throw new InvalidOperationException ();

			ResponseWriter writer = oldWriter.cloneWithWriter (new AspNetResponseWriter (httpWriter));
			
			facesContext.setResponseWriter (writer);
			return oldWriter;
		}

		string DecodeNamespace (string id) {
			if (Namespace.Length > 0 && id.Length > Namespace.Length && id.StartsWith (Namespace, StringComparison.Ordinal))
				id = id.Substring (Namespace.Length);
			return id;
		}

		protected override FacesContext getFacesContext () {
			return _facesContext ?? (_facesContext = FacesContext.getCurrentInstance ());
		}

		internal FacesContext FacesContext {
			get { return getFacesContext (); }
		}

		#region EventRaiserFacesEvent
		sealed class EventRaiserFacesEvent : FacesEvent
		{
			public EventRaiserFacesEvent (Page page)
				: base (page) {
			}

			public override bool isAppropriateListener (FacesListener __p1) {
				throw new NotSupportedException ();
			}

			public override void processListener (FacesListener __p1) {
				throw new NotSupportedException ();
			}
		}
		#endregion

		#region AspNetResponseWriter
		sealed class AspNetResponseWriter : java.io.Writer
		{
			readonly TextWriter _writer;
			public AspNetResponseWriter (TextWriter writer) {
				_writer = writer;
			}
			public override void close () {
				_writer.Close ();
			}

			public override void flush () {
				_writer.Flush ();
			}

			public override void write (char [] __p1, int __p2, int __p3) {
				_writer.Write (__p1, __p2, __p3);
			}

			public override void write (int __p1) {
				_writer.Write ((char) __p1);
			}

			public override void write (char [] __p1) {
				_writer.Write (__p1);
			}

			public override void write (string __p1) {
				_writer.Write (__p1);
			}

			public override void write (string __p1, int __p2, int __p3) {
				_writer.Write (__p1, __p2, __p3);
			}
		}
		#endregion

		#region StateSerializer
		public sealed class StateSerializer : java.io.Externalizable
		{
			object _state;

			public StateSerializer ()
			{
			}

			public StateSerializer (object state)
			{
				_state = state;
			}

			public object State
			{
				get { return _state; }
			}

			public void readExternal (java.io.ObjectInput __p1)
			{
				Page page = CurrentPage;
				ObjectStateFormatter osf = new ObjectStateFormatter (page);
				ObjectInputStream inputStream = new ObjectInputStream (__p1);

				if (page.NeedViewStateEncryption || page.EnableViewStateMac)
					_state = osf.Deserialize ((string) inputStream.readObject ());
				else
					_state = osf.Deserialize (inputStream);
			}

			public void writeExternal (java.io.ObjectOutput __p1)
			{
				Page page = CurrentPage;
				ObjectStateFormatter osf = new ObjectStateFormatter (page);
				ObjectOutputStream outputStream = new ObjectOutputStream (__p1);

				if (page.NeedViewStateEncryption || page.EnableViewStateMac)
					outputStream.writeObject (osf.Serialize (_state));
				else
					osf.Serialize (outputStream, _state);
			}

			Page CurrentPage
			{
				get
				{
					HttpContext context = HttpContext.Current;
					if (context.CurrentHandler is Page)
						return (Page) context.CurrentHandler;
					
					return context.CurrentHandlerInternal;
				}
			}
		} 
		#endregion
	}
}
