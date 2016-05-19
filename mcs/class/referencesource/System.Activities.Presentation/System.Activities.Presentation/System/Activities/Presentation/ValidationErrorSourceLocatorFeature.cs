// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Validation;
    using System.Collections.Generic;
    using System.Runtime;

    internal abstract class ValidationErrorSourceLocatorFeature : Feature
    {
        protected abstract IValidationErrorSourceLocator ValidationErrorSourceLocator
        {
            get;
        }

        public override void Initialize(EditingContext context, Type modelType)
        {
            Fx.Assert(context != null, "Context should not be null.");
            Fx.Assert(modelType != null, "modelType should not be null.");
            ValidationService validationService = context.Services.GetRequiredService<ValidationService>();
            validationService.RegisterValidationErrorSourceLocator(modelType, this.ValidationErrorSourceLocator);
        }
    }
}
