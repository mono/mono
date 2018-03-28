//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Xml.XPath;

    [Serializable]
    public class MessageFilterException : CommunicationException
    {
        [NonSerialized]
        Collection<MessageFilter> filters;
        
        protected MessageFilterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.filters = null;
        }
        
        public MessageFilterException()
        {
        }
        
        public MessageFilterException(string message)
            : this(message, null, null)
        {
        }
        
        public MessageFilterException(string message, Exception innerException)
            : this(message, innerException, null)
        {
        }

        public MessageFilterException(string message, Collection<MessageFilter> filters)
            : this(message, null, filters)
        {
        }
        
        public MessageFilterException(string message, Exception innerException, Collection<MessageFilter> filters)
            : base(message, innerException)
        {
            this.filters = filters;
        }
        
        public Collection<MessageFilter> Filters
        {
            get
            {
                return this.filters;
            }
        }
    }
}
