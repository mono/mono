//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Runtime;
    using System.Activities.Validation;

    public static class WorkflowInspectionServices
    {
        public static void CacheMetadata(Activity rootActivity)
        {
            CacheMetadata(rootActivity, null);
        }

        public static void CacheMetadata(Activity rootActivity, LocationReferenceEnvironment hostEnvironment)
        {
            if (rootActivity == null)
            {
                throw FxTrace.Exception.ArgumentNull("rootActivity");
            }

            if (rootActivity.HasBeenAssociatedWithAnInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.RootActivityAlreadyAssociatedWithInstance(rootActivity.DisplayName)));
            }

            IList<ValidationError> validationErrors = null;

            if (hostEnvironment == null)
            {
                hostEnvironment = new ActivityLocationReferenceEnvironment();
            }

            ActivityUtilities.CacheRootMetadata(rootActivity, hostEnvironment, ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);

            ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
        }

        public static Activity Resolve(Activity root, string id)
        {
            if (root == null)
            {
                throw FxTrace.Exception.ArgumentNull("root");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("id");
            }

            if (!root.IsMetadataCached)
            {
                IList<ValidationError> validationErrors = null;

                ActivityUtilities.CacheRootMetadata(root, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);

                ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
            }

            QualifiedId parsedId = QualifiedId.Parse(id);

            Activity result;
            if (!QualifiedId.TryGetElementFromRoot(root, parsedId, out result))
            {
                throw FxTrace.Exception.Argument("id", SR.IdNotFoundInWorkflow(id));
            }

            return result;
        }

        public static IEnumerable<Activity> GetActivities(Activity activity)
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }

            if (!activity.IsMetadataCached)
            {
                IList<ValidationError> validationErrors = null;

                ActivityUtilities.CacheRootMetadata(activity, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);

                ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
            }
            
            int i = 0;
            for (; i < activity.RuntimeArguments.Count; i++)
            {
                RuntimeArgument argument = activity.RuntimeArguments[i];

                if (argument.BoundArgument != null && argument.BoundArgument.Expression != null)
                {
                    yield return argument.BoundArgument.Expression;
                }
            }

            for (i = 0; i < activity.RuntimeVariables.Count; i++)
            {
                Variable variable = activity.RuntimeVariables[i];

                if (variable.Default != null)
                {
                    yield return variable.Default;
                }
            }

            for (i = 0; i < activity.ImplementationVariables.Count; i++)
            {
                Variable variable = activity.ImplementationVariables[i];

                if (variable.Default != null)
                {
                    yield return variable.Default;
                }
            }

            for (i = 0; i < activity.Children.Count; i++)
            {
                yield return activity.Children[i];
            }

            for (i = 0; i < activity.ImportedChildren.Count; i++)
            {
                yield return activity.ImportedChildren[i];
            }

            for (i = 0; i < activity.ImplementationChildren.Count; i++)
            {
                yield return activity.ImplementationChildren[i];
            }

            for (i = 0; i < activity.Delegates.Count; i++)
            {
                ActivityDelegate activityDelegate = activity.Delegates[i];

                if (activityDelegate.Handler != null)
                {
                    yield return activityDelegate.Handler;
                }
            }

            for (i = 0; i < activity.ImportedDelegates.Count; i++)
            {
                ActivityDelegate activityDelegate = activity.ImportedDelegates[i];

                if (activityDelegate.Handler != null)
                {
                    yield return activityDelegate.Handler;
                }
            }

            for (i = 0; i < activity.ImplementationDelegates.Count; i++)
            {
                ActivityDelegate activityDelegate = activity.ImplementationDelegates[i];

                if (activityDelegate.Handler != null)
                {
                    yield return activityDelegate.Handler;
                }
            }
        }

        public static Version GetImplementationVersion(Activity activity)
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }

            return activity.ImplementationVersion;
        }

        public static bool CanInduceIdle(Activity activity)
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }

            if (!activity.IsMetadataCached)
            {
                IList<ValidationError> validationErrors = null;

                ActivityUtilities.CacheRootMetadata(activity, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);

                ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
            }

            return activity.InternalCanInduceIdle;
        }
    }
}
