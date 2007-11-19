//
// System.Web.UI.Page.jsf.cs
//
// Authors:
//   Igor Zelmanovich (igorz@mainsoft.com)
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
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

using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.el;
using java.lang;
using javax.faces.context;
using javax.faces.component;
using javax.faces.@event;
using javax.servlet;
using System.Collections;
using javax.faces.application;
using javax.faces.render;
using javax.faces;
using System.IO;
using System.Threading;

namespace System.Web.UI
{
	public partial class Page : ActionSource
	{
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

		static Class StringClass { get { return vmw.common.TypeUtils.ToClass (String.Empty); } }
		//kostat
		// this is what it should be.
		sealed class PostBackEventMethodBinding : MethodBinding
		{
			readonly IPostBackEventHandler _handler;

			public PostBackEventMethodBinding (IPostBackEventHandler handler) {
				_handler = handler;
			}

			public override object invoke (FacesContext context, object [] pars) {
				_handler.RaisePostBackEvent ((string) pars [0]);
				return null;
			}

			public override Class getType (FacesContext context) {
				return Page.StringClass;
			}
		}

		sealed class EventRaiserMethodBinding : MethodBinding
		{
			readonly Page _handler;
			static readonly Class StringClass = Class.forName ("java.lang.String");

			public EventRaiserMethodBinding (Page handler) {
				_handler = handler;
			}

			public override object invoke (FacesContext context, object [] pars) {
				_handler.EnterThread (context);
				try {
					_handler.ProcessRaisePostBackEvents ();
					_handler.ProcessLoadComplete ();
					//#if NET_2_0
					//                    _handler._lifeCycle = PageLifeCycle.LoadComplete;
					//                    _handler.OnLoadComplete (EventArgs.Empty);

					//                    if (_handler.IsCallback) {
					//                        string result = _handler.ProcessCallbackData ();
					//                        HtmlTextWriter callbackOutput = new HtmlTextWriter (_handler._context.Response.Output);
					//                        callbackOutput.Write (result);
					//                        callbackOutput.Flush ();
					//                        return null; // will be redirected!
					//                    }

					//                    _handler._lifeCycle = PageLifeCycle.PreRender;
					//#endif
					//                    _handler.Trace.Write ("aspx.page", "Begin PreRender");
					//                    _handler.PreRenderRecursiveInternal ();
					//                    _handler.Trace.Write ("aspx.page", "End PreRender");

					//#if NET_2_0
					//                    _handler.ExecuteRegisteredAsyncTasks ();

					//                    _handler._lifeCycle = PageLifeCycle.PreRenderComplete;
					//                    _handler.OnPreRenderComplete (EventArgs.Empty);
					//#endif

					//                    _handler.Trace.Write ("aspx.page", "Begin SaveViewState");
					//                    _handler.SavePageViewState ();
					//                    _handler.Trace.Write ("aspx.page", "End SaveViewState");

					//#if NET_2_0
					//                    _handler._lifeCycle = PageLifeCycle.SaveStateComplete;
					//                    _handler.OnSaveStateComplete (EventArgs.Empty);
					//#endif // NET_2_0

					//                    javax.faces.application.StateManager manager = _handler.getFacesContext ().getApplication ().getStateManager ();
					//                    _handler._facesSerializedView = manager.saveSerializedView (_handler.getFacesContext ());

					//                    if (_handler.Context.IsActionRequest)
					//                        manager.writeState (_handler.getFacesContext (), _handler._facesSerializedView);

					//try {
					//    _handler.RenderTrace ();
					//    _handler.UnloadRecursive (true);
					//}
					//catch { }

					//return null;
				}
				catch (ThreadAbortException) {
					context.responseComplete ();
				}
				catch (Exception ex) {
					_handler.ProcessException (ex);
					context.renderResponse ();
				}
				finally {
					//_handler.Response.Flush ();
					_handler.ExitThread (context);
				}
				return null;
			}

			public override Class getType (FacesContext context) {
				return StringClass;
			}
		}

		MethodBinding _action;
		MethodBinding _actionListener;
		bool _immediate;

		public void EnterThread (FacesContext context) {
			if (_lifeCycle == PageLifeCycle.Unknown)
				ProcessRequest (HttpContext.Current);
			else
				SetContext (HttpContext.Current);
		}

		public void ExitThread (FacesContext context) {
		}

		public override bool isTransient () {
			//kostat
			return false;
		}

