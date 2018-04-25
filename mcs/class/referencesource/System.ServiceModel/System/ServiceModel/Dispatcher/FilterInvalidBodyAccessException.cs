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
    public class FilterInvalidBodyAccessException : InvalidBodyAccessException
    {
        [NonSerialized]
        Collection<MessageFilter> filters;
        
        protected FilterInvalidBodyAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.filters = null;
        }
        
        public FilterInvalidBodyAccessException()
            : this(SR.GetString(SR.SeekableMessageNavBodyForbidden))
        {
        }
        
        public FilterInvalidBodyAccessException(string message)
            : this(message, null, null)
        {
        }
        
        public FilterInvalidBodyAccessException(string message, Exception innerException)
            : this(message, innerException, null)
        {
        }

        public FilterInvalidBodyAccessException(string message, Collection<MessageFilter> filters)
            : this(message, null, filters)
        {
        }
        
        public FilterInvalidBodyAccessException(string message, Exception innerException, Collection<MessageFilter> filters)
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
