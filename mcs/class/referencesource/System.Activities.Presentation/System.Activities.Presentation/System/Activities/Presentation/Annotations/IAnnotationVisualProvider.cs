//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    internal interface IAnnotationVisualProvider
    {
        IAnnotationIndicator GetAnnotationIndicator();

        IFloatingAnnotation GetFloatingAnnotation();

        IDockedAnnotation GetDockedAnnotation();
    }
}
