//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;

    public interface ICorrelationDataSource
    {
        ICollection<CorrelationDataDescription> DataSources
        {
            get;
        }
    }
}
