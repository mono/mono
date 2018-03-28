//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Xml;

    abstract class BufferedMessageData : IBufferedMessageData
    {
        ArraySegment<byte> buffer;
        BufferManager bufferManager;
        int refCount;
        int outstandingReaders;
        bool multipleUsers;
        RecycledMessageState messageState;
        SynchronizedPool<RecycledMessageState> messageStatePool;

        public BufferedMessageData(SynchronizedPool<RecycledMessageState> messageStatePool)
        {
            this.messageStatePool = messageStatePool;
        }

        public ArraySegment<byte> Buffer
        {
            get { return buffer; }
        }

        public BufferManager BufferManager
        {
            get { return bufferManager; }
        }

        public virtual XmlDictionaryReaderQuotas Quotas
        {
            get { return XmlDictionaryReaderQuotas.Max; }
        }

        public abstract MessageEncoder MessageEncoder { get; }

        object ThisLock
        {
            get { return this; }
        }

        public void EnableMultipleUsers()
        {
            multipleUsers = true;
        }

        public void Close()
        {
            if (multipleUsers)
            {
                lock (ThisLock)
                {
                    if (--this.refCount == 0)
                    {
                        DoClose();
                    }
                }
            }
            else
            {
                DoClose();
            }
        }

        void DoClose()
        {
            bufferManager.ReturnBuffer(buffer.Array);
            if (outstandingReaders == 0)
            {
                bufferManager = null;
                buffer = new ArraySegment<byte>();
                OnClosed();
            }
        }

        public void DoReturnMessageState(RecycledMessageState messageState)
        {
            if (this.messageState == null)
            {
                this.messageState = messageState;
            }
            else
            {
                messageStatePool.Return(messageState);
            }
        }

        void DoReturnXmlReader(XmlDictionaryReader reader)
        {
            ReturnXmlReader(reader);
            outstandingReaders--;
        }

        public RecycledMessageState DoTakeMessageState()
        {
            RecycledMessageState messageState = this.messageState;
            if (messageState != null)
            {
                this.messageState = null;
                return messageState;
            }
            else
            {
                return messageStatePool.Take();
            }
        }

        XmlDictionaryReader DoTakeXmlReader()
        {
            XmlDictionaryReader reader = TakeXmlReader();
            outstandingReaders++;
            return reader;
        }

        public XmlDictionaryReader GetMessageReader()
        {
            if (multipleUsers)
            {
                lock (ThisLock)
                {
                    return DoTakeXmlReader();
                }
            }
            else
            {
                return DoTakeXmlReader();
            }
        }

        public void OnXmlReaderClosed(XmlDictionaryReader reader)
        {
            if (multipleUsers)
            {
                lock (ThisLock)
                {
                    DoReturnXmlReader(reader);
                }
            }
            else
            {
                DoReturnXmlReader(reader);
            }
        }

        protected virtual void OnClosed()
        {
        }

        public RecycledMessageState TakeMessageState()
        {
            if (multipleUsers)
            {
                lock (ThisLock)
                {
                    return DoTakeMessageState();
                }
            }
            else
            {
                return DoTakeMessageState();
            }
        }

        protected abstract XmlDictionaryReader TakeXmlReader();

        public void Open()
        {
            lock (ThisLock)
            {
                this.refCount++;
            }
        }

        public void Open(ArraySegment<byte> buffer, BufferManager bufferManager)
        {
            this.refCount = 1;
            this.bufferManager = bufferManager;
            this.buffer = buffer;
            multipleUsers = false;
        }

        protected abstract void ReturnXmlReader(XmlDictionaryReader xmlReader);

        public void ReturnMessageState(RecycledMessageState messageState)
        {
            if (multipleUsers)
            {
                lock (ThisLock)
                {
                    DoReturnMessageState(messageState);
                }
            }
            else
            {
                DoReturnMessageState(messageState);
            }
        }
    }
}
