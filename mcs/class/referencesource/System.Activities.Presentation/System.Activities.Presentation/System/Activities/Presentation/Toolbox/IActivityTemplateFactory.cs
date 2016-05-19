//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Windows;

    public interface IActivityTemplateFactory
    {
        Activity Create(DependencyObject target);
    }

    public interface IActivityTemplateFactory<T> where T : class
    {
        T Create(DependencyObject target, IDataObject dataObject);
    }
}