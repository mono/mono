//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.Runtime;

    // Enforces an overall timeout based on the TimeoutHelper passed in
    class TimeoutStream : DelegatingStream
    {
        TimeoutHelper timeoutHelper;
        public TimeoutStream(Stream stream, ref TimeoutHelper timeoutHelper)
            : base(stream)
        {
            if (!stream.CanTimeout)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("stream", SR.GetString(SR.StreamDoesNotSupportTimeout));
            }

            this.timeoutHelper = timeoutHelper;
        }

        void UpdateReadTimeout()
        {
            this.ReadTimeout = TimeoutHelper.ToMilliseconds(this.timeoutHelper.RemainingTime());
        }

        void UpdateWriteTimeout()
        {
            this.WriteTimeout = TimeoutHelper.ToMilliseconds(this.timeoutHelper.RemainingTime());
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            UpdateReadTimeout();
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            UpdateWriteTimeout();
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            UpdateReadTimeout();
            return base.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            UpdateReadTimeout();
            return base.ReadByte();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            UpdateWriteTimeout();
            base.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            UpdateWriteTimeout();
            base.WriteByte(value);
        }
    }
}
