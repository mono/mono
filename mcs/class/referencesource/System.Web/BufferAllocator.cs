//------------------------------------------------------------------------------
// <copyright file="BufferAllocator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Buffer Allocators with recycling
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web {

    using System.Collections;
    using System.IO;
    using System.Globalization;

    using System.Web.Util;

    //////////////////////////////////////////////////////////////////////////////
    // Generic buffer recycling

    /*
     * Base class for allocator doing buffer recycling
     */
    internal abstract class BufferAllocator {
        private int _maxFree;
        private int _numFree;
        private Stack _buffers;

        private static int s_ProcsFudgeFactor;

        static BufferAllocator() {
            s_ProcsFudgeFactor = SystemInfo.GetNumProcessCPUs();
            if (s_ProcsFudgeFactor < 1) 
                s_ProcsFudgeFactor = 1;

            if (s_ProcsFudgeFactor > 4)
                s_ProcsFudgeFactor = 4;
        }


        internal BufferAllocator(int maxFree) {
            _buffers = new Stack();
            _numFree = 0;
            _maxFree = maxFree * s_ProcsFudgeFactor;
        }

        internal void ReleaseAllBuffers() {
            if (_numFree > 0) {
                lock (this) {
                    _buffers.Clear();
                    _numFree = 0;
                }
            }
        }

        internal /*public*/ Object GetBuffer() {
            Object buffer = null;

            if (_numFree > 0) {
                lock(this) {
                    if (_numFree > 0) {
                        buffer = _buffers.Pop();
                        _numFree--;
                    }
                }
            }

            if (buffer == null)
                buffer = AllocBuffer();

            return buffer;
        }

        internal void ReuseBuffer(Object buffer) {
            if (_numFree < _maxFree) {
                lock(this) {
                    if (_numFree < _maxFree) {
                        _buffers.Push(buffer);
                        _numFree++;
                    }
                }
            }
        }

        /*
         * To be implemented by a derived class
         */
        abstract protected Object AllocBuffer();
    }

    /*
     * Concrete allocator class for ubyte[] buffer recycling
     */
    internal class UbyteBufferAllocator : BufferAllocator {
        private int _bufferSize;

        internal UbyteBufferAllocator(int bufferSize, int maxFree) : base(maxFree) {
            _bufferSize = bufferSize;
        }

        protected override Object AllocBuffer() {
            return new byte[_bufferSize];
        }
    }

    /*
     * Concrete allocator class for char[] buffer recycling
     */
    internal class CharBufferAllocator : BufferAllocator {
        private int _bufferSize;

        internal CharBufferAllocator(int bufferSize, int maxFree)

        : base(maxFree) {
            _bufferSize = bufferSize;
        }

        protected override Object AllocBuffer() {
            return new char[_bufferSize];
        }
    }

    /*
     * Concrete allocator class for int[] buffer recycling
     */
    internal class IntegerArrayAllocator : BufferAllocator {
        private int _arraySize;

        internal IntegerArrayAllocator(int arraySize, int maxFree)

        : base(maxFree) {
            _arraySize = arraySize;
        }

        protected override Object AllocBuffer() {
            return new int[_arraySize];
        }
    }

    /*
     * Concrete allocator class for IntPtr[] buffer recycling
     */
    internal class IntPtrArrayAllocator : BufferAllocator {
        private int _arraySize;

        internal IntPtrArrayAllocator(int arraySize, int maxFree)

        : base(maxFree) {
            _arraySize = arraySize;
        }

        protected override Object AllocBuffer() {
            return new IntPtr[_arraySize];
        }
    }

}
