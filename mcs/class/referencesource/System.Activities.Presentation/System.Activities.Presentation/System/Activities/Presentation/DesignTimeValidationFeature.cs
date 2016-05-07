//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Runtime;
    using System.Globalization;
    using System.Activities.Presentation.Validation;
    using System.Activities.Validation;
    using System.Collections.Generic;

    abstract class DesignTimeValidationFeature : Feature
    {
        public override void Initialize(EditingContext context, Type modelType)
        {
            if (modelType != this.ApplyTo)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException (
                    string.Format(CultureInfo.CurrentCulture, SR.DesignTimeValidationFeatureOnlyAppliesToType, this.GetType(), this.ApplyTo, modelType)));
            }

            ValidationService validationService = context.Services.GetService<ValidationService>();
            if (validationService != null)
            {
                validationService.Settings.AdditionalConstraints.Add(this.ApplyTo, this.DesignTimeConstraints);
            }
        }

        protected abstract Type ApplyTo
        {
            get;
        }

        protected abstract IList<Constraint> DesignTimeConstraints
        {
            get;
        }
    }
}
