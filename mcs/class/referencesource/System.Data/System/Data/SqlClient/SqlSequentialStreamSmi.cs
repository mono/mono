using System;
using System.Data.Common;
using Microsoft.SqlServer.Server;

namespace System.Data.SqlClient
{
    sealed internal class SqlSequentialStreamSmi : System.IO.Stream
    {
        private SmiEventSink_Default _sink;
        private ITypedGettersV3 _getters;
        private int _columnIndex;       // The index of out column in the table
        private long _position;         // Current position in the stream
        private long _length;           // Total length of the stream

        internal SqlSequentialStreamSmi(SmiEventSink_Default sink, ITypedGettersV3 getters, int columnIndex, long length)
        {
            _sink = sink;
            _getters = getters;
            _columnIndex = columnIndex;
            _length = length;
            _position = 0;
        }

        public override bool CanRead
        {
            get { return ((_sink != null) && (_getters != null)); }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        { }

        public override long Length
        {
            get { throw ADP.NotSupported(); }
        }

        public override long Position
        {
            get { throw ADP.NotSupported(); }
            set { throw ADP.NotSupported(); }
        }
        
        internal int ColumnIndex
        {
            get { return _columnIndex; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            SqlSequentialStream.ValidateReadParameters(buffer, offset, count);
            if (!CanRead)
            {
                throw ADP.ObjectDisposed(this);
            }

            try
            {
                // Read whichever is less: however much the user asked for, or however much we have
                // NOTE: It is safe to do this since count <= Int32.MaxValue, therefore the Math.Min should always result in an int
                int bytesNeeded = (int)Math.Min((long)count, _length - _position);
                int bytesRead = 0;
                if (bytesNeeded > 0)
                {
                    bytesRead = ValueUtilsSmi.GetBytes_Unchecked(_sink, _getters, _columnIndex, _position, buffer, offset, bytesNeeded);
                    _position += bytesRead;
                }
                return bytesRead;
            }
            catch (SqlException ex)
            {
                // Stream.Read() can't throw a SqlException - so wrap it in an IOException
                throw ADP.ErrorReadingFromStream(ex);
            }
        }
        
        public override long Seek(long offset, IO.SeekOrigin origin)
        {
            throw ADP.NotSupported();
        }

        public override void SetLength(long value)
        {
            throw ADP.NotSupported();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw ADP.NotSupported();
        }

        /// <summary>
        /// Forces the stream to act as if it was closed (i.e. CanRead=false and Read() throws)
        /// This does not actually close the stream, read off the rest of the data or dispose this
        /// </summary>
        internal void SetClosed()
        {
            _sink = null;
            _getters = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SetClosed();
            }

            base.Dispose(disposing);
        }
    }
}
