// 
// System.Web.HttpClientCertificate
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Web;
using System.Collections.Specialized;

namespace System.Web {
   [MonoTODO()]
   public class HttpClientCertificate : NameValueCollection {
      private HttpContext _Context;
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
         _Context = Context;
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
