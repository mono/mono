// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Text;

    enum UpdateBlockedReason
    {
        NotBlocked = 0,
        Custom = 1,
        TypeChange,
        PublicChildrenChange,
        InvalidImplementationMap,
        PrivateMembersHaveChanged,
        ChangeMatchesInImplementation,
        GeneratedAndProvidedMapConflict,
        SavedOriginalValuesForReferencedChildren,
        AddedIdleExpression,
        DelegateArgumentChange,
        DynamicArguments,
        NewHandle
    }

    static class UpdateBlockedReasonMessages
    {
        public static string Get(UpdateBlockedReason reason)
        {
            switch (reason)
            {
                case UpdateBlockedReason.Custom:
                    return SR.BlockedUpdateInsideActivityUpdateError;
                case UpdateBlockedReason.TypeChange:
                    return SR.DUActivityTypeMismatchRuntime;
                case UpdateBlockedReason.PublicChildrenChange:
                    return SR.PublicChildrenChangeBlockDU;
                case UpdateBlockedReason.InvalidImplementationMap:
                    return SR.InvalidImplementationMapRuntime;
                case UpdateBlockedReason.PrivateMembersHaveChanged:
                    return SR.PrivateMembersHaveChanged;
                case UpdateBlockedReason.ChangeMatchesInImplementation:
                    return SR.CannotChangeMatchesInImplementation;
                case UpdateBlockedReason.GeneratedAndProvidedMapConflict:
                    return SR.GeneratedAndProvidedMapConflictRuntime;
                case UpdateBlockedReason.SavedOriginalValuesForReferencedChildren:
                    return SR.CannotSaveOriginalValuesForReferencedChildren;
                case UpdateBlockedReason.AddedIdleExpression:
                    return SR.AddedIdleExpressionBlockDU;
                case UpdateBlockedReason.DelegateArgumentChange:
                    return SR.DelegateArgumentChangeBlockDU;
                case UpdateBlockedReason.DynamicArguments:
                    return SR.NoDynamicArgumentsInActivityDefinitionChangeRuntime;
                case UpdateBlockedReason.NewHandle:
                    return SR.CannotAddHandlesUpdateError;
                default:
                    Fx.Assert("Every block reason should have a corresponding message");
                    return null;
            }
        }
    }
}
