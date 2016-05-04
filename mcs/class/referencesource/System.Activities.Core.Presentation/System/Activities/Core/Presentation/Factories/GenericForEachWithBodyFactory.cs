//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation.Factories
{
    using System.Activities.Presentation;
    using System.Windows;

    public sealed class ForEachWithBodyFactory<T> : IActivityTemplateFactory
    {
        public Activity Create(DependencyObject target)
        {
            return new System.Activities.Statements.ForEach<T>()
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
