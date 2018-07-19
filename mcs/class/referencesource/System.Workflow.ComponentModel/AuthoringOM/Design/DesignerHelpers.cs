namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Drawing;
    using System.Resources;
    using System.Reflection;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.ComponentModel;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Workflow.Interop;
    using System.Workflow.ComponentModel.Compiler;
    using Microsoft.Win32;
    using System.Runtime.CompilerServices;

    #region Enum LightSourcePosition
    [Flags]
    internal enum LightSourcePosition
    {
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
        Center = 16
    }
    #endregion

    #region Class ActivityDesignerPaint
    /// <summary>
    /// Provides useful methods used to paint user interface elements on activity designers.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public static class ActivityDesignerPaint
    {
        #region Members and Enumerations

        internal enum XpThemeColorStyles { Blue = 0, Silver = 1, Green = 2 }
        [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
        private enum XpSchemeColorIndex { FgGnd = 0, BkGnd = 1, Border = 2, Highlight = 3, Shadow = 4 };
        private static Color[,] XPColorSchemes = new Color[,]
            {
                { Color.FromArgb(0, 60, 165), Color.FromArgb(255, 255, 255), Color.FromArgb(181, 186, 214), Color.FromArgb(66, 142, 255), Color.FromArgb(181, 195, 231) },
                { Color.FromArgb(49, 68, 115), Color.FromArgb(255, 255, 255), Color.FromArgb(186, 187, 201), Color.FromArgb(126, 124, 124), Color.FromArgb(206, 207, 216) },
                { Color.FromArgb(86, 102, 45), Color.FromArgb(255, 255, 255), Color.FromArgb(210, 219, 197), Color.FromArgb(114, 146, 29), Color.FromArgb(212, 220, 190) }
            };
        #endregion

        #region Drawing Functions
        #region Text Rendering
        internal static Size MeasureString(Graphics graphics, Font font, string text, StringAlignment alignment, Size maxSize)
        {
            SizeF textSize = SizeF.Empty;
            if (maxSize.IsEmpty)
            {
                textSize = graphics.MeasureString(text, font);
            }
            else
            {
                StringFormat format = new StringFormat();
                format.Alignment = alignment;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoClip;
                textSize = graphics.MeasureString(text, font, new SizeF(maxSize.Width, maxSize.Height), format);
            }

            return new Size(Convert.ToInt32(Math.Ceiling(textSize.Width)), Convert.ToInt32(Math.Ceiling(textSize.Height)));
        }

        /// <summary>
        /// Draws the text as per the parameters specified on given graphics object
        /// </summary>
        /// <param name="graphics">Graphics on which to draw the text</param>
        /// <param name="font">Font used in drawing</param>
        /// <param name="text">Text to draw on the graphics</param>
        /// <param name="boundingRect">Bounding rectangle in which text must be drawn</param>
        /// <param name="alignment">Alignment for the text</param>
        /// <param name="textQuality">Text quality to be used in drawing</param>
        /// <param name="textBrush">Brush using which to draw the text</param>
        public static void DrawText(Graphics graphics, Font font, string text, Rectangle boundingRect, StringAlignment alignment, TextQuality textQuality, Brush textBrush)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (font == null)
                throw new ArgumentNullException("font");

            if (text == null)
                throw new ArgumentNullException("text");

            if (textBrush == null)
                throw new ArgumentNullException("textBrush");

            if (boundingRect.IsEmpty)
                return;

            StringFormat format = new StringFormat();
            format.Alignment = alignment;
            format.LineAlignment = StringAlignment.Center;
            format.Trimming = StringTrimming.EllipsisCharacter;
            format.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox;

            TextRenderingHint oldTextHint = graphics.TextRenderingHint;
            graphics.TextRenderingHint = (textQuality == TextQuality.AntiAliased) ? TextRenderingHint.AntiAlias : TextRenderingHint.SystemDefault;
            graphics.DrawString(text, font, textBrush, boundingRect, format);
            graphics.TextRenderingHint = oldTextHint;
        }
        #endregion

        #region Image Rendering
        /// <summary>
        /// Draws image with specified paramters.
        /// </summary>
        /// <param name="graphics">Graphics on which to draw the image</param>
        /// <param name="image">Image to be drawn</param>
        /// <param name="destination">Bounding rectangle for the image</param>
        /// <param name="alignment">Alignment specifying how the image will be aligned against bounding rectangle</param>
        public static void DrawImage(Graphics graphics, Image image, Rectangle destination, DesignerContentAlignment alignment)
        {
            if (image == null)
                throw new ArgumentNullException("image"); //we are accessing .Size property on the "image" argument

            DrawImage(graphics, image, destination, new Rectangle(Point.Empty, image.Size), alignment, 1.0f, false);
        }

        internal static void DrawImage(Graphics graphics, Image image, Rectangle destination, float transparency)
        {
            if (image == null)
                throw new ArgumentNullException("image"); //we are accessing .Size property on the "image" argument

            DrawImage(graphics, image, destination, new Rectangle(Point.Empty, image.Size), DesignerContentAlignment.Center, transparency, false);
        }

        /// <summary>
        /// Draws image with specified paramters.
        /// </summary>
        /// <param name="graphics">Graphics on which to draw image</param>
        /// <param name="image">Image to be drawn</param>
        /// <param name="destination">Bounding rectangle for the image</param>
        /// <param name="source">Source rectangle of the image</param>
        /// <param name="alignment">Alignment specifying how image will be aligned against the bounding rectangle</param>
        /// <param name="transparency">Transparency for the image</param>
        /// <param name="grayscale">Value indicating if the image should be gray scaled</param>
        public static void DrawImage(Graphics graphics, Image image, Rectangle destination, Rectangle source, DesignerContentAlignment alignment, float transparency, bool grayscale)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (image == null)
                throw new ArgumentNullException("image");

            if (destination.IsEmpty)
                throw new ArgumentNullException("destination");

            if (source.IsEmpty)
                throw new ArgumentNullException("source");

            if (transparency < 0 || transparency > 1.0f)
                throw new ArgumentNullException("transparency");

            Rectangle imageRectangle = GetRectangleFromAlignment(alignment, destination, source.Size);
            if (image != null && !imageRectangle.IsEmpty)
            {
                ColorMatrix colorMatrix = new ColorMatrix();
                if (grayscale)
                {
                    colorMatrix.Matrix00 = 1 / 3f;
                    colorMatrix.Matrix01 = 1 / 3f;
                    colorMatrix.Matrix02 = 1 / 3f;
                    colorMatrix.Matrix10 = 1 / 3f;
                    colorMatrix.Matrix11 = 1 / 3f;
                    colorMatrix.Matrix12 = 1 / 3f;
                    colorMatrix.Matrix20 = 1 / 3f;
                    colorMatrix.Matrix21 = 1 / 3f;
                    colorMatrix.Matrix22 = 1 / 3f;
                }
                colorMatrix.Matrix33 = transparency; //Alpha factor

                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(image, imageRectangle, source.X, source.Y, source.Width, source.Height, GraphicsUnit.Pixel, imageAttributes);
            }
        }

        internal static Rectangle GetRectangleFromAlignment(DesignerContentAlignment alignment, Rectangle destination, Size size)
        {
            if (size.IsEmpty || destination.IsEmpty)
                return Rectangle.Empty;

            Rectangle rectangle = Rectangle.Empty;
            rectangle.Width = Math.Min(size.Width, destination.Width);
            rectangle.Height = Math.Min(size.Height, destination.Height);

            if ((alignment & DesignerContentAlignment.Fill) > 0)
            {
                rectangle = destination;
            }
            else
            {
                if ((alignment & DesignerContentAlignment.Left) > 0)
                    rectangle.X = destination.Left;
                else if ((alignment & DesignerContentAlignment.Right) > 0)
                    rectangle.X = destination.Right - rectangle.Width;
                else
                    rectangle.X = destination.Left + destination.Width / 2 - rectangle.Width / 2;

                if ((alignment & DesignerContentAlignment.Top) > 0)
                    rectangle.Y = destination.Top;
                else if ((alignment & DesignerContentAlignment.Bottom) > 0)
                    rectangle.Y = destination.Bottom - rectangle.Height;
                else
                    rectangle.Y = destination.Top + destination.Height / 2 - rectangle.Height / 2;
            }

            return rectangle;
        }
        #endregion

        #region Selection Rendering
        internal static void DrawSelection(Graphics graphics, Rectangle boundingRect, bool isPrimary, Size selectionSize, Rectangle[] grabHandles)
        {
            InterpolationMode oldInterpolationMode = graphics.InterpolationMode;
            SmoothingMode oldSmoothingMode = graphics.SmoothingMode;

            //dashes dont show up when smoothing is set
            graphics.InterpolationMode = InterpolationMode.High;
            graphics.SmoothingMode = SmoothingMode.None;

            Rectangle selectionRect = boundingRect;
            selectionRect.Inflate(selectionSize.Width, selectionSize.Height);

            selectionRect.Inflate(-selectionSize.Width / 2, -selectionSize.Height / 2);
            graphics.DrawRectangle(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionPatternPen, selectionRect);

            selectionRect.Inflate(selectionSize.Width / 2, selectionSize.Height / 2);

            DrawGrabHandles(graphics, grabHandles, isPrimary);

            graphics.InterpolationMode = oldInterpolationMode;
            graphics.SmoothingMode = oldSmoothingMode;
        }

        internal static void DrawGrabHandles(Graphics graphics, Rectangle[] grabHandles, bool isPrimary)
        {
            foreach (Rectangle grabHandle in grabHandles)
            {
                if (isPrimary)
                {
                    //primary
                    graphics.FillRectangle(Brushes.White, grabHandle);
                    graphics.DrawRectangle(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForegroundPen, grabHandle);
                }
                else
                {
                    Pen patternPen = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionPatternPen;
                    DashStyle oldDashStyle = patternPen.DashStyle;
                    patternPen.DashStyle = DashStyle.Solid;

                    //secondary
                    graphics.FillRectangle(Brushes.White, grabHandle);
                    graphics.DrawRectangle(patternPen, grabHandle);

                    patternPen.DashStyle = oldDashStyle;
                }
            }
        }
        #endregion

        #region General Rendering Methods
        /// <summary>
        /// Draws three dimentional button on the designer
        /// </summary>
        /// <param name="graphics">Graphics on which to draw the button</param>
        /// <param name="image">Image which is to be drawn on the button</param>
        /// <param name="bounds">Bounding rectangle for the button</param>
        /// <param name="transparency">Transparency value for the button</param>
        /// <param name="buttonState">State in which to draw the button</param>
        public static void Draw3DButton(Graphics graphics, Image image, Rectangle bounds, float transparency, ButtonState buttonState)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            int alpha = Math.Max(0, Convert.ToInt32(transparency * 255));
            using (SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(alpha, SystemColors.Control)))
            using (Pen lightPen = new Pen(Color.FromArgb(alpha, SystemColors.ControlLightLight)))
            using (Pen darkPen = new Pen(Color.FromArgb(alpha, SystemColors.ControlDark)))
            using (Pen darkdarkPen = new Pen(Color.FromArgb(alpha, SystemColors.ControlDarkDark)))
            {
                graphics.FillRectangle(backgroundBrush, bounds);

                if (buttonState == ButtonState.Normal || buttonState == ButtonState.Inactive)
                {
                    graphics.DrawLine(lightPen, bounds.Left + 1, bounds.Bottom - 1, bounds.Left + 1, bounds.Top + 1);
                    graphics.DrawLine(lightPen, bounds.Left + 1, bounds.Top + 1, bounds.Right - 1, bounds.Top + 1);
                    graphics.DrawLine(darkPen, bounds.Left + 1, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
                    graphics.DrawLine(darkPen, bounds.Right - 1, bounds.Bottom - 1, bounds.Right - 1, bounds.Top + 1);
                    graphics.DrawLine(darkdarkPen, bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom);
                    graphics.DrawLine(darkdarkPen, bounds.Right, bounds.Bottom, bounds.Right, bounds.Top);
                }
                else if (buttonState == ButtonState.Pushed)
                {
                    graphics.DrawRectangle(darkPen, bounds);
                    bounds.Offset(1, 1);
                }

                if (image != null)
                {
                    bounds.Inflate(-2, -2);
                    DrawImage(graphics, image, bounds, new Rectangle(Point.Empty, image.Size), DesignerContentAlignment.Fill, transparency, (buttonState == ButtonState.Inactive));
                }
            }
        }

        internal static void DrawGrid(Graphics graphics, Rectangle viewableRectangle)
        {
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            if (ambientTheme.GridStyle == DashStyle.Dot)
            {
                Point gridStart = Point.Empty;
                gridStart.X = viewableRectangle.X - (viewableRectangle.X % ambientTheme.GridSize.Width);
                gridStart.Y = viewableRectangle.Y - (viewableRectangle.Y % ambientTheme.GridSize.Height);

                for (int gridCoOrdX = gridStart.X; gridCoOrdX <= viewableRectangle.Right; gridCoOrdX += Math.Max(ambientTheme.GridSize.Width, 1))
                {
                    for (int gridCoOrdY = gridStart.Y; gridCoOrdY <= viewableRectangle.Bottom; gridCoOrdY += Math.Max(ambientTheme.GridSize.Height, 1))
                    {
                        graphics.FillRectangle(ambientTheme.MajorGridBrush, new Rectangle(new Point(gridCoOrdX, gridCoOrdY), new Size(1, 1)));

                        if (((gridCoOrdX + ambientTheme.GridSize.Width / 2) >= viewableRectangle.Left && (gridCoOrdX + ambientTheme.GridSize.Width / 2) <= viewableRectangle.Right) &&
                            ((gridCoOrdY + ambientTheme.GridSize.Height / 2) >= viewableRectangle.Top && (gridCoOrdY + ambientTheme.GridSize.Height / 2) <= viewableRectangle.Bottom))
                            graphics.FillRectangle(ambientTheme.MinorGridBrush, new Rectangle(new Point(gridCoOrdX + ambientTheme.GridSize.Width / 2, gridCoOrdY + ambientTheme.GridSize.Height / 2), new Size(1, 1)));
                    }
                }
            }
            else
            {
                //We use native pens to draw the grid for efficiency reason
                using (Hdc hdc = new Hdc(graphics))
                using (HPen majorGridPen = new HPen(ambientTheme.MajorGridPen))
                using (HPen minorGridPen = new HPen(ambientTheme.MinorGridPen))
                {
                    hdc.DrawGrid(majorGridPen, minorGridPen, viewableRectangle, ambientTheme.GridSize, true);
                }
            }
        }

        /// <summary>
        /// Draws expand / collapse button based on the theme passed
        /// </summary>
        /// <param name="graphics">Graphics on which to draw the button</param>
        /// <param name="boundingRect">Bounding rectangle for the button</param>
        /// <param name="drawExpanded">Value indicating if the button should be drawn in expanded state</param>
        /// <param name="compositeDesignerTheme">Theme associated with the designer used in rendering of the button</param>
        public static void DrawExpandButton(Graphics graphics, Rectangle boundingRect, bool drawExpanded, CompositeDesignerTheme compositeDesignerTheme)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (compositeDesignerTheme == null)
                throw new ArgumentNullException("compositeDesignerTheme");

            if (boundingRect.IsEmpty)
                return;

            graphics.FillRectangle(compositeDesignerTheme.GetExpandButtonBackgroundBrush(boundingRect), boundingRect);
            graphics.DrawRectangle(CompositeDesignerTheme.ExpandButtonBorderPen, boundingRect);

            graphics.DrawLine(CompositeDesignerTheme.ExpandButtonForegoundPen, boundingRect.Left + 2, boundingRect.Top + boundingRect.Height / 2, boundingRect.Right - 2, boundingRect.Top + boundingRect.Height / 2);
            if (drawExpanded)
                graphics.DrawLine(CompositeDesignerTheme.ExpandButtonForegoundPen, boundingRect.Left + boundingRect.Width / 2, boundingRect.Top + 2, boundingRect.Left + boundingRect.Width / 2, boundingRect.Bottom - 2);
        }

        /// <summary>
        /// Draws a rounded rectangle with specified parameters
        /// </summary>
        /// <param name="graphics">Graphics on which to draw the rounded rectangle</param>
        /// <param name="drawingPen">Pen used to render the rectangle</param>
        /// <param name="rectangle">Bounding rectangle</param>
        /// <param name="radius">Radius used for the rounded edges</param>
        public static void DrawRoundedRectangle(Graphics graphics, Pen drawingPen, Rectangle rectangle, int radius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (drawingPen == null)
                throw new ArgumentNullException("drawingPen");

            GraphicsPath roundedRectangle = null;

            checked
            {
                roundedRectangle = GetRoundedRectanglePath(rectangle, radius * 2);
            }
            graphics.DrawPath(drawingPen, roundedRectangle);
            roundedRectangle.Dispose();
        }

        /// <summary>
        /// Draws the drop shadow using specified base color
        /// </summary>
        /// <param name="graphics">Graphics object on which to draw the shadow</param>
        /// <param name="shadowSourceRectangle">Rectangle around which to draw the shadow</param>
        /// <param name="baseColor">Base color used to draw the shadow</param>
        /// <param name="shadowDepth">Depth of the shadow, this has to be between 1 and 12 inclusive</param>
        /// <param name="lightSourcePosition">Position of the light source determining the way shadow will be drawn</param>
        /// <param name="lightSourceIntensity">Intensity of the light source, this needs to be inbetween 0.01 and 1</param>
        /// <param name="roundEdges">Flag indicating whether to round the edges of the shadow</param>
        internal static void DrawDropShadow(Graphics graphics, Rectangle shadowSourceRectangle, Color baseColor, int shadowDepth, LightSourcePosition lightSourcePosition, float lightSourceIntensity, bool roundEdges)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (shadowSourceRectangle.IsEmpty || shadowSourceRectangle.Width < 0 || shadowSourceRectangle.Height < 0)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidShadowRectangle), "shadowRectangle");

            if (shadowDepth < 1 || shadowDepth > 12)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidShadowDepth), "shadowDepth");

            if (lightSourceIntensity <= 0.0f || lightSourceIntensity > 1.0f)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidLightSource), "lightSourceIntensity");

            const int baseAlphaLevel = 40;
            Rectangle shadowRectangle = shadowSourceRectangle;

            Size offset = Size.Empty;
            if ((lightSourcePosition & LightSourcePosition.Center) > 0)
                shadowRectangle.Inflate(shadowDepth, shadowDepth);

            if ((lightSourcePosition & LightSourcePosition.Left) > 0)
                offset.Width += (shadowDepth + 1);
            else if ((lightSourcePosition & LightSourcePosition.Right) > 0)
                offset.Width -= (shadowDepth + 1);
            if ((lightSourcePosition & LightSourcePosition.Top) > 0)
                offset.Height += (shadowDepth + 1);
            else if ((lightSourcePosition & LightSourcePosition.Bottom) > 0)
                offset.Height -= (shadowDepth + 1);
            shadowRectangle.Offset(offset.Width, offset.Height);

            GraphicsContainer graphicsState = graphics.BeginContainer();
            GraphicsPath excludePath = new GraphicsPath();
            if (roundEdges)
                excludePath.AddPath(GetRoundedRectanglePath(shadowSourceRectangle, AmbientTheme.ArcDiameter), true);
            else
                excludePath.AddRectangle(shadowSourceRectangle);

            try
            {
                using (Region clipRegion = new Region(excludePath))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.ExcludeClip(clipRegion);
                    Color shadowColor = Color.FromArgb(Convert.ToInt32((baseAlphaLevel * lightSourceIntensity)), baseColor);
                    int alphaIncreament = Math.Max((baseAlphaLevel / shadowDepth), 2);

                    for (int i = 0; i < shadowDepth; i++)
                    {
                        shadowRectangle.Inflate(-1, -1);

                        using (Brush shadowBrush = new SolidBrush(shadowColor))
                        using (GraphicsPath shadowPath = new GraphicsPath())
                        {
                            if (roundEdges)
                                shadowPath.AddPath(GetRoundedRectanglePath(shadowRectangle, AmbientTheme.ArcDiameter), true);
                            else
                                shadowPath.AddRectangle(shadowRectangle);
                            graphics.FillPath(shadowBrush, shadowPath);
                        }
                        shadowColor = Color.FromArgb(shadowColor.A + alphaIncreament, shadowColor.R, shadowColor.G, shadowColor.B);
                    }
                }
            }
            finally
            {
                graphics.EndContainer(graphicsState);
            }
        }

        internal static void DrawDesignerBackground(Graphics graphics, ActivityDesigner designer)
        {
            ActivityDesignerTheme designerTheme = designer.DesignerTheme;
            CompositeDesignerTheme compositeDesignerTheme = designerTheme as CompositeDesignerTheme;

            //Draw the designer
            Rectangle designerBounds = designer.Bounds;
            Point location = designerBounds.Location;
            designerBounds.Location = Point.Empty;

            Matrix oldMatrix = graphics.Transform;
            graphics.TranslateTransform(location.X, location.Y);

            GraphicsPath designerPath = GetDesignerPath(designer, new Point(-location.X, -location.Y), Size.Empty, DesignerEdges.None);
            RectangleF boundsF = designerPath.GetBounds();
            Rectangle bounds = new Rectangle(0, 0, Convert.ToInt32(Math.Ceiling(boundsF.Width)), Convert.ToInt32(Math.Ceiling(boundsF.Height)));

            //Draw background
            graphics.FillPath(designerTheme.GetBackgroundBrush(bounds), designerPath);

            //Draw watermark, we draw the watermark only when the designer is not collapsed
            bool expanded = (designer is CompositeActivityDesigner) ? ((CompositeActivityDesigner)designer).Expanded : false;
            if (compositeDesignerTheme != null && expanded && compositeDesignerTheme.WatermarkImage != null)
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                bounds.Inflate(-margin.Width, -margin.Height);
                DrawImage(graphics, compositeDesignerTheme.WatermarkImage, bounds, new Rectangle(Point.Empty, compositeDesignerTheme.WatermarkImage.Size), compositeDesignerTheme.WatermarkAlignment, AmbientTheme.WatermarkTransparency, false);
            }

            //Draw border
            if (WorkflowTheme.CurrentTheme.AmbientTheme.ShowDesignerBorder)
                graphics.DrawPath(designerTheme.BorderPen, designerPath);

            designerPath.Dispose();
            graphics.Transform = oldMatrix;
        }

        internal static GraphicsPath GetDesignerPath(ActivityDesigner designer, bool enableRoundedCorners)
        {
            return GetDesignerPath(designer, Point.Empty, Size.Empty, DesignerEdges.None, enableRoundedCorners);
        }

        internal static GraphicsPath GetDesignerPath(ActivityDesigner designer, Point offset, Size inflate, DesignerEdges edgeToInflate)
        {
            return GetDesignerPath(designer, offset, inflate, edgeToInflate, true);
        }

        internal static GraphicsPath GetDesignerPath(ActivityDesigner designer, Point offset, Size inflate, DesignerEdges edgeToInflate, bool enableRoundedCorners)
        {
            GraphicsPath designerPath = new GraphicsPath();

            Rectangle bounds = designer.Bounds;
            bounds.Offset(offset);

            if ((edgeToInflate & DesignerEdges.Left) > 0)
            {
                bounds.X -= inflate.Width;
                bounds.Width += inflate.Width;
            }

            if ((edgeToInflate & DesignerEdges.Right) > 0)
                bounds.Width += inflate.Width;

            if ((edgeToInflate & DesignerEdges.Top) > 0)
            {
                bounds.Y -= inflate.Height;
                bounds.Height += inflate.Height;
            }

            if ((edgeToInflate & DesignerEdges.Bottom) > 0)
                bounds.Height += inflate.Height;

            //
            if (designer == ActivityDesigner.GetSafeRootDesigner(designer.Activity.Site) && ((IWorkflowRootDesigner)designer).InvokingDesigner == null)
            {
                designerPath.AddRectangle(bounds);
            }
            else
            {
                ActivityDesignerTheme designerTheme = designer.DesignerTheme;
                if (enableRoundedCorners && designerTheme != null && designerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle)
                    designerPath.AddPath(GetRoundedRectanglePath(bounds, AmbientTheme.ArcDiameter), true);
                else
                    designerPath.AddRectangle(bounds);
            }

            return designerPath;
        }

        internal static void DrawInvalidDesignerIndicator(Graphics graphics, ActivityDesigner activityDesigner)
        {
            Rectangle bounds = activityDesigner.Bounds;
            graphics.DrawRectangle(Pens.Red, bounds);
            graphics.DrawLine(Pens.Red, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
            graphics.DrawLine(Pens.Red, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
        }
        #endregion

        #region Connector Drawing
        internal static void DrawConnectors(Graphics graphics, Pen pen, Point[] points, Size connectorCapSize, Size maxCapSize, LineAnchor startConnectorCap, LineAnchor endConnectorCap)
        {
            if (points.GetLength(0) < 2)
                return;

            //First we start with drawing start cap
            GraphicsPath startCap = null;
            float startCapInset = 0.0f;
            if (startConnectorCap != LineAnchor.None)
            {
                Point[] startSegment = new Point[] { points[0], points[1] };
                int capSize = (startSegment[0].Y == startSegment[1].Y) ? connectorCapSize.Width : connectorCapSize.Height;
                capSize += (capSize % 2);
                capSize = Math.Min(Math.Min(capSize, maxCapSize.Width), maxCapSize.Height);
                startCap = GetLineCap(startConnectorCap, capSize, out startCapInset);

                //Now if user has requested us to fill the line cap then we do so
                //THIS IS A WORKAROUND IN FILLING THE CUSTOM CAPS AS GDI+ HAS A 
                bool fill = (startCap != null && (((int)startConnectorCap % 2) == 0) && (startSegment[0].X == startSegment[1].X || startSegment[0].Y == startSegment[1].Y));
                if (fill)
                {
                    Matrix oldTransform = graphics.Transform;
                    graphics.TranslateTransform(startSegment[0].X, startSegment[0].Y);
                    if (startSegment[0].Y == startSegment[1].Y)
                        graphics.RotateTransform((startSegment[0].X < startSegment[1].X) ? 90.0f : 270.0f);
                    else
                        graphics.RotateTransform((startSegment[0].Y < startSegment[1].Y) ? 180.0f : 0.0f);
                    using (Brush penBrush = new SolidBrush(pen.Color))
                    {
                        graphics.FillPath(penBrush, startCap);
                        graphics.DrawPath(pen, startCap);
                    }
                    graphics.Transform = (oldTransform != null) ? oldTransform : new Matrix();
                }
            }

            GraphicsPath endCap = null;
            float endCapInset = 0.0f;
            if (endConnectorCap != LineAnchor.None)
            {
                Point[] endSegment = new Point[] { points[points.GetLength(0) - 2], points[points.GetLength(0) - 1] };
                int capSize = (endSegment[0].Y == endSegment[1].Y) ? connectorCapSize.Width : connectorCapSize.Height;
                capSize += (capSize % 2);
                capSize = Math.Min(Math.Min(capSize, maxCapSize.Width), maxCapSize.Height);
                endCap = GetLineCap(endConnectorCap, capSize, out endCapInset);

                //Now if user has requested us to fill the line cap then we do so,
                //THIS IS A WORKAROUND IN FILLING THE CUSTOM CAPS AS GDI+ HAS A 
                bool fill = (endCap != null && (((int)endConnectorCap % 2) == 0) && (endSegment[0].X == endSegment[1].X || endSegment[0].Y == endSegment[1].Y));
                if (fill)
                {
                    Matrix oldTransform = graphics.Transform;
                    graphics.TranslateTransform(endSegment[1].X, endSegment[1].Y);
                    if (endSegment[0].Y == endSegment[1].Y)
                        graphics.RotateTransform((endSegment[0].X < endSegment[1].X) ? 270.0f : 90.0f);
                    else
                        graphics.RotateTransform((endSegment[0].Y < endSegment[1].Y) ? 0.0f : 180.0f);
                    using (Brush penBrush = new SolidBrush(pen.Color))
                    {
                        graphics.FillPath(penBrush, endCap);
                        graphics.DrawPath(pen, endCap);
                    }
                    graphics.Transform = (oldTransform != null) ? oldTransform : new Matrix();
                }
            }

            if (startCap != null)
            {
                CustomLineCap customStartCap = new CustomLineCap(null, startCap);
                customStartCap.WidthScale = 1.0f / pen.Width;
                customStartCap.BaseInset = startCapInset;
                pen.CustomStartCap = customStartCap;
            }

            if (endCap != null)
            {
                CustomLineCap customEndCap = new CustomLineCap(null, endCap);
                customEndCap.WidthScale = 1.0f / pen.Width;
                customEndCap.BaseInset = endCapInset;
                pen.CustomEndCap = customEndCap;
            }

            graphics.DrawLines(pen, points);

            if (startCap != null)
            {
                CustomLineCap disposableLineCap = pen.CustomStartCap;
                pen.StartCap = LineCap.Flat;
                disposableLineCap.Dispose();
            }

            if (endCap != null)
            {
                CustomLineCap disposableLineCap = pen.CustomEndCap;
                pen.EndCap = LineCap.Flat;
                disposableLineCap.Dispose();
            }
        }
        #endregion

        #region General purpose functions
        //

        internal static GraphicsPath GetLineCap(LineAnchor lineCap, int capsize, out float capinset)
        {
            //WE DO NOT SUPPORT ARROWCAPS FOR ANGULAR CONNECTORS FOR NOW
            capinset = 0.0f;
            capinset = (float)capsize;
            Size capSize = new Size(capsize, capsize);

            GraphicsPath lineCapPath = new GraphicsPath();
            switch (lineCap)
            {
                case LineAnchor.Arrow:
                case LineAnchor.ArrowAnchor:
                    int arcRadius = capSize.Height / 3;
                    lineCapPath.AddLine(capSize.Width / 2, -capSize.Height, 0, 0);
                    lineCapPath.AddLine(0, 0, -capSize.Width / 2, -capSize.Height);
                    lineCapPath.AddLine(-capSize.Width / 2, -capSize.Height, 0, -capSize.Height + arcRadius);
                    lineCapPath.AddLine(0, -capSize.Height + arcRadius, capSize.Width / 2, -capSize.Height);
                    capinset = capSize.Height - arcRadius;
                    break;

                case LineAnchor.Diamond:
                case LineAnchor.DiamondAnchor:
                    lineCapPath.AddLine(0, -capSize.Height, capSize.Width / 2, -capSize.Height / 2);
                    lineCapPath.AddLine(capSize.Width / 2, -capSize.Height / 2, 0, 0);
                    lineCapPath.AddLine(0, 0, -capSize.Width / 2, -capSize.Height / 2);
                    lineCapPath.AddLine(-capSize.Width / 2, -capSize.Height / 2, 0, -capSize.Height);
                    break;

                case LineAnchor.Round:
                case LineAnchor.RoundAnchor:
                    lineCapPath.AddEllipse(new Rectangle(-capSize.Width / 2, -capSize.Height, capSize.Width, capSize.Height));
                    break;

                case LineAnchor.Rectangle:
                case LineAnchor.RectangleAnchor:
                    lineCapPath.AddRectangle(new Rectangle(-capSize.Width / 2, -capSize.Height, capSize.Width, capSize.Height));
                    break;

                case LineAnchor.RoundedRectangle:
                case LineAnchor.RoundedRectangleAnchor:
                    arcRadius = capSize.Height / 4;
                    lineCapPath.AddPath(GetRoundedRectanglePath(new Rectangle(-capSize.Width / 2, -capSize.Height, capSize.Width, capSize.Height), arcRadius), true);
                    break;
            }

            lineCapPath.CloseFigure();
            return lineCapPath;
        }

        /// <summary>
        /// Get the rounded rectangle path with specified radius
        /// </summary>
        /// <param name="rectangle">Bounding rectangle used for getting the rounded rectangular path</param>
        /// <param name="radius">Radius used to obtain the rounded rectangle path</param>
        /// <returns></returns>
        public static GraphicsPath GetRoundedRectanglePath(Rectangle rectangle, int radius)
        {
            if (rectangle.IsEmpty)
                throw new ArgumentException(SR.GetString(SR.Error_EmptyRectangleValue), "rectangle");

            if (radius <= 0)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidRadiusValue), "radius");

            int diameter = radius * 2;

            GraphicsPath roundedRectangle = new GraphicsPath();
            roundedRectangle.AddLine(rectangle.Left, rectangle.Bottom - radius, rectangle.Left, rectangle.Top + radius);
            roundedRectangle.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180.0f, 90.0f);
            roundedRectangle.AddLine(rectangle.Left + radius, rectangle.Top, rectangle.Right - radius, rectangle.Top);
            roundedRectangle.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270.0f, 90.0f);
            roundedRectangle.AddLine(rectangle.Right, rectangle.Top + radius, rectangle.Right, rectangle.Bottom - radius);
            roundedRectangle.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0.0f, 90.0f);
            roundedRectangle.AddLine(rectangle.Right - radius, rectangle.Bottom, rectangle.Left + radius, rectangle.Bottom);
            roundedRectangle.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90.0f, 90.0f);
            roundedRectangle.CloseFigure();
            return roundedRectangle;
        }

        internal static GraphicsPath GetScrollIndicatorPath(Rectangle bounds, ScrollButton button)
        {
            GraphicsPath scrollIndicatorPath = new GraphicsPath();

            if (!bounds.IsEmpty)
            {
                if (button == ScrollButton.Left || button == ScrollButton.Right)
                {
                    int arrowHeight = bounds.Height + bounds.Height % 2;
                    int midPoint = arrowHeight / 2;
                    Size arrowSize = new Size(arrowHeight / 2, arrowHeight);

                    if (button == ScrollButton.Right)
                    {
                        scrollIndicatorPath.AddLine(bounds.Left + (bounds.Width - arrowSize.Width) / 2, bounds.Top, bounds.Left + (bounds.Width - arrowSize.Width) / 2, bounds.Top + arrowSize.Height);
                        scrollIndicatorPath.AddLine(bounds.Left + (bounds.Width - arrowSize.Width) / 2, bounds.Top + arrowSize.Height, bounds.Left + (bounds.Width - arrowSize.Width) / 2 + arrowSize.Width, bounds.Top + midPoint);
                        scrollIndicatorPath.AddLine(bounds.Left + (bounds.Width - arrowSize.Width) / 2 + arrowSize.Width, bounds.Top + midPoint, bounds.Left + (bounds.Width - arrowSize.Width) / 2, bounds.Top);
                    }
                    else
                    {
                        scrollIndicatorPath.AddLine(bounds.Left + (bounds.Width - arrowSize.Width) / 2, bounds.Top + midPoint, bounds.Left + (bounds.Width - arrowSize.Width) / 2 + arrowSize.Width, bounds.Top + arrowSize.Height);
                        scrollIndicatorPath.AddLine(bounds.Left + (bounds.Width - arrowSize.Width) / 2 + arrowSize.Width, bounds.Top + arrowSize.Height, bounds.Left + (bounds.Width - arrowSize.Width) / 2 + arrowSize.Width, bounds.Top);
                        scrollIndicatorPath.AddLine(bounds.Left + (bounds.Width - arrowSize.Width) / 2 + arrowSize.Width, bounds.Top, bounds.Left + (bounds.Width - arrowSize.Width) / 2, bounds.Top + midPoint);
                    }
                }
                else if (button == ScrollButton.Up || button == ScrollButton.Down)
                {
                    int arrowWidth = bounds.Width + bounds.Width % 2;
                    int midPoint = arrowWidth / 2;
                    Size arrowSize = new Size(arrowWidth, arrowWidth / 2);

                    if (button == ScrollButton.Down)
                    {
                        scrollIndicatorPath.AddLine(bounds.Left, bounds.Top + (bounds.Height - arrowSize.Height) / 2, bounds.Left + arrowSize.Width, bounds.Top + (bounds.Height - arrowSize.Height) / 2);
                        scrollIndicatorPath.AddLine(bounds.Left + arrowSize.Width, bounds.Top + (bounds.Height - arrowSize.Height) / 2, bounds.Left + midPoint, bounds.Top + (bounds.Height - arrowSize.Height) / 2 + arrowSize.Height);
                        scrollIndicatorPath.AddLine(bounds.Left + midPoint, bounds.Top + (bounds.Height - arrowSize.Height) / 2 + arrowSize.Height, bounds.Left, bounds.Top + (bounds.Height - arrowSize.Height) / 2);
                    }
                    else
                    {
                        scrollIndicatorPath.AddLine(bounds.Left + midPoint, bounds.Top + (bounds.Height - arrowSize.Height) / 2, bounds.Left + arrowSize.Width, bounds.Top + (bounds.Height - arrowSize.Height) / 2 + arrowSize.Height);
                        scrollIndicatorPath.AddLine(bounds.Left + arrowSize.Width, bounds.Top + (bounds.Height - arrowSize.Height) / 2 + arrowSize.Height, bounds.Left, bounds.Top + (bounds.Height - arrowSize.Height) / 2 + arrowSize.Height);
                        scrollIndicatorPath.AddLine(bounds.Left, bounds.Top + (bounds.Height - arrowSize.Height) / 2 + arrowSize.Height, bounds.Left + midPoint, bounds.Top + (bounds.Height - arrowSize.Height) / 2);
                    }
                }
            }

            scrollIndicatorPath.CloseFigure();
            return scrollIndicatorPath;
        }

        internal static GraphicsPath[] GetPagePaths(Rectangle pageBounds, int pageFoldSize, DesignerContentAlignment foldAlignment)
        {
            GraphicsPath[] pagePaths = new GraphicsPath[2];

            if (foldAlignment == DesignerContentAlignment.TopLeft)
            {
                //Page path
                pagePaths[0] = new GraphicsPath();
                pagePaths[0].AddLine(pageBounds.Left, pageBounds.Top + pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Top + pageFoldSize);
                pagePaths[0].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Top + pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Top);
                pagePaths[0].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Top, pageBounds.Right, pageBounds.Top);
                pagePaths[0].AddLine(pageBounds.Right, pageBounds.Top, pageBounds.Right, pageBounds.Bottom);
                pagePaths[0].AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.Left, pageBounds.Bottom);
                pagePaths[0].AddLine(pageBounds.Left, pageBounds.Bottom, pageBounds.Left, pageBounds.Top + pageFoldSize);

                //Page fold path
                pagePaths[1] = new GraphicsPath();
                pagePaths[1].AddLine(pageBounds.Left, pageBounds.Top + pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Top + pageFoldSize);
                pagePaths[1].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Top + pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Top);
                pagePaths[1].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Top, pageBounds.Left, pageBounds.Top + pageFoldSize);
            }
            else if (foldAlignment == DesignerContentAlignment.BottomLeft)
            {
                //Page path
                pagePaths[0] = new GraphicsPath();
                pagePaths[0].AddLine(pageBounds.Left, pageBounds.Top, pageBounds.Right, pageBounds.Top);
                pagePaths[0].AddLine(pageBounds.Right, pageBounds.Top, pageBounds.Right, pageBounds.Bottom);
                pagePaths[0].AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.Left + pageFoldSize, pageBounds.Bottom);
                pagePaths[0].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Bottom, pageBounds.Left + pageFoldSize, pageBounds.Bottom - pageFoldSize);
                pagePaths[0].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Bottom - pageFoldSize, pageBounds.Left, pageBounds.Bottom - pageFoldSize);
                pagePaths[0].AddLine(pageBounds.Left, pageBounds.Bottom - pageFoldSize, pageBounds.Left, pageBounds.Top);

                //Page fold path
                pagePaths[1] = new GraphicsPath();
                pagePaths[1].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Bottom, pageBounds.Left + pageFoldSize, pageBounds.Bottom - pageFoldSize);
                pagePaths[1].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Bottom - pageFoldSize, pageBounds.Left, pageBounds.Bottom - pageFoldSize);
                pagePaths[1].AddLine(pageBounds.Left, pageBounds.Bottom - pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Bottom);

            }
            else if (foldAlignment == DesignerContentAlignment.TopRight)
            {
                //Page path
                pagePaths[0] = new GraphicsPath();
                pagePaths[0].AddLine(pageBounds.Left, pageBounds.Top, pageBounds.Right - pageFoldSize, pageBounds.Top);
                pagePaths[0].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Top, pageBounds.Right - pageFoldSize, pageBounds.Top + pageFoldSize);
                pagePaths[0].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Top + pageFoldSize, pageBounds.Right, pageBounds.Top + pageFoldSize);
                pagePaths[0].AddLine(pageBounds.Right, pageBounds.Top + pageFoldSize, pageBounds.Right, pageBounds.Bottom);
                pagePaths[0].AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.Left, pageBounds.Bottom);
                pagePaths[0].AddLine(pageBounds.Left, pageBounds.Bottom, pageBounds.Left, pageBounds.Top);

                //Page fold path
                pagePaths[1] = new GraphicsPath();
                pagePaths[1].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Top, pageBounds.Right - pageFoldSize, pageBounds.Top + pageFoldSize);
                pagePaths[1].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Top + pageFoldSize, pageBounds.Right, pageBounds.Top + pageFoldSize);
                pagePaths[1].AddLine(pageBounds.Right, pageBounds.Top + pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Top);
            }
            else if (foldAlignment == DesignerContentAlignment.BottomRight)
            {
                //Page path
                pagePaths[0] = new GraphicsPath();
                pagePaths[0].AddLine(pageBounds.Left, pageBounds.Top, pageBounds.Right, pageBounds.Top);
                pagePaths[0].AddLine(pageBounds.Right, pageBounds.Top, pageBounds.Right, pageBounds.Bottom - pageFoldSize);
                pagePaths[0].AddLine(pageBounds.Right, pageBounds.Bottom - pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Bottom - pageFoldSize);
                pagePaths[0].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Bottom - pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Bottom);
                pagePaths[0].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Bottom, pageBounds.Left, pageBounds.Bottom);
                pagePaths[0].AddLine(pageBounds.Left, pageBounds.Bottom, pageBounds.Left, pageBounds.Top);

                //Page fold path
                pagePaths[1] = new GraphicsPath();
                pagePaths[1].AddLine(pageBounds.Right, pageBounds.Bottom - pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Bottom - pageFoldSize);
                pagePaths[1].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Bottom - pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Bottom);
                pagePaths[1].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Bottom, pageBounds.Right, pageBounds.Bottom - pageFoldSize);
            }

            return pagePaths;
        }
        #endregion
        #endregion

        #region Native Drawing Calls

        #region Class Hdc
        private sealed class Hdc : IDisposable
        {
            private Graphics graphics;
            private HandleRef hdc;
            private HandleRef oldPen;
            private HandleRef oldPenEx;
            private HandleRef oldBrush;
            private int oldGraphicsMode = 0;

            internal Hdc(Graphics graphics)
            {
                this.graphics = graphics;

                NativeMethods.XFORM xform = new NativeMethods.XFORM(this.graphics.Transform);

                this.hdc = new HandleRef(this, this.graphics.GetHdc());

                //If the function fails, the return value is zero
                this.oldGraphicsMode = NativeMethods.SetGraphicsMode(this.hdc, NativeMethods.GM_ADVANCED);
                if (this.oldGraphicsMode == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                //If the function fails, the return value is zero.
                int result = NativeMethods.SetWorldTransform(this.hdc, xform);
                if (result == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                IntPtr handle = NativeMethods.GetCurrentObject(this.hdc, NativeMethods.OBJ_PEN);
                if (handle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                this.oldPen = new HandleRef(this, handle);

                handle = NativeMethods.GetCurrentObject(this.hdc, NativeMethods.OBJ_EXTPEN);
                if (handle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                this.oldPenEx = new HandleRef(this, handle);

                handle = NativeMethods.GetCurrentObject(this.hdc, NativeMethods.OBJ_BRUSH);
                if (handle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                this.oldBrush = new HandleRef(this, handle);
            }

            void IDisposable.Dispose()
            {
                if (this.graphics != null)
                {
                    string msg;
                    IntPtr objectPtr = NativeMethods.SelectObject(this.hdc, this.oldPen);
                    if (objectPtr == IntPtr.Zero)
                    {
                        Win32Exception e = new Win32Exception();
                        msg = e.Message;
                        Debug.Assert(false, msg);
                    }

                    objectPtr = NativeMethods.SelectObject(this.hdc, this.oldPenEx);
                    if (objectPtr == IntPtr.Zero)
                    {
                        Win32Exception e = new Win32Exception();
                        msg = e.Message;
                        Debug.Assert(false, msg);
                    }

                    objectPtr = NativeMethods.SelectObject(this.hdc, this.oldBrush);
                    if (objectPtr == IntPtr.Zero)
                    {
                        Win32Exception e = new Win32Exception();
                        msg = e.Message;
                        Debug.Assert(false, msg);
                    }

                    int result = NativeMethods.SetWorldTransform(this.hdc, new NativeMethods.XFORM());
                    if (result == 0)
                    {
                        Win32Exception e = new Win32Exception();
                        msg = e.Message;
                        Debug.Assert(false, msg);
                    }

                    result = NativeMethods.SetGraphicsMode(this.hdc, this.oldGraphicsMode);
                    if (result == 0)
                    {
                        Win32Exception e = new Win32Exception();
                        msg = e.Message;
                        Debug.Assert(false, msg);
                    }

                    this.graphics.ReleaseHdc();
                    this.graphics = null;
                }
            }

            //internal void DrawLines(HPen pen, Point[] points)
            //{
            //    try
            //    {
            //        IntPtr objectPtr = NativeMethods.SelectObject(this.hdc, pen.Handle);
            //        if (objectPtr == IntPtr.Zero)
            //            throw new Win32Exception(Marshal.GetLastWin32Error());

            //        if (!NativeMethods.MoveToEx(this.hdc, points[0].X, points[0].Y, null))
            //            throw new Win32Exception(Marshal.GetLastWin32Error());

            //        for (int i = 0; i < points.Length - 1; i++)
            //        {
            //            if (!NativeMethods.LineTo(this.hdc, points[i + 1].X, points[i + 1].Y))
            //                throw new Win32Exception(Marshal.GetLastWin32Error());
            //        }
            //    }
            //    finally
            //    {
            //        IntPtr objectPtr = NativeMethods.SelectObject(this.hdc, this.oldPen);
            //        if (objectPtr == IntPtr.Zero)
            //            throw new Win32Exception(Marshal.GetLastWin32Error());
            //    }
            //}

            internal void DrawGrid(HPen majorGridPen, HPen minorGridPen, Rectangle viewableRectangle, Size gridUnit, bool showMinorGrid)
            {
                try
                {
                    Point gridStart = Point.Empty;
                    gridStart.X = viewableRectangle.X - (viewableRectangle.X % gridUnit.Width);
                    gridStart.Y = viewableRectangle.Y - (viewableRectangle.Y % gridUnit.Height);
                    IntPtr objectPtr = NativeMethods.SelectObject(this.hdc, majorGridPen.Handle);
                    if (objectPtr == IntPtr.Zero)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    for (int gridCoOrd = gridStart.X; gridCoOrd <= viewableRectangle.Right; gridCoOrd += Math.Max(gridUnit.Width, 1))
                    {
                        if (gridCoOrd >= viewableRectangle.Left)
                        {
                            if (!NativeMethods.MoveToEx(this.hdc, gridCoOrd, viewableRectangle.Top + 1, null))
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                            if (!NativeMethods.LineTo(this.hdc, gridCoOrd, viewableRectangle.Bottom - 1))
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        if (showMinorGrid && (gridCoOrd + gridUnit.Width / 2) >= viewableRectangle.Left && (gridCoOrd + gridUnit.Width / 2) <= viewableRectangle.Right)
                        {
                            objectPtr = NativeMethods.SelectObject(this.hdc, minorGridPen.Handle);
                            if (objectPtr == IntPtr.Zero)
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                            if (!NativeMethods.MoveToEx(this.hdc, gridCoOrd + gridUnit.Width / 2, viewableRectangle.Top + 1, null))
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                            if (!NativeMethods.LineTo(this.hdc, gridCoOrd + gridUnit.Width / 2, viewableRectangle.Bottom - 1))
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                            objectPtr = NativeMethods.SelectObject(this.hdc, majorGridPen.Handle);
                            if (objectPtr == IntPtr.Zero)
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    }

                    for (int gridCoOrd = gridStart.Y; gridCoOrd <= viewableRectangle.Bottom; gridCoOrd += Math.Max(gridUnit.Height, 1))
                    {
                        if (gridCoOrd >= viewableRectangle.Top)
                        {
                            if (!NativeMethods.MoveToEx(this.hdc, viewableRectangle.Left + 1, gridCoOrd, null))
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                            if (!NativeMethods.LineTo(this.hdc, viewableRectangle.Right - 1, gridCoOrd))
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        if (showMinorGrid && (gridCoOrd + gridUnit.Height / 2) >= viewableRectangle.Top && (gridCoOrd + gridUnit.Height / 2) <= viewableRectangle.Bottom)
                        {
                            objectPtr = NativeMethods.SelectObject(this.hdc, minorGridPen.Handle);
                            if (objectPtr == IntPtr.Zero)
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                            if (!NativeMethods.MoveToEx(this.hdc, viewableRectangle.Left + 1, gridCoOrd + gridUnit.Height / 2, null))
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                            if (!NativeMethods.LineTo(this.hdc, viewableRectangle.Right - 1, gridCoOrd + gridUnit.Height / 2))
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                            objectPtr = NativeMethods.SelectObject(this.hdc, majorGridPen.Handle);
                            if (objectPtr == IntPtr.Zero)
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                        }
                    }
                }
                finally
                {
                    IntPtr objectPtr = NativeMethods.SelectObject(this.hdc, this.oldPen);
                    if (objectPtr == IntPtr.Zero)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                }
            }
        }
        #endregion

        #region Class HPen
        private sealed class HPen : IDisposable
        {
            private Pen pen;
            private HandleRef hpen;

            internal HPen(Pen pen)
            {
                this.pen = pen;
                int penStyle = ((int)pen.DashStyle < 4) ? (int)pen.DashStyle : 0;
                IntPtr penPtr = NativeMethods.ExtCreatePen(NativeMethods.PS_COSMETIC | NativeMethods.PS_USERSTYLE | penStyle, 1, new NativeMethods.LOGBRUSH(NativeMethods.BS_SOLID, ColorTranslator.ToWin32(pen.Color), 0), 2, new int[] { 1, 1 });
                if (penPtr == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                this.hpen = new HandleRef(this, penPtr);

            }

            void IDisposable.Dispose()
            {
                if (this.pen != null)
                {
                    //If the function succeeds, the return value is nonzero
                    int result = NativeMethods.DeleteObject(this.hpen);
                    if (result == 0)
                    {
                        Win32Exception e = new Win32Exception();
                        string msg = e.Message;
                        Debug.Assert(false, msg);
                    }

                    this.pen = null;
                }
            }

            internal HandleRef Handle
            {
                get
                {
                    return this.hpen;
                }
            }

            //internal DashStyle Style 
            //{
            //    get
            //    {
            //        return this.pen.DashStyle;
            //    }
            //}
        }
        #endregion

        #endregion
    }
    #endregion

    internal static class UnsafeNativeMethods
    {
        internal const int S_OK = 0x00000000;
        internal const int S_FALSE = 0x00000001;

        internal static readonly int GWL_EXSTYLE = (-20);
        internal static readonly int WS_EX_LAYOUTRTL = 0x400000;

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    }

    internal static class DesignerHelpers
    {
        private static readonly string WorkflowDesignerSubKey = "Workflow Designer";
        internal const string DeclarativeRulesRef = "System.Workflow.Activities.Rules.RuleConditionReference, " + AssemblyRef.ActivitiesAssemblyRef;
        internal const string SequentialWorkflowTypeRef = "System.Workflow.Activities.SequentialWorkflowActivity, " + AssemblyRef.ActivitiesAssemblyRef;
        internal const string StateMachineWorkflowTypeRef = "System.Workflow.Activities.StateMachineWorkflowActivity, " + AssemblyRef.ActivitiesAssemblyRef;
        internal const string EventHandlersTypeRef = "System.Workflow.Activities.EventHandlersActivity, " + AssemblyRef.ActivitiesAssemblyRef;
        internal const string IfElseBranchTypeRef = "System.Workflow.Activities.IfElseBranchActivity, " + AssemblyRef.ActivitiesAssemblyRef;
        internal const string CodeActivityTypeRef = "System.Workflow.Activities.CodeActivity, " + AssemblyRef.ActivitiesAssemblyRef;

        internal static Point SnapToGrid(Point location)
        {
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            if (ambientTheme.ShowGrid)
            {
                Size snapSize = WorkflowTheme.CurrentTheme.AmbientTheme.GridSize;
                snapSize.Width /= 2; snapSize.Height /= 2;
                location.X = ((location.X / snapSize.Width) * snapSize.Width) + (((location.X % snapSize.Width) > snapSize.Width / 2) ? snapSize.Width : 0);
                location.Y = ((location.Y / snapSize.Height) * snapSize.Height) + (((location.Y % snapSize.Height) > snapSize.Height / 2) ? snapSize.Height : 0);
            }
            return location;
        }

        internal static string DesignerPerUserRegistryKey
        {
            get
            {
                return Helpers.PerUserRegistryKey + "\\" + DesignerHelpers.WorkflowDesignerSubKey;
            }
        }

        internal static bool AreAssociatedDesignersMovable(ICollection components)
        {
            foreach (object obj in components)
            {
                Activity activity = obj as Activity;
                if (activity == null)
                {
                    HitTestInfo hitInfo = obj as HitTestInfo;
                    activity = (hitInfo != null && hitInfo.AssociatedDesigner != null) ? hitInfo.AssociatedDesigner.Activity : null;
                }

                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null && designer.IsLocked)
                    return false;
            }

            return true;
        }

        internal static Activity GetNextSelectableActivity(Activity currentActivity)
        {
            ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(currentActivity);
            CompositeActivityDesigner parentDesigner = (activityDesigner != null) ? activityDesigner.ParentDesigner : null;
            if (parentDesigner == null)
                return null;

            DesignerNavigationDirection navigation = (parentDesigner is ParallelActivityDesigner || parentDesigner is ActivityPreviewDesigner) ? DesignerNavigationDirection.Right : DesignerNavigationDirection.Down;
            Activity nextSelectableActivity = null;
            object nextSelectableObject = parentDesigner.GetNextSelectableObject(currentActivity, navigation);
            while (nextSelectableActivity == null && nextSelectableObject != null && nextSelectableObject != currentActivity)
            {
                nextSelectableActivity = nextSelectableObject as Activity;
                nextSelectableObject = parentDesigner.GetNextSelectableObject(nextSelectableObject, navigation);
            }

            if (nextSelectableActivity == null)
            {
                navigation = (parentDesigner is ParallelActivityDesigner || parentDesigner is ActivityPreviewDesigner) ? DesignerNavigationDirection.Left : DesignerNavigationDirection.Up;
                nextSelectableObject = parentDesigner.GetNextSelectableObject(currentActivity, navigation);
                while (nextSelectableActivity == null && nextSelectableObject != null && nextSelectableObject != currentActivity)
                {
                    nextSelectableActivity = nextSelectableObject as Activity;
                    nextSelectableObject = parentDesigner.GetNextSelectableObject(nextSelectableObject, navigation);
                }
            }

            if (nextSelectableActivity == null)
                nextSelectableActivity = parentDesigner.Activity;

            return nextSelectableActivity;
        }

        internal static void SerializeDesignerStates(IDesignerHost designerHost, BinaryWriter writer)
        {
            writer.Write(designerHost.Container.Components.Count);
            foreach (IComponent component in designerHost.Container.Components)
            {
                // write activity identifier
                writer.Write(component.Site.Name);

                //placeholder for length
                int activityDataLengthPosition = (int)writer.BaseStream.Length;

                writer.Write((int)0);

                ActivityDesigner designer = designerHost.GetDesigner(component) as ActivityDesigner;

                if (designer != null)
                {
                    int activityDataPosition = (int)writer.BaseStream.Length;

                    // write activity data
                    ((IPersistUIState)designer).SaveViewState(writer);

                    // place length
                    writer.Seek(activityDataLengthPosition, SeekOrigin.Begin);
                    writer.Write((int)writer.BaseStream.Length - activityDataPosition);
                    writer.Seek(0, SeekOrigin.End);
                }
            }
        }

        internal static bool DeserializeDesignerStates(IDesignerHost designerHost, BinaryReader reader)
        {
            int componentCount = reader.ReadInt32();
            bool outdated = (componentCount != designerHost.Container.Components.Count);

            for (int loop = 0; loop < componentCount; loop++)
            {
                string componentName = reader.ReadString();
                int length = reader.ReadInt32();

                if (designerHost.Container.Components[componentName] != null)
                {
                    ActivityDesigner designer = designerHost.GetDesigner(designerHost.Container.Components[componentName]) as ActivityDesigner;

                    if (designer != null)
                    {
                        ((IPersistUIState)designer).LoadViewState(reader);
                    }
                    else
                    {
                        outdated = true;
                        reader.BaseStream.Position += length; // skip activity data
                    }

                }
                else
                {
                    outdated = true;
                    reader.BaseStream.Position += length; // skip activity data
                }
            }

            return outdated;
        }

        internal static void MakePropertiesReadOnly(IServiceProvider serviceProvider, object topComponent)
        {
            Hashtable visitedComponents = new Hashtable();
            Queue nestedComponents = new Queue();
            nestedComponents.Enqueue(topComponent);

            while (nestedComponents.Count > 0)
            {
                object component = nestedComponents.Dequeue();
                if (visitedComponents[component.GetHashCode()] == null)
                {
                    visitedComponents[component.GetHashCode()] = component;

                    //add custom readonly type descriptor to the component (to set ForceReadonly flag in the property grid)
                    TypeDescriptor.AddProvider(new ReadonlyTypeDescriptonProvider(TypeDescriptor.GetProvider(component)), component);

                    //now go through all the properties and add custom readonly type descriptor to all composite ones
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component, new Attribute[] { BrowsableAttribute.Yes });
                    foreach (PropertyDescriptor property in properties)
                    {
                        if (!property.PropertyType.IsPrimitive)
                        {
                            object component1 = property.GetValue(component);
                            if (component1 != null)
                            {
                                TypeConverter converter = TypeDescriptor.GetConverter(component1);
                                TypeDescriptorContext context = new TypeDescriptorContext(serviceProvider, property, component);
                                if (converter.GetPropertiesSupported(context))
                                {
                                    TypeDescriptor.AddProvider(new ReadonlyTypeDescriptonProvider(TypeDescriptor.GetProvider(component1)), component1);
                                    nestedComponents.Enqueue(component1);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void UpdateSiteName(Activity activity, string newID)
        {
            if (activity == null)
                throw new ArgumentException("activity");

            string name = newID;
            if (Helpers.IsActivityLocked(activity))
                name = InternalHelpers.GenerateQualifiedNameForLockedActivity(activity, newID);
            ((IComponent)activity).Site.Name = name;

            // If this activity is a custom composite activity, we also need to update the site name
            // for all locked children.
            if (activity is CompositeActivity)
            {
                foreach (Activity childActivity in Helpers.GetNestedActivities(activity as CompositeActivity))
                {
                    if (Helpers.IsActivityLocked(childActivity))
                    {
                        Activity declaringActivity = Helpers.GetDeclaringActivity(childActivity);
                        ((IComponent)childActivity).Site.Name = ((IComponent)declaringActivity).Site.Name + "." + childActivity.Name; //the parent should have already been updated by now
                    }
                }
            }
        }

        internal static DialogResult ShowMessage(IServiceProvider serviceProvider, string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            IWin32Window owner = null;

            IUIService uiService = serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (uiService != null)
                owner = uiService.GetDialogOwnerWindow();
            else
                owner = Form.ActiveForm as IWin32Window;

            Control control = owner as Control;
            MessageBoxOptions options = 0;

            if (owner != null)
            {
                if (control != null)
                {
                    options = (control.RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RightAlign & MessageBoxOptions.RtlReading : 0;
                }
                else if (owner.Handle != IntPtr.Zero)
                {
                    int style = UnsafeNativeMethods.GetWindowLong(owner.Handle, UnsafeNativeMethods.GWL_EXSTYLE);
                    int hr = Marshal.GetLastWin32Error();
                    if (hr == UnsafeNativeMethods.S_OK && (style & UnsafeNativeMethods.WS_EX_LAYOUTRTL) == UnsafeNativeMethods.WS_EX_LAYOUTRTL)
                        options = MessageBoxOptions.RightAlign & MessageBoxOptions.RtlReading;
                }

                return MessageBox.Show(owner, message, title, buttons, icon, defaultButton, options);
            }
            else
            {
                return MessageBox.Show(message, title, buttons, icon, defaultButton, options);
            }
        }

        internal static void ShowHelpFromKeyword(IServiceProvider serviceProvider, string helpKeyword)
        {
            IHelpService helpService = serviceProvider.GetService(typeof(IHelpService)) as IHelpService;
            if (helpService != null)
            {
                helpService.ShowHelpFromKeyword(helpKeyword);
                return;
            }

            DesignerHelpers.ShowError(serviceProvider, DR.GetString(DR.NoHelpAvailable));
        }

        internal static void ShowError(IServiceProvider serviceProvider, string message)
        {
            ShowMessage(serviceProvider, message, DR.GetString(DR.WorkflowDesignerTitle), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        internal static void ShowError(IServiceProvider serviceProvider, Exception e)
        {
            if (e == CheckoutException.Canceled)
                return;

            while (e is TargetInvocationException && e.InnerException != null)
                e = e.InnerException;

            string message = e.Message;
            if (message == null || message.Length == 0)
                message = e.ToString();

            ShowMessage(serviceProvider, message, DR.GetString(DR.WorkflowDesignerTitle), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        internal static bool IsValidImageResource(DesignerTheme designerTheme, string directory, string path)
        {
            Image image = GetImageFromPath(designerTheme, directory, path);
            bool validImage = (image != null);
            if (image != null)
                image.Dispose();
            return validImage;
        }

        internal static string GetRelativePath(string pathFrom, string pathTo)
        {
            Uri uri = new Uri(pathFrom);
            string relativePath = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(pathTo)).ToString());
            relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (!relativePath.Contains(Path.DirectorySeparatorChar.ToString()))
                relativePath = "." + Path.DirectorySeparatorChar + relativePath;
            return relativePath;
        }

        internal static Image GetImageFromPath(DesignerTheme designerTheme, string directory, string path)
        {
            Bitmap image = null;
            if (path.Contains(Path.DirectorySeparatorChar.ToString()) && directory.Length > 0)
            {
                string imageFilePath = Uri.UnescapeDataString((new Uri(new Uri(directory), path).LocalPath));
                if (File.Exists(imageFilePath))
                {
                    try
                    {
                        image = new Bitmap(imageFilePath);
                    }
                    catch
                    {
                    }
                }
            }
            else if (designerTheme.DesignerType != null)
            {
                int index = path.LastIndexOf('.');
                if (index > 0)
                {
                    string nameSpace = path.Substring(0, index);
                    string name = path.Substring(index + 1);
                    if (nameSpace != null && nameSpace.Length > 0 &&
                        name != null && name.Length > 0)
                    {
                        try
                        {
                            ResourceManager resourceManager = new ResourceManager(nameSpace, designerTheme.DesignerType.Assembly);
                            image = resourceManager.GetObject(name) as Bitmap;
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (image != null)
                image.MakeTransparent(AmbientTheme.TransparentColor);

            return image;
        }

        private static bool ShowingMenu = false;

        internal static DesignerVerb[] GetDesignerActionVerbs(ActivityDesigner designer, ReadOnlyCollection<DesignerAction> designerActions)
        {
            List<DesignerVerb> actionVerbs = new List<DesignerVerb>();

            for (int i = 0; i < designerActions.Count; i++)
            {
                DesignerVerb actionVerb = new DesignerVerb(designerActions[i].Text, new EventHandler(new EventHandler(OnExecuteDesignerAction)), new CommandID(WorkflowMenuCommands.MenuGuid, WorkflowMenuCommands.VerbGroupDesignerActions + i));
                actionVerb.Properties[DesignerUserDataKeys.DesignerAction] = designerActions[i];
                actionVerb.Properties[DesignerUserDataKeys.Designer] = designer;
                actionVerbs.Add(actionVerb);
            }

            return actionVerbs.ToArray();
        }

        internal static void ShowDesignerVerbs(ActivityDesigner designer, Point location, ICollection<DesignerVerb> designerVerbs)
        {
            if (ShowingMenu || designerVerbs.Count == 0)
                return;

            IMenuCommandService menuCommandService = designer.Activity.Site.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (menuCommandService == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(IMenuCommandService).FullName));

            try
            {
                foreach (DesignerVerb designerVerb in designerVerbs)
                    menuCommandService.AddCommand(designerVerb);

                ShowingMenu = true;
                menuCommandService.ShowContextMenu(WorkflowMenuCommands.DesignerActionsMenu, location.X - 2, location.Y + 1);
            }
            finally
            {
                ShowingMenu = false;
                foreach (DesignerVerb designerVerb in designerVerbs)
                    menuCommandService.RemoveCommand(designerVerb);
            }
        }

        private static void OnExecuteDesignerAction(object sender, EventArgs e)
        {
            DesignerVerb designerVerb = sender as DesignerVerb;
            if (designerVerb != null)
            {
                DesignerAction designerAction = designerVerb.Properties[DesignerUserDataKeys.DesignerAction] as DesignerAction;
                if (designerAction != null)
                {
                    ActivityDesigner designer = designerVerb.Properties[DesignerUserDataKeys.Designer] as ActivityDesigner;
                    if (designer != null)
                        designer.OnExecuteDesignerAction(designerAction);
                }
            }
        }

        internal static string CreateUniqueMethodName(IComponent component, string propName, Type delegateType)
        {
            IServiceProvider serviceProvider = component.Site as IServiceProvider;
            if (serviceProvider == null)
                throw new ArgumentException("component");

            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            ITypeProvider typeProvider = (ITypeProvider)serviceProvider.GetService(typeof(ITypeProvider));
            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            string name = null;
            IReferenceService referenceService = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            if (referenceService != null)
            {
                name = referenceService.GetName(component);
            }
            else
            {
                ISite site = ((IComponent)component).Site;
                if (site != null)
                    name = site.Name;
            }

            if (name == null)
                name = component.GetType().Name;

            name = name.Replace('.', '_');
            name = name.Replace('/', '_') + "_" + propName;
            name = name.Replace('(', '_');
            name = name.Replace(')', '_');
            name = name.Replace(" ", "");

            DelegateTypeInfo dti = new DelegateTypeInfo(delegateType);

            Activity contextActivity = host.RootComponent as Activity;
            if (contextActivity == null)
            {
                Activity activity = component as Activity;
                throw new InvalidOperationException(SR.GetString(SR.Error_CantCreateMethod, activity != null ? activity.QualifiedName : string.Empty));
            }

            Type type = Helpers.GetDataSourceClass(contextActivity, serviceProvider);
            if (type == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_CantCreateMethod, contextActivity.QualifiedName));

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            MethodInfo[] methods = type.GetMethods(bindingFlags);
            ArrayList compatibleMethods = new ArrayList();

            foreach (MethodInfo method in methods)
            {
                if (method.GetParameters().Length == dti.Parameters.Length)
                {
                    bool match = true;
                    for (int i = 0; i < dti.Parameters.Length; i++)
                    {
                        ParameterInfo right = method.GetParameters()[i];
                        CodeParameterDeclarationExpression left = dti.Parameters[i];

                        FieldDirection direction = left.Direction;
                        if ((direction == FieldDirection.In && !right.IsIn) ||
                            (direction == FieldDirection.Out && !right.IsOut) ||
                            (direction == FieldDirection.Ref && !(right.IsIn && right.IsOut)) ||
                            !Helpers.TypesEqual(left.Type, right.ParameterType))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        compatibleMethods.Add(method.Name);
                }
            }

            int tryCount = 0;
            bool duplicate = true;
            string uniqueName = name;

            MemberInfo[] members = type.GetMembers();

            while (duplicate && tryCount < Int32.MaxValue)
            {
                duplicate = false;
                foreach (string existingMethod in compatibleMethods)
                {
                    if (string.Compare(existingMethod, uniqueName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate)
                {
                    foreach (MemberInfo member in members)
                    {
                        if (!(member is MethodInfo) && string.Compare(member.Name, uniqueName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            duplicate = true;
                            break;
                        }
                    }
                }
                if (!duplicate)
                {
                    MethodInfo mi = host.RootComponent.GetType().GetMethod(uniqueName, bindingFlags, null, dti.ParameterTypes, null);
                    if (mi != null && !mi.IsPrivate)
                        duplicate = true;
                }
                if (duplicate)
                    uniqueName = name + "_" + (++tryCount).ToString(CultureInfo.InvariantCulture);
            }

            return uniqueName;
        }

        internal static string GenerateUniqueIdentifier(IServiceProvider serviceProvider, string baseIdentifier, string[] existingNames)
        {
            CodeDomProvider provider = null;
            if (serviceProvider != null)
            {
                provider = serviceProvider.GetService(typeof(CodeDomProvider)) as CodeDomProvider;
                if (provider == null)
                {
                    IdentifierCreationService identifierCreationService = serviceProvider.GetService(typeof(IIdentifierCreationService)) as IdentifierCreationService;
                    if (identifierCreationService != null)
                        provider = identifierCreationService.Provider;
                }
            }

            if (provider != null)
                baseIdentifier = provider.CreateValidIdentifier(baseIdentifier);

            baseIdentifier = baseIdentifier.Replace('.', '_');
            baseIdentifier = baseIdentifier.Replace('/', '_');
            baseIdentifier = baseIdentifier.Replace('(', '_');
            baseIdentifier = baseIdentifier.Replace(')', '_');
            baseIdentifier = baseIdentifier.Replace(" ", "");

            ArrayList identifiers = new ArrayList(existingNames);
            int index = 1;
            string finalIdentifier = string.Format(CultureInfo.InvariantCulture, "{0}{1}", baseIdentifier, index);
            identifiers.Sort();
            while (identifiers.BinarySearch(finalIdentifier.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase) >= 0)
            {
                finalIdentifier = string.Format(CultureInfo.InvariantCulture, "{0}{1}", baseIdentifier, index);
                index += 1;
            }
            return finalIdentifier;
        }

        internal static IDictionary<string, string> GetDeclarativeRules(Activity activity)
        {
            IDictionary<string, string> rules = new Dictionary<string, string>();

            Type ruleConditionType = Type.GetType(DeclarativeRulesRef, false);
            if (ruleConditionType != null)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(activity);
                foreach (PropertyDescriptor property in properties)
                {
                    object value = property.GetValue(activity);
                    if (value != null && ruleConditionType.IsAssignableFrom(value.GetType()))
                    {
                        PropertyDescriptor namePropertyDescriptor = TypeDescriptor.GetConverter(value).GetProperties(new TypeDescriptorContext(activity.Site, null, value), value)["ConditionName"];
                        PropertyDescriptor expressionPropertyDescriptor = TypeDescriptor.GetConverter(value).GetProperties(new TypeDescriptorContext(activity.Site, null, value), value)["Expression"];
                        if (namePropertyDescriptor != null && expressionPropertyDescriptor != null)
                        {
                            string ruleName = namePropertyDescriptor.GetValue(value) as String;
                            object expression = expressionPropertyDescriptor.GetValue(value);
                            if (!string.IsNullOrEmpty(ruleName) && !rules.ContainsKey(ruleName))
                            {
                                string rulesText = (expression != null) ? expressionPropertyDescriptor.Converter.ConvertTo(new TypeDescriptorContext(activity.Site, null, value), System.Threading.Thread.CurrentThread.CurrentUICulture, expression, typeof(string)) as string : null;
                                if (rulesText == null)
                                    rulesText = string.Empty;

                                rules.Add(ruleName, rulesText);
                            }
                        }
                    }
                }
            }

            return rules;
        }

        internal static void RefreshDesignerActions(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IDesignerHost designerHost = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (designerHost != null)
                {
                    foreach (object component in designerHost.Container.Components)
                    {
                        Activity activity = component as Activity;
                        if (activity != null)
                        {
                            ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                            if (designer != null)
                                designer.RefreshDesignerActions();
                        }
                    }
                }
            }
        }

        //if a components is a connector, see if it can be removed
        internal static bool AreComponentsRemovable(ICollection components)
        {
            if (components == null)
                throw new ArgumentNullException("components");

            foreach (object obj in components)
            {
                Activity activity = obj as Activity;
                ConnectorHitTestInfo connector = obj as ConnectorHitTestInfo;

                //only activities and free form designer connectors are allowed to be deleted
                if (activity == null && connector == null)
                    return false;

                if (activity != null)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                    if (designer != null && designer.IsLocked)
                        return false;
                }

                if (connector != null && !(connector.AssociatedDesigner is FreeformActivityDesigner))
                    return false;
            }

            return true;
        }
    }

    #region Class DesignerUserDataKeys
    internal static class DesignerUserDataKeys
    {
        internal static readonly Guid DesignerAction = new Guid("3BD4A275-FCCD-49f0-B617-765CE63B4340");
        internal static readonly Guid DesignerVerb = new Guid("07E3C73A-5908-4ed1-8578-D1423B7481A2");
        internal static readonly Guid Image = new Guid("B4C2B905-A6D3-4cd6-A91A-5005A02B9676");
        internal static readonly Guid ViewActivity = new Guid("06B3FD23-2309-40a9-917E-831B9E160DB0");
        internal static readonly Guid PreviewActivity = new Guid("109316ED-A8A5-489f-ABD3-460E5C4C0529");
        internal static readonly Guid DesignerView = new Guid("94B8FC95-2D8A-4e27-93D5-73FA4BEBC315");
        internal static readonly Guid TemplateActivityIndex = new Guid("8EA852B4-48FC-45d3-91BE-CA4CF23E9114");
        internal static readonly Guid MoveBranchKey = new Guid("D43D31AA-3C43-4a65-8071-51288B491FBA");
        internal static readonly Guid Designer = new Guid("CF82A1DD-FD3E-4feb-8AED-EE1CAED551D7");
        internal static readonly Guid Activity = new Guid("10BDBBD7-8C63-46e8-B3B8-5006E70820B8");
        internal static readonly Guid ZOrderKey = new Guid("8F424588-5227-4273-A594-713454275670");
    }
    #endregion

    #region Class DesignerGeometry
    internal static class DesignerGeometryHelper
    {
        #region Helper Methods
        internal static DesignerEdges ClosestEdgeToPoint(Point point, Rectangle rect, DesignerEdges edgesToConsider)
        {
            List<double> distances = new List<double>();
            List<DesignerEdges> edges = new List<DesignerEdges>();

            if ((edgesToConsider & DesignerEdges.Left) > 0)
            {
                distances.Add(DistanceFromPointToLineSegment(point, new Point[] { new Point(rect.Left, rect.Top), new Point(rect.Left, rect.Bottom) }));
                edges.Add(DesignerEdges.Left);
            }

            if ((edgesToConsider & DesignerEdges.Top) > 0)
            {
                distances.Add(DistanceFromPointToLineSegment(point, new Point[] { new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Top) }));
                edges.Add(DesignerEdges.Top);
            }

            if ((edgesToConsider & DesignerEdges.Right) > 0)
            {
                distances.Add(DistanceFromPointToLineSegment(point, new Point[] { new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom) }));
                edges.Add(DesignerEdges.Right);
            }

            if ((edgesToConsider & DesignerEdges.Bottom) > 0)
            {
                distances.Add(DistanceFromPointToLineSegment(point, new Point[] { new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom) }));
                edges.Add(DesignerEdges.Bottom);
            }

            if (distances.Count > 0)
            {
                double minDistance = distances[0];
                for (int i = 1; i < distances.Count; i++)
                    minDistance = Math.Min(minDistance, distances[i]);
                return edges[distances.IndexOf(minDistance)];
            }
            else
            {
                return DesignerEdges.None;
            }
        }

        //Find rectangle enclosing line segments
        internal static Rectangle RectangleFromLineSegments(Point[] segments)
        {
            Debug.Assert(segments.Length > 0);
            if (segments.Length == 0)
                return Rectangle.Empty;

            Point leftTop = segments[0];
            Point rightBottom = segments[0];

            foreach (Point point in segments)
            {
                leftTop.X = Math.Min(leftTop.X, point.X);
                leftTop.Y = Math.Min(leftTop.Y, point.Y);
                rightBottom.X = Math.Max(rightBottom.X, point.X);
                rightBottom.Y = Math.Max(rightBottom.Y, point.Y);
            }

            Rectangle enclosingRect = new Rectangle(leftTop, new Size(rightBottom.X - leftTop.X, rightBottom.Y - leftTop.Y));
            enclosingRect.Inflate(4, 4);
            return enclosingRect;
        }

        //Is point on line segment
        internal static bool PointOnLineSegment(Point point, Point[] line, Size hitAreaSize)
        {
            Rectangle rect = RectangleFromLineSegments(line);

            rect.Inflate(hitAreaSize);
            if (rect.Contains(point))
            {
                double distance = DistanceFromPointToLineSegment(point, line);
                if (distance < hitAreaSize.Width && distance < hitAreaSize.Height)
                    return true;
            }

            return false;
        }

        //Shortest distance between point and rectangle
        internal static double DistanceFromPointToRectangle(Point point, Rectangle rect)
        {
            List<double> distances = new List<double>();
            distances.Add(DistanceBetweenPoints(point, new Point(rect.Left, rect.Top)));
            distances.Add(DistanceBetweenPoints(point, new Point(rect.Left + rect.Width / 2, rect.Top)));
            distances.Add(DistanceBetweenPoints(point, new Point(rect.Right, rect.Top)));
            distances.Add(DistanceBetweenPoints(point, new Point(rect.Right, rect.Top + rect.Height / 2)));
            distances.Add(DistanceBetweenPoints(point, new Point(rect.Right, rect.Bottom)));
            distances.Add(DistanceBetweenPoints(point, new Point(rect.Right + rect.Width / 2, rect.Bottom)));
            distances.Add(DistanceBetweenPoints(point, new Point(rect.Left, rect.Bottom)));
            distances.Add(DistanceBetweenPoints(point, new Point(rect.Left, rect.Bottom - rect.Height / 2)));

            double minDistance = distances[0];
            for (int i = 1; i < distances.Count; i++)
                minDistance = Math.Min(minDistance, distances[i]);

            return minDistance;
        }

        //Distance perpendicular from point to line segment
        internal static double DistanceFromPointToLineSegment(Point point, Point[] line)
        {
            int area = Math.Abs((((point.Y - line[0].Y) * (line[1].X - line[0].X)) - ((point.X - line[0].X) * (line[1].Y - line[0].Y))));
            return Math.Sqrt(Math.Pow(area, 2) / (Math.Pow((line[1].X - line[0].X), 2) + Math.Pow((line[1].Y - line[0].Y), 2)));
        }

        //Slope of horizontal line is 0
        //Slope of verical line will be infinite and we will represent it as -1
        //Slope of forward \ line will be       (Start at uppper point)
        //Slope of backward \ line will be      (Start in lower point)
        //Slope of forward / line will be       (Start at uppper point)
        //Slope of forward / line will be       (Start at lower point)
        //m = y2-y1 / x2 - x1
        //Line slope is +ve in 1st and 3rd quad and -ve in 2nd and 4th quad
        internal static float SlopeOfLineSegment(Point start, Point end)
        {
            //If line is vertical then the slope is infinite
            if (start.X == end.X)
                return float.MaxValue;

            //If the line is horizontal then slope is 0
            if (start.Y == end.Y)
                return 0;

            return ((float)(end.Y - start.Y)) / (end.X - start.X);
        }

        //Following function is used in two cases
        //1. The distance from a line to apoint not on the line is the length of the segment 
        //perpendicular to the line from the point
        //
        //2. The distance between two parallel lines is the distance between one of the lines 
        //and any point on the other line.
        //Midpoint of line mid = X1 + X2 / 2, Y1 + Y2 /2
        //Distance between points d = sqrt(sqr(x2-x1) + sqr(y2-y1))
        internal static double DistanceBetweenPoints(Point point1, Point point2)
        {
            double d = Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
            return d;
        }

        //This function calculates the total length of line segments by adding individual lengths
        internal static double DistanceOfLineSegments(Point[] segments)
        {
            double distance = 0;
            for (int i = 1; i < segments.Length; i++)
                distance += DistanceBetweenPoints(segments[i - 1], segments[i]);
            return distance;
        }

        //Midpoint of line mid = X1 + X2 / 2, Y1 + Y2 /2
        internal static Point MidPointOfLineSegment(Point point1, Point point2)
        {
            return new Point((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
        }
        #endregion
    }
    #endregion
}
