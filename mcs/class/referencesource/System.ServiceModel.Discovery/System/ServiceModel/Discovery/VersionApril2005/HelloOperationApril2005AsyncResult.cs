//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.Runtime;

    sealed class HelloOperationApril2005AsyncResult : HelloOperationAsyncResult<HelloMessageApril2005>
    {
        public HelloOperationApril2005AsyncResult(
            IAnnouncementServiceImplementation announcementServiceImpl,
            HelloMessageApril2005 message, 
            AsyncCallback callback, 
            object state)
            : base(announcementServiceImpl, message, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<HelloOperationApril2005AsyncResult>(result);
        }

        protected override bool ValidateContent(HelloMessageApril2005 message)
        {
            return (message.Hello != null);
        }

        protected override DiscoveryMessageSequence GetMessageSequence(HelloMessageApril2005 message)
        {
            return DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(message.MessageSequence);
        }

        protected override EndpointDiscoveryMetadata GetEndpointDiscoveryMetadata(HelloMessageApril2005 message)
        {
            return message.Hello.ToEndpointDiscoveryMetadata();
        }
    }
}
