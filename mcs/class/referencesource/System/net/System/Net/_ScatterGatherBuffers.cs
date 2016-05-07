//------------------------------------------------------------------------------
// <copyright file="_ScatterGatherBuffers.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System;
    using System.Collections;

    internal class ScatterGatherBuffers {

        private MemoryChunk headChunk; // = null;
        private MemoryChunk currentChunk; // = null;  

        private int nextChunkLength = 1024; // this could be customized at construction time
        private int totalLength; // = 0;
        private int chunkCount; // = 0;

        internal ScatterGatherBuffers() {
        }

        internal ScatterGatherBuffers(long totalSize)
        {
            // We know up front how much data is to be written.
            if (totalSize > 0)
            {
                currentChunk = AllocateMemoryChunk(totalSize > Int32.MaxValue ? Int32.MaxValue : (int) totalSize);
            }
        }

        internal BufferOffsetSize[] GetBuffers() {
            if (Empty) {
                return null;
            }
            GlobalLog.Print("ScatterGatherBuffers#" + ValidationHelper.HashString(this) + "::ToArray() chunkCount:" + chunkCount.ToString());
            BufferOffsetSize[] array = new BufferOffsetSize[chunkCount];
            int index = 0;
            MemoryChunk thisMemoryChunk = headChunk;
            while (thisMemoryChunk!=null) {
                GlobalLog.Print("ScatterGatherBuffers#" + ValidationHelper.HashString(this) + "::ToArray() index:" + index.ToString() + " size:" + thisMemoryChunk.FreeOffset);
                //
                // buffer itself is referenced by the BufferOffsetSize struct, data is not copied
                //
                array[index] = new BufferOffsetSize(thisMemoryChunk.Buffer, 0, thisMemoryChunk.FreeOffset, false);
                index++;
                thisMemoryChunk = thisMemoryChunk.Next;
            }
            return array;
        }

        private bool Empty {
            get {
                return headChunk==null || chunkCount==0;
            }
        }

        internal int Length {
            get {
                return totalLength;
            }
        }

        internal void Write(byte[] buffer, int offset, int count) {
            GlobalLog.Print("ScatterGatherBuffers#" + ValidationHelper.HashString(this) + "::Add() count:" + count.ToString());
            while (count > 0) {
                //
                // compute available space in current allocated buffer (0 if there's no buffer)
                //
                int available = Empty ? 0 : currentChunk.Buffer.Length - currentChunk.FreeOffset;
                GlobalLog.Assert(available >= 0, "ScatterGatherBuffers::Add()|available < 0");
                //
                // if the current chunk is is full, allocate a new one
                //
                if (available==0) {
                    // ask for at least count bytes so that we need at most one allocation
                    MemoryChunk newChunk = AllocateMemoryChunk(count);
                    if (currentChunk!=null) {
                        currentChunk.Next = newChunk;
                    }
                    //
                    // move ahead in the linked list (or at the beginning if this is the fist buffer)
                    //
                    currentChunk = newChunk;
                }
                int copyCount = count < available ? count : available;

                Buffer.BlockCopy(
                    buffer,                     // src
                    offset,                     // src index
                    currentChunk.Buffer,        // dest
                    currentChunk.FreeOffset,    // dest index
                    copyCount );                // total size to copy

                //
                // update offsets and counts
                //
                offset += copyCount;
                count -= copyCount;
                totalLength += copyCount;
                currentChunk.FreeOffset += copyCount;
            }
            GlobalLog.Print("ScatterGatherBuffers#" + ValidationHelper.HashString(this) + "::Add() totalLength:" + totalLength.ToString());
        }

        private MemoryChunk AllocateMemoryChunk(int newSize) {
            if (newSize > nextChunkLength) {
                nextChunkLength = newSize;
            }
            MemoryChunk newChunk = new MemoryChunk(nextChunkLength);
            if (Empty) {
                headChunk = newChunk;
            }
            //
            // next time allocate twice as much. check fot possible overflows
            //
            nextChunkLength *= 2;
            //
            // update number of chunks in the linked list
            //
            chunkCount++;
            GlobalLog.Print("ScatterGatherBuffers#" + ValidationHelper.HashString(this) + "::AllocateMemoryChunk() chunkCount:" + chunkCount.ToString() + " nextChunkLength:" + nextChunkLength.ToString());
            return newChunk;
        }

        private class MemoryChunk {
            internal byte[] Buffer;
            internal int FreeOffset; // = 0
            internal MemoryChunk Next; // = null
            internal MemoryChunk(int bufferSize) {
                Buffer = new byte[bufferSize];
            }
        }

    } // class ScatterGatherBuffers



} // namespace System.Net
