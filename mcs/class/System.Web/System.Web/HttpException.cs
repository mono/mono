// 
// System.Web.HttpException
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Runtime.InteropServices;

namespace System.Web {
   [MonoTODO("This class contains a lot of windows specific methods, solve this.. :)")]
   public class HttpException : ExternalException {
      private int _HttpCode;

      public HttpException(string sMessage) : base(sMessage) {
      }

      public HttpException(string sMessage, Exception InnerException) : base(sMessage, InnerException) {
      }

      public HttpException(int iHttpCode, string sMessage) : base(sMessage) {
         _HttpCode = iHttpCode;
      }

      public HttpException(int iHttpCode, string sMessage, Exception InnerException) : base(sMessage, InnerException) {
         _HttpCode = iHttpCode;
      }

      public int GetHttpCode() {
         return _HttpCode;
      }
   }
}
