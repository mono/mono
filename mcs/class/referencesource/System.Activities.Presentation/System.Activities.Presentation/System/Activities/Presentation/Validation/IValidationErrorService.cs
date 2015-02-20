//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Validation
{
    using System.Collections.Generic;

    public interface IValidationErrorService
    {
        void ShowValidationErrors(IList<ValidationErrorInfo> errors);
    }
}
