//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.Runtime;

    sealed class ByeOperationApril2005AsyncResult : ByeOperationAsyncResult<ByeMessageApril2005>
    {
        public ByeOperationApril2005AsyncResult(
            IAnnouncementServiceImplementation announcementServiceImpl,
            ByeMessageApril2005 message, 
            AsyncCallback callback, 
            object state)
            : base(announcementServiceImpl, message, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ByeOperationApril2005AsyncResult>(result);
        }

        protected override bool ValidateContent(ByeMessageApril2005 message)
        {
            return (message.Bye != null);
        }

        protected override DiscoveryMessageSequence GetMessageSequence(ByeMessageApril2005 message)
        {
            return DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(message.MessageSequence);
        }

        protected override EndpointDiscoveryMetadata GetEndpointDiscoveryMetadata(ByeMessageApril2005 message)
        {
            return message.Bye.ToEndpointDiscoveryMetadata();
        }
    }
}
