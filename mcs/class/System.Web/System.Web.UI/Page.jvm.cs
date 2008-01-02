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

namespace System.Web.UI
{
	public partial class Page
	{
		string _namespace = null;
		StateManager.SerializedView _facesSerializedView;
		MethodBinding _action;
		MethodBinding _actionListener;
		bool _immediate;
		Pair _state;
		bool [] _validatorsState;
		ICallbackEventHandler _callbackTarget;
		string _callbackEventError = String.Empty;
		IHttpHandler _jsfHandler;
		static readonly object CrossPagePostBack = new object ();

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

		void EnterThread (HttpContext context) {

			_jsfHandler = context.CurrentHandler;
			context.PopHandler ();
			context.PushHandler (this);
			if (_jsfHandler == context.Handler)
				context.Handler = this;

			SetContext (context);
		}

		void ExitThread () {
			// TODO
			//if (context.getResponseComplete ())
			//    Response.End ();

			_context.PopHandler ();
			_context.PushHandler (_jsfHandler);
			if (this == _context.Handler)
				_context.Handler = _jsfHandler;

		}

		public override void encodeBegin (FacesContext context) {
			// do nothing
		}

		public override void encodeChildren (FacesContext context) {
			System.Diagnostics.Trace.WriteLine ("encodeChildren");

			EnterThread (HttpContext.Current);
			bool wasException = false;
			try {
				if (!context.getResponseComplete ()) {

					if (IsCrossPagePostBack)
						return;

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
				if (!wasException)
					ProcessUnload ();
				ExitThread ();
			}
		}

		public override void encodeEnd (FacesContext context) {
			// do nothing
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

			object state = new Pair (_state, GetValidatorsState ());
			if (getFacesContext ().getApplication ().getStateManager ().isSavingStateInClient (getFacesContext ())) {
				int length;
				byte [] buffer = new ObjectStateFormatter (this).SerializeInternal (state, out length);
				if (buffer.Length != length) {
					byte [] trimmedBuffer = new byte [length];
					Array.Copy (buffer, trimmedBuffer, length);
					buffer = trimmedBuffer;
				}
				state = vmw.common.TypeUtils.ToSByteArray (buffer);
			}
			return state;
		}

		public override void processRestoreState (FacesContext context, object state) {
			System.Diagnostics.Trace.WriteLine ("processRestoreState");

			if (state == null)
				throw new ArgumentNullException ("state");
			EnterThread (HttpContext.Current);
			try {
				if (getFacesContext ().getApplication ().getStateManager ().isSavingStateInClient (getFacesContext ())) {
					byte [] buffer = (byte []) vmw.common.TypeUtils.ToByteArray ((sbyte []) state);
					state = new ObjectStateFormatter (this).DeserializeInternal (buffer);
				}
				_state = (Pair) ((Pair) state).First;
				_validatorsState = (bool []) ((Pair) state).Second;
				RestorePageState ();
			}
			catch (Exception ex) {
				HandleException (ex);
			}
			finally {
				ExitThread ();
			}
		}

		public override void processDecodes (FacesContext context) {
			System.Diagnostics.Trace.WriteLine ("processDecodes");

			EnterThread (HttpContext.Current);
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
				ExitThread ();
			}
		}

		public override void processValidators (FacesContext context) {
			System.Diagnostics.Trace.WriteLine ("processValidators");

			EnterThread (HttpContext.Current);
			try {
				base.processValidators (context);
			}
			catch (Exception ex) {
				HandleException (ex);
			}
			finally {
				ExitThread ();
			}
		}

		public override void processUpdates (FacesContext context) {
			System.Diagnostics.Trace.WriteLine ("processUpdates");

			EnterThread (HttpContext.Current);
			try {
				base.processUpdates (context);
			}
			catch (Exception ex) {
				HandleException (ex);
			}
			finally {
				ExitThread ();
			}
		}

		public override void broadcast (FacesEvent e) {
			System.Diagnostics.Trace.WriteLine ("broadcast");

			if (!(e is EventRaiserFacesEvent))
				throw new NotSupportedException ("FacesEvent of class " + e.GetType ().Name + " not supported by Page");

			EnterThread (HttpContext.Current);
			try {
				ProcessRaiseEvents ();
				ProcessLoadComplete ();
			}
			catch (Exception ex) {
				HandleException (ex);
			}
			finally {
				ExitThread ();
			}
		}

		void HandleException (Exception ex) {
			try {
				if (ex is ThreadAbortException) {
					if (_context.Response.FlagEnd == ((ThreadAbortException) ex).ExceptionState) {
						Thread.ResetAbort ();
						if (getFacesContext () != null)
							getFacesContext ().responseComplete ();
						return;
					}
				}
				else
					ProcessException (ex);

				vmw.common.TypeUtils.Throw (ex);
			}
			finally {
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
			RenderKitFactory renderFactory = (RenderKitFactory) FactoryFinder.getFactory (FactoryFinder.RENDER_KIT_FACTORY);
			RenderKit renderKit = renderFactory.getRenderKit (facesContext,
															 facesContext.getViewRoot ().getRenderKitId ());

			ServletResponse response = (ServletResponse) facesContext.getExternalContext ().getResponse ();

			ResponseWriter writer = renderKit.createResponseWriter (new AspNetResponseWriter (httpWriter),
													 response.getContentType (), //TODO: is this the correct content type?
													 response.getCharacterEncoding ());
			facesContext.setResponseWriter (writer);

			return oldWriter;
		}

		string DecodeNamespace (string id) {
			if (Namespace.Length > 0 && id.Length > Namespace.Length && id.StartsWith (Namespace, StringComparison.Ordinal))
				id = id.Substring (Namespace.Length);
			return id;
		}

		#region FacesPageStatePersister
		sealed class FacesPageStatePersister : PageStatePersister
		{
			public FacesPageStatePersister (Page page)
				: base (page) {
			}

			public override void Load () {
				if (Page._state != null) {
					ViewState = Page._state.First;
					ControlState = Page._state.Second;
				}
			}

			public override void Save () {
				if (ViewState != null || ControlState != null)
					Page._state = new Pair (ViewState, ControlState);
			}
		}
		#endregion

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
		private sealed class AspNetResponseWriter : java.io.Writer
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
	}
}
