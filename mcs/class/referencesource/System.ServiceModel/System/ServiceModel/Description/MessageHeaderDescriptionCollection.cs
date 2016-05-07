//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.Runtime.Serialization;

    public class MessageHeaderDescriptionCollection : KeyedCollection<XmlQualifiedName, MessageHeaderDescription>
    {
        internal MessageHeaderDescriptionCollection() : base(null, 4)
        {

        }

        protected override XmlQualifiedName GetKeyForItem(MessageHeaderDescription item)
        {
            return new XmlQualifiedName(item.Name, item.Namespace);
        }
    }
}
