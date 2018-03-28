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
    [KnownType(typeof(string[]))]
    public class XPathNavigatorException : XPathException
    {
        protected XPathNavigatorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        public XPathNavigatorException()
        {
        }
        
        public XPathNavigatorException(string message)
            : this(message, null)
        {
        }
        
        public XPathNavigatorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        
        internal MessageFilterException Process(Opcode op)
        {
            Collection<MessageFilter> list = new Collection<MessageFilter>();
            op.CollectXPathFilters(list);
            return new MessageFilterException(this.Message, this.InnerException, list);
        }
    }
}
