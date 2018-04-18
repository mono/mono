//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		SmartLabelStyle.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	SmartLabelStyle, SmartLabelStyle
//
//  Purpose:	Smart Labels are used to avoid data point's labels 
//				overlapping. SmartLabelStyle class is exposed from 
//				the Series and Annotation classes and allows enabling 
//              and adjusting of SmartLabelStyle algorithm. SmartLabelStyle class 
//              exposes a set of helper utility methods and store 
//              information about labels in a chart area.
//
//	Reviewed:	AG - Microsoft 14, 2007
//
//===================================================================

#region Used namespaces
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
#if Microsoft_CONTROL

using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.Windows.Forms.DataVisualization.Charting.Utilities;
using System.Windows.Forms.DataVisualization.Charting.Borders3D;
using System.Windows.Forms.DataVisualization.Charting;

using System.Globalization;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Windows.Forms.Design;
#else
	using System.Web;
	using System.Web.UI;
	using System.Globalization;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
	using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI.DataVisualization.Charting.Borders3D;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
		#region Enumerations

		/// <summary>
		/// Line anchor cap style.
		/// </summary>
		[
		SRDescription("DescriptionAttributeLineAnchorCapStyle_LineAnchorCapStyle")
		]
		public enum LineAnchorCapStyle
		{
			/// <summary>
			/// No line anchor cap.
			/// </summary>
			None,
			/// <summary>
			/// Arrow line anchor cap.
			/// </summary>
			Arrow,
			/// <summary>
			/// Diamond line anchor cap.
			/// </summary>
			Diamond,
			/// <summary>
			/// Square line anchor cap.
			/// </summary>
			Square,
			/// <summary>
			/// Round line anchor cap.
			/// </summary>
			Round
		}

		/// <summary>
		/// Data point label callout style.
		/// </summary>
		[
		SRDescription("DescriptionAttributeLabelCalloutStyle_LabelCalloutStyle")
		]
		public enum LabelCalloutStyle
		{
			/// <summary>
			/// Label connected with the marker using just a line.
			/// </summary>
			None,
			/// <summary>
			/// Label is undelined and connected with the marker using a line.
			/// </summary>
			Underlined,
			/// <summary>
			/// Box is drawn around the label and it's connected with the marker using a line.
			/// </summary>
			Box
		}

		/// <summary>
		/// Data point label outside of the plotting area style.
		/// </summary>
		[
		SRDescription("DescriptionAttributeLabelOutsidePlotAreaStyle_LabelOutsidePlotAreaStyle")
		]
		public enum LabelOutsidePlotAreaStyle
		{
			/// <summary>
			/// Labels can be positioned outside of the plotting area.
			/// </summary>
			Yes,
			/// <summary>
			/// Labels can not be positioned outside of the plotting area.
			/// </summary>
			No,
			/// <summary>
			/// Labels can be partially outside of the plotting area.
			/// </summary>
			Partial
		}

		#endregion

		/// <summary>
        /// SmartLabelStyle class is used to enable and configure the 
        /// SmartLabelStyle algorithm for data point labels and annotations. 
        /// In most of the cases it is enough just to enable the algorithm, 
        /// but this class also contains properties which allow controlling 
        /// how the labels are moved around to avoid collisions. Visual 
        /// appearance of callouts can also be set through this class.
		/// </summary>
		[
		DefaultProperty("Enabled"),
		SRDescription("DescriptionAttributeSmartLabelsStyle_SmartLabelsStyle"),
        TypeConverter(typeof(NoNameExpandableObjectConverter))
		]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
        [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
        public class SmartLabelStyle
		{
			#region Fields

			// Reference to the series this style belongs to
			internal	object					chartElement = null;

			// Indicates if SmartLabelStyle algorithm is enabled.
			private		bool					_enabled = true;

			// Indicates that marker overlapping by label is allowed.
			private		bool					_isMarkerOverlappingAllowed = false;

			// Indicates that overlapped labels that can't be repositioned will be hidden.
			private		bool					_isOverlappedHidden = true;

			// Possible moving directions for the overlapped SmartLabelStyle.
			private		LabelAlignmentStyles			_movingDirection = LabelAlignmentStyles.Top | LabelAlignmentStyles.Bottom | LabelAlignmentStyles.Right | LabelAlignmentStyles.Left | LabelAlignmentStyles.TopLeft | LabelAlignmentStyles.TopRight | LabelAlignmentStyles.BottomLeft | LabelAlignmentStyles.BottomRight;

			// Minimum distance the overlapped SmartLabelStyle can be moved from the marker. Distance is measured in pixels.
			private		double					_minMovingDistance = 0.0;

			// Maximum distance the overlapped SmartLabelStyle can be moved from the marker. Distance is measured in pixels.
			private		double					_maxMovingDistance = 30.0;

			// Defines if SmartLabelStyle are allowed to be drawn outside of the plotting area.
			private	LabelOutsidePlotAreaStyle	_allowOutsidePlotArea = LabelOutsidePlotAreaStyle.Partial;

			// Callout style of the repositioned SmartLabelStyle.
			private		LabelCalloutStyle		_calloutStyle = LabelCalloutStyle.Underlined;

			// Label callout line color.
			private		Color					_calloutLineColor = Color.Black;

			// Label callout line style.
			private		ChartDashStyle			_calloutLineDashStyle = ChartDashStyle.Solid;

			// Label callout back color. Applies to the Box style only!
			private		Color					_calloutBackColor = Color.Transparent;

			// Label callout line width.
			private		int						_calloutLineWidth = 1;

			// Label callout line anchor cap.
			private		LineAnchorCapStyle			_calloutLineAnchorCapStyle = LineAnchorCapStyle.Arrow;
			
			#endregion

			#region Constructors and initialization

			/// <summary>
			/// Default public constructor.
			/// </summary>
			public SmartLabelStyle()
			{
				this.chartElement = null;
			}

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="chartElement">Chart element this style belongs to.</param>
			internal SmartLabelStyle(Object chartElement)
			{
				this.chartElement = chartElement;
			}

			#endregion

			#region Properties

			/// <summary>
			/// SmartLabelStyle algorithm enabled flag.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			Bindable(true),
			DefaultValue(true),
			SRDescription("DescriptionAttributeEnabled13"),
			ParenthesizePropertyNameAttribute(true),
			]
			virtual public bool Enabled
			{
				get
				{
					return _enabled;
				}
				set
				{
					_enabled = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Indicates that marker overlapping by label is allowed.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			Bindable(true),
			DefaultValue(false),
			SRDescription("DescriptionAttributeMarkerOverlapping"),
			]
			virtual public bool IsMarkerOverlappingAllowed
			{
				get
				{
					return _isMarkerOverlappingAllowed;
				}
				set
				{
					_isMarkerOverlappingAllowed = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Indicates that overlapped labels that can't be repositioned will be hidden.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			Bindable(true),
			DefaultValue(true),
			SRDescription("DescriptionAttributeHideOverlapped"),
			]
			virtual public bool IsOverlappedHidden
			{
				get
				{
					return _isOverlappedHidden;
				}
				set
				{
					_isOverlappedHidden = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Possible moving directions for the overlapped SmartLabelStyle.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			Bindable(true),
			DefaultValue(typeof(LabelAlignmentStyles), "Top, Bottom, Right, Left, TopLeft, TopRight, BottomLeft, BottomRight"),
			SRDescription("DescriptionAttributeMovingDirection"),
            Editor(Editors.FlagsEnumUITypeEditor.Editor, Editors.FlagsEnumUITypeEditor.Base),
			]
			virtual public LabelAlignmentStyles MovingDirection
			{
				get
				{
					return _movingDirection;
				}
				set
				{
					if(value == 0)
					{
                        throw (new InvalidOperationException(SR.ExceptionSmartLabelsDirectionUndefined));
					}

					_movingDirection = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Minimum distance the overlapped SmartLabelStyle can be moved from the marker. Distance is measured in pixels.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			Bindable(true),
			DefaultValue(0.0),
			SRDescription("DescriptionAttributeMinMovingDistance"),
			]
			virtual public double MinMovingDistance
			{
				get
				{
					return _minMovingDistance;
				}
				set
				{
					if(value < 0)
					{
                        throw (new InvalidOperationException(SR.ExceptionSmartLabelsMinMovingDistanceIsNegative));
					}
					_minMovingDistance = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Maximum distance the overlapped SmartLabelStyle can be moved from the marker. Distance is measured in pixels.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			Bindable(true),
			DefaultValue(30.0),
			SRDescription("DescriptionAttributeMaxMovingDistance"),
			]
			virtual public double MaxMovingDistance
			{
				get
				{
					return _maxMovingDistance;
				}
				set
				{
					if(value < 0 )
					{
                        throw (new InvalidOperationException(SR.ExceptionSmartLabelsMaxMovingDistanceIsNegative));
					}
					_maxMovingDistance = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Defines if SmartLabelStyle are allowed to be drawn outside of the plotting area.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			Bindable(true),
			DefaultValue(LabelOutsidePlotAreaStyle.Partial),
			SRDescription("DescriptionAttributeAllowOutsidePlotArea"),
			]
			virtual public LabelOutsidePlotAreaStyle AllowOutsidePlotArea
			{
				get
				{
					return _allowOutsidePlotArea;
				}
				set
				{
					_allowOutsidePlotArea = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Callout style of the repositioned SmartLabelStyle.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			Bindable(true),
			DefaultValue(LabelCalloutStyle.Underlined),
			SRDescription("DescriptionAttributeCalloutStyle3"),
			]
			virtual public LabelCalloutStyle CalloutStyle
			{
				get
				{
					return _calloutStyle;
				}
				set
				{
					_calloutStyle = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Label callout line color.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(typeof(Color), "Black"),
            SRDescription("DescriptionAttributeCalloutLineColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
			]
			virtual public Color CalloutLineColor
			{
				get
				{
					return _calloutLineColor;
				}
				set
				{
					_calloutLineColor = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Label callout line style.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(ChartDashStyle.Solid),
            SRDescription("DescriptionAttributeLineDashStyle"),
			#if !Microsoft_CONTROL
			PersistenceMode(PersistenceMode.Attribute)
			#endif
			]
			virtual public ChartDashStyle CalloutLineDashStyle
			{
				get
				{
					return _calloutLineDashStyle;
				}
				set
				{
					_calloutLineDashStyle = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Label callout back color. Applies to the Box style only!
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(typeof(Color), "Transparent"),
            SRDescription("DescriptionAttributeCalloutBackColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
			]
			virtual public Color CalloutBackColor
			{
				get
				{
					return _calloutBackColor;
				}
				set
				{
					_calloutBackColor = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Label callout line width.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(1),
            SRDescription("DescriptionAttributeLineWidth"),
			]
			virtual public int CalloutLineWidth
			{
				get
				{
					return _calloutLineWidth;
				}
				set
				{
					_calloutLineWidth = value;
					Invalidate();
				}
			}

			/// <summary>
			/// Label callout line anchor cap.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(LineAnchorCapStyle.Arrow),
			SRDescription(SR.Keys.DescriptionAttributeCalloutLineAnchorCap),
			]
			virtual public LineAnchorCapStyle CalloutLineAnchorCapStyle
			{
				get
				{
					return _calloutLineAnchorCapStyle;
				}
				set
				{
					_calloutLineAnchorCapStyle = value;
					Invalidate();
				}
			}

			#endregion

			#region Methods

			/// <summary>
			/// Invalidates ----osiated chart element.
			/// </summary>
			private void Invalidate()
			{
				if(chartElement != null)
				{
					if(chartElement is Series)
					{
						((Series)chartElement).Invalidate(false, false);
					}

					else if(chartElement is Annotation)
					{
						((Annotation)chartElement).Invalidate();
					}

				}
			}

			#endregion 
		}

		/// <summary>
        /// SmartLabelStyle class implements the SmartLabelStyle algorithm for the 
        /// data series points. It keeps track of all labels drawn and 
        /// detects their collisions. When labels collision is detected 
        /// the algorithm tries to resolve it by repositioning the labels. 
        /// If label can not be repositioned it maybe hidden depending on 
        /// the current settings.
		/// </summary>
		[
		SRDescription("DescriptionAttributeSmartLabels_SmartLabels"),
		]
		internal class SmartLabel
		{
			#region Fields

			// List of all SmartLabelStyle positions in the area
			internal		ArrayList			smartLabelsPositions = null;

			// Indicates that not a single collision is allowed
			internal		bool				checkAllCollisions = false;

			// Number of positions in array for the markers
			internal		int					markersCount = 0;

			#endregion

			#region Constructors and initialization

			/// <summary>
			/// Default public constructor.
			/// </summary>
			public SmartLabel()
			{
			}

			#endregion

			#region Methods

			/// <summary>
			/// Reset SmartLabelStyle object.
			/// </summary>
			internal void Reset()
			{
				// Re-initialize list of labels position
				smartLabelsPositions = new ArrayList();
			}

			/// <summary>
			/// Process single SmartLabelStyle by adjusting it's position in case of collision.
			/// </summary>
			/// <param name="common">Reference to common elements.</param>
			/// <param name="graph">Reference to chart graphics object.</param>
			/// <param name="area">Chart area.</param>
			/// <param name="smartLabelStyle">Smart labels style.</param>
			/// <param name="labelPosition">Original label position.</param>
			/// <param name="labelSize">Label text size.</param>
			/// <param name="format">Label string format.</param>
			/// <param name="markerPosition">Marker position.</param>
			/// <param name="markerSize">Marker size.</param>
			/// <param name="labelAlignment">Original label alignment.</param>
			/// <returns>Adjusted position of the label.</returns>
			internal PointF AdjustSmartLabelPosition(
				CommonElements common, 
				ChartGraphics graph, 
				ChartArea area,
				SmartLabelStyle smartLabelStyle,
				PointF labelPosition, 
				SizeF labelSize,
				StringFormat format,
				PointF markerPosition, 
				SizeF markerSize,
				LabelAlignmentStyles labelAlignment)
			{
				return AdjustSmartLabelPosition(
					common, 
					graph, 
					area,
					smartLabelStyle,
					labelPosition, 
					labelSize,
					format,
					markerPosition, 
					markerSize,
					labelAlignment,
					false);
			}

			/// <summary>
			/// Process single SmartLabelStyle by adjusting it's position in case of collision.
			/// </summary>
			/// <param name="common">Reference to common elements.</param>
			/// <param name="graph">Reference to chart graphics object.</param>
			/// <param name="area">Chart area.</param>
			/// <param name="smartLabelStyle">Smart labels style.</param>
			/// <param name="labelPosition">Original label position.</param>
			/// <param name="labelSize">Label text size.</param>
			/// <param name="format">Label string format.</param>
			/// <param name="markerPosition">Marker position.</param>
			/// <param name="markerSize">Marker size.</param>
			/// <param name="labelAlignment">Original label alignment.</param>
			/// <param name="checkCalloutLineOverlapping">Indicates that labels overlapping by callout line must be checked.</param>
			/// <returns>Adjusted position of the label.</returns>
			internal PointF AdjustSmartLabelPosition(
				CommonElements common, 
				ChartGraphics graph, 
				ChartArea area,
				SmartLabelStyle smartLabelStyle,
				PointF labelPosition, 
				SizeF labelSize,
				StringFormat format,
				PointF markerPosition, 
				SizeF markerSize,
				LabelAlignmentStyles labelAlignment,
				bool checkCalloutLineOverlapping)
			{
				// Check if SmartLabelStyle are enabled
				if(smartLabelStyle.Enabled)
				{
					bool	labelMovedAway = false;

					// Add series markers positions to avoid their overlapping
					bool rememberMarkersCount = (this.smartLabelsPositions.Count == 0);
					AddMarkersPosition(common, area);
					if(rememberMarkersCount)
					{
						this.markersCount = this.smartLabelsPositions.Count;
					}

					// Check label collision
					if(IsSmartLabelCollide(
						common, 
						graph, 
						area, 
						smartLabelStyle, 
						labelPosition, 
						labelSize, 
						markerPosition,
						format,
						labelAlignment, 
						checkCalloutLineOverlapping))
					{
						// Try to find a new position for the SmartLabelStyle
						labelMovedAway = FindNewPosition(
							common,
							graph, 
							area, 
							smartLabelStyle, 
							ref labelPosition,
							labelSize,
							format,
							markerPosition, 
							ref markerSize,
							ref labelAlignment,
							checkCalloutLineOverlapping);

						// Draw label callout if label was moved away or 
						// it's displayed in the corners of the marker
						if(labelMovedAway || 
							(labelAlignment == LabelAlignmentStyles.BottomLeft ||
							labelAlignment == LabelAlignmentStyles.BottomRight ||
							labelAlignment == LabelAlignmentStyles.TopLeft ||
							labelAlignment == LabelAlignmentStyles.TopRight))
						{
							if(!labelPosition.IsEmpty)
							{
								DrawCallout(
									common,
									graph, 
									area, 
									smartLabelStyle, 
									labelPosition,
									labelSize,
									format,
									markerPosition, 
									markerSize,
									labelAlignment);
							}
						}
					}

					// Add label position into the list
					AddSmartLabelPosition(graph, labelPosition, labelSize, format);
				}

				// Return label position
				return labelPosition;
			}	


			/// <summary>
			/// Process single SmartLabelStyle by adjusting it's position in case of collision.
			/// </summary>
			/// <param name="common">Reference to common elements.</param>
			/// <param name="graph">Reference to chart graphics object.</param>
			/// <param name="area">Chart area.</param>
			/// <param name="smartLabelStyle">Smart labels style.</param>
			/// <param name="labelPosition">Original label position.</param>
			/// <param name="labelSize">Label text size.</param>
			/// <param name="format">Label string format.</param>
			/// <param name="markerPosition">Marker position.</param>
			/// <param name="markerSize">Marker size.</param>
			/// <param name="labelAlignment">Label alignment.</param>
			/// <param name="checkCalloutLineOverlapping">Indicates that labels overlapping by callout line must be checked.</param>
			/// <returns>True if label was moved away from the marker.</returns>
			private bool FindNewPosition(
				CommonElements common, 
				ChartGraphics graph, 
				ChartArea area,
				SmartLabelStyle smartLabelStyle,
				ref PointF labelPosition, 
				SizeF labelSize,
				StringFormat format,
				PointF markerPosition, 
				ref SizeF markerSize,
				ref LabelAlignmentStyles labelAlignment,
				bool checkCalloutLineOverlapping)
			{
				SizeF	newMarkerSize = SizeF.Empty;
				PointF	newLabelPosition = PointF.Empty;
				int		positionIndex = 0;
				float	labelMovement = 0f;
				bool	labelMovedAway = false;
				LabelAlignmentStyles[] positions = new LabelAlignmentStyles[] {
									LabelAlignmentStyles.Top,
									LabelAlignmentStyles.Bottom,
									LabelAlignmentStyles.Left,
									LabelAlignmentStyles.Right,
									LabelAlignmentStyles.TopLeft,																		
									LabelAlignmentStyles.TopRight,
									LabelAlignmentStyles.BottomLeft,
									LabelAlignmentStyles.BottomRight,
									LabelAlignmentStyles.Center
								};

				// Get relative size of single pixel
				SizeF pixelSize = graph.GetRelativeSize(new SizeF(1f, 1f));

				// Try to find a new position for the label
				bool	positionFound = false;
				float	movingStep = 2f;
				float	minMove = (float)Math.Min(smartLabelStyle.MinMovingDistance, smartLabelStyle.MaxMovingDistance);
				float	maxMove = (float)Math.Max(smartLabelStyle.MinMovingDistance, smartLabelStyle.MaxMovingDistance);
				for(labelMovement = minMove; !positionFound && labelMovement <= maxMove; labelMovement += movingStep)
				{
					// Move label by increasing marker size by 4 pixels
					newMarkerSize = new SizeF(
						markerSize.Width + labelMovement*(pixelSize.Width * 2f), 
						markerSize.Height + labelMovement*(pixelSize.Height * 2f));

					// Loop through different alignment types
					for(positionIndex = 0; positionIndex < positions.Length; positionIndex++)
					{
						// Center label alignment should only be tried once!
						if(positions[positionIndex] == LabelAlignmentStyles.Center && labelMovement != minMove)
						{
							continue;
						}

						// Check if this alignment is valid
						if((smartLabelStyle.MovingDirection & positions[positionIndex]) == positions[positionIndex])
						{
							// Calculate new position of the label
							newLabelPosition = CalculatePosition(
								positions[positionIndex],
								markerPosition,
								newMarkerSize,
								labelSize,
								ref format);
					
							// Check new position collision
							if(!IsSmartLabelCollide(
								common, 
								null, 
								area, 
								smartLabelStyle, 
								newLabelPosition, 
								labelSize, 
								markerPosition,
								format,
								positions[positionIndex],
								checkCalloutLineOverlapping))
							{
								positionFound = true;
								labelMovedAway = (labelMovement == 0f) ? false : true;
								break;
							}
						}
					}
				}
				
				// Set new data if new label position was found
				if(positionFound)
				{
					markerSize = newMarkerSize;
					labelPosition = newLabelPosition;
					labelAlignment = positions[positionIndex];
				}

				// DEBUG code
#if DEBUG
				if(common.Chart.ShowDebugMarkings)
				{
					RectangleF	lp = GetLabelPosition(graph, labelPosition, labelSize, format, false);
					if(positionFound)
					{
						graph.Graphics.DrawRectangle(Pens.Green, Rectangle.Round(graph.GetAbsoluteRectangle(lp)));
					}
					else
					{
						graph.Graphics.DrawRectangle(new Pen(Color.Magenta, 3), Rectangle.Round(graph.GetAbsoluteRectangle(lp)));
					}
				}
#endif
				// Do not draw overlapped labels that can't be repositioned
				if(!positionFound && smartLabelStyle.IsOverlappedHidden)
				{
					labelPosition = PointF.Empty;
				}

	
				return (labelMovedAway && positionFound) ? true : false;
			}

			/// <summary>
			/// Process single SmartLabelStyle by adjusting it's position in case of collision.
			/// </summary>
			/// <param name="common">Reference to common elements.</param>
			/// <param name="graph">Reference to chart graphics object.</param>
			/// <param name="area">Chart area.</param>
			/// <param name="smartLabelStyle">Smart labels style.</param>
			/// <param name="labelPosition">Original label position.</param>
			/// <param name="labelSize">Label text size.</param>
			/// <param name="format">Label string format.</param>
			/// <param name="markerPosition">Marker position.</param>
			/// <param name="markerSize">Marker size.</param>
			/// <param name="labelAlignment">Label alignment.</param>
			/// <returns>Adjusted position of the label.</returns>
			virtual internal void DrawCallout(
				CommonElements common, 
				ChartGraphics graph, 
				ChartArea area,
				SmartLabelStyle smartLabelStyle,
				PointF labelPosition, 
				SizeF labelSize,
				StringFormat format,
				PointF markerPosition, 
				SizeF markerSize,
				LabelAlignmentStyles labelAlignment)
			{
				// Calculate label position rectangle
				RectangleF	labelRectAbs = graph.GetAbsoluteRectangle(
					GetLabelPosition(graph, labelPosition, labelSize, format, true));

				// Create callout pen
				Pen calloutPen = new Pen(smartLabelStyle.CalloutLineColor, smartLabelStyle.CalloutLineWidth);
				calloutPen.DashStyle = graph.GetPenStyle(smartLabelStyle.CalloutLineDashStyle);

				// Draw callout frame
				if(smartLabelStyle.CalloutStyle == LabelCalloutStyle.Box)
				{
					// Fill callout box around the label
					if(smartLabelStyle.CalloutBackColor != Color.Transparent)
					{
                        using (Brush calloutBrush = new SolidBrush(smartLabelStyle.CalloutBackColor))
                        {
                            graph.FillRectangle(calloutBrush, labelRectAbs);
                        }
					}

					// Draw box border
					graph.DrawRectangle(calloutPen, labelRectAbs.X, labelRectAbs.Y, labelRectAbs.Width, labelRectAbs.Height);
				}

				else if(smartLabelStyle.CalloutStyle == LabelCalloutStyle.Underlined)
				{
					if(labelAlignment == LabelAlignmentStyles.Right)
					{
						// Draw line to the left of label's text
						graph.DrawLine(calloutPen, labelRectAbs.X, labelRectAbs.Top, labelRectAbs.X, labelRectAbs.Bottom);
					}
					else if(labelAlignment == LabelAlignmentStyles.Left)
					{
						// Draw line to the right of label's text
						graph.DrawLine(calloutPen, labelRectAbs.Right, labelRectAbs.Top, labelRectAbs.Right, labelRectAbs.Bottom);
					}
					else if(labelAlignment == LabelAlignmentStyles.Bottom)
					{
						// Draw line on top of the label's text
						graph.DrawLine(calloutPen, labelRectAbs.X, labelRectAbs.Top, labelRectAbs.Right, labelRectAbs.Top);
					}
					else
					{
						// Draw line under the label's text
						graph.DrawLine(calloutPen, labelRectAbs.X, labelRectAbs.Bottom, labelRectAbs.Right, labelRectAbs.Bottom);
					}
				}

				// Calculate connector line point on the label
				PointF	connectorPosition = graph.GetAbsolutePoint(labelPosition);
				if(labelAlignment == LabelAlignmentStyles.Top)
				{
					connectorPosition.Y = labelRectAbs.Bottom;
				}
				else if(labelAlignment == LabelAlignmentStyles.Bottom)
				{
					connectorPosition.Y = labelRectAbs.Top;
				}

				if(smartLabelStyle.CalloutStyle == LabelCalloutStyle.Underlined)
				{
					if(labelAlignment == LabelAlignmentStyles.TopLeft ||
						labelAlignment == LabelAlignmentStyles.TopRight ||
						labelAlignment == LabelAlignmentStyles.BottomLeft ||
						labelAlignment == LabelAlignmentStyles.BottomRight)
					{
						connectorPosition.Y = labelRectAbs.Bottom;
					}
				}

				// Apply anchor cap settings
				if(smartLabelStyle.CalloutLineAnchorCapStyle == LineAnchorCapStyle.Arrow)
				{
					//calloutPen.StartCap = LineCap.ArrowAnchor;
					calloutPen.StartCap = LineCap.Custom;
					calloutPen.CustomStartCap = new AdjustableArrowCap(calloutPen.Width + 2, calloutPen.Width + 3, true);
				}
				else if(smartLabelStyle.CalloutLineAnchorCapStyle == LineAnchorCapStyle.Diamond)
				{
					calloutPen.StartCap = LineCap.DiamondAnchor;
				}
				else if(smartLabelStyle.CalloutLineAnchorCapStyle == LineAnchorCapStyle.Round)
				{
					calloutPen.StartCap = LineCap.RoundAnchor;
				}
				else if(smartLabelStyle.CalloutLineAnchorCapStyle == LineAnchorCapStyle.Square)
				{
					calloutPen.StartCap = LineCap.SquareAnchor;
				}

				// Draw connection line between marker position and label
				PointF	markerPositionAbs = graph.GetAbsolutePoint(markerPosition);
				graph.DrawLine(
					calloutPen, 
					markerPositionAbs.X, 
					markerPositionAbs.Y, 
					connectorPosition.X, 
					connectorPosition.Y);
			}
				
			/// <summary>
			/// Checks SmartLabelStyle collision.
			/// </summary>
			/// <param name="common">Reference to common elements.</param>
			/// <param name="graph">Reference to chart graphics object.</param>
			/// <param name="area">Chart area.</param>
			/// <param name="smartLabelStyle">Smart labels style.</param>
			/// <param name="position">Original label position.</param>
			/// <param name="size">Label text size.</param>
			/// <param name="markerPosition">Marker position.</param>
			/// <param name="format">Label string format.</param>
			/// <param name="labelAlignment">Label alignment.</param>
			/// <param name="checkCalloutLineOverlapping">Indicates that labels overlapping by callout line must be checked.</param>
			/// <returns>True if label collides.</returns>
			virtual internal bool IsSmartLabelCollide(
				CommonElements common, 
				ChartGraphics graph, 
				ChartArea area,
				SmartLabelStyle smartLabelStyle,
				PointF position, 
				SizeF size,
				PointF markerPosition,
				StringFormat format,
				LabelAlignmentStyles labelAlignment,
				bool checkCalloutLineOverlapping)
			{
				bool	collisionDetected = false;

				// Calculate label position rectangle
				RectangleF	labelPosition = GetLabelPosition(graph, position, size, format, false);

				// Check if label goes outside of the chart picture
				if(labelPosition.X < 0f || labelPosition.Y < 0f ||
					labelPosition.Bottom > 100f || labelPosition.Right > 100f)
				{
#if DEBUG					
					// DEBUG: Mark collided labels
                    if (graph != null && common!=null && common.Chart!=null && common.Chart.ShowDebugMarkings)
					{
						graph.Graphics.DrawRectangle(Pens.Cyan, Rectangle.Round(graph.GetAbsoluteRectangle(labelPosition)));
					}
#endif					
					collisionDetected = true;
				}


				// Check if label is drawn outside of plotting area (collides with axis?).
				if(!collisionDetected && area != null)
				{
					if(area.chartAreaIsCurcular)
					{
						using( GraphicsPath areaPath = new GraphicsPath() )
						{
							// Add circular shape of the area into the graphics path
							areaPath.AddEllipse(area.PlotAreaPosition.ToRectangleF());

							if(smartLabelStyle.AllowOutsidePlotArea == LabelOutsidePlotAreaStyle.Partial)
							{
								PointF	centerPos = new PointF(
									labelPosition.X + labelPosition.Width/2f,
									labelPosition.Y + labelPosition.Height/2f);
								if(!areaPath.IsVisible(centerPos))
								{
									// DEBUG: Mark collided labels
#if DEBUG							
									if(graph != null && common.Chart.ShowDebugMarkings)
									{
										graph.Graphics.DrawRectangle(Pens.Cyan, Rectangle.Round(graph.GetAbsoluteRectangle(labelPosition)));
									}
#endif
									collisionDetected = true;
								}
							}
							else if(smartLabelStyle.AllowOutsidePlotArea == LabelOutsidePlotAreaStyle.No)
							{
								if(!areaPath.IsVisible(labelPosition.Location) ||
									!areaPath.IsVisible(new PointF(labelPosition.Right, labelPosition.Y)) ||
									!areaPath.IsVisible(new PointF(labelPosition.Right, labelPosition.Bottom)) ||
									!areaPath.IsVisible(new PointF(labelPosition.X, labelPosition.Bottom)) )
								{
									// DEBUG: Mark collided labels
#if DEBUG
									if(graph != null && common.Chart.ShowDebugMarkings)
									{
										graph.Graphics.DrawRectangle(Pens.Cyan, Rectangle.Round(graph.GetAbsoluteRectangle(labelPosition)));
									}
#endif
									collisionDetected = true;
								}
							}
						}
					}
					else
					{
						if(smartLabelStyle.AllowOutsidePlotArea == LabelOutsidePlotAreaStyle.Partial)
						{
							PointF	centerPos = new PointF(
								labelPosition.X + labelPosition.Width/2f,
								labelPosition.Y + labelPosition.Height/2f);
							if(!area.PlotAreaPosition.ToRectangleF().Contains(centerPos))
							{
								// DEBUG: Mark collided labels
#if DEBUG							
								if(graph != null && common.Chart.ShowDebugMarkings)
								{
									graph.Graphics.DrawRectangle(Pens.Cyan, Rectangle.Round(graph.GetAbsoluteRectangle(labelPosition)));
								}
#endif
								collisionDetected = true;
							}
						}
						else if(smartLabelStyle.AllowOutsidePlotArea == LabelOutsidePlotAreaStyle.No)
						{
							if(!area.PlotAreaPosition.ToRectangleF().Contains(labelPosition))
							{
								// DEBUG: Mark collided labels
#if DEBUG
								if(graph != null && common.Chart.ShowDebugMarkings)
								{
									graph.Graphics.DrawRectangle(Pens.Cyan, Rectangle.Round(graph.GetAbsoluteRectangle(labelPosition)));
								}
#endif
								collisionDetected = true;
							}
						}
					}
				}

				// Check if 1 collisuion is aceptable in case of cennter alignment
				bool	allowOneCollision = 
					(labelAlignment == LabelAlignmentStyles.Center && !smartLabelStyle.IsMarkerOverlappingAllowed) ? true : false;
				if(this.checkAllCollisions)
				{
					allowOneCollision = false;
				}


				// Loop through all smart label positions
				if(!collisionDetected && this.smartLabelsPositions != null)
				{
					int index = -1;
					foreach(RectangleF pos in this.smartLabelsPositions)
					{
						// Increase index
						++index;

						// Check if label collide with other labels or markers.
						bool collision = pos.IntersectsWith(labelPosition);

						// Check if label callout line collide with other labels or markers.
						// Line may overlap markers!
						if(!collision && 
							checkCalloutLineOverlapping &&
							index >= markersCount)
						{
							PointF labelCenter = new PointF(
								labelPosition.X + labelPosition.Width / 2f,
								labelPosition.Y + labelPosition.Height / 2f);
							if(LineIntersectRectangle(pos, markerPosition, labelCenter))
							{
								collision = true;
							}
						}

						// Collision detected
						if(collision)
						{
							// Check if 1 collision allowed
							if(allowOneCollision)
							{
								allowOneCollision = false;
								continue;
							}

							// DEBUG: Mark collided labels
#if DEBUG							
							if(graph != null &&
								common.ChartPicture != null && 
								common.ChartPicture.ChartGraph != null &&
								common.Chart.ShowDebugMarkings)
							{
								common.ChartPicture.ChartGraph.Graphics.DrawRectangle(Pens.Blue, Rectangle.Round(common.ChartPicture.ChartGraph.GetAbsoluteRectangle(pos)));
								common.ChartPicture.ChartGraph.Graphics.DrawRectangle(Pens.Red, Rectangle.Round(common.ChartPicture.ChartGraph.GetAbsoluteRectangle(labelPosition)));
							}
#endif
							collisionDetected = true;
							break;
						}
					}
				}

				return collisionDetected;
			}

			/// <summary>
			/// Checks if rectangle intersected by the line.
			/// </summary>
			/// <param name="rect">Rectangle to be tested.</param>
			/// <param name="point1">First line point.</param>
			/// <param name="point2">Second line point.</param>
			/// <returns>True if line intersects rectangle.</returns>
			private bool LineIntersectRectangle(RectangleF rect, PointF point1, PointF point2)
			{
				// Check for horizontal line
				if(point1.X == point2.X)
				{
					if(point1.X >= rect.X && point1.X <= rect.Right)
					{
						if(point1.Y < rect.Y && point2.Y < rect.Y)
						{
							return false;
						}
						if(point1.Y > rect.Bottom && point2.Y > rect.Bottom)
						{
							return false;
						}
						return true;
					}
					return false;
				}

				// Check for vertical line
				if(point1.Y == point2.Y)
				{
					if(point1.Y >= rect.Y && point1.Y <= rect.Bottom)
					{
						if(point1.X < rect.X && point2.X < rect.X)
						{
							return false;
						}
						if(point1.X > rect.Right && point2.X > rect.Right)
						{
							return false;
						}
						return true;
					}
					return false;

				}

				// Check if line completly outside rectangle
				if(point1.X < rect.X && point2.X < rect.X)
				{
					return false;
				}
				else if(point1.X > rect.Right && point2.X > rect.Right)
				{
					return false;
				}
				else if(point1.Y < rect.Y && point2.Y < rect.Y)
				{
					return false;
				}
				else if(point1.Y > rect.Bottom && point2.Y > rect.Bottom)
				{
					return false;
				}

				// Check if one of the points inside rectangle
				if( rect.Contains(point1) ||
					rect.Contains(point2) )
				{
					return true;
				}

				// Calculate intersection point of the line with each side of the rectangle
				PointF intersection = CalloutAnnotation.GetIntersectionY(point1, point2, rect.Y);
				if( rect.Contains(intersection))
				{
					return true;
				}
				intersection = CalloutAnnotation.GetIntersectionY(point1, point2, rect.Bottom);
				if( rect.Contains(intersection))
				{
					return true;
				}
				intersection = CalloutAnnotation.GetIntersectionX(point1, point2, rect.X);
				if( rect.Contains(intersection))
				{
					return true;
				}
				intersection = CalloutAnnotation.GetIntersectionX(point1, point2, rect.Right);
				if( rect.Contains(intersection))
				{
					return true;
				}

				return false;
			}

			/// <summary>
			/// Adds positions of the series markers into the list.
			/// </summary>
			/// <param name="common">Reference to common elements.</param>
			/// <param name="area">Chart area.</param>
			virtual internal void AddMarkersPosition(
				CommonElements common, 
				ChartArea area)
			{
				// Proceed only if there is no items in the list yet
				if(this.smartLabelsPositions.Count == 0 && area != null)
				{
					// Get chart types registry
					ChartTypeRegistry	registry = common.ChartTypeRegistry;

					// Loop through all the series from this chart area
					foreach(Series series in common.DataManager.Series)
					{
						// Check if marker overapping is enabled for the series
						if(series.ChartArea == area.Name && 
							series.SmartLabelStyle.Enabled &&
							!series.SmartLabelStyle.IsMarkerOverlappingAllowed)
						{
							// Get series chart type
							IChartType chartType = registry.GetChartType(series.ChartTypeName);

							// Add series markers positions into the list
							chartType.AddSmartLabelMarkerPositions(common, area, series, this.smartLabelsPositions);
						}
					}



					// Make sure labels do not intersect with scale breaks
					foreach(Axis currentAxis in area.Axes)
					{
						// Check if scale breaks are defined and there are non zero spacing
						if(currentAxis.ScaleBreakStyle.Spacing > 0.0 
							&& currentAxis.ScaleSegments.Count > 0)
						{
							for(int index = 0; index < (currentAxis.ScaleSegments.Count - 1); index++)
							{
								// Get break position in pixel coordinates
								RectangleF breakPosition = currentAxis.ScaleSegments[index].GetBreakLinePosition(common.graph, currentAxis.ScaleSegments[index + 1]);
								breakPosition = common.graph.GetRelativeRectangle(breakPosition);

								// Create array list if needed
								if(this.smartLabelsPositions == null)
								{
									this.smartLabelsPositions = new ArrayList();
								}

								// Add label position into the list
								this.smartLabelsPositions.Add(breakPosition);

							}
						}
					}


				}
			}

			/// <summary>
			/// Adds single Smart Label position into the list.
			/// </summary>
			/// <param name="graph">Chart graphics object.</param>
			/// <param name="position">Original label position.</param>
			/// <param name="size">Label text size.</param>
			/// <param name="format">Label string format.</param>
			internal void AddSmartLabelPosition(
				ChartGraphics graph,
				PointF position, 
				SizeF size,
				StringFormat format)
			{
				// Calculate label position rectangle
				RectangleF	labelPosition = GetLabelPosition(graph, position, size, format, false);

				if(this.smartLabelsPositions == null)
				{
					this.smartLabelsPositions = new ArrayList();
				}

				// Add label position into the list
				this.smartLabelsPositions.Add(labelPosition);
			}

			/// <summary>
			/// Gets rectangle position of the label.
			/// </summary>
			/// <param name="graph">Chart graphics object.</param>
			/// <param name="position">Original label position.</param>
			/// <param name="size">Label text size.</param>
			/// <param name="format">Label string format.</param>
			/// <param name="adjustForDrawing">Result position is adjusted for drawing.</param>
			/// <returns>Label rectangle position.</returns>
			internal RectangleF GetLabelPosition(
				ChartGraphics graph,
				PointF position, 
				SizeF size,
				StringFormat format,
				bool adjustForDrawing)
			{
				// Calculate label position rectangle
				RectangleF	labelPosition = RectangleF.Empty;
				labelPosition.Width = size.Width;
				labelPosition.Height = size.Height;

				// Calculate pixel size in relative coordiantes
				SizeF	pixelSize = SizeF.Empty;
				if(graph != null)
				{
					pixelSize = graph.GetRelativeSize(new SizeF(1f, 1f));
				}

				if(format.Alignment == StringAlignment.Far)
				{
					labelPosition.X = position.X - size.Width;
					if(adjustForDrawing && !pixelSize.IsEmpty)
					{
						labelPosition.X -= 4f*pixelSize.Width;
						labelPosition.Width += 4f*pixelSize.Width;
					}
				}
				else if(format.Alignment == StringAlignment.Near)
				{
					labelPosition.X = position.X;
					if(adjustForDrawing && !pixelSize.IsEmpty)
					{
						labelPosition.Width += 4f*pixelSize.Width;
					}
				}
				else if(format.Alignment == StringAlignment.Center)
				{
					labelPosition.X = position.X - size.Width/2F;
					if(adjustForDrawing && !pixelSize.IsEmpty)
					{
						labelPosition.X -= 2f*pixelSize.Width;
						labelPosition.Width += 4f*pixelSize.Width;
					}
				}

				if(format.LineAlignment == StringAlignment.Far)
				{
					labelPosition.Y = position.Y - size.Height;
				}
				else if(format.LineAlignment == StringAlignment.Near)
				{
					labelPosition.Y = position.Y;
				}
				else if(format.LineAlignment == StringAlignment.Center)
				{
					labelPosition.Y = position.Y - size.Height/2F;
				}

				return labelPosition;
			}

			/// <summary>
			/// Gets point position of the label.
			/// </summary>
			/// <param name="labelAlignment">Label alignment.</param>
			/// <param name="markerPosition">Marker position.</param>
			/// <param name="sizeMarker">Marker size.</param>
			/// <param name="sizeFont">Label size.</param>
			/// <param name="format">String format.</param>
			/// <returns>Label point position.</returns>
			private PointF CalculatePosition(
				LabelAlignmentStyles labelAlignment, 
				PointF markerPosition, 
				SizeF sizeMarker, 
				SizeF sizeFont, 
				ref StringFormat format)
			{
				format.Alignment = StringAlignment.Near;
				format.LineAlignment = StringAlignment.Center;

				// Calculate label position
				PointF	position = new PointF(markerPosition.X, markerPosition.Y);
				switch(labelAlignment)
				{
					case LabelAlignmentStyles.Center:
						format.Alignment = StringAlignment.Center;
						break;
					case LabelAlignmentStyles.Bottom:
						format.Alignment = StringAlignment.Center;
						position.Y += sizeMarker.Height / 1.75F;
						position.Y += sizeFont.Height/ 2F;
						break;
					case LabelAlignmentStyles.Top:
						format.Alignment = StringAlignment.Center;
						position.Y -= sizeMarker.Height / 1.75F;
						position.Y -= sizeFont.Height/ 2F;
						break;
					case LabelAlignmentStyles.Left:
						format.Alignment = StringAlignment.Far;
						position.X -= sizeMarker.Height / 1.75F;
						break;
					case LabelAlignmentStyles.TopLeft:
						format.Alignment = StringAlignment.Far;
						position.X -= sizeMarker.Height / 1.75F;
						position.Y -= sizeMarker.Height / 1.75F;
						position.Y -= sizeFont.Height/ 2F;
						break;
					case LabelAlignmentStyles.BottomLeft:
						format.Alignment = StringAlignment.Far;
						position.X -= sizeMarker.Height / 1.75F;
						position.Y += sizeMarker.Height / 1.75F;
						position.Y += sizeFont.Height/ 2F;
						break;
					case LabelAlignmentStyles.Right:
						position.X += sizeMarker.Height / 1.75F;
						break;
					case LabelAlignmentStyles.TopRight:
						position.X += sizeMarker.Height / 1.75F;
						position.Y -= sizeMarker.Height / 1.75F;
						position.Y -= sizeFont.Height/ 2F;
						break;
					case LabelAlignmentStyles.BottomRight:
						position.X += sizeMarker.Height / 1.75F;
						position.Y += sizeMarker.Height / 1.75F;
						position.Y += sizeFont.Height/ 2F;
						break;
				}

				return position;
			}

			#endregion
		}

		/// <summary>
        /// AnnotationSmartLabel class provides SmartLabelStyle functionality 
        /// specific to the annotation objects.
		/// </summary>
		[
		SRDescription("DescriptionAttributeAnnotationSmartLabels_AnnotationSmartLabels"),
		]
        internal class AnnotationSmartLabel : SmartLabel
		{
			#region Constructors and initialization

			/// <summary>
			/// Default public constructor.
			/// </summary>
			public AnnotationSmartLabel()
			{
			}

			#endregion

			#region Methods

			/// <summary>
			/// Checks SmartLabelStyle collision.
			/// </summary>
			/// <param name="common">Reference to common elements.</param>
			/// <param name="graph">Reference to chart graphics object.</param>
			/// <param name="area">Chart area.</param>
			/// <param name="smartLabelStyle">Smart labels style.</param>
			/// <param name="position">Original label position.</param>
			/// <param name="size">Label text size.</param>
			/// <param name="markerPosition">Marker position.</param>
			/// <param name="format">Label string format.</param>
			/// <param name="labelAlignment">Label alignment.</param>
			/// <param name="checkCalloutLineOverlapping">Indicates that labels overlapping by callout line must be checked.</param>
			/// <returns>True if label collides.</returns>
			override internal bool IsSmartLabelCollide(
				CommonElements common, 
				ChartGraphics graph, 
				ChartArea area,
				SmartLabelStyle smartLabelStyle,
				PointF position, 
				SizeF size,
				PointF markerPosition,
				StringFormat format,
				LabelAlignmentStyles labelAlignment,
				bool checkCalloutLineOverlapping)
			{
				bool	collisionDetected = false;

				//*******************************************************************
				//** Check collision with smatl labels of series in chart area
				//*******************************************************************
                if (area != null && area.Visible)
				{
                    area.smartLabels.checkAllCollisions = true;
                    if (area.smartLabels.IsSmartLabelCollide(
						common, 
						graph,
                        area,
						smartLabelStyle,
						position, 
						size,
						markerPosition,
						format,
						labelAlignment,
						checkCalloutLineOverlapping) )
					{
                        area.smartLabels.checkAllCollisions = false;
						return true;
					}
                    area.smartLabels.checkAllCollisions = false;
				}

				//*******************************************************************
				//** Check collision with other annotations.
				//*******************************************************************

				// Calculate label position rectangle
				RectangleF	labelPosition = GetLabelPosition(graph, position, size, format, false);

				// Check if 1 collisuion is aceptable in case of cennter alignment
				bool	allowOneCollision = 
					(labelAlignment == LabelAlignmentStyles.Center && !smartLabelStyle.IsMarkerOverlappingAllowed) ? true : false;
				if(this.checkAllCollisions)
				{
					allowOneCollision = false;
				}

				// Check if label collide with other labels or markers.
				foreach(RectangleF pos in this.smartLabelsPositions)
				{
					if(pos.IntersectsWith(labelPosition))
					{
						// Check if 1 collision allowed
						if(allowOneCollision)
						{
							allowOneCollision = false;
							continue;
						}

						// DEBUG: Mark collided labels
#if DEBUG							
						if(graph != null && common.Chart.ShowDebugMarkings)
						{
							graph.Graphics.DrawRectangle(Pens.Blue, Rectangle.Round(graph.GetAbsoluteRectangle(pos)));
							graph.Graphics.DrawRectangle(Pens.Red, Rectangle.Round(graph.GetAbsoluteRectangle(labelPosition)));
						}
#endif
						collisionDetected = true;
						break;
					}
				}

				return collisionDetected;
			}

			/// <summary>
			/// Adds positions of the series markers into the list.
			/// </summary>
			/// <param name="common">Reference to common elements.</param>
			/// <param name="area">Chart area.</param>
			override internal void AddMarkersPosition(
				CommonElements common, 
				ChartArea area)
			{
				// Proceed only if there is no items in the list yet
				if(this.smartLabelsPositions.Count == 0 &&
					common != null &&
					common.Chart != null)
				{
					// Add annotations anchor points
					foreach(Annotation annotation in common.Chart.Annotations)
					{
						annotation.AddSmartLabelMarkerPositions(this.smartLabelsPositions);
					}
				}
			}

			/// <summary>
			/// Process single SmartLabelStyle by adjusting it's position in case of collision.
			/// </summary>
			/// <param name="common">Reference to common elements.</param>
			/// <param name="graph">Reference to chart graphics object.</param>
			/// <param name="area">Chart area.</param>
			/// <param name="smartLabelStyle">Smart labels style.</param>
			/// <param name="labelPosition">Original label position.</param>
			/// <param name="labelSize">Label text size.</param>
			/// <param name="format">Label string format.</param>
			/// <param name="markerPosition">Marker position.</param>
			/// <param name="markerSize">Marker size.</param>
			/// <param name="labelAlignment">Label alignment.</param>
			/// <returns>Adjusted position of the label.</returns>
			override internal void DrawCallout(
				CommonElements common, 
				ChartGraphics graph, 
				ChartArea area,
				SmartLabelStyle smartLabelStyle,
				PointF labelPosition, 
				SizeF labelSize,
				StringFormat format,
				PointF markerPosition, 
				SizeF markerSize,
				LabelAlignmentStyles labelAlignment)
			{
				// No callout is drawn for the annotations
			}

			#endregion
		}
}
