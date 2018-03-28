//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Xml.XPath;

    [Serializable]
    public class NavigatorInvalidBodyAccessException : InvalidBodyAccessException
    {
        protected NavigatorInvalidBodyAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        public NavigatorInvalidBodyAccessException()
            : this(SR.GetString(SR.SeekableMessageNavBodyForbidden))
        {
        }
        
        public NavigatorInvalidBodyAccessException(string message)
            : this(message, null)
        {
        }
        
        public NavigatorInvalidBodyAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal FilterInvalidBodyAccessException Process(Opcode op)
        {
            Collection<MessageFilter> list = new Collection<MessageFilter>();
            op.CollectXPathFilters(list);
            return new FilterInvalidBodyAccessException(this.Message, this.InnerException, list);
        }
    }
}
