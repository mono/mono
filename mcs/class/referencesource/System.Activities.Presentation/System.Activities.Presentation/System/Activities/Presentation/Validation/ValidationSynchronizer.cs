// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Validation
{
    internal abstract class ValidationSynchronizer
    {
        internal abstract void Validate(ValidationReason reason);

        internal abstract void DeactivateValidation();

        internal abstract void ActivateValidation();
    }
}
