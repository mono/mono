namespace System.Net.Mime
{
    internal class WriteStateInfoBase
    {
        protected byte[] _header;
        protected byte[] _footer;
        protected int _maxLineLength;
        protected byte[] buffer;
        protected int _currentLineLength;
        protected int _currentBufferUsed;

        //1024 was originally set in the encoding streams
        protected const int defaultBufferSize = 1024;

        internal WriteStateInfoBase()
        {
            this.buffer = new byte[defaultBufferSize];
            this._header = new byte[0]; ;
            this._footer = new byte[0];
            this._maxLineLength = EncodedStreamFactory.DefaultMaxLineLength;
            this._currentLineLength = 0;
            this._currentBufferUsed = 0;
        }

        internal WriteStateInfoBase(int bufferSize, byte[] header, byte[] footer, int maxLineLength)
            : this(bufferSize, header, footer, maxLineLength, 0) 
        { 
        }

        internal WriteStateInfoBase(int bufferSize, byte[] header, byte[] footer, int maxLineLength, int mimeHeaderLength)
        {
            this.buffer = new byte[bufferSize];
            this._header = header;
            this._footer = footer;
            this._maxLineLength = maxLineLength;
            // Account for header name, if any.  e.g. "Subject: "
            this._currentLineLength = mimeHeaderLength;
            this._currentBufferUsed = 0;
        }

        internal int FooterLength
        {
            get
            {
                return _footer.Length;
            }
        }

        internal byte[] Footer
        {
            get
            {
                return _footer;
            }
        }

        internal byte[] Header
        {
            get
            {
                return _header;
            }
        }

        internal byte[] Buffer
        {
            get
            {
                return this.buffer;
            }
        }

        internal int Length
        {
            get
            {
                return this._currentBufferUsed;
            }
        }

        internal int CurrentLineLength 
        {
            get
            {
                return this._currentLineLength;
            }
        }

        // Make sure there is enough space in the buffer to write at least this many more bytes.
        // This should be called before ANY direct write to Buffer.
        private void EnsureSpaceInBuffer(int moreBytes)
        {
            int newsize = Buffer.Length;
            while (_currentBufferUsed + moreBytes >= newsize)
            {
                newsize *= 2;
            }

            if (newsize > Buffer.Length)
            {
                //try to resize- if the machine doesn't have the memory to resize just let it throw
                byte[] tempBuffer = new byte[newsize];

                buffer.CopyTo(tempBuffer, 0);
                this.buffer = tempBuffer;
            }
        }

        internal void Append(byte aByte)
        {
            this.EnsureSpaceInBuffer(1);
            this.Buffer[this._currentBufferUsed++] = aByte;
            this._currentLineLength++;
        }

        internal void Append(params byte[] bytes)
        {
            this.EnsureSpaceInBuffer(bytes.Length);
            bytes.CopyTo(this.buffer, this.Length);
            this._currentLineLength += bytes.Length;
            this._currentBufferUsed += bytes.Length;
        }

        internal void AppendCRLF(bool includeSpace)
        {
            AppendFooter();            

            //add soft line break
            Append((byte)'\r', (byte)'\n');
            _currentLineLength = 0; // New Line
            if (includeSpace)
            {
                //add whitespace to new line (RFC 2045, soft CRLF must be followed by whitespace char)
                //space selected for parity with other MS email clients
                Append((byte)' ');
            }

            AppendHeader();
        }

        internal void AppendHeader()
        {
            if (this.Header != null && this.Header.Length != 0)
            {
                Append(this.Header);
            }
        }

        internal void AppendFooter()
        {
            if (this.Footer != null && this.Footer.Length != 0)
            {
                Append(this.Footer);
            }
        }

        internal int MaxLineLength
        {
            get
            {
                return this._maxLineLength;
            }
        }

        internal void Reset()
        {
            _currentBufferUsed = 0;
            _currentLineLength = 0;
        }

        internal void BufferFlushed()
        {
            _currentBufferUsed = 0;
        }
    }
}
