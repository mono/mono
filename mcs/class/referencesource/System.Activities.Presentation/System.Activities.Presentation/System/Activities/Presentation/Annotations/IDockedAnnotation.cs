//---------------------------------------------------------------------------
// <copyright file="IDockedAnnotation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System.Windows;

    internal interface IDockedAnnotation
    {
        event Action UndockButtonClicked;

        bool IsReadOnly
        {
            set;
        }

        Visibility Visibility
        {
            set;
        }

        void FocusOnContent();
    }
}
