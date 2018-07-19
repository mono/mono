//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;

    class FlowDecisionLabelFeature : ViewStateAttachedPropertyFeature
    {
        protected override IEnumerable<AttachedPropertyInfo> AttachedProperties
        {
            get
            {
                yield return new AttachedPropertyInfo<string> { PropertyName = "TrueLabel", DefaultValue = SR.FCFlowDecisionTrueMarker };
                yield return new AttachedPropertyInfo<string> { PropertyName = "FalseLabel", DefaultValue = SR.FCFlowDecisionFalseMarker };
            }
        }
    }
}
