//-----------------------------------------------------------------------------
// <copyright file="MimeWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mime
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Specialized;

    /// <summary>
    /// Provides an abstraction for writing a MIME multi-part
    /// message.
    /// </summary>
    internal class MimeWriter:BaseWriter
    {
        private static byte[] DASHDASH = new byte[] { (byte)'-', (byte)'-' };

        private byte[] boundaryBytes;
        private bool writeBoundary = true;

        internal MimeWriter(Stream stream, string boundary)
            : base(stream, false) // Unnecessary, the underlying MailWriter stream already encodes dots
        {
            if (boundary == null)
                throw new ArgumentNullException("boundary");

            this.boundaryBytes = Encoding.ASCII.GetBytes(boundary);
        }

        internal override void WriteHeaders(NameValueCollection headers, bool allowUnicode)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            foreach (string key in headers)
                WriteHeader(key, headers[key], allowUnicode);
        }

        #region Cleanup

        internal IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            MultiAsyncResult multiResult = new MultiAsyncResult(this, callback, state);

            Close(multiResult);

            multiResult.CompleteSequence();

            return multiResult;
        }

        internal void EndClose(IAsyncResult result)
        {
            MultiAsyncResult.End(result);

            this.stream.Close();
        }

        internal override void Close()
        {
            Close(null);

            this.stream.Close();
        }

        void Close(MultiAsyncResult multiResult)
        {
            this.bufferBuilder.Append(CRLF);
            this.bufferBuilder.Append(DASHDASH);
            this.bufferBuilder.Append(this.boundaryBytes);
            this.bufferBuilder.Append(DASHDASH);
            this.bufferBuilder.Append(CRLF);
            Flush(multiResult);
        }

        /// <summary>
        /// Called when the current stream is closed.  Allows us to 
        /// prepare for the next message part.
        /// </summary>
        /// <param name="sender">Sender of the close event</param>
        /// <param name="args">Event args (not used)</param>
        protected override void OnClose(object sender, EventArgs args)
        {
            if (this.contentStream != sender)
                return; // may have called WriteHeader

            this.contentStream.Flush();
            this.contentStream = null;
            this.writeBoundary = true;

            this.isInContent = false;
        }

        #endregion Cleanup

        /// <summary>
        /// Writes the boundary sequence if required.
        /// </summary>
        protected override void CheckBoundary()
        {
            if (this.writeBoundary)
            {
                this.bufferBuilder.Append(CRLF);
                this.bufferBuilder.Append(DASHDASH);
                this.bufferBuilder.Append(this.boundaryBytes);
                this.bufferBuilder.Append(CRLF);
                this.writeBoundary = false;
            }
        }
    }
}
