// 
// System.Web.HttpPostedFile
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.IO;

namespace System.Web {
   public sealed class HttpPostedFile {
      private HttpRequestStream _Stream;
      private string _ContentType;
      private string _FileName;

      internal HttpPostedFile(string file, string type, HttpRequestStream data) {
         _Stream = data;
         _FileName = file;
         _ContentType = type;
      }

      public void SaveAs(string filename) {
         FileStream File = new FileStream(filename, FileMode.Create);
         if (_Stream.DataLength > 0) {
            File.Write(_Stream.Data, _Stream.DataOffset, _Stream.DataLength);
         }

         File.Flush();
         File.Close();
      }

      public int ContentLength {
         get {
            return _Stream.DataLength;
         }
      }

      public string ContentType {
         get {
            return _ContentType;
         }
      }

      public string FileName {
         get {
            return _FileName;
         }
      }

      public Stream InputStream {
         get {
            return _Stream;
         }
      }
   }
}
