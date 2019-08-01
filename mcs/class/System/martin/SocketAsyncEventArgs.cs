// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Net.Sockets
{
    public partial class SocketAsyncEventArgs : EventArgs, IDisposable
    {
        // AcceptSocket property variables.
        private Socket _acceptSocket;

        // Single buffer.
        private Memory<byte> _buffer;
        private int _offset;
        private int _count;
        private bool _bufferIsExplicitArray;

        // BufferList property variables.
        private IList<ArraySegment<byte>> _bufferList;
        private List<ArraySegment<byte>> _bufferListInternal;

        // BytesTransferred property variables.
        private int _bytesTransferred;

        // DisconnectReuseSocket propery variables.
        private bool _disconnectReuseSocket;

        // LastOperation property variables.
        private SocketAsyncOperation _completedOperation;

        // ReceiveMessageFromPacketInfo property variables.
        private IPPacketInformation _receiveMessageFromPacketInfo;

        // RemoteEndPoint property variables.
        private EndPoint _remoteEndPoint;

        // SendPacketsSendSize property variable.
        private int _sendPacketsSendSize;

        // SendPacketsElements property variables.
        private SendPacketsElement[] _sendPacketsElements;

        // SendPacketsFlags property variable.
        private TransmitFileOptions _sendPacketsFlags;

        // SocketError property variables.
        private SocketError _socketError;
        private Exception _connectByNameError;

        // SocketFlags property variables.
        private SocketFlags _socketFlags;

        // UserToken property variables.
        private object _userToken;

        // Misc state variables.
        private Socket _currentSocket;

        public Socket AcceptSocket
        {
            get { return _acceptSocket; }
            set { _acceptSocket = value; }
        }

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

        // SendPacketsFlags property.
        public TransmitFileOptions SendPacketsFlags
        {
            get { return _sendPacketsFlags; }
            set { _sendPacketsFlags = value; }
        }

        // NOTE: this property is mutually exclusive with Buffer.
        // Setting this property with an existing non-null Buffer will throw.
        public IList<ArraySegment<byte>> BufferList
        {
            get { return _bufferList; }
            set
            {
#if !MONO
                StartConfiguring();
#endif
                try
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

                    SetupMultipleBuffers();
                }
                finally
                {
#if !MONO
                    Complete();
#endif
                }
            }
        }

        public int BytesTransferred
        {
            get { return _bytesTransferred; }
        }

        // DisconnectResuseSocket property.
        public bool DisconnectReuseSocket
        {
            get { return _disconnectReuseSocket; }
            set { _disconnectReuseSocket = value; }
        }

        public SocketAsyncOperation LastOperation
        {
            get { return _completedOperation; }
        }

        public IPPacketInformation ReceiveMessageFromPacketInfo
        {
            get { return _receiveMessageFromPacketInfo; }
        }

        public EndPoint RemoteEndPoint
        {
            get { return _remoteEndPoint; }
            set { _remoteEndPoint = value; }
        }

        public SendPacketsElement[] SendPacketsElements
        {
            get { return _sendPacketsElements; }
            set
            {
#if !MONO
                StartConfiguring();
#endif
                try
                {
                    _sendPacketsElements = value;
                }
                finally
                {
#if !MONO
                    Complete();
#endif
                }
            }
        }

        public int SendPacketsSendSize
        {
            get { return _sendPacketsSendSize; }
            set { _sendPacketsSendSize = value; }
        }

        public SocketError SocketError
        {
            get { return _socketError; }
            set { _socketError = value; }
        }

        public Exception ConnectByNameError
        {
            get { return _connectByNameError; }
        }

        public SocketFlags SocketFlags
        {
            get { return _socketFlags; }
            set { _socketFlags = value; }
        }

        public object UserToken
        {
            get { return _userToken; }
            set { _userToken = value; }
        }

        public void SetBuffer(int offset, int count)
        {
#if !MONO
            StartConfiguring();
#endif
            try
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
            finally
            {
#if !MONO
                Complete();
#endif
            }
        }

        internal void CopyBufferFrom(SocketAsyncEventArgs source)
        {
#if !MONO
            StartConfiguring();
#endif
            try
            {
                _buffer = source._buffer;
                _offset = source._offset;
                _count = source._count;
                _bufferIsExplicitArray = source._bufferIsExplicitArray;
            }
            finally
            {
#if !MONO
                Complete();
#endif
            }
        }

        public void SetBuffer(byte[] buffer, int offset, int count)
        {
#if !MONO
            StartConfiguring();
#endif
            try
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
            finally
            {
#if !MONO
                Complete();
#endif
            }
        }

        public void SetBuffer(Memory<byte> buffer)
        {
#if !MONO
            StartConfiguring();
#endif
            try
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
            finally
            {
#if !MONO
                Complete();
#endif
            }
        }

        internal bool HasMultipleBuffers => _bufferList != null;

        internal void SetResults(SocketError socketError, int bytesTransferred, SocketFlags flags)
        {
            _socketError = socketError;
            _connectByNameError = null;
            _bytesTransferred = bytesTransferred;
            _socketFlags = flags;
        }

        internal void SetResults(Exception exception, int bytesTransferred, SocketFlags flags)
        {
            _connectByNameError = exception;
            _bytesTransferred = bytesTransferred;
            _socketFlags = flags;

            if (exception == null)
            {
                _socketError = SocketError.Success;
            }
            else
            {
                SocketException socketException = exception as SocketException;
                if (socketException != null)
                {
                    _socketError = socketException.SocketErrorCode;
                }
                else
                {
                    _socketError = SocketError.SocketError;
                }
            }
        }
    }
}
