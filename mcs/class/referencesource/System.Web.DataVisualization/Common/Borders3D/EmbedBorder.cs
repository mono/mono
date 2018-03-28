//-------------------------------------------------------------
// <copyright company=�Microsoft Corporation�>
//   Copyright � Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		EmbedBorder.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Borders3D
//
//	Classes:	EmbedBorder, FrameTitle1Border, FrameTitle2Border,
//				FrameTitle3Border, FrameTitle4Border, FrameTitle5Border,
//				FrameTitle6Border, FrameTitle7Border, FrameTitle8Border,
//				FrameThin2Border, FrameThin3Border, FrameThin4Border, 
//				FrameThin5Border, FrameThin6Border, FrameThin1Border, 
//				RaisedBorder, SunkenBorder
//
//  Purpose:	Classes that implement different 3D border styles.
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

#if WINFORMS_CONTROL
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

#if WINFORMS_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Borders3D
#else
	namespace System.Web.UI.DataVisualization.Charting.Borders3D
#endif
{
	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle1Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle1Border()
		{
			sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle1";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }
		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override RectangleF GetTitlePositionInBorder()
		{
			return new RectangleF(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle2Border : FrameThin2Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle2Border()
		{
			sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle2";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override RectangleF GetTitlePositionInBorder()
		{
			return new RectangleF(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle3Border : FrameThin3Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle3Border()
		{
			sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle3";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override RectangleF GetTitlePositionInBorder()
		{
			return new RectangleF(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}
		#endregion
	}
	
	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle4Border : FrameThin4Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle4Border()
		{
			sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle4";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override RectangleF GetTitlePositionInBorder()
		{
			return new RectangleF(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}
	
		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle5Border : FrameThin5Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle5Border()
		{
			sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize*2f);
			this.drawScrews = true;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle5";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override RectangleF GetTitlePositionInBorder()
		{
			return new RectangleF(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}
	
		#endregion
	}
	
	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle6Border : FrameThin6Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle6Border()
		{
			sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle6";}}

        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SizeF(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override RectangleF GetTitlePositionInBorder()
		{
			return new RectangleF(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}
	
		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle7Border : FrameTitle1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle7Border()
		{
			this.sizeRightBottom = new SizeF(0, sizeRightBottom.Height);
			float[] corners = {15f, 1f, 1f, 1f, 1f, 15f, 15f, 15f};
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle7";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                this.sizeRightBottom = new SizeF(0, sizeRightBottom.Height);
                float largeRadius = 15f * resolution / 96.0f;
                float smallRadius = 1 * resolution / 96.0f;
                float[] corners = { largeRadius, smallRadius, smallRadius, smallRadius, smallRadius, largeRadius, largeRadius, largeRadius };
                innerCorners = corners;
            }
        }

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle8Border : FrameTitle1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle8Border()
		{
			this.sizeLeftTop = new SizeF(0, sizeLeftTop.Height);
			this.sizeRightBottom = new SizeF(0, sizeRightBottom.Height);
			float[] corners = {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle8";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;

                this.sizeLeftTop = new SizeF(0, sizeLeftTop.Height);
                this.sizeRightBottom = new SizeF(0, sizeRightBottom.Height);
                float radius = 1 * resolution / 96.0f;
                float[] corners = { radius, radius, radius, radius, radius, radius, radius, radius };
                innerCorners = corners;
            }
        }

		#endregion
	}
	
	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin2Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin2Border()
		{
			float[] corners = {15f, 15f, 15f, 1f, 1f, 1f, 1f, 15f};
			cornerRadius = corners;
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin2";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;

                float largeRadius = 15f * resolution / 96.0f;
                float smallRadius = 1 * resolution / 96.0f;
                float[] corners = { largeRadius, largeRadius, largeRadius, smallRadius, smallRadius, smallRadius, smallRadius, largeRadius };
                cornerRadius = corners;
                innerCorners = corners;
            }
        }

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin3Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin3Border()
		{
			float[] corners = {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
			cornerRadius = corners;
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin3";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                float radius = resolution / 96.0f;
                float[] corners = { radius, radius, radius, radius, radius, radius, radius, radius };
                cornerRadius = corners;
                innerCorners = corners;
            }
        }

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin4Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin4Border()
		{
			float[] corners = {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
			cornerRadius = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin4";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                float radius = 1f * resolution / 96.0f;
                cornerRadius = new float[] { radius, radius, radius, radius, radius, radius, radius, radius };
            }
        }

        #endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin5Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin5Border()
		{
			drawScrews = true;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin5";}}

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin6Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin6Border()
		{
			float[] corners = {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin6";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                float radius = resolution / 96.0f;
                float[] corners = { radius, radius, radius, radius, radius, radius, radius, radius };
                innerCorners = corners;
            }
        }

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin1Border : RaisedBorder
	{
		#region Border properties and methods

		/// <summary>
		/// Inner corners radius array
		/// </summary>
        internal float[] innerCorners = { 15f, 15f, 15f, 15f, 15f, 15f, 15f, 15f };

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin1Border()
		{
			sizeLeftTop = new SizeF(defaultRadiusSize * .8f, defaultRadiusSize * .8f);
			sizeRightBottom = new SizeF(defaultRadiusSize * .8f, defaultRadiusSize * .8f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin1";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                float radius = 15.0f * resolution / 96.0f;
                innerCorners = new float[] { radius, radius, radius, radius, radius, radius, radius, radius };
                sizeLeftTop = new SizeF(defaultRadiusSize * .8f, defaultRadiusSize * .8f);
                sizeRightBottom = new SizeF(defaultRadiusSize * .8f, defaultRadiusSize * .8f);

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
		public override void DrawBorder(
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
			drawBottomShadow = true;
			sunken = false;
			outsideShadowRate = .9f;
			drawOutsideTopLeftShadow = false;
			bool oldScrewsFlag = this.drawScrews;
			this.drawScrews = false;
			base.DrawBorder(
				graph, 
				borderSkin, 
				rect, 
				borderSkin.BackColor, 
				borderSkin.BackHatchStyle, 
				borderSkin.BackImage, 
				borderSkin.BackImageWrapMode, 
				borderSkin.BackImageTransparentColor, 
				borderSkin.BackImageAlignment, 
				borderSkin.BackGradientStyle, 
				borderSkin.BackSecondaryColor, 
				borderSkin.BorderColor, 
				borderSkin.BorderWidth, 
				borderSkin.BorderDashStyle);

			this.drawScrews = oldScrewsFlag;
			rect.X += sizeLeftTop.Width;
			rect.Y += sizeLeftTop.Height;
			rect.Width -= sizeRightBottom.Width + sizeLeftTop.Width;
			rect.Height -= sizeRightBottom.Height + sizeLeftTop.Height;
			if(rect.Width > 0 && rect.Height > 0 )
			{
				float[] oldCorners = new float[8];
				oldCorners = (float[])cornerRadius.Clone();
				cornerRadius = innerCorners;
				drawBottomShadow = false;
				sunken = true;
				drawOutsideTopLeftShadow = true;
				outsideShadowRate = 1.4f;
				Color oldPageColor = borderSkin.PageColor;
				borderSkin.PageColor = Color.Transparent;
				base.DrawBorder(
					graph, 
					borderSkin,
					rect, 
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
					borderDashStyle	);
				borderSkin.PageColor = oldPageColor;
				cornerRadius = oldCorners;
			}
		}

		#endregion
	}


	/// <summary>
	/// Implements raised border.
	/// </summary>
	internal class RaisedBorder : SunkenBorder
	{
		#region Border properties and methods

		/// <summary>
		/// Public constructor
		/// </summary>
		public RaisedBorder()
		{
			sunken = false;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "Raised";}}

		#endregion
	}

	/// <summary>
	/// Implements embed 3D border.
	/// </summary>
	internal class SunkenBorder : IBorderType
	{
		#region Border properties and methods

		/// <summary>
		/// Radius for rounded rectangle
		/// </summary>
        internal float defaultRadiusSize = 15f;

		/// <summary>
		/// Outside shadow rate
		/// </summary>
        internal float outsideShadowRate = .9f;
		
		/// <summary>
		/// Indicates that sunken shadows should be drawn
		/// </summary>
        internal bool sunken = true;

		/// <summary>
		/// Indicates that bottom shadow should be drawn
		/// </summary>
        internal bool drawBottomShadow = true;

		/// <summary>
		/// Indicates that top left outside dark shadow must be drawn
		/// </summary>
        internal bool drawOutsideTopLeftShadow = false;

		/// <summary>
		/// Array of corner radius
		/// </summary>
        internal float[] cornerRadius = { 15f, 15f, 15f, 15f, 15f, 15f, 15f, 15f };

		/// <summary>
		/// Border top/left size 
		/// </summary>
        internal SizeF sizeLeftTop = SizeF.Empty;

		/// <summary>
		/// Border right/bottom size
		/// </summary>
        internal SizeF sizeRightBottom = SizeF.Empty;

		/// <summary>
		/// Indicates that ----s should be drawn in the corners of the frame
		/// </summary>
        internal bool drawScrews = false;


        internal float resolution = 96f;


		/// <summary>
		/// Public constructor
		/// </summary>
		public SunkenBorder()
		{
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public virtual string Name			{ get{ return "Sunken";}}


        public virtual float Resolution
        {
            set
            {
                resolution = value;
                defaultRadiusSize = 15 * resolution / 96;
                //X = defaultRadiusSize;
                //Y = defaultRadiusSize;
                cornerRadius = new float[] { defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize };
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
        /// Adjust areas rectangle coordinate to fit the 3D border
        /// </summary>
        /// <param name="graph">Graphics to draw the border on.</param>
        /// <param name="areasRect">Position to adjust.</param>
		public virtual void AdjustAreasPosition(ChartGraphics graph, ref RectangleF areasRect)
		{
			SizeF relSizeLeftTop = new SizeF(sizeLeftTop);
			SizeF relSizeRightBottom = new SizeF(sizeRightBottom);
			relSizeLeftTop.Width += defaultRadiusSize * 0.7f;
			relSizeLeftTop.Height += defaultRadiusSize * 0.85f;
			relSizeRightBottom.Width += defaultRadiusSize * 0.7f;
			relSizeRightBottom.Height += defaultRadiusSize * 0.7f;
			relSizeLeftTop = graph.GetRelativeSize(relSizeLeftTop);
			relSizeRightBottom = graph.GetRelativeSize(relSizeRightBottom);

			if(relSizeLeftTop.Width > 30f)
				relSizeLeftTop.Width = 0;
			if(relSizeLeftTop.Height > 30f)
				relSizeLeftTop.Height = 0;
			if(relSizeRightBottom.Width > 30f)
				relSizeRightBottom.Width = 0;
			if(relSizeRightBottom.Height > 30f)
				relSizeRightBottom.Height = 0;


			areasRect.X += relSizeLeftTop.Width;
			areasRect.Width -= (float)Math.Min(areasRect.Width, relSizeLeftTop.Width + relSizeRightBottom.Width);
			areasRect.Y += relSizeLeftTop.Height;
			areasRect.Height -= (float)Math.Min(areasRect.Height, relSizeLeftTop.Height + relSizeRightBottom.Height);

			if(areasRect.Right > 100f)
			{
				if(areasRect.Width > 100f - areasRect.Right)
					areasRect.Width -= 100f - areasRect.Right;
				else
					areasRect.X -= 100f - areasRect.Right;
			}
			if(areasRect.Bottom > 100f)
			{
				if(areasRect.Height > 100f - areasRect.Bottom)
					areasRect.Height -= 100f - areasRect.Bottom;
				else
					areasRect.Y -= 100f - areasRect.Bottom;

			}
		}

        /// <summary>
        /// Draws 3D border
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
			float colorDarkeningIndex = 0.3f + (0.4f * (borderSkin.PageColor.R + borderSkin.PageColor.G + borderSkin.PageColor.B) / 765f);
			Color	shadowColor = Color.FromArgb(
				(int)(backColor.R*colorDarkeningIndex), 
				(int)(backColor.G*colorDarkeningIndex), 
				(int)(backColor.B*colorDarkeningIndex));

			colorDarkeningIndex += 0.2f;
			Color	shadowLightColor = Color.FromArgb(
				(int)(borderSkin.PageColor.R*colorDarkeningIndex), 
				(int)(borderSkin.PageColor.G*colorDarkeningIndex), 
				(int)(borderSkin.PageColor.B*colorDarkeningIndex));
			if(borderSkin.PageColor == Color.Transparent)
			{
				shadowLightColor = Color.FromArgb(60, 0, 0, 0);
			}
			
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

			if(drawOutsideTopLeftShadow)
			{
				// Top/Left outside shadow
				shadowRect = absolute;
				shadowRect.X -= radius * 0.3f;
				shadowRect.Y -= radius * 0.3f;
				shadowRect.Width -= radius * .3f;
				shadowRect.Height -= radius * .3f;
				graph.DrawRoundedRectShadowAbs(shadowRect, cornerRadius, radius, Color.FromArgb(128, Color.Black), borderSkin.PageColor, outsideShadowRate);
			}

			// Bottom/Right outside shadow
			shadowRect = absolute;
			shadowRect.X += radius * 0.3f;
			shadowRect.Y += radius * 0.3f;
			shadowRect.Width -= radius * .3f;
			shadowRect.Height -= radius * .3f;
			graph.DrawRoundedRectShadowAbs(shadowRect, cornerRadius, radius, shadowLightColor, borderSkin.PageColor, outsideShadowRate);

			// Background
			shadowRect = absolute;
			shadowRect.Width -= radius * .3f;
			shadowRect.Height -= radius * .3f;
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

			// Draw ----s imitation in the corners of the farame
			if(drawScrews)
			{
				// Left/Top ----
				RectangleF	screwRect = RectangleF.Empty;
				float offset = radius * 0.4f;
				screwRect.X = shadowRect.X + offset;
				screwRect.Y = shadowRect.Y + offset;
				screwRect.Width = radius * 0.55f;
				screwRect.Height = screwRect.Width;
				DrawScrew(graph, screwRect);

				// Right/Top ----
				screwRect.X = shadowRect.Right - offset - screwRect.Width;
				DrawScrew(graph, screwRect);

				// Right/Bottom ----
				screwRect.X = shadowRect.Right - offset - screwRect.Width;
				screwRect.Y = shadowRect.Bottom - offset - screwRect.Height;
				DrawScrew(graph, screwRect);
		
				// Left/Bottom ----
				screwRect.X = shadowRect.X + offset;
				screwRect.Y = shadowRect.Bottom - offset - screwRect.Height;
				DrawScrew(graph, screwRect);
			}

			// Bottom/Right inner shadow
			Region	innerShadowRegion = null;
			if(drawBottomShadow)
			{
				shadowRect = absolute;
				shadowRect.Width -= radius * .3f;
				shadowRect.Height -= radius * .3f;
				innerShadowRegion = new Region(
					graph.CreateRoundedRectPath(
					new RectangleF(
					shadowRect.X - radius, 
					shadowRect.Y - radius, 
					shadowRect.Width + 0.5f*radius, 
					shadowRect.Height + 0.5f*radius), 
					cornerRadius));
				innerShadowRegion.Complement(graph.CreateRoundedRectPath(shadowRect, cornerRadius));
				graph.Clip = innerShadowRegion;

				shadowRect.X -= 0.5f*radius;
				shadowRect.Width += 0.5f*radius;
				shadowRect.Y -= 0.5f*radius;
				shadowRect.Height += 0.5f*radius;

				graph.DrawRoundedRectShadowAbs(
					shadowRect, 
					cornerRadius,
					radius,
					Color.Transparent, 
					Color.FromArgb(175, (sunken) ? Color.White : shadowColor), 
					1.0f);
				graph.Clip = new Region();
			}

			// Top/Left inner shadow					
			shadowRect = absolute;
			shadowRect.Width -= radius * .3f;
			shadowRect.Height -= radius * .3f;
			innerShadowRegion = new Region(
				graph.CreateRoundedRectPath(
				new RectangleF(
				shadowRect.X + radius*.5f, 
				shadowRect.Y + radius*.5f, 
				shadowRect.Width - .2f*radius, 
				shadowRect.Height - .2f*radius), 
				cornerRadius));

			RectangleF shadowWithOffset = shadowRect;
			shadowWithOffset.Width += radius;
			shadowWithOffset.Height += radius;
			innerShadowRegion.Complement(graph.CreateRoundedRectPath(shadowWithOffset, cornerRadius));
			
			innerShadowRegion.Intersect(graph.CreateRoundedRectPath(shadowRect, cornerRadius));
			graph.Clip = innerShadowRegion;
			graph.DrawRoundedRectShadowAbs(
				shadowWithOffset, 
				cornerRadius, 
				radius, 
				Color.Transparent, 
				Color.FromArgb(175, (sunken) ? shadowColor : Color.White), 
				1.0f);
			graph.Clip = new Region();

		}

		/// <summary>
		/// Helper function, which draws a ---- on the frame
		/// </summary>
		/// <param name="graph">Chart graphics to use.</param>
		/// <param name="rect">---- position.</param>
		private void DrawScrew(ChartGraphics graph, RectangleF rect)
		{
			// Draw ----
			Pen screwPen = new Pen(Color.FromArgb(128,255,255,255), 1);
			graph.DrawEllipse(screwPen, rect.X, rect.Y, rect.Width, rect.Height);
            graph.DrawLine(screwPen, rect.X + 2 * resolution / 96.0f, rect.Y + rect.Height - 2 * resolution / 96.0f, rect.Right - 2 * resolution / 96.0f, rect.Y + 2 * resolution / 96.0f);
			screwPen = new Pen(Color.FromArgb(128, Color.Black), 1);
            graph.DrawEllipse(screwPen, rect.X + 1 * resolution / 96.0f, rect.Y + 1 * resolution / 96.0f, rect.Width, rect.Height);
            graph.DrawLine(screwPen, rect.X + 3 * resolution / 96.0f, rect.Y + rect.Height - 1 * resolution / 96.0f, rect.Right - 1 * resolution / 96.0f, rect.Y + 3 * resolution / 96.0f);
		}

		#endregion
	}
}
