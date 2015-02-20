//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System;
    using System.Activities.Presentation.Model;
    using System.Windows;

    internal interface IFloatingAnnotation
    {
        event DependencyPropertyChangedEventHandler IsKeyboardFocusWithinChanged;

        event EventHandler IsMouseOverChanged;

        event Action DockButtonClicked;

        bool IsReadOnly
        {
            set;
        }

        ModelItem ModelItem
        {
            get;
            set;
        }

        bool IsKeyboardFocusWithin
        {
            get;
        }

        bool IsMouseOver
        {
            get;
        }

        void FocusOnContent();

        void UpdateModelItem();
    }
}
