// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.Validation;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using Microsoft.Activities.Presentation;
    using Microsoft.VisualBasic.Activities;

    internal class VisualBasicValueValidationFeature : Feature
    {
        private static readonly Type VisualBasicValueType = typeof(VisualBasicValue<>);        

        public override void Initialize(EditingContext context, Type modelType)
        {
            Fx.Assert(
                modelType.IsGenericType && (modelType.GetGenericTypeDefinition() == VisualBasicValueType),
                "This Feature should only apply to VisualBasicValue<>");

            ValidationService validationService = context.Services.GetService<ValidationService>();
            if (validationService != null && WorkflowDesigner.GetTargetFramework(context).IsLessThan45())
            {
                validationService.Settings.AdditionalConstraints.Add(VisualBasicValueType, new List<Constraint> { VisualBasicDesignerHelper.NameShadowingConstraint });                
            }
        }
    }
}
