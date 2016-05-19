//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml;

    [Serializable]
    public class RedirectionLocation
    {
        // For Serialization
        private RedirectionLocation() { }

        public RedirectionLocation(Uri address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            if (!address.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("address", SR.GetString(SR.UriMustBeAbsolute));
            }

            //Xml schema anyUri can be either relative or absolute...
            this.Address = address;
        }

        public Uri Address { get; private set; }
    }
}
