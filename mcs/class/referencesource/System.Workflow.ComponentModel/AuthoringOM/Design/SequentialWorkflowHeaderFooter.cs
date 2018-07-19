#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Globalization;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization.Formatters.Binary;

    //

    #region Class Header/Footer
    /// <summary>
    /// Base class for Header and Footer to be associated with SequentialWorkflowRootDesigner
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SequentialWorkflowHeaderFooter
    {
        private SequentialWorkflowRootDesigner rootDesigner;
        private Image image;
        private string text = String.Empty;
        private bool isHeader = true;
        internal Size textSize = Size.Empty;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="parent">Parent designer associated with the Header or Footer</param>
        /// <param name="isHeader">True if it is a header, false if it is a footer</param>
        public SequentialWorkflowHeaderFooter(SequentialWorkflowRootDesigner parent, bool isHeader)
        {
            this.rootDesigner = parent;
            this.isHeader = isHeader;
        }

        /// <summary>
        /// Gets the image associated with Header/Footer
        /// </summary>
        public virtual Image Image
        {
            get
            {
                return this.image;
            }

            set
            {
                if (this.image == value)
                    return;

                this.image = value;

                AssociatedDesigner.InternalPerformLayout();
            }
        }

        /// <summary>
        /// Gets the text associated with Header/Footer
        /// </summary>
        public virtual string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                if (this.text == value)
                    return;

                this.text = value;

                AssociatedDesigner.InternalPerformLayout();
            }
        }

        /// <summary>
        /// Gets the bonding rectangle for Header/Footer
        /// </summary>
        public virtual Rectangle Bounds
        {
            get
            {
                Rectangle bounds = Rectangle.Empty;

                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;

                Rectangle textBounds = TextRectangle;
                Rectangle imageBounds = ImageRectangle;
                if (!textBounds.Size.IsEmpty || !imageBounds.Size.IsEmpty)
                {
                    bounds.Width = Math.Max(imageBounds.Width, textBounds.Width) + 2 * margin.Width;
                    bounds.Height = margin.Height + imageBounds.Height;
                    bounds.Height += (imageBounds.Height > 0) ? margin.Height : 0;
                    bounds.Height += textBounds.Height;
                    bounds.Height += (textBounds.Height > 0) ? margin.Height : 0;

                    //Before returning we adjust the bounds based on the header / footer setting
                    Rectangle designerBounds = this.rootDesigner.Bounds;
                    bounds.X = designerBounds.Left + designerBounds.Width / 2 - bounds.Width / 2;
                    bounds.Y = (this.isHeader) ? designerBounds.Top : designerBounds.Bottom - bounds.Height;
                }

                return bounds;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle for text associated with Header/Footer
        /// </summary>
        public virtual Rectangle TextRectangle
        {
            get
            {
                Rectangle bounds = Rectangle.Empty;
                if (!String.IsNullOrEmpty(Text))
                {
                    Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                    Rectangle designerBounds = this.rootDesigner.Bounds;
                    bounds.Size = this.textSize;
                    bounds.X = designerBounds.Left + designerBounds.Width / 2 - this.textSize.Width / 2;
                    bounds.Y = (this.isHeader) ? designerBounds.Top + margin.Height : designerBounds.Bottom - margin.Height - this.textSize.Height;
                }

                return bounds;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle for Image associated with Header/Footer
        /// </summary>
        public virtual Rectangle ImageRectangle
        {
            get
            {
                Rectangle bounds = Rectangle.Empty;
                if (Image != null)
                {
                    ActivityDesignerTheme designerTheme = this.rootDesigner.DesignerTheme;
                    Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                    Rectangle designerBounds = this.rootDesigner.Bounds;
                    Rectangle textRectangle = TextRectangle;

                    bounds.Size = designerTheme.ImageSize;
                    bounds.X = designerBounds.Left + designerBounds.Width / 2 - bounds.Width / 2;
                    if (this.isHeader)
                    {
                        bounds.Y = designerBounds.Top + margin.Height;
                        bounds.Y += textRectangle.Height;
                        bounds.Y += (textRectangle.Height > 0) ? margin.Height : 0;
                    }
                    else
                    {
                        bounds.Y = designerBounds.Bottom - margin.Height;
                        bounds.Y -= textRectangle.Height;
                        bounds.Y -= (textRectangle.Height > 0) ? margin.Height : 0;
                        bounds.Y -= bounds.Height;
                    }
                }

                return bounds;
            }
        }

        /// <summary>
        /// Layouts the visual cues inside Header/Footer
        /// </summary>
        /// <param name="e">ActivityDesignerLayoutEventArgs holding layouting arguments</param>
        public virtual void OnLayout(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (!String.IsNullOrEmpty(Text) && e.DesignerTheme != null && e.DesignerTheme.Font != null)
            {
                using (Font font = new Font(e.DesignerTheme.Font.FontFamily, e.DesignerTheme.Font.SizeInPoints + 1.0f, FontStyle.Bold))
                    this.textSize = ActivityDesignerPaint.MeasureString(e.Graphics, font, Text, StringAlignment.Center, Size.Empty);
            }
        }

        /// <summary>
        /// Draws the Header/Footer associated with workflow root designer.
        /// </summary>
        /// <param name="e">ActivityDesignerPaintEventArgs holding drawing arguments</param>
        public virtual void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (!String.IsNullOrEmpty(Text) && !TextRectangle.Size.IsEmpty && e.DesignerTheme != null && e.DesignerTheme.Font != null)
            {
                //use bold or regular font based on mouse over status
                using (Font font = new Font(e.DesignerTheme.Font.FontFamily, e.DesignerTheme.Font.SizeInPoints + 1.0f, (this.AssociatedDesigner.SmartTagVisible) ? FontStyle.Bold : FontStyle.Regular))
                    ActivityDesignerPaint.DrawText(e.Graphics, font, Text, TextRectangle, StringAlignment.Center, TextQuality.AntiAliased, e.DesignerTheme.ForegroundBrush);
            }

            if (Image != null && !ImageRectangle.Size.IsEmpty)
                ActivityDesignerPaint.DrawImage(e.Graphics, Image, ImageRectangle, DesignerContentAlignment.Fill);
        }

        /// <summary>
        /// Gets the designer associated with header/footer
        /// </summary>
        protected SequentialWorkflowRootDesigner AssociatedDesigner
        {
            get
            {
                return this.rootDesigner;
            }
        }
    }
    #endregion

}
