//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		TextAnnotation.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	TextAnnotation, AnnotationSmartLabelStyle
//
//  Purpose:	Text annotation class.
//
//	Reviewed:	
//
//===================================================================

#region Used namespace
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Security;

#if Microsoft_CONTROL
using System.Windows.Forms;
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
#else
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.DataVisualization.Charting.Data;
using System.Web.UI.DataVisualization.Charting.Utilities;
#endif


#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
	/// <b>TextAnnotation</b> is a class that represents a text annotation.
	/// </summary>
	/// <remarks>
	/// Note that other annotations do display inner text (e.g. rectangle, 
	/// ellipse annotations.).
	/// </remarks>
	[
		SRDescription("DescriptionAttributeTextAnnotation_TextAnnotation"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class TextAnnotation : Annotation
	{
		#region Fields

		// Annotation text
		private		string			_text = "";

		// Indicates multiline text
		private		bool			_isMultiline = false;

		// Current content size
		internal	SizeF			contentSize = SizeF.Empty;

		// Indicates that annotion is an ellipse
		internal	bool			isEllipse = false;

#if Microsoft_CONTROL

		// Control used to edit text
		private		TextBox			_editTextBox = null;

#endif // Microsoft_CONTROL

		#endregion

		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public TextAnnotation() 
            : base()
		{
		}

		#endregion

		#region Properties

		#region Text Visual Attributes

		/// <summary>
		/// Annotation's text.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(""),
		SRDescription("DescriptionAttributeText"),
		]
		virtual public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
				Invalidate();

				// Reset content size to empty
				contentSize = SizeF.Empty;
			}
		}

		/// <summary>
		/// Indicates whether the annotation text is multiline.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(false),
		SRDescription("DescriptionAttributeMultiline"),
		]
		virtual public bool IsMultiline
		{
			get
			{
				return _isMultiline;
			}
			set
			{
				_isMultiline = value;
				Invalidate();
			}
		}

        /// <summary>
        /// Gets or sets the font of an annotation's text.
        /// <seealso cref="Annotation.ForeColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="Font"/> object used for an annotation's text.
        /// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt"),
		SRDescription("DescriptionAttributeTextFont4"),
		]
		override public Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;

				// Reset content size to empty
				contentSize = SizeF.Empty;
			}
		}

		#endregion

		#region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), "Black"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		override public Color LineColor
		{
			get
			{
				return base.LineColor;
			}
			set
			{
				base.LineColor = value;
			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(1),
        SRDescription("DescriptionAttributeLineWidth"),
		]
		override public int LineWidth
		{
			get
			{
				return base.LineWidth;
			}
			set
			{
				base.LineWidth = value;

			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(ChartDashStyle.Solid),
		]
		override public ChartDashStyle LineDashStyle
		{
			get
			{
				return base.LineDashStyle;
			}
			set
			{
				base.LineDashStyle = value;
			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		override public Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(ChartHatchStyle.None),
		NotifyParentPropertyAttribute(true),
		Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base)
		]
		override public ChartHatchStyle BackHatchStyle
		{
			get
			{
				return base.BackHatchStyle;
			}
			set
			{
				base.BackHatchStyle = value;
			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(GradientStyle.None),
		NotifyParentPropertyAttribute(true),
		Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base)
		]		
		override public GradientStyle BackGradientStyle
		{
			get
			{
				return base.BackGradientStyle;
			}
			set
			{
				base.BackGradientStyle = value;
			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		] 
		override public Color BackSecondaryColor
		{
			get
			{
				return base.BackSecondaryColor;
			}
			set
			{
				base.BackSecondaryColor = value;
			}
		}

		#endregion

		#region Other

        /// <summary>
        /// Gets or sets an annotation's type name.
        /// </summary>
        /// <remarks>
        /// This property is used to get the name of each annotation type 
        /// (e.g. Line, Rectangle, Ellipse). 
        /// <para>
        /// This property is for internal use and is hidden at design and run time.
        /// </para>
        /// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeTextAnnotation_AnnotationType"),
		]
		public override string AnnotationType
		{
			get
			{
				return "Text";
			}
		}

		/// <summary>
		/// Annotation selection points style.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(SelectionPointsStyle.Rectangle),
		ParenthesizePropertyNameAttribute(true),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeSelectionPointsStyle"),
		]
		override internal SelectionPointsStyle SelectionPointsStyle
		{
			get
			{
				return SelectionPointsStyle.Rectangle;
			}
		}

		#endregion

		#endregion

		#region Methods

		#region Painting

		/// <summary>
		/// Paints an annotation object on the specified graphics.
		/// </summary>
		/// <param name="graphics">
		/// A <see cref="ChartGraphics"/> object, used to paint an annotation object.
		/// </param>
		/// <param name="chart">
		/// Reference to the <see cref="Chart"/> owner control.
		/// </param>
        override internal void Paint(Chart chart, ChartGraphics graphics)
		{
			// Get annotation position in relative coordinates
			PointF firstPoint = PointF.Empty;
			PointF anchorPoint = PointF.Empty;
			SizeF size = SizeF.Empty;
			GetRelativePosition(out firstPoint, out size, out anchorPoint);
			PointF	secondPoint = new PointF(firstPoint.X + size.Width, firstPoint.Y + size.Height);

			// Create selection rectangle
			RectangleF selectionRect = new RectangleF(firstPoint, new SizeF(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y));

			// Get text position
			RectangleF	textPosition = new RectangleF(selectionRect.Location, selectionRect.Size);
			if(textPosition.Width < 0)
			{
				textPosition.X = textPosition.Right;
				textPosition.Width = -textPosition.Width;
			}
			if(textPosition.Height < 0)
			{
				textPosition.Y = textPosition.Bottom;
				textPosition.Height = -textPosition.Height;
			}

			// Check if text position is valid
			if( textPosition.IsEmpty ||
				float.IsNaN(textPosition.X) || 
				float.IsNaN(textPosition.Y) || 
				float.IsNaN(textPosition.Right) || 
				float.IsNaN(textPosition.Bottom) )
			{
				return;
			}

			if(this.Common.ProcessModePaint)
			{
				DrawText(graphics, textPosition, false, false);
			}

			if(this.Common.ProcessModeRegions)
			{
				// Add hot region
				if(isEllipse)
				{
                    using (GraphicsPath ellipsePath = new GraphicsPath())
                    {
                        ellipsePath.AddEllipse(textPosition);
                        this.Common.HotRegionsList.AddHotRegion(
                            graphics,
                            ellipsePath,
                            true,
                            ReplaceKeywords(this.ToolTip),
#if Microsoft_CONTROL
						String.Empty,
						String.Empty,
						String.Empty,
#else // Microsoft_CONTROL
 ReplaceKeywords(this.Url),
				ReplaceKeywords(this.MapAreaAttributes),
                ReplaceKeywords(this.PostBackValue),
#endif // Microsoft_CONTROL
 this,
                            ChartElementType.Annotation);
                    }
				}
				else
				{
					this.Common.HotRegionsList.AddHotRegion(
						textPosition,
						ReplaceKeywords(this.ToolTip),
#if Microsoft_CONTROL
						String.Empty,
						String.Empty,
						String.Empty,
#else // Microsoft_CONTROL
                        ReplaceKeywords(this.Url),
					    ReplaceKeywords(this.MapAreaAttributes),
                        ReplaceKeywords(this.PostBackValue),
#endif // Microsoft_CONTROL
						this,
						ChartElementType.Annotation,
						String.Empty);
				}
			}

			// Paint selection handles
			PaintSelectionHandles(graphics, selectionRect, null);
		}

		/// <summary>
		/// Draws text in specified rectangle.
		/// </summary>
		/// <param name="graphics">Chart graphics.</param>
		/// <param name="textPosition">Text position.</param>
		/// <param name="noSpacingForCenteredText">True if text allowed to be outside of position when centered.</param>
		/// <param name="getTextPosition">True if position text must be returned by the method.</param>
		/// <returns>Text actual position if required.</returns>
		internal RectangleF DrawText(ChartGraphics graphics, RectangleF textPosition, bool noSpacingForCenteredText, bool getTextPosition)
		{
			RectangleF	textActualPosition = RectangleF.Empty;

			//***************************************************************
			//** Adjust text position uing text spacing
			//***************************************************************
			bool annotationRelative = false;
			RectangleF	textSpacing = GetTextSpacing(out annotationRelative);
			float spacingScaleX = 1f;
			float spacingScaleY = 1f;
			if(annotationRelative)
			{
				if(textPosition.Width > 25f)
				{
					spacingScaleX = textPosition.Width / 50f;
					spacingScaleX = Math.Max(1f, spacingScaleX);
				}
				if(textPosition.Height > 25f)
				{
					spacingScaleY = textPosition.Height / 50f;
					spacingScaleY = Math.Max(1f, spacingScaleY);
				}
			}

			RectangleF	textPositionWithSpacing = new RectangleF(textPosition.Location, textPosition.Size);
			textPositionWithSpacing.Width -= (textSpacing.Width + textSpacing.X) * spacingScaleX;
			textPositionWithSpacing.X += textSpacing.X * spacingScaleX;
			textPositionWithSpacing.Height -= (textSpacing.Height + textSpacing.Y) * spacingScaleY;
			textPositionWithSpacing.Y += textSpacing.Y * spacingScaleY;

			//***************************************************************
			//** Replace new line characters
			//***************************************************************
			string titleText = this.ReplaceKeywords(this.Text.Replace("\\n", "\n"));

			//***************************************************************
			//** Check if centered text require spacing.
			//** Use only half of the spacing required.
			//** Apply only for 1 line of text.
			//***************************************************************
			if(noSpacingForCenteredText &&
				titleText.IndexOf('\n') == -1)
			{
				if(this.Alignment == ContentAlignment.MiddleCenter ||
					this.Alignment == ContentAlignment.MiddleLeft ||
					this.Alignment == ContentAlignment.MiddleRight)
				{
					textPositionWithSpacing.Y = textPosition.Y;
					textPositionWithSpacing.Height = textPosition.Height;
					textPositionWithSpacing.Height -= textSpacing.Height/2f + textSpacing.Y / 2f;
					textPositionWithSpacing.Y += textSpacing.Y / 2f;
				}
				if(this.Alignment == ContentAlignment.BottomCenter ||
					this.Alignment == ContentAlignment.MiddleCenter ||
					this.Alignment == ContentAlignment.TopCenter)
				{
					textPositionWithSpacing.X = textPosition.X;
					textPositionWithSpacing.Width = textPosition.Width;
					textPositionWithSpacing.Width -= textSpacing.Width/2f + textSpacing.X / 2f;
					textPositionWithSpacing.X += textSpacing.X / 2f;
				}
			}

			// Draw text
			using( Brush textBrush = new SolidBrush(this.ForeColor) )
			{
                using (StringFormat format = new StringFormat(StringFormat.GenericTypographic))
                {
                    //***************************************************************
				    //** Set text format
				    //***************************************************************
                    format.FormatFlags = format.FormatFlags ^ StringFormatFlags.LineLimit;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    if (this.Alignment == ContentAlignment.BottomRight ||
                        this.Alignment == ContentAlignment.MiddleRight ||
                        this.Alignment == ContentAlignment.TopRight)
                    {
                        format.Alignment = StringAlignment.Far;
                    }
                    if (this.Alignment == ContentAlignment.BottomCenter ||
                        this.Alignment == ContentAlignment.MiddleCenter ||
                        this.Alignment == ContentAlignment.TopCenter)
                    {
                        format.Alignment = StringAlignment.Center;
                    }
                    if (this.Alignment == ContentAlignment.BottomCenter ||
                        this.Alignment == ContentAlignment.BottomLeft ||
                        this.Alignment == ContentAlignment.BottomRight)
                    {
                        format.LineAlignment = StringAlignment.Far;
                    }
                    if (this.Alignment == ContentAlignment.MiddleCenter ||
                        this.Alignment == ContentAlignment.MiddleLeft ||
                        this.Alignment == ContentAlignment.MiddleRight)
                    {
                        format.LineAlignment = StringAlignment.Center;
                    }

                    //***************************************************************
                    //** Set shadow color and offset
                    //***************************************************************
                    Color textShadowColor = ChartGraphics.GetGradientColor(this.ForeColor, Color.Black, 0.8);
                    int textShadowOffset = 1;
                    TextStyle textStyle = this.TextStyle;
                    if (textStyle == TextStyle.Shadow &&
                        ShadowOffset != 0)
                    {
                        // Draw shadowed text
                        textShadowColor = ShadowColor;
                        textShadowOffset = ShadowOffset;
                    }

                    if (textStyle == TextStyle.Shadow)
                    {
                        textShadowColor = (textShadowColor.A != 255) ? textShadowColor : Color.FromArgb(textShadowColor.A / 2, textShadowColor);
                    }

                    //***************************************************************
                    //** Get text actual position
                    //***************************************************************
                    if (getTextPosition)
                    {
                        // Measure text size
                        SizeF textSize = graphics.MeasureStringRel(
                            this.ReplaceKeywords(_text.Replace("\\n", "\n")),
                            this.Font,
                            textPositionWithSpacing.Size,
                            format);

                        // Get text position
                        textActualPosition = new RectangleF(textPositionWithSpacing.Location, textSize);
                        if (this.Alignment == ContentAlignment.BottomRight ||
                            this.Alignment == ContentAlignment.MiddleRight ||
                            this.Alignment == ContentAlignment.TopRight)
                        {
                            textActualPosition.X += textPositionWithSpacing.Width - textSize.Width;
                        }
                        if (this.Alignment == ContentAlignment.BottomCenter ||
                            this.Alignment == ContentAlignment.MiddleCenter ||
                            this.Alignment == ContentAlignment.TopCenter)
                        {
                            textActualPosition.X += (textPositionWithSpacing.Width - textSize.Width) / 2f;
                        }
                        if (this.Alignment == ContentAlignment.BottomCenter ||
                            this.Alignment == ContentAlignment.BottomLeft ||
                            this.Alignment == ContentAlignment.BottomRight)
                        {
                            textActualPosition.Y += textPositionWithSpacing.Height - textSize.Height;
                        }
                        if (this.Alignment == ContentAlignment.MiddleCenter ||
                            this.Alignment == ContentAlignment.MiddleLeft ||
                            this.Alignment == ContentAlignment.MiddleRight)
                        {
                            textActualPosition.Y += (textPositionWithSpacing.Height - textSize.Height) / 2f;
                        }

                        // Do not allow text to go outside annotation position
                        textActualPosition.Intersect(textPositionWithSpacing);
                    }

                    RectangleF	absPosition = graphics.GetAbsoluteRectangle(textPositionWithSpacing);
                    Title.DrawStringWithStyle(
                            graphics, 
                            titleText, 
                            this.TextStyle, 
                            this.Font, 
                            absPosition, 
                            this.ForeColor, 
                            textShadowColor, 
                            textShadowOffset, 
                            format, 
                            TextOrientation.Auto
                      );
                }
			}

			return textActualPosition;
		}

		#endregion	// Painting

		#region Text Editing

#if Microsoft_CONTROL

		/// <summary>
		/// Stops editing of the annotation text.
		/// <seealso cref="BeginTextEditing"/>
		/// </summary>
		/// <remarks>
		/// Call this method to cancel text editing, which was started via a call to 
		/// the <see cref="BeginTextEditing"/> method, or after the end-user double-clicks 
		/// on the annotation.
		/// </remarks>
		public void StopTextEditing()
		{
			// Check if text is currently edited
			if(_editTextBox != null)
			{
				// Set annotation text
				this.Text = _editTextBox.Text;

				// Remove and dispose the text box
				try
				{
					_editTextBox.KeyDown -= new KeyEventHandler(OnTextBoxKeyDown);
					_editTextBox.LostFocus -= new EventHandler(OnTextBoxLostFocus);
				}
				catch(SecurityException)
				{
					// Ignore security issues
				}
				
				if(this.Chart.Controls.Contains(_editTextBox))
				{
					TextBox tempControl = null;
					try
					{
						// NOTE: Workaround .Net bug. Issue with appplication closing if
						// active control is removed.
						Form parentForm = this.Chart.FindForm();
						if(parentForm != null)
						{
							tempControl = new TextBox();
							tempControl.Visible = false;
			
							// Add temp. control as active
							parentForm.Controls.Add(tempControl);
							parentForm.ActiveControl = tempControl;
						}
					}
					catch(SecurityException)
					{
						// Ignore security issues
					}

					// Remove text editor
					this.Chart.Controls.Remove(_editTextBox);

					// Dispose temp. text box
					if(tempControl != null)
					{
						tempControl.Dispose();
					}
				}

				// Dispose edit box
				_editTextBox.Dispose();
				_editTextBox = null;

				// Raise notification event
				if(this.Chart != null)
				{
					this.Chart.OnAnnotationTextChanged(this);
				}

				// Update chart
				if(this.Chart != null)
				{
					this.Chart.Invalidate();
					this.Chart.Update();
				}

			}
		}

		/// <summary>
		/// Handles event when focus is lost by the text editing control.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">Event arguments.</param>
		private void OnTextBoxLostFocus(object sender, EventArgs e)
		{
			StopTextEditing();
		}

		/// <summary>
		/// Handles event when key is pressed in the text editing control.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">Event arguments.</param>
		private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Escape)
			{
				// Reset text and stop editing
				_editTextBox.Text = this.Text;
				StopTextEditing();
			}
			else if(e.KeyCode == Keys.Enter &&
				this.IsMultiline == false)
			{
				// Stop editing
				StopTextEditing();
			}
		}

		/// <summary>
		/// Begins editing the annotation's text by an end user.
		/// <seealso cref="StopTextEditing"/>
		/// </summary>
		/// <remarks>
		/// After calling this method, the annotation displays an editing box which allows 
		/// for editing of the annotation's text.
		/// <para>
		/// Call the <see cref="StopTextEditing"/> method to cancel this mode programatically.  
		/// Note that editing ends when the end-user hits the <c>Enter</c> key if multi-line 
		/// is false, or when the end-user clicks outside of the editing box if multi-line is true.
		/// </para>
		/// </remarks>
		public void BeginTextEditing()
		{

			if(this.Chart != null && this.AllowTextEditing)
			{
				// Dispose previous text box
				if(_editTextBox != null)
				{
					if(this.Chart.Controls.Contains(_editTextBox))
					{
						this.Chart.Controls.Remove(_editTextBox);
					}
					_editTextBox.Dispose();
					_editTextBox = null;
				}

				// Create a text box inside the chart
				_editTextBox = new TextBox();
				_editTextBox.Text = this.Text;
				_editTextBox.Multiline = this.IsMultiline;
				_editTextBox.Font = this.Font;
				_editTextBox.BorderStyle = BorderStyle.FixedSingle;
				_editTextBox.BackColor = Color.FromArgb(255, (this.BackColor.IsEmpty) ? Color.White : this.BackColor);
				_editTextBox.ForeColor = Color.FromArgb(255, this.ForeColor);

				// Calculate text position in relative coordinates
				PointF firstPoint = PointF.Empty;
				PointF anchorPoint = PointF.Empty;
				SizeF size = SizeF.Empty;
				GetRelativePosition(out firstPoint, out size, out anchorPoint);
				PointF	secondPoint = new PointF(firstPoint.X + size.Width, firstPoint.Y + size.Height);
				RectangleF textPosition = new RectangleF(firstPoint, new SizeF(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y));
				if(textPosition.Width < 0)
				{
					textPosition.X = textPosition.Right;
					textPosition.Width = -textPosition.Width;
				}
				if(textPosition.Height < 0)
				{
					textPosition.Y = textPosition.Bottom;
					textPosition.Height = -textPosition.Height;
				}

				// Set text control position in pixels
				if(GetGraphics() != null)
				{
					// Convert point to relative coordinates
					textPosition = GetGraphics().GetAbsoluteRectangle(textPosition);
				}

				// Adjust Location and Size
				if(this.IsMultiline)
				{
					textPosition.X -= 1;
					textPosition.Y -= 1;
					textPosition.Width += 2;
					textPosition.Height += 2;
				}
				else
				{
					textPosition.Y += textPosition.Height / 2f - _editTextBox.Size.Height / 2f;
				}
                _editTextBox.Location = Point.Round(textPosition.Location);
				_editTextBox.Size = Size.Round(textPosition.Size);

				// Add control to the chart
				this.Chart.Controls.Add(_editTextBox);
				try
				{
					_editTextBox.SelectAll();
					_editTextBox.Focus();
				}
				catch(SecurityException)
				{
					// Ignore security issues
				}

				try
				{
					// Set text box event hanlers
					_editTextBox.KeyDown += new KeyEventHandler(OnTextBoxKeyDown);
					_editTextBox.LostFocus += new EventHandler(OnTextBoxLostFocus);
				}
				catch(SecurityException)
				{
					// Ignore security issues
				}
			}
		}

