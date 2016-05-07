//------------------------------------------------------------------------------
// <copyright file="HttpInputStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Input stream used in response and uploaded file objects
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {

    using System.IO;
    using System.CodeDom.Compiler; // needed for TempFilesCollection
    using System.Security;
    using System.Security.Permissions;
    using System.Web.Hosting;


    /*
     * Wrapper around temporary file or byte[] for input stream
     * 
     * Pattern of use:
     *      ctor
     *      AddBytes
     *      ...
     *      DoneAddingBytes
     *      access bytes: [] / CopyBytes / WriteBytes / GetAsByteArray
     *      Dispose
     */
    internal class HttpRawUploadedContent : IDisposable {
        private int _fileThreshold; // for sizes over this use file
        private int _expectedLength;// content-length
        private bool _completed;    // true when all data's in
        private int _length;        // length of the data
        private byte[] _data;       // contains data (either all of it or part read from file)
        private TempFile _file;     // temporary file with content (null when using byte[])
        private int _chunkOffset;   // which part of file is cached in data - offset
        private int _chunkLength;   // which part of file is cached in data - length

        internal HttpRawUploadedContent(int fileThreshold, int expectedLength) {
            _fileThreshold = fileThreshold;
            _expectedLength = expectedLength;

            if (_expectedLength >= 0 && _expectedLength < _fileThreshold)
                _data = new byte[_expectedLength];
            else
                _data = new byte[_fileThreshold];
        }

        public void Dispose() {
            if (_file != null)
                _file.Dispose();
        }

        internal void AddBytes(byte[] data, int offset, int length) {
            if (_completed)
                throw new InvalidOperationException();

            if (length <= 0)
                return;

            if (_file == null) {
                // fits in the existing _data
                if (_length + length <= _data.Length) {
                    Array.Copy(data, offset, _data, _length, length);
                    _length += length;
                    return;
                }

                // doesn't fit in _data but still under threshold
                // possible if content-length is -1, or when filtering
                if (_length + length <= _fileThreshold) {
                    byte[] newData = new byte[_fileThreshold];
                    if (_length > 0)
                        Array.Copy(_data, 0, newData, 0, _length);
                    Array.Copy(data, offset, newData, _length, length);

                    _data = newData;
                    _length += length;
                    return;
                }

                // need to convert to file
                _file = new TempFile();
                _file.AddBytes(_data, 0, _length);
            }

            // using file
            _file.AddBytes(data, offset, length);
            _length += length;
        }

        internal void DoneAddingBytes() {
            if (_data == null)
                _data = new byte[0];

            if (_file != null)
                _file.DoneAddingBytes();

            _completed = true;
        }

        internal int Length {
            get { return _length; }
        }

        internal byte this[int index] {
            get {
                if (!_completed)
                    throw new InvalidOperationException();

                // all data in memory
                if (_file == null)
                    return _data[index];

                // index in the chunk already read
                if (index >= _chunkOffset && index < _chunkOffset + _chunkLength)
                    return _data[index - _chunkOffset];

                // check bounds
                if (index < 0 || index >= _length)
                    throw new ArgumentOutOfRangeException("index");

                // read from file
                _chunkLength = _file.GetBytes(index, _data.Length, _data, 0);
                _chunkOffset = index;
                return _data[0];
            }
        }

        internal void CopyBytes(int offset, byte[] buffer, int bufferOffset, int length) {
            if (!_completed)
                throw new InvalidOperationException();

            if (_file != null) {
                if (offset >= _chunkOffset && offset+length < _chunkOffset + _chunkLength) {
                    // preloaded
                    Array.Copy(_data, offset - _chunkOffset, buffer, bufferOffset, length);
                }
                else {
                    if (length <= _data.Length) {
                        // read from file and remember the chunk
                        _chunkLength = _file.GetBytes(offset, _data.Length, _data, 0);
                        _chunkOffset = offset;
                        Array.Copy(_data, offset - _chunkOffset, buffer, bufferOffset, length);
                    }
                    else {
                        // read from file
                        _file.GetBytes(offset, length, buffer, bufferOffset);
                    }
                }
            }
            else {
                Array.Copy(_data, offset, buffer, bufferOffset, length);
            }
        }

        internal void WriteBytes(int offset, int length, Stream stream) {
            if (!_completed)
                throw new InvalidOperationException();

            if (_file != null) {
                int readPosition = offset;
                int bytesRemaining = length;
                byte[] buf = new byte[bytesRemaining > _fileThreshold ? _fileThreshold : bytesRemaining];

                while (bytesRemaining > 0) {
                    int bytesToRead = bytesRemaining > _fileThreshold ? _fileThreshold : bytesRemaining;
                    int bytesRead = _file.GetBytes(readPosition, bytesToRead, buf, 0);
                    if (bytesRead == 0)
                        break;

                    stream.Write(buf, 0, bytesRead);

                    readPosition += bytesRead;
                    bytesRemaining -= bytesRead;
                }
            }
            else {
                stream.Write(_data, offset, length);
            }
        }

        internal byte[] GetAsByteArray() {
            // If the request is chunked, _data can be much larger than 
            // the actual number of bytes read, and FillInFormCollection
            // will call FillFromEncodedBytes and incorrectly append a
            // bunch of zeros to the last form value.  Therefore, we copy 
            // the data into a smaller array if _length < _data.Length
            if (_file == null && _length == _data.Length) {
                return _data;
            }
            return GetAsByteArray(0, _length);
        }

        internal byte[] GetAsByteArray(int offset, int length) {
            if (!_completed)
                throw new InvalidOperationException();

            if (length == 0)
                return new byte[0];

            byte[] result = new byte[length];
            CopyBytes(offset, result, 0, length);
            return result;
        }

        // helper class for a temp file for large posted data
        class TempFile : IDisposable {
            TempFileCollection _tempFiles;
            String _filename;
            Stream _filestream;

            internal TempFile() {
                // suspend the impersonation for the file creation
                using (new ApplicationImpersonationContext()) {
                    String tempDir = Path.Combine(HttpRuntime.CodegenDirInternal, "uploads");

                    // Assert IO access to the temporary directory
                    new FileIOPermission(FileIOPermissionAccess.AllAccess, tempDir).Assert();

                    if (!Directory.Exists(tempDir)) {
                        try {
                            Directory.CreateDirectory(tempDir);
                        }
                        catch {
                        }
                    }

                    _tempFiles = new TempFileCollection(tempDir, false /*keepFiles*/);
                    _filename = _tempFiles.AddExtension("post", false /*keepFiles*/);
                    //using 4096 as the buffer size, same as the BCL
                    _filestream = new FileStream(_filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
                }
            }

            public void Dispose() {
                // suspend the impersonation for the file creation
                using (new ApplicationImpersonationContext()) {
                    try {
                        // force filestream handle to close
                        // since we're using FILE_FLAG_DELETE_ON_CLOSE
                        // this will delete it from disk as well
                        if (_filestream != null) {
                            _filestream.Close();
                        }

                        _tempFiles.Delete();
                        ((IDisposable)_tempFiles).Dispose();
                    }
                    catch {
                    }
                }
            }

            internal void AddBytes(byte[] data, int offset, int length) {
                if (_filestream == null)
                    throw new InvalidOperationException();

                _filestream.Write(data, offset, length);
            }

            internal void DoneAddingBytes() {
                if (_filestream == null)
                    throw new InvalidOperationException();

                _filestream.Flush();
                _filestream.Seek(0, SeekOrigin.Begin);
            }

            internal int GetBytes(int offset, int length, byte[] buffer, int bufferOffset) {
                if (_filestream == null)
                    throw new InvalidOperationException();

                _filestream.Seek(offset, SeekOrigin.Begin);
                return _filestream.Read(buffer, bufferOffset, length);
            }
        }
    }

    /*
     * Stream object over HttpRawUploadedContent
     * Not a publc class - used internally, returned as Stream
     */
    internal class HttpInputStream : Stream {
        private HttpRawUploadedContent _data; // the buffer with the content
        private int _offset;        // offset to the start of this stream
        private int _length;        // length of this stream
        private int _pos;           // current reader posision

        //
        // Internal access (from this package)
        //

        internal HttpInputStream(HttpRawUploadedContent data, int offset, int length) {
            Init(data, offset, length);
        }

        protected void Init(HttpRawUploadedContent data, int offset, int length) {
            _data = data;
            _offset = offset;
            _length = length;
            _pos = 0; 
        }

        protected void Uninit() {
            _data = null;
            _offset = 0;
            _length = 0;
            _pos = 0;
        }

        internal byte[] GetAsByteArray() {
            if (_length == 0)
                return null;

            return _data.GetAsByteArray(_offset, _length);
        }

        internal void WriteTo(Stream s) {
            if (_data != null && _length > 0)
                _data.WriteBytes(_offset, _length, s);
        }

        //
        // BufferedStream implementation
        //

        public override bool CanRead {
            get {return true;}
        }

        public override bool CanSeek {
            get {return true;}
        }

        public override bool CanWrite {
            get {return false;}
        }         

        public override long Length {
            get {return _length;}                       
        }

        public override long Position {
            get {return _pos;}

            set {
                Seek(value, SeekOrigin.Begin);
            }            
        }                     

        protected override void Dispose(bool disposing) {
            try {
                if (disposing)
                    Uninit();
            }
            finally {
                base.Dispose(disposing);
            }
        }        

        public override void Flush() {
        }

        public override long Seek(long offset, SeekOrigin origin) {
            int newpos = _pos;
            int offs = (int)offset;

            switch (origin) {
                case SeekOrigin.Begin:
                    newpos = offs;
                    break;
                case SeekOrigin.Current:
                    newpos = _pos + offs;
                    break;
                case SeekOrigin.End:
                    newpos = _length + offs;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("origin");
            }

            if (newpos < 0 || newpos > _length)
                throw new ArgumentOutOfRangeException("offset");

            _pos = newpos;
            return _pos;
        }

        public override void SetLength(long length) {
            throw new NotSupportedException(); 
        }

        public override int Read(byte[] buffer, int offset, int count) {
            // find the number of bytes to copy
            int numBytes = _length - _pos;
            if (count < numBytes)
                numBytes = count;

            // copy the bytes
            if (numBytes > 0)
                _data.CopyBytes(_offset + _pos, buffer, offset, numBytes);

            // adjust the position
            _pos += numBytes;
            return numBytes;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }
    }

    /*
     * Stream used as the source for input filtering
     */

    internal class HttpInputStreamFilterSource : HttpInputStream {
        internal HttpInputStreamFilterSource() : base(null, 0, 0) {
        }

        internal void SetContent(HttpRawUploadedContent data) {
            if (data != null)
                base.Init(data, 0, data.Length);
            else
                base.Uninit();
        }
    }

}
