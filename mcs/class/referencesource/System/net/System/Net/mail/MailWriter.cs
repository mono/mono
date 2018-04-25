//-----------------------------------------------------------------------------
// <copyright file="MailWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mail
{
    using System;
    using System.IO;
    using System.Collections.Specialized;
    using System.Net.Mime;

    internal class MailWriter:BaseWriter
    {
        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="stream">Underlying stream</param>
        internal MailWriter(Stream stream)
            : base(stream, true) 
            // This is the only stream that should encoding leading dots on a line.
            // This way it is done message wide and only once.
        {
        }

        internal override void WriteHeaders(NameValueCollection headers, bool allowUnicode)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            foreach (string key in headers) 
            {
                string[] values = headers.GetValues(key);
                foreach (string value in values)
                    WriteHeader(key, value, allowUnicode);
            }
        }

        /// <summary>
        /// Closes underlying stream.
        /// </summary>
        internal override void Close()
        {
            this.bufferBuilder.Append(CRLF);
            Flush(null);
            this.stream.Close();
        }

        /// <summary>
        /// Called when the current stream is closed.  Allows us to 
        /// prepare for the next message part.
        /// </summary>
        /// <param name="sender">Sender of the close event</param>
        /// <param name="args">Event args (not used)</param>
        protected override void OnClose(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.Assert(this.contentStream == sender);

            this.contentStream.Flush();
            
            this.contentStream = null;
        }
    }
}
