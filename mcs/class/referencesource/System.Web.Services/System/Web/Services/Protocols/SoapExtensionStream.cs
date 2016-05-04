//------------------------------------------------------------------------------
// <copyright file="SoapExtensionStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.IO;
    using System.Web.Services;

    internal class SoapExtensionStream : Stream {
        internal Stream innerStream;
        bool hasWritten = false;
        bool streamReady;

        internal SoapExtensionStream() {
        }

        private bool EnsureStreamReady() {
            if (streamReady) return true;
            throw new InvalidOperationException(Res.GetString(Res.WebBadStreamState));            
        }

        public override bool CanRead {
            get {
                EnsureStreamReady();
                return innerStream.CanRead;
            }
        }

        public override bool CanSeek {
            get {
                EnsureStreamReady();
                return innerStream.CanSeek;
            }
        }

        public override bool CanWrite {
            get {
                EnsureStreamReady();
                return innerStream.CanWrite;
            }
        }

        internal bool HasWritten
        {
            get { return this.hasWritten; }
        }

        public override long Length
        {
            get {
                EnsureStreamReady();
                return innerStream.Length;
            }
        }

        public override long Position {
            get {
                EnsureStreamReady();
                return innerStream.Position;
            }
            set {
                EnsureStreamReady();
                this.hasWritten = true;
                innerStream.Position = value;
            }
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    EnsureStreamReady();
                    this.hasWritten = true;
                    innerStream.Close();
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        public override void Flush() {
            EnsureStreamReady();
            this.hasWritten = true;
            innerStream.Flush();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state) {
            EnsureStreamReady();
            return innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult) {
            EnsureStreamReady();
            return innerStream.EndRead(asyncResult);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state) {
            EnsureStreamReady();
            this.hasWritten = true;
            return innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult) {
            EnsureStreamReady();
            this.hasWritten = true;
            innerStream.EndWrite(asyncResult);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            EnsureStreamReady();
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            EnsureStreamReady();
            innerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count) {
            EnsureStreamReady();
            return innerStream.Read(buffer, offset, count);
        }

        public override int ReadByte() {
            EnsureStreamReady();
            return innerStream.ReadByte();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            EnsureStreamReady();
            this.hasWritten = true;
            innerStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value) {
            EnsureStreamReady();
            this.hasWritten = true;
            innerStream.WriteByte(value);
        }

        internal void SetInnerStream(Stream stream) {
            innerStream = stream;
            this.hasWritten = false;
        }

        internal void SetStreamReady() {
            streamReady = true;
        }
    }
}
