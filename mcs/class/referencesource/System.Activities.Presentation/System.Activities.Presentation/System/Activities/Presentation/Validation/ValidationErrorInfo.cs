//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Activities.Validation;
    using System.Runtime;

    [Serializable]
    [Fx.Tag.XamlVisible(false)]
    public class ValidationErrorInfo
    {
        public ValidationErrorInfo(string message)
        {
            // Used to initialize violations that correspond
            // to exceptions triggered during validation.
            this.Message = message;
            this.PropertyName = string.Empty;
            this.FileName = string.Empty;
            this.IsWarning = false;
        }

        public ValidationErrorInfo(ValidationError validationError)
        {
            this.Id = validationError.Id;
            this.Message = validationError.Message;
            this.PropertyName = validationError.PropertyName;
            this.FileName = string.Empty;
            this.IsWarning = validationError.IsWarning;
        }

        public string Id { get; private set; }

        public Guid SourceReferenceId { get; set; }

        public string Message { get; private set; }

        public string PropertyName { get; private set; }

        public string FileName { get; set; }

        public bool IsWarning { get; private set; }
    }
}
