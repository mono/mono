//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Xml;

    class XmlBufferedByteStreamReader : XmlByteStreamReader
    {
        ByteStreamBufferedMessageData bufferedMessageData; 
        int offset;
        static byte[] emptyByteArray = new byte[0];

        public XmlBufferedByteStreamReader(ByteStreamBufferedMessageData bufferedMessageData, XmlDictionaryReaderQuotas quotas) : base (quotas)
        {
            Fx.Assert(bufferedMessageData != null, "bufferedMessageData is null");
            this.bufferedMessageData = bufferedMessageData;
            this.bufferedMessageData.Open(); 

            this.offset = bufferedMessageData.Buffer.Offset;
            this.quotas = quotas;
            this.position = ReaderPosition.None;
        }

        protected override void OnClose()
        {
            this.bufferedMessageData.Close();
            this.bufferedMessageData = null; 
            this.offset = 0;
            base.OnClose();
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            EnsureInContent();
            ByteStreamMessageUtility.EnsureByteBoundaries(buffer, index, count, true);

            if (count == 0)
            {
                return 0; 
            }

            int bytesToCopy = Math.Min(bufferedMessageData.Buffer.Count - this.offset, count);

            if (bytesToCopy == 0)
            {
                this.position = ReaderPosition.EndElement;
                return 0; 
            }

            Buffer.BlockCopy(this.bufferedMessageData.Buffer.Array, this.offset, buffer, index, bytesToCopy);
            this.offset += bytesToCopy;
            
            return bytesToCopy;
        }

        protected override byte[] OnToByteArray()
        {
            int bytesToCopy = bufferedMessageData.Buffer.Count;
            byte[] buffer = new byte[bytesToCopy];
            Buffer.BlockCopy(this.bufferedMessageData.Buffer.Array, this.bufferedMessageData.Buffer.Offset, buffer, 0, bytesToCopy);
            return buffer;
        }

        protected override Stream OnToStream()
        {
            return this.bufferedMessageData.ToStream();
        }

        public override bool TryGetBase64ContentLength(out int length)
        {
            if (!this.IsClosed)
            {
                // in ByteStream encoder, we're not concerned about individual xml nodes
                // therefore we can just return the entire segment of the buffer we're using in this reader.
                length = bufferedMessageData.Buffer.Count;
                return true;
            }
            length = -1; 
            return false;
        }
    }
}