#endif // Microsoft_CONTROL

		#endregion	// Text Editing

		#region Content Size

		/// <summary>
		/// Gets text annotation content size based on the text and font.
		/// </summary>
		/// <returns>Annotation content position.</returns>
		override internal RectangleF GetContentPosition()
		{
			// Return pre calculated value
			if(!contentSize.IsEmpty)
			{
				return new RectangleF(float.NaN, float.NaN, contentSize.Width, contentSize.Height);
			}

			// Create temporary bitmap based chart graphics if chart was not 
			// rendered yet and the graphics was not created.
			// NOTE: Fix for issue #3978.
			Graphics		graphics = null;
System.Drawing.Image		graphicsImage = null;
			ChartGraphics	tempChartGraph = null;
			if(GetGraphics() == null &&	this.Common != null)
			{
                graphicsImage = new System.Drawing.Bitmap(Common.ChartPicture.Width, Common.ChartPicture.Height);
				graphics = Graphics.FromImage( graphicsImage );
				tempChartGraph = new ChartGraphics( Common );
				tempChartGraph.Graphics = graphics;
				tempChartGraph.SetPictureSize( Common.ChartPicture.Width, Common.ChartPicture.Height );
				this.Common.graph = tempChartGraph;
			}

			// Calculate content size
			RectangleF result = RectangleF.Empty;
			if(GetGraphics() != null && this.Text.Trim().Length > 0)
			{
				// Measure text using current font and slightly increase it
                contentSize = GetGraphics().MeasureString(
                     "W" + this.ReplaceKeywords(this.Text.Replace("\\n", "\n")),
                     this.Font,
                     new SizeF(2000, 2000),
                     StringFormat.GenericTypographic);

				contentSize.Height *= 1.04f;

				// Convert to relative coordinates
				contentSize = GetGraphics().GetRelativeSize(contentSize);

				// Add spacing
				bool annotationRelative = false;
				RectangleF	textSpacing = GetTextSpacing(out annotationRelative);
				float spacingScaleX = 1f;
				float spacingScaleY = 1f;
				if(annotationRelative)
				{
					if(contentSize.Width > 25f)
					{
						spacingScaleX = contentSize.Width / 25f;
						spacingScaleX = Math.Max(1f, spacingScaleX);
					}
					if(contentSize.Height > 25f)
					{
						spacingScaleY = contentSize.Height / 25f;
						spacingScaleY = Math.Max(1f, spacingScaleY);
					}
				}

				contentSize.Width += (textSpacing.X + textSpacing.Width) * spacingScaleX;
				contentSize.Height += (textSpacing.Y + textSpacing.Height) * spacingScaleY;

				result = new RectangleF(float.NaN, float.NaN, contentSize.Width, contentSize.Height);
			}

			// Dispose temporary chart graphics
			if(tempChartGraph != null)
			{
				tempChartGraph.Dispose();
				graphics.Dispose();
				graphicsImage.Dispose();
				this.Common.graph = null;
			}

			return result;
		}

		/// <summary>
		/// Gets text spacing on four different sides in relative coordinates.
		/// </summary>
		/// <param name="annotationRelative">Indicates that spacing is in annotation relative coordinates.</param>
		/// <returns>Rectangle with text spacing values.</returns>
		internal virtual RectangleF GetTextSpacing(out bool annotationRelative)
		{
			annotationRelative = false;
			RectangleF rect = new RectangleF(3f, 3f, 3f, 3f);
			if(GetGraphics() != null)
			{
				rect = GetGraphics().GetRelativeRectangle(rect);
			}
			return rect;
		}

		#endregion

		#region Placement Methods

