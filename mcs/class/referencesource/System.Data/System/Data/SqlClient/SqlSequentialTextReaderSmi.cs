using System;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.SqlServer.Server;

namespace System.Data.SqlClient
{
    sealed internal class SqlSequentialTextReaderSmi : System.IO.TextReader
    {
        private SmiEventSink_Default _sink;
        private ITypedGettersV3 _getters;
        private int _columnIndex;       // The index of out column in the table
        private long _position;         // Current position in the stream
        private long _length;           // Total length of the stream
        private int _peekedChar;        // Current peeked character (if any)

        internal SqlSequentialTextReaderSmi(SmiEventSink_Default sink, ITypedGettersV3 getters, int columnIndex, long length)
        {
            _sink = sink;
            _getters = getters;
            _columnIndex = columnIndex;
            _length = length;
            _position = 0;
            _peekedChar = -1;
        }

        internal int ColumnIndex
        {
            get { return _columnIndex; }
        }

        public override int Peek()
        {
            if (!HasPeekedChar)
            {
                _peekedChar = Read();
            }

            Debug.Assert(_peekedChar == -1 || ((_peekedChar >= char.MinValue) && (_peekedChar <= char.MaxValue)), string.Format("Bad peeked character: {0}", _peekedChar));
            return _peekedChar;
        }

        public override int Read()
        {
            if (IsClosed)
            {
                throw ADP.ObjectDisposed(this);
            }

            int readChar = -1;

            // If there is already a peeked char, then return it
            if (HasPeekedChar)
            {
                readChar = _peekedChar;
                _peekedChar = -1;
            }
            // If there is data available try to read a char
            else if (_position < _length)
            {
                char[] tempBuffer = new char[1];
                int charsRead = ValueUtilsSmi.GetChars_Unchecked(_sink, _getters, _columnIndex, _position, tempBuffer, 0, 1);
                if (charsRead == 1)
                {
                    readChar = tempBuffer[0];
                    _position++;
                }
            }

            Debug.Assert(readChar == -1 || ((readChar >= char.MinValue) && (readChar <= char.MaxValue)), string.Format("Bad read character: {0}", readChar));
            return readChar;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            SqlSequentialTextReader.ValidateReadParameters(buffer, index, count);
            if (IsClosed)
            {
                throw ADP.ObjectDisposed(this);
            }

            int charsRead = 0;
            // Load in peeked char
            if ((count > 0) && (HasPeekedChar))
            {
                Debug.Assert((_peekedChar >= char.MinValue) && (_peekedChar <= char.MaxValue), string.Format("Bad peeked character: {0}", _peekedChar));
                buffer[index + charsRead] = (char)_peekedChar;
                charsRead++;
                _peekedChar = -1;
            }

            // Read whichever is less: however much the user asked for, or however much we have
            // NOTE: It is safe to do this since count <= Int32.MaxValue, therefore the Math.Min should always result in an int
            int charsNeeded = (int)Math.Min((long)(count - charsRead), _length - _position);
            // If we need more data and there is data avaiable, read
            if (charsNeeded > 0)
            {
                int newCharsRead = ValueUtilsSmi.GetChars_Unchecked(_sink, _getters, _columnIndex, _position, buffer, index + charsRead, charsNeeded);
                _position += newCharsRead;
                charsRead += newCharsRead;
            }

            return charsRead;
        }
        
        /// <summary>
        /// Forces the TextReader to act as if it was closed
        /// This does not actually close the stream, read off the rest of the data or dispose this
        /// </summary>
        internal void SetClosed()
        {
            _sink = null;
            _getters = null;
            _peekedChar = -1;
        }

        /// <summary>
        /// True if this TextReader is supposed to be closed
        /// </summary>
        private bool IsClosed
        {
            get { return ((_sink == null) || (_getters == null)); } 
        }

        /// <summary>
        /// True if there is a peeked character available
        /// </summary>
        private bool HasPeekedChar
        {
            get { return (_peekedChar >= char.MinValue); }
        }
    }
}
