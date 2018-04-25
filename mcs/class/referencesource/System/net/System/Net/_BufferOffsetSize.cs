//------------------------------------------------------------------------------
// <copyright file="_BufferOffsetSize.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    //
    // this class is used by the BeginMultipleSend() API
    // to allow a user to send multiple buffers on a socket
    //
    internal class BufferOffsetSize {
        //
        // internal members
        //
        internal byte[] Buffer;
        internal int Offset;
        internal int Size;

        internal BufferOffsetSize(byte[] buffer, int offset, int size, bool copyBuffer) {
            GlobalLog.Assert(buffer != null && buffer.Length >= size + offset, "BufferOffsetSize::.ctor|Illegal parameters.");
            if (copyBuffer) {
                byte[] newBuffer = new byte[size];

                System.Buffer.BlockCopy(
                    buffer,     // src
                    offset,     // src index
                    newBuffer,  // dest
                    0,          // dest index
                    size );     // total size to copy

                offset = 0;
                buffer = newBuffer;
            }
            Buffer = buffer;
            Offset = offset;
            Size = size;
            GlobalLog.Print("BufferOffsetSize#" + ValidationHelper.HashString(this) + "::.ctor() copyBuffer:" + copyBuffer.ToString() + " this:[" + ToString() + "]");
        }

        /*
        // Consider removing.
        internal BufferOffsetSize(byte[] buffer, int offset, bool copyBuffer)
            : this(buffer, offset, buffer.Length - offset, copyBuffer) {
        }
        */

        /*
        // Consider removing.
        internal BufferOffsetSize(int size, byte[] buffer, bool copyBuffer)
            : this(buffer, 0, size, copyBuffer) {
        }
        */

        internal BufferOffsetSize(byte[] buffer, bool copyBuffer)
            : this(buffer, 0, buffer.Length, copyBuffer) {
        }

#if TRAVE
        public override string ToString() {
            return "BufferOffsetSize#" + ValidationHelper.HashString(this) + " Buffer#" + ValidationHelper.HashString(Buffer) + " Offset:" + Offset.ToString() + " Size:" + Size.ToString();
        }
#endif

    } // class BufferOffsetSize



} // namespace System.Net
