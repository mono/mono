//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Windows;
    using System.Windows.Controls;

    internal class DockedAnnotationDecorator : Decorator
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = new Size();

            if (this.Child != null)
            {
                this.Child.Measure(availableSize);
                size.Height = this.Child.DesiredSize.Height;
            }

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Child != null)
            {
                this.Child.Arrange(new Rect(finalSize));
            }

            return finalSize;
        }
    }
}
