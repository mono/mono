// 
// System.Web.HttpRequestStream
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.IO;

namespace System.Web {
   class HttpRequestStream : Stream {
      private byte []	_arrData;
      private int			_iLength;
      private int			_iOffset;
      private int			_iPos;

      internal HttpRequestStream(byte [] buffer, int offset, int length) {
         _iPos = 0;
         _iOffset = offset;
         _iLength = length;

         _arrData = buffer;
      }
		
      private void Reset() {
         _iPos = 0;
         _iOffset = 0;
         _iLength = 0;

         _arrData = null;
      }

      public override void Flush() {
      }

      public override void Close() {
         Reset();
      }

      public override int Read(byte [] buffer, int offset, int length) {
         int iBytes = length;

         if (_iPos + length > _arrData.Length) {
            iBytes = (int) _arrData.Length - _iPos;
         }

         if (iBytes <= 0) {
            return 0;
         }

         Buffer.BlockCopy(_arrData, _iPos, buffer, offset, iBytes);
         _iPos += iBytes;

         return iBytes;
      }

      public override long Seek(long offset, SeekOrigin origin) {
         switch (origin) {
            case SeekOrigin.Begin	:	if (offset > _arrData.Length) {
                                          throw new ArgumentException();
                                       }
               _iPos = (int) offset;
               break;
											
            case SeekOrigin.Current	:	if (((long) _iPos + offset > _arrData.Length) || (_iPos + (int) offset < 0)) {
                                          throw new ArgumentException();
                                       }
               _iPos += Convert.ToInt32(offset);
               break;

            case SeekOrigin.End:		if (_arrData.Length - offset < 0) {
                                       throw new ArgumentException();
                                    }
											
               _iPos = Convert.ToInt32( _arrData.Length - offset);
               break;
         }

         return (long) _iPos;										
      }

      public override void SetLength(long length) {
         throw new NotSupportedException();
      }

      public override void Write(byte [] buffer, int offset, int length) {
         throw new NotSupportedException();
      }

      public override bool CanRead {
         get {
            return true;
         }
      }

      public override bool CanSeek {
         get {
            return true;
         }
      }

      public override bool CanWrite {
         get {
            return false;
         }
      }

      public byte [] Data {
         get {
            return _arrData;
         }
      }

      public int DataLength {
         get {
            return _iLength;
         }
      }

      public int DataOffset {
         get {
            return _iOffset;
         }
      }

      public override long Length {
         get {
            return (long) _arrData.Length;
         }
      }

      public override long Position {
         get {
            return (long) _iPos;
         }

         set {
            Seek(value, SeekOrigin.Begin);
         }
      }
   }
}
