// 
// System.Web.HttpResponseStream
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
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
