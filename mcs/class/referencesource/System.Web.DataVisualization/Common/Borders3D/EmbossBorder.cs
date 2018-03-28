//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		EmbossBorder.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Borders3D
//
//	Classes:	EmbossBorder
//
//  Purpose:	Class that implements Emboss 3D border style.
//
//	Reviewed:	AG - August 7, 2002
//
//===================================================================

#region Used namespaces

using System;
using System.Resources;
using System.Reflection;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel.Design;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;

#else
	using System.Web.UI.DataVisualization.Charting;	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.Utilities;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Borders3D
#else
	namespace System.Web.UI.DataVisualization.Charting.Borders3D
#endif
	{

	/// <summary>
	/// Implements emboss 3D border.
	/// </summary>
	internal class EmbossBorder : IBorderType
	{
		#region Border properties and methods

		/// <summary>
		/// Default border radius size (relative)
		/// </summary>
		public float	defaultRadiusSize = 15f;

        public float resolution = 96f;

        /// <summary>
        /// Array of corner radius
        /// </summary>
        internal float[] cornerRadius = { 15f, 15f, 15f, 15f, 15f, 15f, 15f, 15f };


		/// <summary>
		/// Public constructor
		/// </summary>
		public EmbossBorder()
		{
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public virtual string Name			{ get{ return "Emboss";}}


        public virtual float Resolution
        {
            set
            {
                resolution = value;
                float radius = 15f * value / 96.0f;
                defaultRadiusSize = radius;
                cornerRadius = new float[] { radius, radius, radius, radius, radius, radius, radius, radius };
            }

        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public virtual RectangleF GetTitlePositionInBorder()
		{
			return RectangleF.Empty;
		}

        /// <summary>
        /// Adjust areas rectangle coordinate to fit the 3D border.
        /// </summary>
        /// <param name="graph">Graphics to draw the border on.</param>
        /// <param name="areasRect">Position to adjust.</param>
		public virtual void AdjustAreasPosition(ChartGraphics graph, ref RectangleF areasRect)
		{
			SizeF	borderSize = new SizeF(defaultRadiusSize/2f, defaultRadiusSize/2f);
			borderSize = graph.GetRelativeSize(borderSize);

			// Do not do anything if rectangle is too small
			if(borderSize.Width < 30f)
			{
				areasRect.X += borderSize.Width;
				areasRect.Width -= (float)Math.Min(areasRect.Width, borderSize.Width * 2.5f);
			}

			if(borderSize.Height < 30f)
			{
				areasRect.Y += borderSize.Height;
				areasRect.Height -= (float)Math.Min(areasRect.Height, borderSize.Height * 2.5f);
			}

			if(areasRect.X + areasRect.Width > 100f)
			{
				areasRect.X -= 100f - areasRect.Width;
			}
			if(areasRect.Y + areasRect.Height > 100f)
			{
				areasRect.Y -= 100f - areasRect.Height;
			}
		}

        /// <summary>
        /// Draws 3D border.
        /// </summary>
        /// <param name="graph">Graphics to draw the border on.</param>
        /// <param name="borderSkin">Border skin object.</param>
        /// <param name="rect">Rectangle of the border.</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
        /// <param name="backGradientStyle">Gradient type</param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
		public virtual void DrawBorder(
			ChartGraphics graph, 
			BorderSkin borderSkin,
			RectangleF rect, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle)	
		{
			RectangleF absolute = graph.Round( rect );
			RectangleF shadowRect = absolute;

			// Calculate shadow colors (0.2 - 0.6)
			float colorDarkeningIndex = 0.2f + (0.4f * (borderSkin.PageColor.R + borderSkin.PageColor.G + borderSkin.PageColor.B) / 765f);
			Color	shadowColor = Color.FromArgb(
				(int)(borderSkin.PageColor.R*colorDarkeningIndex), 
				(int)(borderSkin.PageColor.G*colorDarkeningIndex), 
				(int)(borderSkin.PageColor.B*colorDarkeningIndex));
			if(borderSkin.PageColor == Color.Transparent)
			{
				shadowColor = Color.FromArgb(60, 0, 0, 0);
			}

			colorDarkeningIndex += 0.2f;
			Color	shadowLightColor = Color.FromArgb(
				(int)(borderSkin.PageColor.R*colorDarkeningIndex), 
				(int)(borderSkin.PageColor.G*colorDarkeningIndex), 
				(int)(borderSkin.PageColor.B*colorDarkeningIndex));

			// Calculate rounded rect radius
			float	radius = defaultRadiusSize;
			radius = (float)Math.Max(radius, 2f * resolution / 96.0f);
			radius = (float)Math.Min(radius, rect.Width/2f);
			radius = (float)Math.Min(radius, rect.Height/2f);
			radius = (float)Math.Ceiling(radius);

			// Fill page background color
            using (Brush brush = new SolidBrush(borderSkin.PageColor))
            {
                graph.FillRectangle(brush, rect);
            }

			// Top/Left shadow
			shadowRect = absolute;
			shadowRect.Width -= radius * .3f;
			shadowRect.Height -= radius * .3f;
			graph.DrawRoundedRectShadowAbs(shadowRect, cornerRadius, radius + 1 * resolution / 96.0f, shadowLightColor, borderSkin.PageColor, 1.4f);

			// Bottom/Right shadow
			shadowRect = absolute;
			shadowRect.X = absolute.X + radius / 3f;
			shadowRect.Y = absolute.Y + radius / 3f;
			shadowRect.Width -= radius / 3.5f;
			shadowRect.Height -= radius / 3.5f;
			graph.DrawRoundedRectShadowAbs(shadowRect, cornerRadius, radius, shadowColor, borderSkin.PageColor, 1.3f);

			// Draw Background
			shadowRect = absolute;
			shadowRect.X = absolute.X + 3f * resolution / 96.0f;
            shadowRect.Y = absolute.Y + 3f * resolution / 96.0f;
			shadowRect.Width -= radius * .75f;
			shadowRect.Height -= radius * .75f;
			GraphicsPath path = graph.CreateRoundedRectPath(shadowRect, cornerRadius);
			graph.DrawPathAbs(
				path, 
				backColor, 
				backHatchStyle, 
				backImage, 
				backImageWrapMode, 
				backImageTransparentColor,
				backImageAlign,
				backGradientStyle, 
				backSecondaryColor, 
				borderColor, 
				borderWidth, 
				borderDashStyle, 
				PenAlignment.Inset );

			// Dispose Graphic path
			if( path != null )
				path.Dispose();

			// Bottom/Right inner shadow
			Region	innerShadowRegion = new Region(
				graph.CreateRoundedRectPath(
				new RectangleF(
				shadowRect.X - radius, 
				shadowRect.Y - radius, 
				shadowRect.Width + radius - radius*0.25f, 
				shadowRect.Height + radius - radius*0.25f), 
				cornerRadius));
			innerShadowRegion.Complement(graph.CreateRoundedRectPath(shadowRect, cornerRadius));
			graph.Clip = innerShadowRegion;
			graph.DrawRoundedRectShadowAbs(
				shadowRect, 
				cornerRadius,
				radius, 
				Color.Transparent, 
				Color.FromArgb(128, Color.Gray), 
				.5f);
			graph.Clip = new Region();
		}

		#endregion
	}
}
