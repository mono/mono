//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.Runtime.Serialization;

    public class MessagePartDescriptionCollection : KeyedCollection<XmlQualifiedName, MessagePartDescription>
    {
        internal MessagePartDescriptionCollection()
            : base(null, 4)
        {

        }

        protected override XmlQualifiedName GetKeyForItem(MessagePartDescription item)
        {
            return new XmlQualifiedName(item.Name, item.Namespace);
        }
    }
}
