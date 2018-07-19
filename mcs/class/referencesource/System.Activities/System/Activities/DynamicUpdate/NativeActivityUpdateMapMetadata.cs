// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Linq;

    public class NativeActivityUpdateMapMetadata : UpdateMapMetadata
    {
        internal NativeActivityUpdateMapMetadata(DynamicUpdateMapBuilder.Finalizer finalizer, 
            DynamicUpdateMapBuilder.IDefinitionMatcher matcher, Activity targetActivity)
            : base(finalizer, matcher, targetActivity)
        {
        }

        public void SaveOriginalValue(Activity updatedChildActivity, object originalValue)
        {
            this.ThrowIfDisposed();
            bool isReferencedChild;
            ValidateOriginalValueAccess(this.TargetActivity, updatedChildActivity, "updatedChildActivity", out isReferencedChild);
            if (GetMatch(updatedChildActivity) == null)
            {
                throw FxTrace.Exception.Argument("updatedChildActivity", SR.CannotSaveOriginalValueForNewActivity(updatedChildActivity));
            }

            this.Finalizer.SetOriginalValue(updatedChildActivity, originalValue, isReferencedChild);
        }

        public void SaveOriginalValue(string propertyName, object originalValue)
        {
            this.ThrowIfDisposed();
            if (propertyName == null)
            {
                throw FxTrace.Exception.ArgumentNull("propertyName");
            }

            if (this.Finalizer.SavedOriginalValuesForCurrentActivity == null)
            {
                this.Finalizer.SavedOriginalValuesForCurrentActivity = new Dictionary<string, object>();
            }
            this.Finalizer.SavedOriginalValuesForCurrentActivity[propertyName] = originalValue;
        }

        internal static void ValidateOriginalValueAccess(Activity parent, Activity child, string paramName, out bool isReferencedChild)
        {
            if (child == null)
            {
                throw FxTrace.Exception.ArgumentNull(paramName);
            }

            if (!IsPublicOrImportedDelegateOrChild(parent, child, out isReferencedChild))
            {
                throw FxTrace.Exception.Argument(paramName, SR.CannotSaveOriginalValueForActivity);
            }
        }

        static bool IsPublicOrImportedDelegateOrChild(Activity parent, Activity child, out bool isReferencedChild)
        {
            isReferencedChild = false;
            if (child.Parent == parent)
            {
                if (child.HandlerOf == null)
                {
                    return child.RelationshipToParent == Activity.RelationshipType.Child || 
                        child.RelationshipToParent == Activity.RelationshipType.ImportedChild;
                }
                else
                {
                    return child.HandlerOf.ParentCollectionType == ActivityCollectionType.Public ||
                        child.HandlerOf.ParentCollectionType == ActivityCollectionType.Imports;
                }
            }
            else if (parent.MemberOf != child.MemberOf)
            {
                isReferencedChild = true;
                bool isImport;
                return IsChild(parent, child, out isImport);
            }
            else
            {
                return false;
            }
        }
    }
}
