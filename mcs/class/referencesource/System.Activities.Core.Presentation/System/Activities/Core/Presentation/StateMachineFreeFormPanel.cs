//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.FreeFormEditing;

    // This is a workaround. Internal type FreeFromPanel cannot be used in xaml even if System.Activities.Presentation is configured to
    // be InternalsVisibleTo System.Activities.Core.Presentation
    internal sealed class StateMachineFreeFormPanel : FreeFormPanel
    {
        public StateMachineFreeFormPanel()
        {
        }
    }
}
