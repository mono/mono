//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;
    using System.ServiceModel.Security;
    using System.Text;
    using System.ComponentModel;
    using System.ServiceModel.Channels;

    public partial class MexTcpBindingElement : MexBindingElement<CustomBinding>
    {
        public MexTcpBindingElement(string name)
            : base(name)
        {
        }

        public MexTcpBindingElement()
            : this(null)
        {
        }
    }
}
