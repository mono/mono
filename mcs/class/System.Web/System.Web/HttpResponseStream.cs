// 
// System.Web.HttpResponseStream
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
   /// <summary>
   /// Simple wrapper around HttpWriter to support the Stream interface
   /// </summary>
   class HttpResponseStream : Stream {
      private HttpWriter _Writer;
	   
      internal HttpResponseStream(HttpWriter Writer) {
         _Writer = Writer;
      }
		
      public override void Flush() {
         _Writer.Flush();
      }

      public override void Close() {
         _Writer.Close();
      }

      public override int Read(byte [] buffer, int offset, int length) {
         throw new NotSupportedException();
      }

      public override long Seek(long offset, SeekOrigin origin) {
         throw new NotSupportedException();
      }

      public override void SetLength(long length) {
         throw new NotSupportedException();
      }

      public override void Write(byte [] buffer, int offset, int length) {
         if (offset < 0) {
            throw new ArgumentOutOfRangeException("offset");
         }

         if (length < 0) {
            throw new ArgumentOutOfRangeException("length");
         }
		   
         _Writer.WriteBytes(buffer, offset, length);
      }

      public override bool CanRead {
         get {
            return false;
         }
      }

      public override bool CanSeek {
         get {
            return false;
         }
      }

      public override bool CanWrite {
         get {
            return true;
         }
      }

      public override long Length {
         get {
            throw new NotSupportedException();
         }
      }

      public override long Position {
         get {
            throw new NotSupportedException();
         }

         set {
            throw new NotSupportedException();
         }
      }
   }
}
