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
    
    [Serializable]
    public class MultipleFilterMatchesException : SystemException
    {
        [NonSerialized]
        Collection<MessageFilter> filters;
        
        protected MultipleFilterMatchesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.filters = null;
        }
        
        public MultipleFilterMatchesException()
            : this(SR.GetString(SR.FilterMultipleMatches))
        {
        }

        public MultipleFilterMatchesException(string message)
            : this(message, null, null)
        {
        }

        public MultipleFilterMatchesException(string message, Exception innerException)
            : this(message, innerException, null)
        {
        }

        public MultipleFilterMatchesException(string message, Collection<MessageFilter> filters)
            : this(message, null, filters)
        {
        }

        public MultipleFilterMatchesException(string message, Exception innerException, Collection<MessageFilter> filters)
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
