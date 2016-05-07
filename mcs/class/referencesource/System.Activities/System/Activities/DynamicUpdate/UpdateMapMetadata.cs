// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Linq;

    public class UpdateMapMetadata
    {
        private DynamicUpdateMapBuilder.Finalizer finalizer;
        DynamicUpdateMapBuilder.IDefinitionMatcher matcher;
        private Activity targetActivity;
        private bool isDisposed;

        internal UpdateMapMetadata(DynamicUpdateMapBuilder.Finalizer finalizer,
            DynamicUpdateMapBuilder.IDefinitionMatcher matcher, Activity targetActivity)
        {
            this.finalizer = finalizer;
            this.matcher = matcher;
            this.targetActivity = targetActivity;
            this.isDisposed = false;
        }

        public void AllowUpdateInsideThisActivity()
        {
            this.ThrowIfDisposed();
            this.finalizer.AllowUpdateInsideCurrentActivity = true;
            this.finalizer.UpdateDisallowedReason = null;
        }

        public void DisallowUpdateInsideThisActivity(string reason)
        {
            this.ThrowIfDisposed();
            this.finalizer.AllowUpdateInsideCurrentActivity = false;
            this.finalizer.UpdateDisallowedReason = reason;
        }

        public void AddMatch(Activity updatedChild, Activity originalChild)
        {
            this.ThrowIfDisposed();

            if (updatedChild != null && originalChild != null)
            {
                this.matcher.AddMatch(updatedChild, originalChild, this.targetActivity);
            }
        }

        public void AddMatch(Variable updatedVariable, Variable originalVariable)
        {
            this.ThrowIfDisposed();

            if (updatedVariable != null && originalVariable != null)
            {
                this.matcher.AddMatch(updatedVariable, originalVariable, this.targetActivity);
            }
        }

        public Activity GetMatch(Activity updatedChild)
        {
            this.ThrowIfDisposed();

            if (updatedChild != null)
            {
                Activity result = this.matcher.GetMatch(updatedChild);
                if (updatedChild.MemberOf == this.targetActivity.MemberOf)
                {
                    return result;
                }
                else if (result != null)
                {
                    // GetMatch checks that the activities have the same relationship to declaring parent.
                    // But for referenced children, we also need to check whether they have the same relationship 
                    // to referencing parent.
                    // In case of multiple references from the same parent, we'll compare the first one we find.
                    bool updatedIsImport;
                    bool updatedIsReferenced = IsChild(this.TargetActivity, updatedChild, out updatedIsImport);
                    bool updatedIsDelegate = updatedChild.HandlerOf != null;

                    bool originalIsImport;
                    bool originalIsReferenced = IsChild(GetMatch(this.TargetActivity), result, out originalIsImport);
                    bool originalIsDelegate = result.HandlerOf != null;

                    if (updatedIsReferenced && originalIsReferenced && updatedIsImport == originalIsImport && updatedIsDelegate == originalIsDelegate)
                    {
                        return result;
                    }
                }
            }
            
            return null;
        }

        public Variable GetMatch(Variable updatedVariable)
        {
            this.ThrowIfDisposed();

            if (updatedVariable != null && updatedVariable.Owner == this.TargetActivity)
            {
                Variable result = this.matcher.GetMatch(updatedVariable);
                return result;
            }
            
            return null;
        }

        public bool IsReferenceToImportedChild(Activity childActivity)
        {
            this.ThrowIfDisposed();

            if (childActivity == null)
            {
                return false;
            }

            Activity parent = (childActivity.RootActivity == this.TargetActivity.RootActivity) ? this.TargetActivity : GetMatch(this.TargetActivity);
            return IsReferenceToImportedChild(parent, childActivity);
        }

        internal bool IsUpdateExplicitlyAllowedOrDisallowed
        {
            get
            {
                return this.finalizer.AllowUpdateInsideCurrentActivity.HasValue;
            }
        }

        internal void Dispose()
        {
            this.isDisposed = true;
        }

        internal bool AreMatch(Activity updatedActivity, Activity originalActivity)
        {
            return this.matcher.GetMatch(updatedActivity) == originalActivity;
        }

        internal bool AreMatch(ActivityDelegate updatedDelegate, ActivityDelegate originalDelegate)
        {
            if (updatedDelegate.Handler != null && originalDelegate.Handler != null)
            {
                return this.matcher.GetMatch(updatedDelegate.Handler) == originalDelegate.Handler;
            }
            else
            {
                return updatedDelegate.Handler == null && originalDelegate.Handler == null;
            }
        }

        internal DynamicUpdateMapBuilder.Finalizer Finalizer
        {
            get
            {
                return this.finalizer;
            }
        }

        internal Activity TargetActivity
        {
            get
            {
                return this.targetActivity;
            }
        }

        internal void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(ToString()));
            }
        }

        internal static bool IsChild(Activity parent, Activity child, out bool isImport)
        {
            if (child.HandlerOf == null)
            {
                isImport = parent.ImportedChildren.Contains(child);
                return isImport || parent.Children.Contains(child);
            }
            else
            {
                isImport = parent.ImportedDelegates.Contains(child.HandlerOf);
                return isImport || parent.Delegates.Contains(child.HandlerOf);
            }
        }

        static bool IsReferenceToImportedChild(Activity parent, Activity child)
        {
            if (child != null && child.MemberOf != parent.MemberOf)
            {
                IdSpace idSpace = parent.MemberOf.Parent;
                while (idSpace != null)
                {
                    if (idSpace == child.MemberOf)
                    {
                        return true;
                    }
                    else
                    {
                        idSpace = idSpace.Parent;
                    }
                }
            }

            return false;
        }
    }
}
