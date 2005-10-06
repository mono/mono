// 
// System.Web.HttpPostedFile
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
