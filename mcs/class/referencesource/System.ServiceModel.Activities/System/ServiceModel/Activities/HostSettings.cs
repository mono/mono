//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Xml.Linq;

    public sealed class HostSettings
    {
        public HostSettings()
        {
        }

        public bool IncludeExceptionDetailInFaults { get; set; }

        public bool UseNoPersistHandle { get; set; }

        public XName ScopeName { get; set; }
    }
}
