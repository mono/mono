//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;

    abstract class DetectEofStream : DelegatingStream
    {
        bool isAtEof;

        protected DetectEofStream(Stream stream)
            : base(stream)
        {
            this.isAtEof = false;
        }

        protected bool IsAtEof
        {
            get { return this.isAtEof; }
        }

        public override int EndRead(IAsyncResult result)
        {
            int returnValue = base.EndRead(result);
            if (returnValue == 0)
            {
                ReceivedEof();
            }
            return returnValue;
        }

        public override int ReadByte()
        {
            int returnValue = base.ReadByte();
            if (returnValue == -1)
            {
                ReceivedEof();
            }
            return returnValue;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (isAtEof)
            {
                return 0;
            }
            int returnValue = base.Read(buffer, offset, count);
            if (returnValue == 0)
            {
                ReceivedEof();
            }
            return returnValue;
        }

        void ReceivedEof()
        {
            if (!isAtEof)
            {
                this.isAtEof = true;
                this.OnReceivedEof();
            }
        }

        protected virtual void OnReceivedEof()
        {
        }
    }
}
