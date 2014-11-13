//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Runtime;

    sealed class HelloOperation11AsyncResult : HelloOperationAsyncResult<HelloMessage11>
    {
        public HelloOperation11AsyncResult(
            IAnnouncementServiceImplementation announcementServiceImpl,
            HelloMessage11 message, 
            AsyncCallback callback, 
            object state)
            : base(announcementServiceImpl, message, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<HelloOperation11AsyncResult>(result);
        }

        protected override bool ValidateContent(HelloMessage11 message)
        {
            return (message.Hello != null);
        }

        protected override DiscoveryMessageSequence GetMessageSequence(HelloMessage11 message)
        {
            return DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(message.MessageSequence);
        }

        protected override EndpointDiscoveryMetadata GetEndpointDiscoveryMetadata(HelloMessage11 message)
        {
            return message.Hello.ToEndpointDiscoveryMetadata();
        }
    }
}
