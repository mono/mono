//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    public sealed class UnderstoodHeaders : IEnumerable<MessageHeaderInfo>
    {
        MessageHeaders messageHeaders;
        bool modified;

        internal UnderstoodHeaders(MessageHeaders messageHeaders, bool modified)
        {
            this.messageHeaders = messageHeaders;
            this.modified = modified;
        }

        internal bool Modified
        {
            get { return modified; }
            set { modified = value; }
        }

        public void Add(MessageHeaderInfo headerInfo)
        {
            messageHeaders.AddUnderstood(headerInfo);
            modified = true;
        }

        public bool Contains(MessageHeaderInfo headerInfo)
        {
            return messageHeaders.IsUnderstood(headerInfo);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<MessageHeaderInfo> GetEnumerator()
        {
            return messageHeaders.GetUnderstoodEnumerator();
        }

        public void Remove(MessageHeaderInfo headerInfo)
        {
            messageHeaders.RemoveUnderstood(headerInfo);
            modified = true;
        }
    }
}
