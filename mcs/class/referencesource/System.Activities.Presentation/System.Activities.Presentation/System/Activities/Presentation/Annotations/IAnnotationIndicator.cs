//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System.Windows;

    internal interface IAnnotationIndicator
    {
        event EventHandler IsMouseOverChanged;

        bool IsMouseOver
        {
            get;
        }

        Visibility Visibility
        {
            set;
        }
    }
}
