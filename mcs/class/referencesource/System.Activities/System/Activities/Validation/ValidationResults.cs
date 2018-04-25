//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Validation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class ValidationResults
    {
        ReadOnlyCollection<ValidationError> allValidationErrors;
        ReadOnlyCollection<ValidationError> errors;
        ReadOnlyCollection<ValidationError> warnings;
        bool processedAllValidationErrors;

        public ValidationResults(IList<ValidationError> allValidationErrors)
        {
            if (allValidationErrors == null)
            {
                this.allValidationErrors = ActivityValidationServices.EmptyValidationErrors;
            }
            else
            {
                this.allValidationErrors = new ReadOnlyCollection<ValidationError>(allValidationErrors);
            }
        }

        public ReadOnlyCollection<ValidationError> Errors
        {
            get
            {
                if (!this.processedAllValidationErrors)
                {
                    ProcessAllValidationErrors();
                }

                return this.errors;
            }
        }

        public ReadOnlyCollection<ValidationError> Warnings
        {
            get
            {
                if (!this.processedAllValidationErrors)
                {
                    ProcessAllValidationErrors();
                }

                return this.warnings;
            }
        }

        void ProcessAllValidationErrors()
        {
            if (this.allValidationErrors.Count == 0)
            {
                this.errors = ActivityValidationServices.EmptyValidationErrors;
                this.warnings = ActivityValidationServices.EmptyValidationErrors;
            }
            else
            {
                IList<ValidationError> warningsList = null;
                IList<ValidationError> errorsList = null;

                for (int i = 0; i < this.allValidationErrors.Count; i++)
                {
                    ValidationError violation = this.allValidationErrors[i];

                    if (violation.IsWarning)
                    {
                        if (warningsList == null)
                        {
                            warningsList = new Collection<ValidationError>();
                        }

                        warningsList.Add(violation);
                    }
                    else
                    {
                        if (errorsList == null)
                        {
                            errorsList = new Collection<ValidationError>();
                        }

                        errorsList.Add(violation);
                    }
                }

                if (warningsList == null)
                {
                    this.warnings = ActivityValidationServices.EmptyValidationErrors;
                }
                else
                {
                    this.warnings = new ReadOnlyCollection<ValidationError>(warningsList);
                }

                if (errorsList == null)
                {
                    this.errors = ActivityValidationServices.EmptyValidationErrors;
                }
                else
                {
                    this.errors = new ReadOnlyCollection<ValidationError>(errorsList);
                }
            }

            this.processedAllValidationErrors = true;
        }
    }
}
