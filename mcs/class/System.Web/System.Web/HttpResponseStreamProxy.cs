// 
// System.Web.HttpResponseStreamProxy
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.IO;

namespace System.Web {
   /// <summary>
   /// Used to detect if there is a valid filter proxy.
   /// </summary>
   class HttpResponseStreamProxy : HttpResponseStream {
      bool _FilteringActive;
	   
      internal HttpResponseStreamProxy(HttpWriter Writer) : base(Writer) {
         _FilteringActive = false;
      }
		
      internal void CheckFilteringState() {
         if (_FilteringActive) {
            throw new HttpException("Invalid response filter state");
         }
      }
		
      internal bool Active {
         get {
            return _FilteringActive;
         }
         set {
            _FilteringActive = value;
         }
      }
		
      public override void Flush() {
      }

      public override void Close() {
      }
		
      public override void Write(byte [] buffer, int offset, int length) {
         CheckFilteringState();
         
         Write(buffer, offset, length);
      }
   }
}
