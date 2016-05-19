//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    // These are information passed from CallbackContextMessageProperty.
    [DataContract]
    class CorrelationCallbackContext 
    {
        [DataMember]
        public EndpointAddress10 ListenAddress
        {
            get;
            set;
        }

        [DataMember]
        public IDictionary<string, string> Context
        {
            get;
            set;
        }
      
    }
}
