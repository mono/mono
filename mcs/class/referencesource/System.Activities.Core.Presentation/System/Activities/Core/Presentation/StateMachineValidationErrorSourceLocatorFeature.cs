// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Validation;
    using System.Activities.Statements;
    using System.Collections.Generic;

    internal class StateMachineValidationErrorSourceLocatorFeature : ValidationErrorSourceLocatorFeature
    {
        protected override IValidationErrorSourceLocator ValidationErrorSourceLocator
        {
            get { return new StateMachineValidationErrorSourceLocator(); }
        }
    }
}
