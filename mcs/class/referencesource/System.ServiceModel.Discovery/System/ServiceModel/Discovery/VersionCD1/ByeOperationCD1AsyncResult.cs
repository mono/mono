//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime;

    sealed class ByeOperationCD1AsyncResult : ByeOperationAsyncResult<ByeMessageCD1>
    {
        public ByeOperationCD1AsyncResult(
            IAnnouncementServiceImplementation announcementServiceImpl,
            ByeMessageCD1 message, 
            AsyncCallback callback, 
            object state)
            : base(announcementServiceImpl, message, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ByeOperationCD1AsyncResult>(result);
        }

        protected override bool ValidateContent(ByeMessageCD1 message)
        {
            return (message.Bye != null);
        }

        protected override DiscoveryMessageSequence GetMessageSequence(ByeMessageCD1 message)
        {
            return DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(message.MessageSequence);
        }

        protected override EndpointDiscoveryMetadata GetEndpointDiscoveryMetadata(ByeMessageCD1 message)
        {
            return message.Bye.ToEndpointDiscoveryMetadata();
        }
    }
}
