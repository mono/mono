//-----------------------------------------------------------------------------
// <copyright file="ClosableStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Provides a stream that notifies an event when the Close method
    /// is called.
    /// </summary>
    internal class ClosableStream : DelegatedStream
    {
        EventHandler onClose;
        int closed;

        internal ClosableStream(Stream stream, EventHandler onClose) : base(stream)
        {
            this.onClose = onClose;
        }

        public override void Close()
        {
            if (Interlocked.Increment(ref closed) == 1)
                if (this.onClose != null)
                    this.onClose(this, new EventArgs());
        }
    }
}
