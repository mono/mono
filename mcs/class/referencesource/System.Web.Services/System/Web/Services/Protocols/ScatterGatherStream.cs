#if false //deadcode

//------------------------------------------------------------------------------
// <copyright file="ScatterGatherStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.IO;
    using System.Diagnostics;

    internal class ScatterGatherStream : Stream {
        private const int MemStreamMaxLength = Int32.MaxValue;        

        private MemoryChunk headChunk = null;
        private MemoryChunk currentChunk = null;  

        private long chunkSize = 0;
        private int currentOffset = 0;
        private int endOffset = 0;
        private long currentChunkStartPos = 0;

        internal ScatterGatherStream(int chunkSize) {
            this.chunkSize = chunkSize;
            currentChunk = headChunk = AllocateMemoryChunk(this.chunkSize);
            currentOffset = endOffset = 0;
            currentChunkStartPos = 0;
        }

        internal ScatterGatherStream() : this(1024) { }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return true; } }
        
        public override void Close() {            
            headChunk = null;
            currentChunk = null;
            endOffset = currentOffset = 0;
            currentChunkStartPos = 0;
        }

        public override void Flush() { }

        public override long Length { 
            get {
                MemoryChunk endChunk;
                return GetLengthInternal(out endChunk);
            }
        }

        private long GetLengthInternal(out MemoryChunk endChunk){
            long length = currentChunkStartPos;
            MemoryChunk chunk = currentChunk;
            while (chunk.Next != null) {
                length += chunk.Buffer.Length;
                chunk = chunk.Next;
            }
            length += endOffset;
            endChunk = chunk;
            return length;
        }

        public override long Position {
            get {
                return Seek(0, SeekOrigin.Current);
            }
             
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        
        public override long Seek(long offset, SeekOrigin loc) {
            MemoryChunk chunk  = null;;
            long relativeOffset = 0;
            long absoluteOffset = 0;
            
            if(loc == SeekOrigin.Begin){
                absoluteOffset = offset;
                if(offset >= currentChunkStartPos){
                    chunk = currentChunk;
                    relativeOffset = offset - currentChunkStartPos;
                }
                else{
                    chunk = headChunk;
                    relativeOffset = absoluteOffset;
                }
            }
            else if( loc == SeekOrigin.Current){
                absoluteOffset = offset + currentOffset + currentChunkStartPos;
                if( (offset + currentOffset) > 0){
                    chunk = currentChunk;
                    relativeOffset = offset + currentOffset;
                }
                else {
                    chunk = headChunk;
                    relativeOffset = absoluteOffset;
                }
            }
            else if (loc == SeekOrigin.End){
                MemoryChunk endChunk;
                long length = GetLengthInternal(out endChunk);
                absoluteOffset = offset + length;
                if ( (offset + endOffset) > 0 ) {
                    relativeOffset = offset + endOffset;
                    chunk = endChunk;
                }
                else if(absoluteOffset >= currentChunkStartPos){
                    chunk = currentChunk;
                    relativeOffset = absoluteOffset - currentChunkStartPos;
                }
                else {
                    chunk = headChunk;
                    relativeOffset = absoluteOffset;
                }
            }
            else
                throw new ArgumentOutOfRangeException("loc");

            if (relativeOffset < 0 || relativeOffset > MemStreamMaxLength)
                throw new ArgumentOutOfRangeException("offset");
            long remaining = relativeOffset;
            while (chunk.Next != null) {
                if (remaining < chunk.Buffer.Length){
                    currentChunk = chunk;
                    currentOffset = (int)remaining;
                    currentChunkStartPos = absoluteOffset - currentOffset;
                    remaining = -1;
                    break;
                }
                remaining -= chunk.Buffer.Length;
                chunk = chunk.Next;
            }

            if (remaining >= 0){
                if (remaining <= chunk.Buffer.Length)
                    currentChunk = chunk;
                else {
                    currentChunk = chunk.Next = AllocateMemoryChunk(2*remaining);
                    endOffset = 0;
                }
                currentOffset = (int)remaining;
                currentChunkStartPos = absoluteOffset - currentOffset;
                SyncEndOffset();                
            }

            return absoluteOffset;
        }
        
        public override void SetLength(long absNewLen) {
            if (absNewLen < 0 || absNewLen > MemStreamMaxLength)
                throw new ArgumentOutOfRangeException("offset");

            MemoryChunk chunk;
            bool currentPastEnd;
            long relNewLen;
            if(absNewLen >= currentChunkStartPos){
                currentPastEnd = false;
                chunk = currentChunk;
                relNewLen = absNewLen - currentChunkStartPos;
            }
            else {
                currentPastEnd = true;
                chunk = headChunk;
                relNewLen = absNewLen;
            }
            long startPos = 0;
            MemoryChunk endChunk = null;
            while (chunk != null) {
                long endPos = startPos + chunk.Buffer.Length;
                if(endPos > relNewLen){
                    chunk.Next = null;
                    endOffset = (int)(relNewLen - startPos);
                    if(chunk == currentChunk)
                        currentOffset = min(currentOffset, endOffset);
                    else if(currentPastEnd){
                        currentChunk = chunk;
                        currentOffset = endOffset;
                        currentChunkStartPos = absNewLen - currentOffset;
                    }
                    return;
                }
                startPos = endPos;
                endChunk = chunk;
                chunk = chunk.Next;
            }
            //assert(endChunk != null)
            endChunk.Next = AllocateMemoryChunk((int)(absNewLen - startPos));
            endOffset = (int)(absNewLen - startPos);
        }
        
        
        
        public override int Read(byte[] buffer, int offset, int count) {
            byte[] chunkBuffer = currentChunk.Buffer;
            int chunkSize = chunkBuffer.Length;
            if (currentChunk.Next == null)
                chunkSize = endOffset;

            int bytesRead = 0;
        
            while (count > 0) {
                if (currentOffset == chunkSize) {
                    // exit if no more chunks are currently available
                    if (currentChunk.Next == null)
                        break;

                    currentChunkStartPos += currentChunk.Buffer.Length;
                    currentChunk = currentChunk.Next;
                    currentOffset = 0;
                    chunkBuffer = currentChunk.Buffer;
                    chunkSize = chunkBuffer.Length;
                    if (currentChunk.Next == null)
                        chunkSize = endOffset;
                }

                int readCount = min(count, chunkSize - currentOffset);
                Buffer.BlockCopy(chunkBuffer, currentOffset, buffer, offset, readCount);
                offset += readCount;
                count -= readCount;
                currentOffset += readCount;
                bytesRead += readCount;
            }

            return bytesRead;
        }

        byte[] oneByteBuffer = new byte[1];
        public override int ReadByte(){
            if(Read(oneByteBuffer, 0, 1) == 1)
                return oneByteBuffer[0];
            return -1;
        }
                
        public override void Write(byte[] buffer, int offset, int count) {
            byte[] chunkBuffer = currentChunk.Buffer;
            int chunkSize = chunkBuffer.Length;
    
            while (count > 0) {
                if (currentOffset == chunkSize) {
                    // allocate a new chunk if the current one is full
                    if(currentChunk.Next == null){
                        currentChunk.Next = AllocateMemoryChunk(count);
                        endOffset = 0;
                    }
                    currentChunkStartPos += currentChunk.Buffer.Length;
                    currentChunk = currentChunk.Next;
                    currentOffset = 0;
                        
                    chunkBuffer = currentChunk.Buffer;
                    chunkSize = chunkBuffer.Length;
                }
                             
                int copyCount = min(count, chunkSize - endOffset);
                Buffer.BlockCopy(buffer, offset, chunkBuffer, endOffset, copyCount);
                offset += copyCount;
                count -= copyCount;
                currentOffset += copyCount;
                SyncEndOffset();
            }            
        }

        public override void WriteByte(byte value) {
            oneByteBuffer[0] = value;
            Write(oneByteBuffer, 0, 1);
        }

        internal bool GetNextBuffer(out byte[] buffer, out int byteOffset, out int byteCount) {
            buffer = null;
            byteOffset = 0;
            byteCount = 0;
            if (currentChunk == null || headChunk == null || (currentChunk.Next == null && currentOffset == endOffset))
                return false;

            buffer = currentChunk.Buffer;
            if (currentChunk.Next == null) {
                byteCount = endOffset;
                currentOffset = endOffset;
            }
            else {
                currentChunkStartPos += currentChunk.Buffer.Length;
                currentChunk = currentChunk.Next;
                byteCount = buffer.Length;
                currentOffset = 0;
            }
            return true;
        }

        // copy entire buffer into an array
        internal virtual byte[] ToArray() {
            int length = (int)Length; // this will throw if stream is closed
            byte[] copy = new byte[length];

            MemoryChunk backupReadChunk = currentChunk;
            int backupReadOffset = currentOffset;

            currentChunk = headChunk;
            currentOffset = 0;            
            Read(copy, 0, length);

            currentChunk = backupReadChunk;
            currentOffset = backupReadOffset;           
            
            return copy;
        }      


        // write remainder of this stream to another stream
        internal virtual void WriteTo(Stream stream) {
            if (stream == null)
                throw new ArgumentNullException("stream");

            byte[] chunkBuffer = currentChunk.Buffer;
            int chunkSize = chunkBuffer.Length;
            if (currentChunk.Next == null)
                chunkSize = endOffset;

            // following code mirrors Read() logic (currentChunk/currentOffset should
            //   point just past last byte of last chunk when done)

            for (;;){ // loop until end of chunks is found
                if (currentOffset == chunkSize) {
                    // exit if no more chunks are currently available
                    if (currentChunk.Next == null)
                        break;

                    currentChunkStartPos += currentChunk.Buffer.Length;
                    currentChunk = currentChunk.Next;
                    currentOffset = 0;
                    chunkBuffer = currentChunk.Buffer;
                    chunkSize = chunkBuffer.Length;
                    if (currentChunk.Next == null)
                        chunkSize = endOffset;
                }

                int writeCount = chunkSize - currentOffset;
                stream.Write(chunkBuffer, currentOffset, writeCount);
                currentOffset = chunkSize;
            }
                
        } 



        private static int min(int a, int b) { return a < b ? a : b;}

        private MemoryChunk AllocateMemoryChunk(long newSize) {
            if(newSize > chunkSize) chunkSize = newSize;
            MemoryChunk chunk = new MemoryChunk();
            chunk.Buffer = new byte[chunkSize];
            chunkSize*=2;//nexttime alloc more
            chunk.Next = null;
            return chunk;
        }

        private void SyncEndOffset() {
            if (currentChunk.Next == null && currentOffset > endOffset) 
                endOffset = currentOffset;
        }

        private class MemoryChunk {
            internal byte[] Buffer = null;
            internal MemoryChunk Next = null;
        }
    }
}
#endif