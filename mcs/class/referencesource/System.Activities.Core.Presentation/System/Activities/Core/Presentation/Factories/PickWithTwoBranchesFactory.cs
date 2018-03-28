//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation.Factories
{
    using System.Activities.Presentation;
    using System.Windows;

    public sealed class PickWithTwoBranchesFactory : IActivityTemplateFactory
    {
        public Activity Create(DependencyObject target)
        {
            return new System.Activities.Statements.Pick
            {
                Branches =
                {
                    new System.Activities.Statements.PickBranch
                    {
                        DisplayName = "Branch1"
                    },
                    new System.Activities.Statements.PickBranch
                    {
                        DisplayName = "Branch2"
                    }
                }
            };
        }
    }
}
