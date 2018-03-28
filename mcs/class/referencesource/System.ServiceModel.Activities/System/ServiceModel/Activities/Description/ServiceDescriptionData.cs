//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System;

    class ServiceDescriptionData 
    {
        public ServiceDescriptionData() { }

        public bool IsInsideTransactedReceiveScope 
        { 
            get; 
            set; 
        }

        //This is the first receive of the transacted receive scope tree
        //i.e. in a nested TRS scenario, this tells if the receive is one associated with the
        //Request property of the outermost TRS or not
        public bool IsFirstReceiveOfTransactedReceiveScopeTree
        {
            get;
            set;
        }
    }
}
