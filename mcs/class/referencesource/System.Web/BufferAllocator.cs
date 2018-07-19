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
    using System.Collections.Generic;
    using System.IO;
    using System.Globalization;

    using System.Web.Util;

    //////////////////////////////////////////////////////////////////////////////
    // Generic buffer recycling

    internal interface IBufferAllocator {
        object GetBuffer();
        void ReuseBuffer(object buffer);
        void ReleaseAllBuffers();
        int BufferSize { get; }
    }

    internal interface IBufferAllocator<T> : IBufferAllocator {
        new T[] GetBuffer();
        T[] GetBuffer(int minSize);
        void ReuseBuffer(T[] buffer);
    }

    internal interface IAllocatorProvider {
        IBufferAllocator<char>  CharBufferAllocator { get; }
        IBufferAllocator<int>   IntBufferAllocator { get; }
        IBufferAllocator<IntPtr> IntPtrBufferAllocator { get; }

        void TrimMemory();
    }

    /*
     * Base class for allocator doing buffer recycling
     */
    internal abstract class BufferAllocator : IBufferAllocator {
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

        public void ReleaseAllBuffers() {
            if (_numFree > 0) {
                lock (this) {
                    _buffers.Clear();
                    _numFree = 0;
                }
            }
        }

        public object GetBuffer() {
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

        public void ReuseBuffer(object buffer) {
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
        abstract public int BufferSize { get; }
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

        public override int BufferSize { 
            get {
                return _bufferSize;
            } 
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

        public override int BufferSize {
            get {
                return _bufferSize;
            }
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

        public override int BufferSize {
            get {
                return _arraySize;
            }
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

        public override int BufferSize {
            get {
                return _arraySize;
            }
        }
    }



    /*
     * Simple Buffer Allocator - Reusable buffers pool
     * Thread UNSAFE! Lock free. Caller must guarantee non-concurent access.
     * Use as member of already pooled instances (like HttpApplication) that prohibit concurent access to avoid taking locks
     */
    internal class SimpleBufferAllocator<T> : IBufferAllocator<T> {
        private Stack<T[]>   _buffers;
        private readonly int _bufferSize;

        public SimpleBufferAllocator(int bufferSize) {
            if (bufferSize <= 0) {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            _buffers = new Stack<T[]>();
            _bufferSize = bufferSize;
        }

        public T[] GetBuffer() {
            return GetBufferImpl();
        }

        public T[] GetBuffer(int minSize) {
            if (minSize < 0) {
                throw new ArgumentOutOfRangeException("minSize");
            }

            T[] buffer = null;

            if (minSize <= BufferSize) {
                // Get from the pool
                buffer = GetBufferImpl();
            }
            else {
                // Allocate a new buffer. It will not be reused later
                buffer = AllocBuffer(minSize);
            }

            return buffer;
        }

        object IBufferAllocator.GetBuffer() {
            return GetBufferImpl();
        }

        public void ReuseBuffer(T[] buffer) {
            ReuseBufferImpl(buffer);
        }

        void IBufferAllocator.ReuseBuffer(object buffer) {
            ReuseBufferImpl((T[]) buffer);
        }

        public void ReleaseAllBuffers() {
            _buffers.Clear();
        }

        public int BufferSize {
            get {
                return _bufferSize;
            }
        }

        private T[] GetBufferImpl() {
            T[] buffer = null;

            if (_buffers.Count > 0) {
                // Get an exisitng buffer
                buffer = _buffers.Pop();
            }
            else {
                // Create a new buffer
                buffer = AllocBuffer(BufferSize);
            }

            return buffer;
        }

        private void ReuseBufferImpl(T[] buffer) {
            // Accept back only buffers that match the orirignal buffer size
            if (buffer != null && buffer.Length == BufferSize) {
                _buffers.Push(buffer);
            }
        }

        private static T[] AllocBuffer(int size) {
            return new T[size];
        }
    }


    /*
     * Adapter to convert IBufferAllocator to generic IBufferAllocator<>
     */
    class BufferAllocatorWrapper<T> : IBufferAllocator<T> {
        private IBufferAllocator _allocator;

        public BufferAllocatorWrapper(IBufferAllocator allocator) {
            Debug.Assert(allocator != null);

            _allocator = allocator;
        }

        public T[] GetBuffer() {
            return (T[])_allocator.GetBuffer();
        }

        public T[] GetBuffer(int minSize) {
            if (minSize < 0) {
                throw new ArgumentOutOfRangeException("minSize");
            }

            T[] buffer = null;

            if (minSize <= BufferSize) {
                // Get from the allocator
                buffer = (T[])_allocator.GetBuffer();
            }
            else {
                // Allocate a new buffer. It will not be reused later
                buffer = new T[minSize];
            }

            return buffer;
        }

        public void ReuseBuffer(T[] buffer) {
            // Accept back only buffers that match the orirignal buffer size
            if (buffer != null && buffer.Length == BufferSize) {
                _allocator.ReuseBuffer(buffer);
            }
        }

        object IBufferAllocator.GetBuffer() {
            return _allocator.GetBuffer();
        }

        void IBufferAllocator.ReuseBuffer(object buffer) {
            ReuseBuffer((T[])buffer);
        }

        public void ReleaseAllBuffers() {
            _allocator.ReleaseAllBuffers();
        }

        public int BufferSize {
            get {
                return _allocator.BufferSize;
            }
        }
    }


    /*
     * Provider for different buffer allocators
     */
    internal class AllocatorProvider : IAllocatorProvider {
        private IBufferAllocator<char>   _charAllocator = null;
        private IBufferAllocator<int> _intAllocator = null;
        private IBufferAllocator<IntPtr> _intPtrAllocator = null;

        public IBufferAllocator<char> CharBufferAllocator {
            get {
                return _charAllocator;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                _charAllocator = value;
            }
        }

        public  IBufferAllocator<int> IntBufferAllocator {
            get {
                return _intAllocator;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                _intAllocator = value;
            }
        }

        public IBufferAllocator<IntPtr> IntPtrBufferAllocator {
            get {
                return _intPtrAllocator;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                _intPtrAllocator = value;
            }
        }

        public void TrimMemory() {
            if (_charAllocator != null) {
                _charAllocator.ReleaseAllBuffers();
            }

            if (_intAllocator != null) {
                _intAllocator.ReleaseAllBuffers();
            }

            if (_intPtrAllocator != null) {
                _intPtrAllocator.ReleaseAllBuffers();
            }
        }
    }
}
