//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    public sealed class UnknownMessageReceivedEventArgs : EventArgs
    {
        Message message;

        internal UnknownMessageReceivedEventArgs(Message message)
        {
            this.message = message;
        }

        public Message Message
        {
            get { return this.message; }
        }
    }
}
