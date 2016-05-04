// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Validation
{
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;

    // ValidationService is responsible for locating the ModelItem containing the violating activities
    // Individual activity designers can register their ValidationErrorLocator to refine the source
    // (e.g.) StateMachine can use this extensibility point to provide error on state level.
    internal interface IValidationErrorSourceLocator
    {
        List<object> FindSourceDetailFromActivity(Activity errorSource, object errorSourceDetail);

        void ReplaceParentChainWithSource(Activity parentActivity, List<object> parentChain);
    }
}
