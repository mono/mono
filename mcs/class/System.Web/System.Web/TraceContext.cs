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
      private bool _haveTrace;
           
      public TraceContext(HttpContext Context) {
	 _Context = Context;
	 _Enabled = false;
      }


	internal bool HaveTrace {
		get {
			return _haveTrace;
		}
	}

      public bool IsEnabled {
	 get {
	    if (!_haveTrace)
	        return HttpRuntime.TraceManager.Enabled;
	    return _Enabled;
	 }

	 set {
		 if (value && data == null)
			 data = new TraceData ();
	     _haveTrace = true;
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

      private void Write(string category, string msg, Exception error, bool Warning) {
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
		   data.RequestPath = _Context.Request.FilePath;
		   data.SessionID = (_Context.Session != null ? _Context.Session.SessionID : String.Empty);
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
