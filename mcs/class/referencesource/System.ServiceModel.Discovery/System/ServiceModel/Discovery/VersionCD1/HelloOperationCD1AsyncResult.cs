//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime;

    sealed class HelloOperationCD1AsyncResult : HelloOperationAsyncResult<HelloMessageCD1>
    {
        public HelloOperationCD1AsyncResult(
            IAnnouncementServiceImplementation announcementServiceImpl,
            HelloMessageCD1 message,
            AsyncCallback callback,
            object state)
            : base(announcementServiceImpl, message, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<HelloOperationCD1AsyncResult>(result);
        }

        protected override bool ValidateContent(HelloMessageCD1 message)
        {
            return (message.Hello != null);
        }

        protected override DiscoveryMessageSequence GetMessageSequence(HelloMessageCD1 message)
        {
            return DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(message.MessageSequence);
        }

        protected override EndpointDiscoveryMetadata GetEndpointDiscoveryMetadata(HelloMessageCD1 message)
        {
            return message.Hello.ToEndpointDiscoveryMetadata();
        }
    }
}
