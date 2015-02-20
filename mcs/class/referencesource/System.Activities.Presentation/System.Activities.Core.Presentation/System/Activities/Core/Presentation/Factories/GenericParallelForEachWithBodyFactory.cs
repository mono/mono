//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation.Factories
{
    using System.Activities.Presentation;
    using System.Windows;

    public sealed class ParallelForEachWithBodyFactory<T> : IActivityTemplateFactory
    {
        public Activity Create(DependencyObject target)
        {
            return new System.Activities.Statements.ParallelForEach<T>()
            {
                Body = new ActivityAction<T>()
                {
                    Argument = new DelegateInArgument<T>()
                    {
                        Name = "item"
                    }
                }
            };
        }
    }
}
