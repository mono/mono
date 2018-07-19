//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.ServiceModel;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    class MaxMessageSizeStream : DelegatingStream
    {
        long maxMessageSize;
        long totalBytesRead;
        long bytesWritten;

        public MaxMessageSizeStream(Stream stream, long maxMessageSize)
            : base(stream)
        {
            this.maxMessageSize = maxMessageSize;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            count = PrepareRead(count);
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            PrepareWrite(count);
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult result)
        {
            return FinishRead(base.EndRead(result));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = PrepareRead(count);
            return FinishRead(base.Read(buffer, offset, count));
        }

        public override int ReadByte()
        {
            PrepareRead(1);
            int i = base.ReadByte();
            if (i != -1)
                FinishRead(1);
            return i;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            PrepareWrite(count);
            base.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            PrepareWrite(1);
            base.WriteByte(value);
        }

        internal static Exception CreateMaxReceivedMessageSizeExceededException(long maxMessageSize)
        {
            string message = SR.GetString(SR.MaxReceivedMessageSizeExceeded, maxMessageSize);
            Exception inner = new QuotaExceededException(message);

            if (TD.MaxReceivedMessageSizeExceededIsEnabled())
            {
                TD.MaxReceivedMessageSizeExceeded(message);
            }

            return new CommunicationException(message, inner);
        }

        internal static Exception CreateMaxSentMessageSizeExceededException(long maxMessageSize)
        {
            string message = SR.GetString(SR.MaxSentMessageSizeExceeded, maxMessageSize);
            Exception inner = new QuotaExceededException(message);

            if (TD.MaxSentMessageSizeExceededIsEnabled())
            {
                TD.MaxSentMessageSizeExceeded(message);
            }

            return new CommunicationException(message, inner);
        }

        int PrepareRead(int bytesToRead)
        {
            if (totalBytesRead >= maxMessageSize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateMaxReceivedMessageSizeExceededException(maxMessageSize));
            }

            long bytesRemaining = maxMessageSize - totalBytesRead;

            if (bytesRemaining > int.MaxValue)
            {
                return bytesToRead;
            }
            else
            {
                return Math.Min(bytesToRead, (int)(maxMessageSize - totalBytesRead));
            }
        }

        int FinishRead(int bytesRead)
        {
            totalBytesRead += bytesRead;
            return bytesRead;
        }

        void PrepareWrite(int bytesToWrite)
        {
            if (bytesWritten + bytesToWrite > maxMessageSize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateMaxSentMessageSizeExceededException(maxMessageSize));
            }

            bytesWritten += bytesToWrite;
        }
    }
}
