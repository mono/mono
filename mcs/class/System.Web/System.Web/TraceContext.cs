// 
// System.Web.TraceContext
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Jackson Harper (jackson@ximian.com)
//
// (C) 2002 2003, Patrik Torstensson
// Copyright (C) 2003-2009 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Collections;
using System.Security.Permissions;
using System.Web.UI;

namespace System.Web
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class TraceContext
	{
		static readonly object traceFinishedEvent = new object ();
		
		HttpContext _Context;
		TraceManager _traceManager;
		bool _Enabled;
		TraceMode _Mode = TraceMode.Default;
		TraceData data;
		bool data_saved;
		bool _haveTrace;
		Hashtable view_states;
		Hashtable control_states;
		Hashtable sizes;		
		EventHandlerList events = new EventHandlerList ();
		
		public event TraceContextEventHandler TraceFinished {
			add { events.AddHandler (traceFinishedEvent, value); }
			remove { events.AddHandler (traceFinishedEvent, value); }
		}

		public TraceContext (HttpContext context)
		{
			_Context = context;
		}

		internal bool HaveTrace {
			get {
				return _haveTrace;
			}
		}

		public bool IsEnabled {
			get {
				if (!_haveTrace)
					return TraceManager.Enabled;
				return _Enabled;
			}

			set {
				if (value && data == null)
					data = new TraceData ();
				_haveTrace = true;
				_Enabled = value;
			}
		}

		TraceManager TraceManager
		{
			get
			{
				if (_traceManager == null)
					_traceManager = HttpRuntime.TraceManager;

				return _traceManager;
			}
		}

		public TraceMode TraceMode {
			get {
				return (_Mode == TraceMode.Default) ? TraceManager.TraceMode : _Mode;
			}
			set {
				_Mode = value;
			}
		}

		public void Warn(string message)
		{
			Write (String.Empty, message, null, true);
		}

		public void Warn(string category, string message)
		{
			Write (category, message, null, true);
		}

		public void Warn (string category, string message, Exception errorInfo)
		{
			Write (category, message, errorInfo, true);
		}

		public void Write (string message)
		{
			Write (String.Empty, message, null, false);
		}

		public void Write (string category, string message)
		{
			Write (category, message, null, false);
		}

		public void Write (string category, string message, Exception errorInfo)
		{
			Write (category, message, errorInfo, false);
		}

		void Write (string category, string msg, Exception error, bool Warning)
		{
			if (!IsEnabled)
				return;
			if (data == null)
				data = new TraceData ();
			data.Write (category, msg, error, Warning);
		}

		internal void SaveData ()
		{
			if (data == null)
				data = new TraceData ();

			data.TraceMode = _Context.Trace.TraceMode;

			SetRequestDetails ();
			if (_Context.Handler is Page)
				data.AddControlTree ((Page) _Context.Handler, view_states, control_states, sizes);

			AddCookies ();
			AddHeaders ();
			AddServerVars ();
			TraceManager.AddTraceData (data);
			data_saved = true;
		}

		internal void SaveViewState (Control ctrl, object vs)
		{
			if (view_states == null)
				view_states = new Hashtable ();

			view_states [ctrl] = vs;
		}

		internal void SaveControlState (Control ctrl, object vs) {
			if (control_states == null)
				control_states = new Hashtable ();

			control_states [ctrl] = vs;
		}

		internal void SaveSize (Control ctrl, int size)
		{
			if (sizes == null)
				sizes = new Hashtable ();

			sizes [ctrl] = size;
		}

		internal void Render (HtmlTextWriter output)
		{
			if (!data_saved)
				SaveData ();
			data.Render (output);
		}

		void SetRequestDetails ()
		{
			data.RequestPath = _Context.Request.FilePath;
			data.SessionID = (_Context.Session != null ? _Context.Session.SessionID : String.Empty);
			data.RequestType = _Context.Request.RequestType;
			data.RequestTime = _Context.Timestamp;
			data.StatusCode = _Context.Response.StatusCode;
			data.RequestEncoding = _Context.Request.ContentEncoding;
			data.ResponseEncoding = _Context.Response.ContentEncoding;
		}

		void AddCookies ()
		{
			foreach (string key in _Context.Request.Cookies.Keys)
				data.AddCookie (key, _Context.Request.Cookies [key].Value);
		}

		void AddHeaders ()
		{
			foreach (string key in _Context.Request.Headers.Keys)
				data.AddHeader (key, _Context.Request.Headers [key]);
		}

		void AddServerVars ()
		{
			foreach (string key in _Context.Request.ServerVariables)
				data.AddServerVar (key, _Context.Request.ServerVariables [key]);
		}
	}
}

