using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;
using System.Threading;

namespace System.Net.Sockets
{
    partial class SocketAsyncEventArgs
    {
        // Single buffer.
        private Memory<byte> _buffer;
        private int _offset;
        private int _count;
        private bool _bufferIsExplicitArray;

        // BufferList property variables.
        private IList<ArraySegment<byte>> _bufferList;
        private List<ArraySegment<byte>> _bufferListInternal;

        public byte[] Buffer
        {
            get
            {
                if (_bufferIsExplicitArray)
                {
                    bool success = MemoryMarshal.TryGetArray(_buffer, out ArraySegment<byte> arraySegment);
                    Debug.Assert(success);
                    return arraySegment.Array;
                }

                return null;
            }
        }

        public Memory<byte> MemoryBuffer => _buffer;

        public int Offset => _offset;

        public int Count => _count;

        // NOTE: this property is mutually exclusive with Buffer.
        // Setting this property with an existing non-null Buffer will throw.
        public IList<ArraySegment<byte>> BufferList
        {
            get { return _bufferList; }
            set
            {
                if (value != null)
                {
                    if (!_buffer.Equals(default))
                    {
                        // Can't have both set
                        throw new ArgumentException(SR.Format(SR.net_ambiguousbuffers, nameof(Buffer)));
                    }

                    // Copy the user-provided list into our internal buffer list,
                    // so that we are not affected by subsequent changes to the list.
                    // We reuse the existing list so that we can avoid reallocation when possible.
                    int bufferCount = value.Count;
                    if (_bufferListInternal == null)
                    {
                        _bufferListInternal = new List<ArraySegment<byte>>(bufferCount);
                    }
                    else
                    {
                        _bufferListInternal.Clear();
                    }

                    for (int i = 0; i < bufferCount; i++)
                    {
                        ArraySegment<byte> buffer = value[i];
                        RangeValidationHelpers.ValidateSegment(buffer);
                        _bufferListInternal.Add(buffer);
                    }
                }
                else
                {
                    _bufferListInternal?.Clear();
                }

                _bufferList = value;
            }
        }

        public void SetBuffer(int offset, int count)
        {
            if (!_buffer.Equals(default))
            {
                if ((uint)offset > _buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                if ((uint)count > (_buffer.Length - offset))
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }
                if (!_bufferIsExplicitArray)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_BufferNotExplicitArray);
                }

                _offset = offset;
                _count = count;
            }
        }

        internal void CopyBufferFrom(SocketAsyncEventArgs source)
        {
            _buffer = source._buffer;
            _offset = source._offset;
            _count = source._count;
            _bufferIsExplicitArray = source._bufferIsExplicitArray;
        }

        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                // Clear out existing buffer.
                _buffer = default;
                _offset = 0;
                _count = 0;
                _bufferIsExplicitArray = false;
            }
            else
            {
                // Can't have both Buffer and BufferList.
                if (_bufferList != null)
                {
                    throw new ArgumentException(SR.Format(SR.net_ambiguousbuffers, nameof(BufferList)));
                }

                // Offset and count can't be negative and the
                // combination must be in bounds of the array.
                if ((uint)offset > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                if ((uint)count > (buffer.Length - offset))
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }

                _buffer = buffer;
                _offset = offset;
                _count = count;
                _bufferIsExplicitArray = true;
            }
        }

        public void SetBuffer(Memory<byte> buffer)
        {
            if (buffer.Length != 0 && _bufferList != null)
            {
                throw new ArgumentException(SR.Format(SR.net_ambiguousbuffers, nameof(BufferList)));
            }

            _buffer = buffer;
            _offset = 0;
            _count = buffer.Length;
            _bufferIsExplicitArray = false;
        }

        internal bool HasMultipleBuffers => _bufferList != null;
    }
}
