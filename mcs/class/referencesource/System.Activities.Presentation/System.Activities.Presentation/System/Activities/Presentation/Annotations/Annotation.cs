//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Xaml;

    /// <summary>
    /// Annotation class that contains methods to access annotation attached property
    /// </summary>
    public static class Annotation
    {
        /// <summary>
        /// attachable property for annotation text
        /// </summary>
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes, Justification = "XAML attached property declaration.")]
        public static readonly AttachableMemberIdentifier AnnotationTextProperty = new AttachableMemberIdentifier(typeof(Annotation), "AnnotationText");

        /// <summary>
        /// property name to access annotation in a ModelItem
        /// </summary>
        public static readonly string AnnotationTextPropertyName = "AnnotationText";

        /// <summary>
        /// property name to access dock annoation view state
        /// </summary>
        internal static readonly string IsAnnotationDockedViewStateName = "IsAnnotationDocked";

        /// <summary>
        /// Get annotation text of an object
        /// </summary>
        /// <param name="instance">instance to get annotation</param>
        /// <returns>annoation text</returns>
        public static string GetAnnotationText(object instance)
        {
            if (instance == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("instance"));
            }

            string annotationText;
            AttachablePropertyServices.TryGetProperty<string>(instance, Annotation.AnnotationTextProperty, out annotationText);
            return annotationText;
        }

        /// <summary>
        /// Set annotation of an object
        /// </summary>
        /// <param name="instance">instance to set annotation text</param>
        /// <param name="annotationText">annoatation text to be set</param>
        public static void SetAnnotationText(object instance, string annotationText)
        {
            if (instance == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("instance"));
            }

            if (annotationText != null)
            {
                AttachablePropertyServices.SetProperty(instance, Annotation.AnnotationTextProperty, annotationText);
            }
            else
            {
                AttachablePropertyServices.RemoveProperty(instance, Annotation.AnnotationTextProperty);
            }
        }
    }
}
