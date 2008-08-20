// 
// System.Web.HttpResponseHeader
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
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
using System.Text;

namespace System.Web {
   internal class HttpResponseHeader {
      private string _sHeader;
      private string _sValue;
      private int _iKnowHeaderId;

      string EncodeHeader (string value)
		{
#if NET_2_0
			if (String.IsNullOrEmpty (value))
				return value;
			
				StringBuilder ret = new StringBuilder ();
				int len = value.Length;

				for (int i = 0; i < len; i++) {
					switch (value [i]) {
						case '\r':
							ret.Append ("%0d");
							break;

						case '\n':
							ret.Append ("%0a");
							break;

						default:
							ret.Append (value [i]);
							break;
					}
				}

				return ret.ToString ();
#else
				return value;
#endif
		}

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
	 set {
		_sValue = value;
	 }
      }

      internal void SendContent(HttpWorkerRequest WorkerRequest) {
         if (null != _sHeader) {
            WorkerRequest.SendUnknownResponseHeader(_sHeader, EncodeHeader (_sValue));
         } else {
            WorkerRequest.SendKnownResponseHeader(_iKnowHeaderId, EncodeHeader (_sValue));
         }
      }
   }
}
