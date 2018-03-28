//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Runtime;

    sealed class ByeOperation11AsyncResult : ByeOperationAsyncResult<ByeMessage11>
    {
        public ByeOperation11AsyncResult(
            IAnnouncementServiceImplementation announcementServiceImpl,
            ByeMessage11 message, 
            AsyncCallback callback, 
            object state)
            : base(announcementServiceImpl, message, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ByeOperation11AsyncResult>(result);
        }

        protected override bool ValidateContent(ByeMessage11 message)
        {
            return (message.Bye != null);
        }

        protected override DiscoveryMessageSequence GetMessageSequence(ByeMessage11 message)
        {
            return DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(message.MessageSequence);
        }

        protected override EndpointDiscoveryMetadata GetEndpointDiscoveryMetadata(ByeMessage11 message)
        {
            return message.Bye.ToEndpointDiscoveryMetadata();
        }
    }
}
