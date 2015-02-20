// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\ValueEditors
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    internal sealed class RenderUtils
    {
        private RenderUtils()
        {
        }

        public static bool DrawInscribedRoundedRect(DrawingContext drawingContext, Brush fill, Pen stroke, Rect outerBounds, double cornerRadius)
        {
            Point spineLeftTop = new Point(outerBounds.Left, outerBounds.Top);
            Point spineRightBottom = new Point(outerBounds.Right, outerBounds.Bottom);
            bool drewSomething = false;

            if (stroke != null && !Tolerances.NearZero(stroke.Thickness))
            {
                double halfThickness = stroke.Thickness / 2d;
                spineLeftTop.X += halfThickness;
                spineLeftTop.Y += halfThickness;
                spineRightBottom.X -= halfThickness;
                spineRightBottom.Y -= halfThickness;
            }

            Rect spineRect = new Rect(spineLeftTop, spineRightBottom);
            if (!Tolerances.NearZero(spineRect.Width) && !Tolerances.NearZero(spineRect.Height))
            {
                drawingContext.DrawRoundedRectangle(fill, stroke, spineRect, cornerRadius, cornerRadius);
                drewSomething = true;
            }

            return drewSomething;
        }

        public static Rect CalculateInnerRect(Rect outerBounds, double strokeThickness)
        {
            if (!Tolerances.NearZero(strokeThickness))
            {
                return new Rect(new Point(outerBounds.Left + strokeThickness, outerBounds.Top + strokeThickness), new Point(outerBounds.Right - strokeThickness, outerBounds.Bottom - strokeThickness));
            }
            else
            {
                return outerBounds;
            }
        }
    }
}
