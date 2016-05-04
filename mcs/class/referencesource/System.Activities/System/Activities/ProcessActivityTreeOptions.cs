//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Activities.Validation;
    using System.Runtime;
    using System.Threading;

    class ProcessActivityTreeOptions
    {
        static ProcessActivityTreeOptions validationOptions;
        static ProcessActivityTreeOptions validationAndPrepareForRuntimeOptions;        
        static ProcessActivityTreeOptions singleLevelValidationOptions;        
        static ProcessActivityTreeOptions fullCachingOptions;
        static ProcessActivityTreeOptions dynamicUpdateOptions;
        static ProcessActivityTreeOptions dynamicUpdateOptionsForImplementation;
        static ProcessActivityTreeOptions finishCachingSubtreeOptionsWithCreateEmptyBindings;
        static ProcessActivityTreeOptions finishCachingSubtreeOptionsWithoutCreateEmptyBindings;
        static ProcessActivityTreeOptions skipRootFinishCachingSubtreeOptions;
        static ProcessActivityTreeOptions skipRootConfigurationValidationOptions;
        static ProcessActivityTreeOptions singleLevelSkipRootConfigurationValidationOptions;

        ProcessActivityTreeOptions()
        {
        }

        public CancellationToken CancellationToken
        {
            get;
            private set;
        }

        public bool SkipIfCached
        {
            get;
            private set;
        }

        public bool CreateEmptyBindings
        {
            get;
            private set;
        }

        public bool SkipPrivateChildren
        {
            get;
            private set;
        }

        public bool OnlyCallCallbackForDeclarations
        {
            get;
            private set;
        }

        public bool SkipConstraints
        {
            get;
            private set;
        }

        public bool OnlyVisitSingleLevel
        {
            get;
            private set;
        }

        public bool SkipRootConfigurationValidation
        {
            get;
            private set;
        }

        public bool StoreTempViolations
        {
            get;
            private set;
        }

        public bool IsRuntimeReadyOptions
        {
            get
            {
                // We don't really support progressive caching at runtime so we only set ourselves
                // as runtime ready if we cached the whole workflow and created empty bindings.
                // In order to support progressive caching we need to deal with the following
                // issues:
                //   * We need a mechanism for supporting activities which supply extensions
                //   * We need to understand when we haven't created empty bindings so that
                //     we can progressively create them
                return !this.SkipPrivateChildren && this.CreateEmptyBindings;
            }
        }

        public static ProcessActivityTreeOptions FullCachingOptions
        {
            get
            {
                if (fullCachingOptions == null)
                {
                    fullCachingOptions = new ProcessActivityTreeOptions
                    {
                        SkipIfCached = true,
                        CreateEmptyBindings = true,
                        OnlyCallCallbackForDeclarations = true
                    };
                }

                return fullCachingOptions;
            }
        }

        public static ProcessActivityTreeOptions ValidationOptions
        {
            get
            {
                if (validationOptions == null)
                {
                    validationOptions = new ProcessActivityTreeOptions
                    {
                        SkipPrivateChildren = false,
                        // We don't want to interfere with activities doing null-checks
                        // by creating empty bindings.
                        CreateEmptyBindings = false
                    };
                }

                return validationOptions;
            }
        }

        public static ProcessActivityTreeOptions ValidationAndPrepareForRuntimeOptions
        {
            get
            {
                if (validationAndPrepareForRuntimeOptions == null)
                {
                    validationAndPrepareForRuntimeOptions = new ProcessActivityTreeOptions
                    {
                        SkipIfCached = false,
                        SkipPrivateChildren = false,                        
                        CreateEmptyBindings = true,
                    };
                }

                return validationAndPrepareForRuntimeOptions;
            }
        }

        static ProcessActivityTreeOptions SkipRootConfigurationValidationOptions
        {
            get
            {
                if (skipRootConfigurationValidationOptions == null)
                {
                    skipRootConfigurationValidationOptions = new ProcessActivityTreeOptions
                    {
                        SkipPrivateChildren = false,
                        // We don't want to interfere with activities doing null-checks
                        // by creating empty bindings.
                        CreateEmptyBindings = false,
                        SkipRootConfigurationValidation = true

                    };
                }

                return skipRootConfigurationValidationOptions;
            }
        }

        static ProcessActivityTreeOptions SingleLevelSkipRootConfigurationValidationOptions
        {
            get
            {
                if (singleLevelSkipRootConfigurationValidationOptions == null)
                {
                    singleLevelSkipRootConfigurationValidationOptions = new ProcessActivityTreeOptions
                    {
                        SkipPrivateChildren = false,
                        // We don't want to interfere with activities doing null-checks
                        // by creating empty bindings.
                        CreateEmptyBindings = false,
                        SkipRootConfigurationValidation = true,
                        OnlyVisitSingleLevel = true
                    };
                }

                return singleLevelSkipRootConfigurationValidationOptions;
            }
        }

        static ProcessActivityTreeOptions SingleLevelValidationOptions
        {
            get
            {
                if (singleLevelValidationOptions == null)
                {
                    singleLevelValidationOptions = new ProcessActivityTreeOptions
                    {
                        SkipPrivateChildren = false,
                        // We don't want to interfere with activities doing null-checks
                        // by creating empty bindings.
                        CreateEmptyBindings = false,
                        OnlyVisitSingleLevel = true
                    };
                }

                return singleLevelValidationOptions;
            }
        }

        static ProcessActivityTreeOptions FinishCachingSubtreeOptionsWithoutCreateEmptyBindings
        {
            get
            {
                if (finishCachingSubtreeOptionsWithoutCreateEmptyBindings == null)
                {
                    // We don't want to run constraints and we only want to hit
                    // the public path.
                    finishCachingSubtreeOptionsWithoutCreateEmptyBindings = new ProcessActivityTreeOptions
                    {
                        SkipConstraints = true,
                        StoreTempViolations = true
                    };
                }

                return finishCachingSubtreeOptionsWithoutCreateEmptyBindings;
            }
        }

        static ProcessActivityTreeOptions SkipRootFinishCachingSubtreeOptions
        {
            get
            {
                if (skipRootFinishCachingSubtreeOptions == null)
                {
                    // We don't want to run constraints and we only want to hit
                    // the public path.
                    skipRootFinishCachingSubtreeOptions = new ProcessActivityTreeOptions
                    {
                        SkipConstraints = true,
                        SkipRootConfigurationValidation = true,
                        StoreTempViolations = true
                    };
                }

                return skipRootFinishCachingSubtreeOptions;
            }
        }

        static ProcessActivityTreeOptions FinishCachingSubtreeOptionsWithCreateEmptyBindings
        {
            get
            {
                if (finishCachingSubtreeOptionsWithCreateEmptyBindings == null)
                {
                    // We don't want to run constraints and we only want to hit
                    // the public path.
                    finishCachingSubtreeOptionsWithCreateEmptyBindings = new ProcessActivityTreeOptions
                    {
                        SkipConstraints = true,
                        CreateEmptyBindings = true,
                        StoreTempViolations = true
                    };
                }

                return finishCachingSubtreeOptionsWithCreateEmptyBindings;
            }
        }

        public static ProcessActivityTreeOptions DynamicUpdateOptions
        {
            get
            {
                if (dynamicUpdateOptions == null)
                {
                    dynamicUpdateOptions = new ProcessActivityTreeOptions
                    {
                        OnlyCallCallbackForDeclarations = true,
                        SkipConstraints = true,
                    };
                }

                return dynamicUpdateOptions;
            }
        }

        public static ProcessActivityTreeOptions DynamicUpdateOptionsForImplementation
        {
            get
            {
                if (dynamicUpdateOptionsForImplementation == null)
                {
                    dynamicUpdateOptionsForImplementation = new ProcessActivityTreeOptions
                    {
                        SkipRootConfigurationValidation = true,
                        OnlyCallCallbackForDeclarations = true,
                        SkipConstraints = true,
                    };
                }

                return dynamicUpdateOptionsForImplementation;
            }
        }

        public static ProcessActivityTreeOptions GetFinishCachingSubtreeOptions(ProcessActivityTreeOptions originalOptions)
        {
            ProcessActivityTreeOptions result;
            if (originalOptions.CreateEmptyBindings)
            {
                Fx.Assert(!originalOptions.SkipRootConfigurationValidation, "If we ever add code that uses this combination of options, " +
                    "we need a new predefined setting on ProcessActivityTreeOptions.");
                result = ProcessActivityTreeOptions.FinishCachingSubtreeOptionsWithCreateEmptyBindings;
            }
            else
            {
                if (originalOptions.SkipRootConfigurationValidation)
                {
                    result = ProcessActivityTreeOptions.SkipRootFinishCachingSubtreeOptions;
                }
                else
                {
                    result = ProcessActivityTreeOptions.FinishCachingSubtreeOptionsWithoutCreateEmptyBindings;
                }
            }

            if (originalOptions.CancellationToken == CancellationToken.None)
            {
                return result;
            }
            else
            {
                return AttachCancellationToken(result, originalOptions.CancellationToken);
            }
        }

        public static ProcessActivityTreeOptions GetValidationOptions(ValidationSettings settings)
        {
            ProcessActivityTreeOptions result = null;
            if (settings.SkipValidatingRootConfiguration && settings.SingleLevel)
            {
                result = ProcessActivityTreeOptions.SingleLevelSkipRootConfigurationValidationOptions;
            }
            else if (settings.SkipValidatingRootConfiguration)
            {
                result = ProcessActivityTreeOptions.SkipRootConfigurationValidationOptions;
            }
            else if (settings.SingleLevel)
            {
                result = ProcessActivityTreeOptions.SingleLevelValidationOptions;
            }
            else if (settings.PrepareForRuntime)
            {
                Fx.Assert(!settings.SkipValidatingRootConfiguration && !settings.SingleLevel && !settings.OnlyUseAdditionalConstraints, "PrepareForRuntime cannot be set at the same time any of the three is set.");
                result = ProcessActivityTreeOptions.ValidationAndPrepareForRuntimeOptions;
            }
            else
            {
                result = ProcessActivityTreeOptions.ValidationOptions;
            }
            if (settings.CancellationToken == CancellationToken.None)
            {
                return result;
            }
            else
            {
                return AttachCancellationToken(result, settings.CancellationToken);
            }
        }

        static ProcessActivityTreeOptions AttachCancellationToken(ProcessActivityTreeOptions result, CancellationToken cancellationToken)
        {
            ProcessActivityTreeOptions clone = result.Clone();
            clone.CancellationToken = cancellationToken;
            return clone;
        }

        ProcessActivityTreeOptions Clone()
        {
            return new ProcessActivityTreeOptions
            {
                CancellationToken = this.CancellationToken,
                SkipIfCached = this.SkipIfCached,
                CreateEmptyBindings = this.CreateEmptyBindings,
                SkipPrivateChildren = this.SkipPrivateChildren,
                OnlyCallCallbackForDeclarations = this.OnlyCallCallbackForDeclarations,
                SkipConstraints = this.SkipConstraints,
                OnlyVisitSingleLevel = this.OnlyVisitSingleLevel,
                SkipRootConfigurationValidation = this.SkipRootConfigurationValidation,
                StoreTempViolations = this.StoreTempViolations,
            };
        }
    }
}
