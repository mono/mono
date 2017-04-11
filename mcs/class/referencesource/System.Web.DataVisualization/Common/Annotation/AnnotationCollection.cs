//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		AnnotationCollection.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	AnnotationCollection, AnnotationCollectionEditor
//
//  Purpose:	Collection of annotation objects.
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
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

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
	/// <b>AnnotationCollection</b> is a collection that stores chart annotation objects.
    /// <seealso cref="Charting.Chart.Annotations"/>
	/// </summary>
	/// <remarks>
	/// All chart annotations are stored in this collection.  It is exposed as 
    /// a <see cref="Charting.Chart.Annotations"/> property of the chart. It is also used to 
	/// store annotations inside the <see cref="AnnotationGroup"/> class.
	/// <para>
	/// This class includes methods for adding, inserting, iterating and removing annotations.
	/// </para>
	/// </remarks>
	[
		SRDescription("DescriptionAttributeAnnotations3"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
	public class AnnotationCollection : ChartNamedElementCollection<Annotation>
	{
		#region Fields

		/// <summary>
        /// Group this collection belongs too
		/// </summary>
        internal AnnotationGroup AnnotationGroup { get; set; }

#if Microsoft_CONTROL

        // Annotation object that was last clicked on
		internal Annotation					lastClickedAnnotation = null;

		// Start point of annotation moving or resizing
		private	PointF						_movingResizingStartPoint = PointF.Empty;

        // Current resizing mode
        private ResizingMode _resizingMode = ResizingMode.None;
        
        // Annotation object which is currently placed on the chart
		internal		Annotation			placingAnnotation = null;

#endif

        #endregion

        #region Construction and Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent chart element.</param>
		internal AnnotationCollection(IChartElement parent) : base(parent)
		{
		}

		#endregion

		#region Items Inserting and Removing Notification methods

        /// <summary>
        /// Initializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal override void Initialize(Annotation item)
        {
            if (item != null)
            {
                TextAnnotation textAnnotation = item as TextAnnotation;
                if (textAnnotation != null && string.IsNullOrEmpty(textAnnotation.Text) && Chart != null && Chart.IsDesignMode())
                {
                    textAnnotation.Text = item.Name;
                }

                //If the collection belongs to annotation group we need to pass a ref to this group to all the child annotations
                if (this.AnnotationGroup != null)
                {
                    item.annotationGroup = this.AnnotationGroup;
                }

                item.ResetCurrentRelativePosition();
            }
            base.Initialize(item);
        }

        /// <summary>
        /// Deinitializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal override void Deinitialize(Annotation item)
        {
            if (item != null)
            {
                item.annotationGroup = null;
                item.ResetCurrentRelativePosition();
            }
            base.Deinitialize(item);
        }


        /// <summary>
		/// Finds an annotation in the collection by name.
		/// </summary>
		/// <param name="name">
		/// Name of the annotation to find.
		/// </param>
		/// <returns>
		/// <see cref="Annotation"/> object, or null (or nothing) if it does not exist.
		/// </returns>
		public override Annotation FindByName(string name) 
		{
			foreach(Annotation annotation in this)
			{
				// Compare annotation name 
				if(annotation.Name == name)
				{
					return annotation;
				}

				// Check if annotation is a group
				AnnotationGroup annotationGroup = annotation as AnnotationGroup;
				if(annotationGroup != null)
				{
					Annotation result = annotationGroup.Annotations.FindByName(name);
					if(result != null)
					{
						return result;
					}
				}
			}

			return null;
		}

        #endregion

        #region Painting

        /// <summary>
		/// Paints all annotation objects in the collection.
		/// </summary>
		/// <param name="chartGraph">Chart graphics used for painting.</param>
		/// <param name="drawAnnotationOnly">Indicates that only annotation objects are redrawn.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification="This parameter is used when compiling for the Microsoft version of Chart")]
		internal void Paint(ChartGraphics chartGraph, bool drawAnnotationOnly)
		{
#if Microsoft_CONTROL
            ChartPicture chartPicture = this.Chart.chartPicture;

			// Restore previous background using double buffered bitmap
			if(!chartPicture.isSelectionMode &&
				this.Count > 0 /*&&
				!this.Chart.chartPicture.isPrinting*/)
			{
				chartPicture.backgroundRestored = true;
				Rectangle chartPosition = new Rectangle(0, 0, chartPicture.Width, chartPicture.Height);
				if(chartPicture.nonTopLevelChartBuffer == null || !drawAnnotationOnly)
				{
					// Dispose previous bitmap
					if(chartPicture.nonTopLevelChartBuffer != null)
					{
						chartPicture.nonTopLevelChartBuffer.Dispose();
						chartPicture.nonTopLevelChartBuffer = null;
					}

					// Copy chart area plotting rectangle from the chart's dubble buffer image into area dubble buffer image
                    if (this.Chart.paintBufferBitmap != null &&
                        this.Chart.paintBufferBitmap.Size.Width >= chartPosition.Size.Width &&
                        this.Chart.paintBufferBitmap.Size.Height >= chartPosition.Size.Height)
					{
                        chartPicture.nonTopLevelChartBuffer = this.Chart.paintBufferBitmap.Clone(
                            chartPosition, this.Chart.paintBufferBitmap.PixelFormat);
					}
				}
				else if(drawAnnotationOnly && chartPicture.nonTopLevelChartBuffer != null)
				{
					// Restore previous background
                    this.Chart.paintBufferBitmapGraphics.DrawImageUnscaled(
						chartPicture.nonTopLevelChartBuffer,
						chartPosition);
				}
			}
#endif // Microsoft_CONTROL

			// Draw all annotation objects
			foreach(Annotation annotation in this)
			{
				// Reset calculated relative position
				annotation.ResetCurrentRelativePosition();

				if(annotation.IsVisible())
				{
					bool	resetClip = false;

					// Check if anchor point ----osiated with plot area is inside the scaleView
					if(annotation.IsAnchorVisible())
					{
						// Set annotation object clipping
						if(annotation.ClipToChartArea.Length > 0 &&
                            annotation.ClipToChartArea != Constants.NotSetValue &&
							Chart != null)
						{
                            int areaIndex = Chart.ChartAreas.IndexOf(annotation.ClipToChartArea);
							if( areaIndex >= 0 )
							{
								// Get chart area object
                                ChartArea chartArea = Chart.ChartAreas[areaIndex];
								chartGraph.SetClip(chartArea.PlotAreaPosition.ToRectangleF());
								resetClip = true;
							}
						}

						// Start Svg Selection mode
						string url = String.Empty;
#if !Microsoft_CONTROL
						url = annotation.Url;
#endif // !Microsoft_CONTROL
						chartGraph.StartHotRegion( 
							annotation.ReplaceKeywords(url), 
							annotation.ReplaceKeywords(annotation.ToolTip) );

						// Draw annotation object
						annotation.Paint(Chart, chartGraph);


						// End Svg Selection mode
						chartGraph.EndHotRegion( );

						// Reset clipping region
						if(resetClip)
						{
							chartGraph.ResetClip();
						}
					}
				}
			}
		}

		#endregion

        #region Mouse Events Handlers

#if Microsoft_CONTROL

        /// <summary>
		/// Mouse was double clicked.
		/// </summary>
		internal void OnDoubleClick()
		{
			if(lastClickedAnnotation != null && 
				lastClickedAnnotation.AllowTextEditing)
			{
                TextAnnotation textAnnotation = lastClickedAnnotation as TextAnnotation;

				if(textAnnotation == null)
				{
                    AnnotationGroup group = lastClickedAnnotation as AnnotationGroup;

                    if (group != null)
                    {
                        // Try to edit text annotation in the group
                        foreach (Annotation annot in group.Annotations)
                        {
                            TextAnnotation groupAnnot = annot as TextAnnotation;
                            if (groupAnnot != null &&
                                groupAnnot.AllowTextEditing)
                            {
                                // Get annotation position in relative coordinates
                                PointF firstPoint = PointF.Empty;
                                PointF anchorPoint = PointF.Empty;
                                SizeF size = SizeF.Empty;
                                groupAnnot.GetRelativePosition(out firstPoint, out size, out anchorPoint);
                                RectangleF textPosition = new RectangleF(firstPoint, size);

                                // Check if last clicked coordinate is inside this text annotation
                                if (groupAnnot.GetGraphics() != null &&
                                    textPosition.Contains(groupAnnot.GetGraphics().GetRelativePoint(this._movingResizingStartPoint)))
                                {
                                    textAnnotation = groupAnnot;
                                    lastClickedAnnotation = textAnnotation;
                                    break;
                                }
                            }
                        }
                    }
				}

				if(textAnnotation != null)
				{
					// Start annotation text editing
					textAnnotation.BeginTextEditing();
				}
			}
		}

		/// <summary>
		/// Checks if specified point is contained by any of the selection handles.
		/// </summary>
		/// <param name="point">Point which is tested in pixel coordinates.</param>
		/// <param name="resizingMode">Handle containing the point or None.</param>
		/// <returns>Annotation that contains the point or Null.</returns>
		internal Annotation HitTestSelectionHandles(PointF point, ref ResizingMode resizingMode)
		{
            Annotation annotation = null;

			if( Common != null &&
				Common.graph != null)
			{
				PointF pointRel = Common.graph.GetRelativePoint(point);
				foreach(Annotation annot in this)
				{
					// Reset selcted path point
					annot.currentPathPointIndex = -1;

					// Check if annotation is selected
					if(annot.IsSelected)
					{
						if(annot.selectionRects != null)
						{
							for(int index = 0; index < annot.selectionRects.Length; index++)
							{
								if(!annot.selectionRects[index].IsEmpty && 
									annot.selectionRects[index].Contains(pointRel))
								{
									annotation = annot; 
									if(index > (int)ResizingMode.AnchorHandle)
									{
										resizingMode = ResizingMode.MovingPathPoints;
										annot.currentPathPointIndex = index - 9;
									}
									else
									{
										resizingMode = (ResizingMode)index;
									}
								}
							}
						}
					}
				}
			}
			return annotation;
		}

		/// <summary>
		/// Mouse button pressed in the control.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		/// <param name="isHandled">Returns true if event is handled and no further processing required.</param>
		internal void OnMouseDown(MouseEventArgs e, ref bool isHandled)
		{
            // Reset last clicked annotation object and stop text editing
			if(lastClickedAnnotation != null)
			{
				TextAnnotation textAnnotation = lastClickedAnnotation as TextAnnotation;
				if(textAnnotation != null)
				{
					// Stop annotation text editing
					textAnnotation.StopTextEditing();
				}
				lastClickedAnnotation = null;
			}

			// Check if in annotation placement mode
			if( this.placingAnnotation != null)
			{
				// Process mouse down
				this.placingAnnotation.PlacementMouseDown(new PointF(e.X, e.Y), e.Button);

				// Set handled flag
				isHandled = true;
				return;
			}

			// Process only left mouse buttons
			if(e.Button == MouseButtons.Left)
			{
				bool	updateRequired = false;
				this._resizingMode = ResizingMode.None;

				// Check if mouse buton was pressed in any selection handles areas
				Annotation annotation = 
					HitTestSelectionHandles(new PointF(e.X, e.Y), ref this._resizingMode);

				// Check if mouse button was pressed over one of the annotation objects
				if(annotation == null && this.Count > 0)
				{
					HitTestResult result = this.Chart.HitTest(e.X, e.Y, ChartElementType.Annotation);
					if(result != null && result.ChartElementType == ChartElementType.Annotation)
					{
						annotation = (Annotation)result.Object;
					}
				}

				// Unselect all annotations if mouse clicked outside any annotations
				if(annotation == null || !annotation.IsSelected)
				{
					if((Control.ModifierKeys & Keys.Control) != Keys.Control &&
						(Control.ModifierKeys & Keys.Shift) != Keys.Shift)
					{
                        foreach (Annotation annot in this.Chart.Annotations)
						{
							if(annot != annotation && annot.IsSelected)
							{
								annot.IsSelected = false;
								updateRequired = true;	

								// Call selection changed notification
                                if (this.Chart != null)
								{
                                    this.Chart.OnAnnotationSelectionChanged(annot);
								}
							}
						}
					}
				}

				// Process mouse action in the annotation object
				if(annotation != null)
				{
					// Mouse down event handled
					isHandled = true;

					// Select/Unselect annotation 
					Annotation selectableAnnotation = annotation;
					if(annotation.AnnotationGroup != null)
					{
						// Select annotation group when click on any child annotations
						selectableAnnotation = annotation.AnnotationGroup;
					}
					if(!selectableAnnotation.IsSelected && selectableAnnotation.AllowSelecting)
					{
						selectableAnnotation.IsSelected = true;
						updateRequired = true;	

						// Call selection changed notification
                        if (this.Chart != null)
						{
                            this.Chart.OnAnnotationSelectionChanged(selectableAnnotation);
						}
					}
					else if((Control.ModifierKeys & Keys.Control) == Keys.Control ||
						(Control.ModifierKeys & Keys.Shift) == Keys.Shift)
					{
						selectableAnnotation.IsSelected = false;
						updateRequired = true;	

						// Call selection changed notification
                        if (this.Chart != null)
						{
                            this.Chart.OnAnnotationSelectionChanged(selectableAnnotation);
						}
					}

					// Remember last clicked and selected annotation
					lastClickedAnnotation = annotation;

					// Rember mouse position
					this._movingResizingStartPoint = new PointF(e.X, e.Y);

					// Start moving, repositioning or resizing of annotation
					if(annotation.IsSelected)
					{
						// Check if one of selection handles was clicked on
						this._resizingMode = annotation.GetSelectionHandle(this._movingResizingStartPoint);
						if(!annotation.AllowResizing && 
							this._resizingMode >= ResizingMode.TopLeftHandle &&
							this._resizingMode <= ResizingMode.LeftHandle)
						{
							this._resizingMode = ResizingMode.None;
						}
						if(!annotation.AllowAnchorMoving && 
							this._resizingMode == ResizingMode.AnchorHandle)
						{
							this._resizingMode = ResizingMode.None;
						}
						if(this._resizingMode == ResizingMode.None && annotation.AllowMoving)
						{
							// Annotation moving mode
							this._resizingMode = ResizingMode.Moving;
						}
					}
					else
					{
						if(this._resizingMode == ResizingMode.None && annotation.AllowMoving)
						{
                            // Do not allow moving child annotations inside the group. 
                            // Only the whole group can be selected, resized or repositioned.
                            if (annotation.AnnotationGroup != null)
                            {
                                // Move the group instead
                                lastClickedAnnotation = annotation.AnnotationGroup;
                            }
                                
                            // Annotation moving mode
                            this._resizingMode = ResizingMode.Moving;
						}
					}
				}

				// Update chart
				if(updateRequired)
				{
					// Invalidate and update the chart
                    this.Chart.Invalidate(true);
                    this.Chart.UpdateAnnotations();
				}
			}
		}

		/// <summary>
		/// Mouse button released in the control.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		internal void OnMouseUp(MouseEventArgs e)
		{
			// Check if in annotation placement mode
			if( this.placingAnnotation != null)
			{
				if(!this.placingAnnotation.PlacementMouseUp(new PointF(e.X, e.Y), e.Button))
				{
					return;
				}
			}

			if(e.Button == MouseButtons.Left)
			{
				// Reset moving sizing start point 
				this._movingResizingStartPoint = PointF.Empty;
				this._resizingMode = ResizingMode.None;
			}

			// Loop through all annotation objects
			for(int index = 0; index < this.Count; index++)
			{
				Annotation	annotation = this[index];

				// NOTE: Automatic deleting feature was disabled. -AG.
				/*
				// Delete all annotation objects moved outside clipping region
				if( annotation.outsideClipRegion )
				{
					this.List.RemoveAt(index);
					--index;
				}
				*/

				// Reset start position/location fields
				annotation.startMovePositionRel = RectangleF.Empty;
				annotation.startMoveAnchorLocationRel = PointF.Empty;
				if(annotation.startMovePathRel != null)
				{
					annotation.startMovePathRel.Dispose();
					annotation.startMovePathRel = null;
				}

				// Fire position changed event
				if( annotation.positionChanged )
				{
					annotation.positionChanged = false;
                    if (this.Chart != null)
					{
                        this.Chart.OnAnnotationPositionChanged(annotation);
					}
				}
			}
		}

		/// <summary>
		/// Mouse moved in the control.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		internal void OnMouseMove(MouseEventArgs e)
		{
			// Check if in annotation placement mode
			if(this.placingAnnotation != null)
			{
                System.Windows.Forms.Cursor newCursor = this.Chart.Cursor;
				if(this.placingAnnotation.IsValidPlacementPosition(e.X, e.Y))
				{
					newCursor = Cursors.Cross;
				}
				else
				{
                    newCursor = this.Chart.defaultCursor;
				}

				// Set current chart cursor
                if (newCursor != this.Chart.Cursor)
				{
                    System.Windows.Forms.Cursor tmpCursor = this.Chart.defaultCursor;
                    this.Chart.Cursor = newCursor;
                    this.Chart.defaultCursor = tmpCursor;
				}

				this.placingAnnotation.PlacementMouseMove(new PointF(e.X, e.Y));
								
				return;
			}

			// Check if currently resizing/moving annotation
			if(!this._movingResizingStartPoint.IsEmpty && 
				this._resizingMode != ResizingMode.None)
			{
				// Calculate how far the mouse was moved
				SizeF	moveDistance = new SizeF(
					this._movingResizingStartPoint.X - e.X,
					this._movingResizingStartPoint.Y - e.Y );

				// Update location of all selected annotation objects
				foreach(Annotation annot in this)
				{
					if(annot.IsSelected &&
						( (this._resizingMode == ResizingMode.MovingPathPoints && annot.AllowPathEditing) ||
						(this._resizingMode == ResizingMode.Moving && annot.AllowMoving) ||
						(this._resizingMode == ResizingMode.AnchorHandle && annot.AllowAnchorMoving) ||
						(this._resizingMode >= ResizingMode.TopLeftHandle && this._resizingMode <= ResizingMode.LeftHandle && annot.AllowResizing) ) )
					{
						annot.AdjustLocationSize(moveDistance, this._resizingMode, true, true);
					}
				}

				// Move last clicked non-selected annotation
				if(lastClickedAnnotation != null && 
					!lastClickedAnnotation.IsSelected)
				{
					if(this._resizingMode == ResizingMode.Moving && 
						lastClickedAnnotation.AllowMoving)
					{
						lastClickedAnnotation.AdjustLocationSize(moveDistance, this._resizingMode, true, true);
					}
				}

				// Invalidate and update the chart
                this.Chart.Invalidate(true);
                this.Chart.UpdateAnnotations();
			}
			else if(this.Count > 0)
			{
				// Check if currently placing annotation from the UserInterface
				bool	process = true;

				if(process)
				{
					// Check if mouse pointer is over the annotation selection handle
					ResizingMode currentResizingMode = ResizingMode.None;
					Annotation annotation = 
						HitTestSelectionHandles(new PointF(e.X, e.Y), ref currentResizingMode);

					// Check if mouse pointer over the annotation object movable area
					if(annotation == null)
					{
                        HitTestResult result = this.Chart.HitTest(e.X, e.Y, ChartElementType.Annotation);
						if(result != null && result.ChartElementType == ChartElementType.Annotation)
						{
							annotation = (Annotation)result.Object;
							if(annotation != null)
							{
								// Check if annotation is in the collection
								if(this.Contains(annotation))
								{
									currentResizingMode = ResizingMode.Moving;
									if(annotation.AllowMoving == false)
									{
										// Movement is not allowed
										annotation = null;
										currentResizingMode = ResizingMode.None;
									}
								}
							}
						}
					}
					// Set mouse cursor			
					SetResizingCursor(annotation, currentResizingMode);
				}
			}
		}

		/// <summary>
		/// Sets mouse cursor shape.
		/// </summary>
		/// <param name="annotation">Annotation object.</param>
		/// <param name="currentResizingMode">Resizing mode.</param>
		private void SetResizingCursor(Annotation annotation, ResizingMode currentResizingMode)
		{
			// Change current cursor
			if(this.Chart != null)
			{
                System.Windows.Forms.Cursor newCursor = this.Chart.Cursor;
				if(annotation != null)
				{
					if(currentResizingMode == ResizingMode.MovingPathPoints &&
						annotation.AllowPathEditing)
					{
						newCursor = Cursors.Cross;
					}

					if(currentResizingMode == ResizingMode.Moving &&
						annotation.AllowMoving)
					{
						newCursor = Cursors.SizeAll;
					}

					if(currentResizingMode == ResizingMode.AnchorHandle &&
						annotation.AllowAnchorMoving)
					{
						newCursor = Cursors.Cross;
					}

					if(currentResizingMode != ResizingMode.Moving &&
						annotation.AllowResizing)
					{
						if(annotation.SelectionPointsStyle == SelectionPointsStyle.TwoPoints)
						{
							if(currentResizingMode == ResizingMode.TopLeftHandle ||
								currentResizingMode == ResizingMode.BottomRightHandle)
							{
								newCursor = Cursors.Cross;
							}
						}
						else
						{
							if(currentResizingMode == ResizingMode.TopLeftHandle ||
								currentResizingMode == ResizingMode.BottomRightHandle)
							{
								newCursor = Cursors.SizeNWSE;
							}
							else if(currentResizingMode == ResizingMode.TopRightHandle ||
								currentResizingMode == ResizingMode.BottomLeftHandle)
							{
								newCursor = Cursors.SizeNESW;
							}
							else if(currentResizingMode == ResizingMode.TopHandle ||
								currentResizingMode == ResizingMode.BottomHandle)
							{
								newCursor = Cursors.SizeNS;
							}
							else if(currentResizingMode == ResizingMode.LeftHandle ||
								currentResizingMode == ResizingMode.RightHandle)
							{
								newCursor = Cursors.SizeWE;
							}
						}
					}
				}
				else
				{
                    newCursor = this.Chart.defaultCursor;
				}

				// Set current chart cursor
                if (newCursor != this.Chart.Cursor)
				{
                    System.Windows.Forms.Cursor tmpCursor = this.Chart.defaultCursor;
                    this.Chart.Cursor = newCursor;
                    this.Chart.defaultCursor = tmpCursor;
				}
			}
		}

#endif // Microsoft_CONTROL

		#endregion 

        #region Event handlers
        internal void ChartAreaNameReferenceChanged(object sender, NameReferenceChangedEventArgs e)
        {
            // If all the chart areas are removed and then a new one is inserted - Annotations don't get bound to it by default
            if (e.OldElement == null) 
                return;

            foreach (Annotation annotation in this)
            {
                if (annotation.ClipToChartArea == e.OldName)
                    annotation.ClipToChartArea = e.NewName;

                AnnotationGroup group = annotation as AnnotationGroup;
                if (group != null)
                {
                    group.Annotations.ChartAreaNameReferenceChanged(sender, e);
                }
            }
        }
        #endregion

    }
}
