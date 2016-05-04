//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    internal abstract class AnnotationAdornerService
    {
        public abstract void Show(AnnotationAdorner adorner);

        public abstract void Hide(AnnotationAdorner adorner);
    }
}
