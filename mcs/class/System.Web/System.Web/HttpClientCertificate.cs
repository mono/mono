// 
// System.Web.HttpClientCertificate
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
using System.Web;
using System.Collections.Specialized;

namespace System.Web {
   [MonoTODO()]
   public class HttpClientCertificate : NameValueCollection {
      //private HttpContext _Context;
      private byte [] _BinaryIssuer;
      private int _CertEncoding;
      private byte [] _Certificate;
      private string _Cookie;
      private int _Flags;
      private string _Issuer;      
      private int _KeySize;
      private byte [] _PublicKey;
      private int _SecretKeySize;
      private string _SerialNumber;
      private string _ServerIssuer;
      private string _ServerSubject;
      private string _Subject;

      private DateTime _ValidFrom;
      private DateTime _ValidTo;

      [MonoTODO("Decode ceritificate from Server variables.. CGI standard?")]
      internal HttpClientCertificate(HttpContext Context) {
         //_Context = Context; not used
         _Flags = 0;
      }

      public byte [] BinaryIssuer {
         get {
            return _BinaryIssuer;
         }
      }

      public int CertEncoding {
         get {
            return _CertEncoding;
         }
      }

      public byte [] Certificate {
         get {
            return _Certificate;
         }
      }

      public string Cookie {
         get {
            return _Cookie;
         }
      }

      public int Flags {
         get {
            return _Flags;
         }
      }

      [MonoTODO()]
      public bool IsPresent {
         get {
            return false;
         }
      }

      public string Issuer {
         get {
            return _Issuer;
         }
      }

      [MonoTODO()]
      public bool IsValid {
         get {
            return false;
         }
      }

      public int KeySize {
         get {
            return _KeySize;
         }
      }

      public byte [] PublicKey {
         get {
            return _PublicKey;
         }
      }

      public int SecretKeySize {
         get {
            return _SecretKeySize;
         }
      }

      public string SerialNumber {
         get {
            return _SerialNumber;
         }
      }

      public string ServerIssuer {
         get {
            return _ServerIssuer;
         }
      }

      public string ServerSubject { 
         get {
            return _ServerSubject;
         }
      }

      public string Subject { 
         get {
            return _Subject;
         }
      }

      public DateTime ValidFrom {
         get {
            return _ValidFrom;
         }
      }

      public DateTime ValidUntil {
         get {
            return _ValidTo;
         }
      }

      [MonoTODO()]
      public override string Get(string s) {
         throw new NotImplementedException();
      }
   }
}
