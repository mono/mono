// 
// System.Web.HttpResponseHeader
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;

namespace System.Web {
   internal class HttpResponseHeader {
      private string _sHeader;
      private string _sValue;
      private int _iKnowHeaderId;

      internal HttpResponseHeader(int KnowHeaderId, string value) {
         _iKnowHeaderId = KnowHeaderId;
         _sValue = value;
      }

      internal HttpResponseHeader(string header, string value) {
         _sHeader = header;
         _sValue = value;
      }

      internal string Name {
         get {
            if (null == _sHeader) {
               return HttpWorkerRequest.GetKnownResponseHeaderName(_iKnowHeaderId);
            }

            return _sHeader;
         }
      }

      internal string Value {
         get {
            return _sValue;
         }
      }

      internal void SendContent(HttpWorkerRequest WorkerRequest) {
         if (null != _sHeader) {
            WorkerRequest.SendUnknownResponseHeader(_sHeader, _sValue);
         } else {
            WorkerRequest.SendKnownResponseHeader(_iKnowHeaderId, _sValue);
         }
      }
   }
}
