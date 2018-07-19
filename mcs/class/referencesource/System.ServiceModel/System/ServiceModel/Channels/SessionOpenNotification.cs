// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    public abstract class SessionOpenNotification
    {
        public abstract bool IsEnabled
        {
            get;
        }

        public abstract void UpdateMessageProperties(MessageProperties inboundMessageProperties);
    }
}
