// 
// System.Web.TraceContext
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;

namespace System.Web {
   public sealed class TraceContext {
      private HttpContext _Context;
      private bool _Enabled;
      private TraceMode _Mode;

      public TraceContext(HttpContext Context) {
         _Context = Context;
         _Enabled = true;
      }

      public bool IsEnabled {
         get {
            return _Enabled;
         }

         set {
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
         throw new NotImplementedException();
      }
   }
}
