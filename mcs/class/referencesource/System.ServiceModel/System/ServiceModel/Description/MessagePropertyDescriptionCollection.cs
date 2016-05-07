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

    public class MessagePropertyDescriptionCollection : KeyedCollection<string, MessagePropertyDescription>
    {
        internal MessagePropertyDescriptionCollection() : base(null, 4)
        {

        }

        protected override string GetKeyForItem(MessagePropertyDescription item)
        {
            return item.Name;
        }
    }
}
