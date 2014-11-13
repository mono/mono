//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Net.Sockets;
    using System.ServiceModel.Dispatcher;

    [DataContract]
    class ListenerChannelContext
    {
        [DataMember]
        string appKey;

        [DataMember]
        int listenerChannelId;

        [DataMember]
        Guid token;

        internal ListenerChannelContext(string appKey, int listenerChannelId, Guid token)
        {
            this.appKey = appKey;
            this.listenerChannelId = listenerChannelId;
            this.token = token;
        }

        internal string AppKey { get { return appKey; } }
        internal int ListenerChannelId { get { return listenerChannelId; } }
        internal Guid Token { get { return token; } }

        public static ListenerChannelContext Hydrate(byte[] blob)
        {
            using (MemoryStream memoryStream = new MemoryStream(blob))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ListenerChannelContext));
                return (ListenerChannelContext)serializer.ReadObject(memoryStream);
            }
        }

        public byte[] Dehydrate()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ListenerChannelContext));
                serializer.WriteObject(memoryStream, this);
                return memoryStream.ToArray();
            }
        }
    }
}