#if Microsoft_CONTROL

		/// <summary>
		/// Ends user placement of an annotation.
		/// </summary>
		/// <remarks>
		/// Ends an annotation placement operation previously started by a 
		/// <see cref="Annotation.BeginPlacement"/> method call.
		/// <para>
		/// Calling this method is not required, since placement will automatically
		/// end when an end user enters all required points. However, it is useful when an annotation 
		/// placement operation needs to be aborted for some reason.
		/// </para>
		/// </remarks>
		override public void EndPlacement()
		{
			// Check if text editing is allowed
			// Maybe changed later in the EndPlacement method.
			bool allowTextEditing = this.AllowTextEditing;

			// Call base class
			base.EndPlacement();

			// Begin text editing
			if(this.Chart != null)
			{
				this.Chart.Annotations.lastClickedAnnotation = this;
				if(allowTextEditing)
				{
					BeginTextEditing();
				}
			}
		}

#endif // Microsoft_CONTROL

        #endregion // Placement Methods

        #endregion	// Methods
    }

	/// <summary>
	/// The <b>AnnotationSmartLabelStyle</b> class is used to store an annotation's smart 
	/// labels properties.
	/// <seealso cref="Annotation.SmartLabelStyle"/>
	/// </summary>
	/// <remarks>
	/// This class is derived from the <b>SmartLabelStyle</b> class
	/// used for <b>Series</b> objects.
	/// </remarks>
	[
		DefaultProperty("Enabled"),
		SRDescription("DescriptionAttributeAnnotationSmartLabelsStyle_AnnotationSmartLabelsStyle"),
		TypeConverter(typeof(NoNameExpandableObjectConverter)),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class AnnotationSmartLabelStyle : SmartLabelStyle
	{
		#region Constructors and initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public AnnotationSmartLabelStyle()
		{
			this.chartElement = null;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="chartElement">
		/// Chart element this style belongs to.
		/// </param>
		public AnnotationSmartLabelStyle(Object chartElement) : base(chartElement)
		{
		}

		#endregion

		#region Non Applicable Appearance Attributes (set as Non-Browsable)


		/// <summary>
		/// Callout style of the repositioned smart labels.
		/// </summary>
		/// <remarks>
		/// This method is for internal use and is hidden at design time and runtime.
		/// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(LabelCalloutStyle.Underlined),
		SRDescription("DescriptionAttributeCalloutStyle3"),
		]
		override public LabelCalloutStyle CalloutStyle
		{
			get
			{
				return base.CalloutStyle;
			}
			set
			{
				base.CalloutStyle = value;
			}
		}

		/// <summary>
		/// Label callout line color.
		/// </summary>
		/// <remarks>
        /// This method is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeCalloutLineColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		override public Color CalloutLineColor
		{
			get
			{
				return base.CalloutLineColor;
			}
			set
			{
				base.CalloutLineColor = value;
			}
		}

		/// <summary>
		/// Label callout line style.
		/// </summary>
		/// <remarks>
        /// This method is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(ChartDashStyle.Solid),
        SRDescription("DescriptionAttributeLineDashStyle"),
			#if !Microsoft_CONTROL
			PersistenceMode(PersistenceMode.Attribute)
			#endif
		]
		override public ChartDashStyle CalloutLineDashStyle
		{
			get
			{
				return base.CalloutLineDashStyle;
			}
			set
			{
				base.CalloutLineDashStyle = value;
			}
		}

		/// <summary>
		/// Label callout back color. Applies to the Box style only.
		/// </summary>
		/// <remarks>
        /// This method is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(typeof(Color), "Transparent"),
        SRDescription("DescriptionAttributeCalloutBackColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		override public Color CalloutBackColor
		{
			get
			{
				return base.CalloutBackColor;
			}
			set
			{
				base.CalloutBackColor = value;
			}
		}

		/// <summary>
		/// Label callout line width.
		/// </summary>
		/// <remarks>
        /// This method is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(1),
        SRDescription("DescriptionAttributeLineWidth"),
		]
		override public int CalloutLineWidth
		{
			get
			{
				return base.CalloutLineWidth;
			}
			set
			{
				base.CalloutLineWidth = value;
			}
		}

		/// <summary>
		/// Label callout line anchor cap.
		/// </summary>
		/// <remarks>
        /// This method is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(LineAnchorCapStyle.Arrow),
		SRDescription("DescriptionAttributeCalloutLineAnchorCapStyle"),
		]
		override public LineAnchorCapStyle CalloutLineAnchorCapStyle
		{
			get
			{
				return base.CalloutLineAnchorCapStyle;
			}
			set
			{
				base.CalloutLineAnchorCapStyle = value;
			}
		}

		#endregion 
	}
}
