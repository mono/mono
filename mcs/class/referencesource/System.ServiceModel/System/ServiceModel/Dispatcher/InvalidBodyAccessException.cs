//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Xml.XPath;

    [Serializable]
    public abstract class InvalidBodyAccessException : SystemException
    {
        protected InvalidBodyAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        protected InvalidBodyAccessException(string message)
            : this(message, null)
        {
        }
        
        protected InvalidBodyAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
