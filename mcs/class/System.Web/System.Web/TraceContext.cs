// 
// System.Web.TraceContext
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Jackson Harper (jackson@ximian.com)
//
// (C) 2002 2003, Patrik Torstensson
// (C) 2003 Novell, Inc (http://www.novell.com) 
//

using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web {
   public sealed class TraceContext {
      private HttpContext _Context;
      private bool _Enabled;
      private TraceMode _Mode;
      private TraceData data;
      private bool data_saved;
           
      public TraceContext(HttpContext Context) {
	 _Context = Context;
	 _Enabled = false;
      }

      public bool IsEnabled {
	 get {
	    return _Enabled;
	 }

	 set {
		 if (value && data == null)
			 data = new TraceData ();
	    _Enabled = value;
	 }
      }

      public TraceMode TraceMode {
	 get {
	    return _Mode;
	 }

	 set {
	    _Mode = value;
	 }
      }

      public void Warn(string msg) {
	 Write(String.Empty, msg, null, true);
      }

      public void Warn(string category, string msg) {
	 Write(category, msg, null, true);
      }

      public void Warn(string category, string msg, Exception error) {
	 Write(category, msg, error, true);
      }

      public void Write(string msg) {
	 Write(String.Empty, msg, null, true);
      }

      public void Write(string category, string msg) {
	 Write(category, msg, null, false);
      }

      public void Write(string category, string msg, Exception error) {
	 Write(category, msg, error, false);
      }

      [MonoTODO("Save the data into a web dataset directly...")]
      private void Write(string category, string msg, Exception error, bool Warning) {
	      if (!_Enabled)
		      return;
	      data.Write (category, msg, error, Warning);
      }

           internal void SaveData ()
           {
 		   SetRequestDetails ();
		   data.AddControlTree ((Page) _Context.Handler);
		   AddCookies ();
		   AddHeaders ();
		   AddServerVars ();
		   HttpRuntime.TraceManager.AddTraceData (data);
		   data_saved = true;
	   }
           
	   internal void Render (HtmlTextWriter output)
	   {
                   if (!data_saved)
                           SaveData ();
		   data.Render (output);
	   }

	   private void SetRequestDetails ()
	   {
		   data.SessionID = _Context.Session.SessionID;
		   data.RequestType = _Context.Request.RequestType;
		   data.RequestTime = _Context.Timestamp;
		   data.StatusCode = _Context.Response.StatusCode;
		   data.RequestEncoding = _Context.Request.ContentEncoding;
		   data.ResponseEncoding = _Context.Response.ContentEncoding;
	   }

	   private void AddCookies ()
	   {
		   foreach (string key in _Context.Request.Cookies.Keys)
			   data.AddCookie (key, _Context.Request.Cookies [key].Value);
	   }
	   
	   private void AddHeaders ()
	   {
		   foreach (string key in _Context.Request.Headers.Keys)
			   data.AddHeader (key, _Context.Request.Headers [key]);
	   }

	   private void AddServerVars ()
	   {
		   foreach (string key in _Context.Request.ServerVariables)
			   data.AddServerVar (key, _Context.Request.ServerVariables [key]);
	   }
   }
}
