//------------------------------------------------------------------------------
// <copyright file="LosWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

#if !OBJECTSTATEFORMATTER

namespace System.Web.UI {
    using System.Text;
    using System.IO;
    using System.Collections;
    using System.Globalization;
    using System.Web.Configuration;

    internal class LosWriter : TextWriter{

        private const int BUFFER_SIZE       = 4096;
        private const int MAX_FREE_BUFFERS  = 16;
        
        private static CharBufferAllocator  _charBufferAllocator;
        private static CharBufferAllocator  _charBufferAllocatorBase64;
        private static UbyteBufferAllocator _byteBufferAllocator;


        private char[]  _charBuffer;
        private byte[]  _byteBuffer;
        private int     _size;
        private int     _freePos;
        private bool    _recyclable;

        static LosWriter() {
            _charBufferAllocator = new CharBufferAllocator(BUFFER_SIZE, MAX_FREE_BUFFERS);

            int byteBufferSize = Encoding.UTF8.GetMaxByteCount(BUFFER_SIZE);
            _byteBufferAllocator = new UbyteBufferAllocator(byteBufferSize, MAX_FREE_BUFFERS);

            // base64 increases data by up to 33%, so we err on the side of caution here
            _charBufferAllocatorBase64 = new CharBufferAllocator((int) (byteBufferSize * 1.35), MAX_FREE_BUFFERS);
        }
        
        internal LosWriter() {
            _charBuffer = (char[])_charBufferAllocator.GetBuffer();
            _size = _charBuffer.Length;
            _freePos = 0;
            _recyclable = true;
        }

        internal void Dispose() {
            if (_recyclable) {
                if (_charBuffer != null)
                _charBufferAllocator.ReuseBuffer(_charBuffer);

                if (_byteBuffer != null)
                _byteBufferAllocator.ReuseBuffer(_byteBuffer);
            }

            _byteBuffer = null;
            _charBuffer = null;
        }

        public override Encoding Encoding {
            get { return null; }
        }
        

        private void Grow(int newSize) {
            if (newSize <= _size)
                return;

            if (newSize < _size*2)
                newSize = _size*2;

            char[] newBuffer = new char[newSize];

            if (_freePos > 0)
                Array.Copy(_charBuffer, newBuffer, _freePos);

            _charBuffer = newBuffer;
            _size = newSize;
            _recyclable = false;
        }

        public override void Write(char ch) {
            if (_freePos >= _size)
                Grow(_freePos+1);

            _charBuffer[_freePos++] = ch;
        }

        public override void Write(String s) {
            int len = s.Length;
            int newFreePos = _freePos + len;

            if (newFreePos > _size)
                Grow(newFreePos);

            s.CopyTo(0, _charBuffer, _freePos, len);
            _freePos = newFreePos;
        }

        
        internal /*public*/ void CompleteTransforms(TextWriter output, bool enableMac, byte[] macKey) {
            int len = 0;

            // convert to bytes
            if (_recyclable) {
                // still using the original recyclable char buffer 
                // -- can use recyclable byte buffer
                _byteBuffer = (byte[])_byteBufferAllocator.GetBuffer();

                if (_freePos > 0)
                    len = Encoding.UTF8.GetBytes(_charBuffer, 0, _freePos, _byteBuffer, 0);

                // do the mac encoding if requested
                if (enableMac) { 
                    // the size of the output array depends on the key length and encryption type
                    // so we can't use the recyclable buffers after this
                    byte[] data = MachineKeySection.GetEncodedData(_byteBuffer, macKey, 0, ref len);
                    string serialized = Convert.ToBase64String(data, 0, len);
                    output.Write(serialized);
                }
                else {
                    char[] base64chars = (char[]) _charBufferAllocatorBase64.GetBuffer();
                    len = Convert.ToBase64CharArray(_byteBuffer, 0, len, base64chars, 0);
                    output.Write(base64chars, 0, len);
                    _charBufferAllocatorBase64.ReuseBuffer(base64chars);
                }
            }
            else {
                _byteBuffer = Encoding.UTF8.GetBytes(_charBuffer, 0, _freePos);


                len = _byteBuffer.Length;
                if (enableMac) 
                    _byteBuffer = MachineKeySection.GetEncodedData(_byteBuffer, macKey, 0, ref len);
                
                string serialized = Convert.ToBase64String(_byteBuffer);
                output.Write(serialized);
            }
        }
    }
}

#endif // !OBJECTSTATEFORMATTER

