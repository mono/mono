//------------------------------------------------------------------------------
// <copyright file="SqlClientWrapperSmiStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.IO;

    // Simple wrapper over SmiStream that handles server events on the SqlClient side of Smi
    internal class SqlClientWrapperSmiStream : Stream {

        private SmiEventSink_Default    _sink;
        private SmiStream               _stream;

        internal SqlClientWrapperSmiStream( SmiEventSink_Default sink, SmiStream stream ) {
            Debug.Assert( null != sink );
            Debug.Assert( null != stream );
            _sink = sink;
            _stream = stream;
        }

        public override bool CanRead {
            get {
                return _stream.CanRead;
            }
        }

        // If CanSeek is false, Position, Seek, Length, and SetLength should throw.
        public override bool CanSeek {
            get {
                return _stream.CanSeek;
            }
        }

        public override bool CanWrite {
            get {
                return _stream.CanWrite;
            }
        }

        public override long Length {
            get {
                long length = _stream.GetLength( _sink );
                _sink.ProcessMessagesAndThrow();
                return length;
            }
        }

        public override long Position {
            get {
                long position = _stream.GetPosition( _sink );
                _sink.ProcessMessagesAndThrow();
                return position;
            }
            set {
                _stream.SetPosition( _sink, value );
                _sink.ProcessMessagesAndThrow();
            }
        }

        public override void Flush() {
            _stream.Flush( _sink );
            _sink.ProcessMessagesAndThrow();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            long result = _stream.Seek( _sink, offset, origin );
            _sink.ProcessMessagesAndThrow();
            return result;
        }

        public override void SetLength(long value) {
            _stream.SetLength( _sink, value );
            _sink.ProcessMessagesAndThrow();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int bytesRead = _stream.Read( _sink, buffer, offset, count );
            _sink.ProcessMessagesAndThrow();
            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _stream.Write( _sink, buffer, offset, count );
            _sink.ProcessMessagesAndThrow();
        }
    }

}


