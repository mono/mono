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

namespace System.Web {
   public sealed class TraceContext {
      private HttpContext _Context;
      private bool _Enabled;
      private TraceMode _Mode;
      private DataTable info;

      public TraceContext(HttpContext Context) {
         _Context = Context;
         _Enabled = false;
      }

      public bool IsEnabled {
         get {
            return _Enabled;
         }

         set {
                 if (value && info == null)
                         InitInfoTable ();
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
         Write(category, msg, null, true);
      }

      public void Write(string category, string msg, Exception error) {
         Write(category, msg, error, true);
      }

      [MonoTODO("Save the data into a web dataset directly...")]
      private void Write(string category, string msg, Exception error, bool Warning) {
              if (!_Enabled)
                      return;
              DataRow r = info.NewRow ();
              r ["Category"] = category;
              r ["Message"] = msg;
              r ["Exception"] = (error != null ? error.ToString () : null);
              r ["IsWarning"] = Warning;

              info.Rows.Add (r);
      }

	   private void InitInfoTable ()
	   {
		   info = new DataTable ();
		   info.Columns.Add (new DataColumn ("Category", typeof (string)));
		   info.Columns.Add (new DataColumn ("Message", typeof (string)));
		   info.Columns.Add (new DataColumn ("Exception", typeof (string)));
		   info.Columns.Add (new DataColumn ("IsWarning", typeof (bool)));
	   }
   }
}
