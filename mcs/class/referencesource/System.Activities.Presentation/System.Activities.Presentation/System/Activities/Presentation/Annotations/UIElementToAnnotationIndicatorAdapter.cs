//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System;
    using System.ComponentModel;
    using System.Windows;

    internal class UIElementToAnnotationIndicatorAdapter : IAnnotationIndicator
    {
        private UIElement element;

        public UIElementToAnnotationIndicatorAdapter(UIElement element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("element"));
            }

            this.element = element;
        }

        public event EventHandler IsMouseOverChanged
        {
            add
            {
                DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                descriptor.AddValueChanged(this.element, value);
            }

            remove
            {
                DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(UIElement));
                descriptor.RemoveValueChanged(this.element, value);
            }
        }

        public bool IsMouseOver
        {
            get
            {
                return this.element.IsMouseOver;
            }
        }

        public Visibility Visibility
        {
            set
            {
                this.element.Visibility = value;
            }
        }
    }
}