		public override void encodeBegin (FacesContext context) {
		}

		public override void encodeChildren (FacesContext context) {
			EnterThread (context);
			try {
				Trace.Write ("aspx.page", "Begin Render");
				RenderPage ();
				//base.encodeChildren (context);
				Trace.Write ("aspx.page", "End Render");
			}
			finally {
				ExitThread (context);
			}
		}

		public override void encodeEnd (FacesContext context) {
		}

		public override void setParent (UIComponent parent) {
			//base.setParent (parent);
			//ignore: parent is root
		}

		public override object saveState (FacesContext context) {
			//return base.saveState (context);
			return _savedViewState;
		}

		public override void restoreState (FacesContext context, object state) {
			EnterThread (context);
			try {
				_savedViewState = state as string;
				RestorePageState ();
			}
			finally {
				ExitThread (context);
			}
		}

		public override void decode (FacesContext context) {
			EnterThread (context);
			try {
				//base.decode (context);
				//Map requestParameterMap = context.getExternalContext ().getRequestParameterMap ();
				ProcessLoadPage ();

				((ActionSource) this).setAction (new EventRaiserMethodBinding (this));

				ActionEvent action = new ActionEvent (this);
				action.setPhaseId (PhaseId.INVOKE_APPLICATION);
				context.getViewRoot ().queueEvent (action);
				//queueEvent (new ActionEvent (this));
			}
			finally {
				ExitThread (context);
			}
		}

		public override void processValidators (FacesContext context) {
			Console.WriteLine ("processValidators");
			EnterThread (context);
			try {
				base.processValidators (context);
			}
			finally {
				ExitThread (context);
			}
		}

		public override void processUpdates (FacesContext context) {
			Console.WriteLine ("processUpdates");
			base.processUpdates (context);
			EnterThread (context);
			try {
				ProcessRaiseChangedEvents ();
			}
			finally {
				ExitThread (context);
			}
		}

		public override void broadcast (FacesEvent e) {
			Console.WriteLine ("broadcast {0},{1},{2}", e.getSource (), e.getComponent (), e.ToString ());
			//Console.WriteLine (Environment.StackTrace);
			base.broadcast (e);

			if (e is ActionEvent) {
				FacesContext context = getFacesContext ();

				//ActionSource source = (ActionSource) this;

				//MethodBinding actionListenerBinding = source.getActionListener ();
				//if (actionListenerBinding != null) {
				//try
				//{
				//    actionListenerBinding.invoke (context, new Object [] { e });
				//}
				//catch (EvaluationException e)
				//{
				//    Throwable cause = e.getCause();
				//    if (cause != null && cause instanceof AbortProcessingException)
				//    {
				//        throw (AbortProcessingException)cause;
				//    }
				//    else
				//    {
				//        throw e;
				//    }
				//}
				//}

				ActionListener defaultActionListener
					= context.getApplication ().getActionListener ();
				if (defaultActionListener != null) {
					defaultActionListener.processAction ((ActionEvent) e);
				}
			}


		}

		#region ActionSource Members

		void ActionSource.addActionListener (ActionListener listener) {
			Console.WriteLine ("addActionListener");
			addFacesListener (listener);
		}

		MethodBinding ActionSource.getAction () {
			Console.WriteLine ("getAction");
			return _action;
		}

		MethodBinding ActionSource.getActionListener () {
			Console.WriteLine ("getActionListener");
			return _actionListener;
		}

		static readonly Class ActionListenerClass = Class.forName ("javax.faces.event.ActionListener");

		ActionListener [] ActionSource.getActionListeners () {
			Console.WriteLine ("getActionListeners");
			return (ActionListener []) getFacesListeners (ActionListenerClass);
		}

		bool ActionSource.isImmediate () {
			Console.WriteLine ("isImmediate");
			return _immediate;
		}

		void ActionSource.removeActionListener (ActionListener actionListener) {
			Console.WriteLine ("removeActionListener");
			removeFacesListener (actionListener);
		}

		void ActionSource.setAction (MethodBinding action) {
			Console.WriteLine ("setAction");
			_action = action;
		}

		void ActionSource.setActionListener (MethodBinding actionListener) {
			Console.WriteLine ("setActionListener");
			_actionListener = actionListener;
		}

		void ActionSource.setImmediate (bool immediate) {
			Console.WriteLine ("setImmediate");
			_immediate = immediate;
		}

		#endregion

	}
}
