// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Validation;

    internal class PickValidationErrorSourceLocatorFeature : ValidationErrorSourceLocatorFeature
    {
        protected override IValidationErrorSourceLocator ValidationErrorSourceLocator
        {
            get { return new PickValidationErrorSourceLocator(); }
        }
    }
}
