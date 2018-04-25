//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		AxisLabels.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	AxisLabels
//
//  Purpose:	Base class for the Axis class which defines axis 
//				labels related properties and methods.
//
//	Reviewed:	GS - August 8, 2002
//				AG - August 8, 2002
//
//===================================================================

#region Used namespaces
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
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
#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
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
	/// The Axis class provides functionality for 
	/// drawing axis labels.
	/// </summary>
	public partial class Axis
	{
        #region Fields

        // Custom Labels collection
		private CustomLabelsCollection	_customLabels = null;

		#endregion 

		#region Axis labels properties

		/// <summary>
		/// Gets or sets the style of the label.
		/// </summary>
		[
		SRCategory("CategoryAttributeLabels"),
		Bindable(true),
		NotifyParentPropertyAttribute(true),
		SRDescription("DescriptionAttributeLabelStyle"),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
		TypeConverter(typeof(NoNameExpandableObjectConverter))
		]
		public LabelStyle LabelStyle
		{
			get
			{
				return labelStyle;
			}
			set
			{
				labelStyle = value;
				labelStyle.Axis = (Axis)this;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets a collection of custom labels.
		/// </summary>
		[
		SRCategory("CategoryAttributeLabels"),
		Bindable(true),
		SRDescription("DescriptionAttributeCustomLabels"),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
        Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base)
		]
		public CustomLabelsCollection CustomLabels
		{
			get
			{
				return _customLabels;
			}
		}

		#endregion

		#region Axis labels methods

		/// <summary>
		/// Indicates that custom grid lines should be painted.
		/// </summary>
		/// <returns>Indicates that custom grid lines should be painted.</returns>
		internal bool IsCustomGridLines()
		{
			if(this.CustomLabels.Count > 0)
			{
				// Check if at least one custom label has a flag set
				foreach(CustomLabel label in this.CustomLabels)
				{
					if((label.GridTicks & GridTickTypes.Gridline) == GridTickTypes.Gridline)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Indicates that custom tick marks should be painted.
		/// </summary>
		/// <returns>Indicates that custom tick marks should be painted.</returns>
		internal bool IsCustomTickMarks()
		{
			if(this.CustomLabels.Count > 0)
			{
				// Check if at least one custom label has a flag set
				foreach(CustomLabel label in this.CustomLabels)
				{
					if((label.GridTicks & GridTickTypes.TickMark) == GridTickTypes.TickMark)
					{
						return true;
					}
				}
			}

			return false;
		}

        /// <summary>
        /// Gets the type of the axis.
        /// </summary>
        /// <value>The type of the axis.</value>
        internal AxisType GetAxisType()
        {
            if (this.axisType == AxisName.X || this.axisType == AxisName.Y)
            {
                return AxisType.Primary;
            }
            else
            {
                return AxisType.Secondary;
            }
        }

        /// <summary>
        /// Gets the axis series.
        /// </summary>
        /// <returns></returns>
        internal ArrayList GetAxisSeries()
        {
            ArrayList dataSeries = new ArrayList();

            // check for attached series.
            foreach (string seriesName in this.ChartArea.Series)
            {
                Series series = this.Common.DataManager.Series[seriesName];
                if (this.axisType == AxisName.X || this.axisType == AxisName.X2)
                {
                    if (series.XAxisType == this.GetAxisType())
                    {
                        dataSeries.Add(series);
                    }
                }
                else
                {
                    if (series.YAxisType == this.GetAxisType())
                    {
                        dataSeries.Add(series);
                    }
                }
            }
            return dataSeries;
        }

        /// <summary>
        /// Gets the other (primary/secondary) axis.
        /// </summary>
        /// <returns></returns>
        internal Axis GetOtherTypeAxis()
        {
            return ChartArea.GetAxis(
                    this.axisType, 
                    this.GetAxisType() == AxisType.Primary ? AxisType.Secondary : AxisType.Primary, 
                    String.Empty
                );
        }

        /// <summary>
        /// Checks if the other (primary/secondary) axis has custom labels labels.
        /// These labels will be added if this axis has no series attached and no custom labels.
        /// This works only on category axes. 
        /// </summary>
        internal void PostFillLabels()
        {
            foreach (CustomLabel label in this.CustomLabels)
            {
                if (label.customLabel)
                {
                    return;
                }
            }

            // Labels are disabled for this axis
            if (
                !this.LabelStyle.Enabled || 
                !this.enabled || 
                !String.IsNullOrEmpty(((Axis)this).SubAxisName) || 
                this.axisType == AxisName.Y || 
                this.axisType == AxisName.Y2
                )
            {
                return;
            }
            
            // check if no series attached.
            if (this.GetAxisSeries().Count > 0)
            {
                return;
            }
            this.CustomLabels.Clear();
            foreach (CustomLabel label in this.GetOtherTypeAxis().CustomLabels)
            {
                this.CustomLabels.Add(label.Clone());
            }
        }

        /// <summary>
        /// Fill labels from data from data manager or 
        /// from axis scale.
        /// </summary>
        /// <param name="removeFirstRow">True if first row of auto generated labels must be removed.</param>
		internal void FillLabels(bool removeFirstRow)
        {
#if SUBAXES
			// Process all sub-axis
			foreach(SubAxis subAxis in ((Axis)this).SubAxes)
			{
				subAxis.FillLabels(true);
			}
#endif // SUBAXES

            // Labels are disabled for this axis
			if( !this.LabelStyle.Enabled || !this.enabled )
			{
				return;
			}

			// For circular chart area fill only Y axis labels
			if(this.ChartArea != null && this.ChartArea.chartAreaIsCurcular)
			{
				if(this.axisType != AxisName.Y)
				{
					ICircularChartType type = this.ChartArea.GetCircularChartType();
					if(type == null || !type.XAxisLabelsSupported())
					{
						return;
					}
				}
			}

			// Check if the custom labels exist
			bool customLabelsFlag = false;
			foreach( CustomLabel lab in CustomLabels )
			{
				if( lab.customLabel )
				{
					if( lab.RowIndex == 0 ||
						this.ChartArea.chartAreaIsCurcular)
					{
						customLabelsFlag = true;
					}
				}
			}


			// Remove the first row of labels if custom labels not exist
			if(removeFirstRow)
			{
				if( customLabelsFlag == false )
				{
					for( int index = 0; index < CustomLabels.Count; index++ )
					{
						if( CustomLabels[index].RowIndex == 0 )
						{
							CustomLabels.RemoveAt( index );
							index = -1;
						}
					}
				}
				else
				{
					return;
				}
			}

			// Get data series for this axis.
			List<string> dataSeries = null;
			switch( axisType )
			{
				case AxisName.X:
					dataSeries = ChartArea.GetXAxesSeries( AxisType.Primary, ((Axis)this).SubAxisName );
					break;
				case AxisName.Y:
					dataSeries = ChartArea.GetYAxesSeries( AxisType.Primary, ((Axis)this).SubAxisName );
					break;
				case AxisName.X2:
					dataSeries = ChartArea.GetXAxesSeries( AxisType.Secondary, ((Axis)this).SubAxisName );
					break;
				case AxisName.Y2:
					dataSeries = ChartArea.GetYAxesSeries( AxisType.Secondary, ((Axis)this).SubAxisName );
					break;
			}

			// There aren't data series connected with this axis.
			if( dataSeries.Count == 0 )
			{
				return;
			}

            //Let's convert the ArrayList of the series names into to string[]
            string[] dataSeriesNames = new string[dataSeries.Count];
            for (int i = 0; i < dataSeries.Count; i++)
                dataSeriesNames[i] = (string)dataSeries[i];

			// Check if series X values all set to zeros
            bool seriesXValuesZeros = ChartHelper.SeriesXValuesZeros(this.Common, dataSeriesNames);

            // Check if series is indexed (All X values zeros or IsXValueIndexed flag set)
            bool indexedSeries = true;
            if (!seriesXValuesZeros)
            {
                indexedSeries = ChartHelper.IndexedSeries(this.Common, dataSeriesNames);
            }

			// Show End Labels
			int endLabels = 0;
			if( labelStyle.IsEndLabelVisible )
			{
				endLabels = 1;
			}

			// Get chart type of the first series
			IChartType	chartType = Common.ChartTypeRegistry.GetChartType( ChartArea.GetFirstSeries().ChartTypeName );
			bool		fromSeries = false;
			if( !chartType.RequireAxes )
			{
				return;
			}
			else if( axisType == AxisName.Y || axisType == AxisName.Y2 )
			{
				fromSeries = false;
			}
			else
			{
				fromSeries = true;
			}

			// X values from data points are not 0.
            if (fromSeries && !ChartHelper.SeriesXValuesZeros(this.Common, dataSeries.ToArray()))
			{
				fromSeries = false;
			}

			// X values from data points are not 0.
			if( fromSeries && ( labelStyle.GetIntervalOffset() != 0 || labelStyle.GetInterval() != 0 ) )
			{
				fromSeries = false;
			}

			// Get value type
			ChartValueType valueType;
			if( axisType == AxisName.X || axisType == AxisName.X2 )
			{
				// If X value is indexed the type is always String. So we use indexed type instead
				valueType = Common.DataManager.Series[dataSeries[0]].indexedXValueType;
			}
			else
			{
				valueType = Common.DataManager.Series[dataSeries[0]].YValueType;
			}

			if( labelStyle.GetIntervalType() != DateTimeIntervalType.Auto && 
                labelStyle.GetIntervalType() != DateTimeIntervalType.Number )
			{
                if (valueType != ChartValueType.Time && 
                    valueType != ChartValueType.Date && 
                    valueType != ChartValueType.DateTimeOffset)
				{
					valueType = ChartValueType.DateTime;
				}
			}

			// ***********************************
			// Pre calculate some values
			// ***********************************
			double viewMaximum = this.ViewMaximum;
			double viewMinimum = this.ViewMinimum;

			// ***********************************
			// Labels are filled from data series.
			// ***********************************
			if( fromSeries )
			{
				int numOfPoints;
				numOfPoints = Common.DataManager.GetNumberOfPoints( dataSeries.ToArray() );

				// Show end labels
				if( endLabels == 1 )
				{
					// min position
					CustomLabels.Add( - 0.5, 0.5, ValueConverter.FormatValue(
						this.Common.Chart,
						this,
                        null,
						0.0, 
						this.LabelStyle.Format, 
						valueType,
						ChartElementType.AxisLabels), 
						false);
				}

				// Labels from point position
				for( int point = 0; point < numOfPoints; point++ )
				{
					CustomLabels.Add( ((double)point)+ 0.5, ((double)point)+ 1.5, 
						ValueConverter.FormatValue(
							this.Common.Chart,
							this,
                            null,
							point + 1, 
							this.LabelStyle.Format, 
							valueType,
							ChartElementType.AxisLabels), 
							false);
				}

				// Show end labels
				if( endLabels == 1 )
				{
					// max position
					CustomLabels.Add( ((double)numOfPoints)+ 0.5, ((double)numOfPoints)+ 1.5, 
						ValueConverter.FormatValue(
							this.Common.Chart,
							this,
                            null,
                            numOfPoints + 1, 
							this.LabelStyle.Format, 
							valueType,
							ChartElementType.AxisLabels), 
							false);
				}

				int pointIndx;
				foreach( string seriesIndx in dataSeries )
				{
					// End labels enabled
					if( endLabels == 1 )
						pointIndx = 1;
					else
						pointIndx = 0;

					// Set labels from data points labels
					foreach( DataPoint dataPoint in Common.DataManager.Series[ seriesIndx ].Points )
					{
						// Find first row of labels
						while( CustomLabels[pointIndx].RowIndex > 0 )
						{
							pointIndx++;
						}

						// Add X labels
						if( axisType == AxisName.X || axisType == AxisName.X2 )
						{
							if( dataPoint.AxisLabel.Length > 0 )
							{
								CustomLabels[pointIndx].Text = dataPoint.AxisLabel;
							}
						}
	
						pointIndx++;
					}
				}
			}
			// ***********************************
			// Labels are filled from axis scale.
			// ***********************************
			else
			{
				if( viewMinimum == viewMaximum )
					return;

				double labValue; // Value, which will be converted to text and used for, labels.
				double beginPosition; // Begin position for a label
				double endPosition; // End position for a label
				double start; // Start position for all labels

				// Get first series attached to this axis
				Series	axisSeries = null;
				if(axisType == AxisName.X || axisType == AxisName.X2)
				{
					List<string> seriesArray = ChartArea.GetXAxesSeries((axisType == AxisName.X) ? AxisType.Primary : AxisType.Secondary, ((Axis)this).SubAxisName);
					if(seriesArray.Count > 0)
					{
						axisSeries = Common.DataManager.Series[seriesArray[0]];
						if(axisSeries != null && !axisSeries.IsXValueIndexed)
						{
							axisSeries = null;
						}
					}
				}

                // ***********************************
                // Check if the AJAX zooming and scrolling mode is enabled.
                // Labels are filled slightly different in this case.
                // ***********************************
                DateTimeIntervalType offsetType = (labelStyle.GetIntervalOffsetType() == DateTimeIntervalType.Auto) ? labelStyle.GetIntervalType() : labelStyle.GetIntervalOffsetType();

				// By default start is equal to minimum
				start = viewMinimum; 

				// Adjust start position depending on the interval type
				if(!this.ChartArea.chartAreaIsCurcular ||
					this.axisType == AxisName.Y || 
					this.axisType == AxisName.Y2 )
				{
                    start = ChartHelper.AlignIntervalStart(start, labelStyle.GetInterval(), labelStyle.GetIntervalType(), axisSeries);
				}
				
				// Move start if there is start position
				if( labelStyle.GetIntervalOffset() != 0 && axisSeries == null)
				{
                    start += ChartHelper.GetIntervalSize(start, labelStyle.GetIntervalOffset(), 
						offsetType, axisSeries, 0, DateTimeIntervalType.Number, true, false);
				}

				// ***************************************
				// Date type
				// ***************************************
				if( valueType == ChartValueType.DateTime || 
					valueType == ChartValueType.Date ||
					valueType == ChartValueType.Time ||
                    valueType == ChartValueType.DateTimeOffset ||
					axisSeries != null)
				{
					double position = start;
					double dateInterval;

					// Too many labels
                    if ((viewMaximum - start) / ChartHelper.GetIntervalSize(start, labelStyle.GetInterval(), labelStyle.GetIntervalType(), axisSeries, 0, DateTimeIntervalType.Number, true) > ChartHelper.MaxNumOfGridlines)
						return;

					int	counter = 0;
                    double endLabelMaxPosition = viewMaximum - ChartHelper.GetIntervalSize(viewMaximum, labelStyle.GetInterval(), labelStyle.GetIntervalType(), axisSeries, labelStyle.GetIntervalOffset(), offsetType, true) / 2f;
                    double endLabelMinPosition = viewMinimum + ChartHelper.GetIntervalSize(viewMinimum, labelStyle.GetInterval(), labelStyle.GetIntervalType(), axisSeries, labelStyle.GetIntervalOffset(), offsetType, true) / 2f;
					while( (decimal)position <= (decimal)viewMaximum )
					{
                        dateInterval = ChartHelper.GetIntervalSize(position, labelStyle.GetInterval(), labelStyle.GetIntervalType(), axisSeries, labelStyle.GetIntervalOffset(), offsetType, true);
						labValue = position;

						// For IsLogarithmic axes
						if( this.IsLogarithmic )
						{
							labValue = Math.Pow( this.logarithmBase, labValue );
						}

                        // Check if we do not exceed max number of elements
                        if (counter++ > ChartHelper.MaxNumOfGridlines)
                        {
                            break;
                        }

                        if (endLabels == 0 && position >= endLabelMaxPosition)
                        {
                            break;
                        }

						beginPosition = position - dateInterval * 0.5;
						endPosition = position + dateInterval * 0.5;

						if(endLabels == 0 && position <=  endLabelMinPosition)
						{
							position += dateInterval;
							continue;
						}
						
						if( (decimal)beginPosition > (decimal)viewMaximum )
						{
							position += dateInterval;
							continue;
						}

                        // NOTE: Fixes issue #6466
                        // Following code is removed due to the issues caused by the rounding error

                        //if( (((decimal)beginPosition + (decimal)endPosition) / 2.0m) < (decimal)viewMinimum )
                        //{
                        //    position += dateInterval;
                        //    continue;
                        //}
                        //if ((decimal)viewMaximum < (((decimal)beginPosition + (decimal)endPosition) / 2m))
                        //{
                        //    position += dateInterval;
                        //    continue;
                        //}

						string pointLabel = GetPointLabel( dataSeries, labValue, !seriesXValuesZeros, indexedSeries );
						if( pointLabel.Length == 0 )
						{
							// Do not draw last label for indexed series
							if( position <= this.maximum )
							{
								// Add a label to the collection
								if( position != this.maximum || !Common.DataManager.Series[ dataSeries[0] ].IsXValueIndexed )
								{
									CustomLabels.Add( beginPosition, 
										endPosition, 
										ValueConverter.FormatValue(
											this.Common.Chart,
											this,
                                            null,
											labValue, 
											this.LabelStyle.Format, 
											valueType,
											ChartElementType.AxisLabels),
										false);
								}
							}
						}
						else
						{
							// Add a label to the collection
							CustomLabels.Add( beginPosition, 
								endPosition, 
								pointLabel,
								false);
						}
						position += dateInterval;
					}
				}
				else
				{
					// ***************************************
					// Scale value type
					// ***************************************

					// Show First label if Start Label position is used
					if( start != viewMinimum )
						endLabels = 1;

					// Set labels
					int labelCounter = 0;
                    for (double position = start - endLabels * labelStyle.GetInterval(); position < viewMaximum - 1.5 * labelStyle.GetInterval() * (1 - endLabels); position = (double)((decimal)position + (decimal)labelStyle.GetInterval()))
					{
						// Prevent endless loop that may be caused by very small interval
						// and double/decimal rounding errors
						++labelCounter;
						if(labelCounter > ChartHelper.MaxNumOfGridlines)
						{
							break;
						}

						labValue = (double)((decimal)position + (decimal)labelStyle.GetInterval());

						// This line is introduce because sometimes 0 value will appear as 
						// very small value close to zero.
						double inter = Math.Log(labelStyle.GetInterval());
						double valu = Math.Log(Math.Abs(labValue));
						int digits = (int)Math.Abs(inter)+5;

						if( digits > 15 )
						{
							digits = 15;
						}

						if( Math.Abs(inter) < Math.Abs(valu)-5 )
						{
							labValue = Math.Round(labValue,digits);
						}

						// Too many labels
						if( ( viewMaximum - start ) / labelStyle.GetInterval() > ChartHelper.MaxNumOfGridlines )
						{
							return;
						}

						// For IsLogarithmic axes
						if( this.IsLogarithmic )
							labValue = Math.Pow( this.logarithmBase, labValue );

						beginPosition = (double)((decimal)position + (decimal)labelStyle.GetInterval() * 0.5m);
						endPosition = (double)((decimal)position + (decimal)labelStyle.GetInterval() * 1.5m);
						
						if( (decimal)beginPosition > (decimal)viewMaximum )
						{
							continue;
						}

						// Show End label if Start Label position is used
						// Use decimal type to solve rounding issues
						if( (decimal)(( beginPosition + endPosition )/2.0) > (decimal)viewMaximum )
						{
							continue;
						}

						string pointLabel = GetPointLabel( dataSeries, labValue, !seriesXValuesZeros, indexedSeries  );
						if( pointLabel.Length > 15 && labValue < 0.000001)
						{
							labValue = 0.0;
						}

						if( pointLabel.Length == 0 )
						{
							// Do not draw last label for indexed series
							if( !(Common.DataManager.Series[ dataSeries[0] ].IsXValueIndexed && position > this.maximum) )
							{
								// Add a label to the collection
								CustomLabels.Add( beginPosition, 
									endPosition, 
									ValueConverter.FormatValue(
										this.Common.Chart,
										this,
                                        null,
										labValue, 
										this.LabelStyle.Format, 
										valueType,
										ChartElementType.AxisLabels),
									false);
							}
						}
						else
						{
							// Add a label to the collection
							CustomLabels.Add( beginPosition, 
								endPosition, 
								pointLabel,
								false);
						}
					}
				}
			}
		}

		/// <summary>
		/// This method checks if there is a data point which has value X equal 
		/// to valuePosition, and returns label from data point if such value exist. 
		/// If data point with this value not exists empty string will be returned. 
		/// If all data points have X value zero, index is used instead of X value.
		/// </summary>
		/// <param name="series">Data series</param>
		/// <param name="valuePosition">A value which should be found in data points x values</param>
		/// <param name="nonZeroXValues">Series X values are not zeros.</param>
		/// <param name="indexedSeries">Series is indexed. All X values are zeros or IsXValueIndexed flag set.</param>
		/// <returns>LabelStyle</returns>
		private string GetPointLabel( 
			List<string> series, 
			double valuePosition, 
			bool nonZeroXValues, 
			bool indexedSeries
			)
		{
            // Get max number of data points in the series
            int maxPointCount = 0;
            foreach (string seriesName in series)
            {
                Series ser = Common.DataManager.Series[seriesName];
                maxPointCount = Math.Max(maxPointCount, ser.Points.Count); 
            }

            // Check if axis only contains axis abels
			bool allEmpty = true;
			foreach( string seriesName in series )
			{
				// Get series by name
				Series ser = Common.DataManager.Series[ seriesName ];

				// Check if series has axis labels set
                if ((axisType == AxisName.X || axisType == AxisName.X2) && (margin != 0 || maxPointCount == 1 || !this._autoMinimum) && !ser.IsXValueIndexed)
				{
					if( ser.Points[ 0 ].AxisLabel.Length > 0 && ser.Points[ ser.Points.Count - 1 ].AxisLabel.Length > 0 )
					{
						allEmpty = false;
					}
				}

				// Try getting label from the point
				if(!ser.noLabelsInPoints || (nonZeroXValues && indexedSeries))
				{
					string result = GetPointLabel( ser, valuePosition, nonZeroXValues, indexedSeries );
					if(!String.IsNullOrEmpty(result))
					{
						return result;
					}
				}

                // VSTS 140676: Serach for IndexedSeriesLabelsSourceAttr attribute 
                // to find if we have indexed series as source of formula generated nonindexed series.
                String labelSeriesName = ser[DataFormula.IndexedSeriesLabelsSourceAttr];
                if (!String.IsNullOrEmpty(labelSeriesName))
                {
                    Series labelsSeries = Common.DataManager.Series[labelSeriesName];
                    if (labelsSeries != null)
                    {
                        string result = GetPointLabel(labelsSeries, valuePosition, nonZeroXValues, true);
                        if (!String.IsNullOrEmpty(result))
                        {
                            return result;
                        }
                    }
                }

			}

			if( !allEmpty )
			{
                return " ";
			}
			else
			{
				return "";
			}
		}
		
		/// <summary>
		/// This method checks if there is a data point which has value X equal 
		/// to valuePosition, and returns label from data point if such value exist. 
		/// If data point with this value not exists empty string will be returned. 
		/// If all data points have X value zero, index is used instead of X value.
		/// </summary>
		/// <param name="series">Data series</param>
		/// <param name="valuePosition">A value which should be found in data points x values</param>
		/// <param name="nonZeroXValues">Series X values are not zeros.</param>
		/// <param name="indexedSeries">Series is indexed. All X values are zeros or IsXValueIndexed flag set.</param>
		/// <returns>LabelStyle</returns>
		private string GetPointLabel( 
			Series series, 
			double valuePosition, 
			bool nonZeroXValues, 
			bool indexedSeries)
		{
			int pointIndx = 1;

			if( axisType == AxisName.Y || axisType == AxisName.Y2 )
			{
				return "";
			}

			if( !(( axisType == AxisName.X && series.XAxisType == AxisType.Primary ) || ( axisType == AxisName.X2 && series.XAxisType == AxisType.Secondary )) )
            {
#if SUBAXES
				if(series.XSubAxisName != ((Axis)this).SubAxisName)
				{
					return "";
				}
#endif // SUBAXES
                return "";
			}

			// Loop through all series data points
			foreach( DataPoint point in series.Points )
			{
				// If series is indexed (all X values are zeros or IsXValueIndexed flag set)
				if( indexedSeries )
				{
					// If axis label position matches point index
					if( valuePosition == pointIndx )
					{
						// Use X value if axis label is not set and X values in series are not zeros
						if(point.AxisLabel.Length == 0 && nonZeroXValues)
						{
							return ValueConverter.FormatValue(
								this.Common.Chart,
								this,
                                null,
								point.XValue, 
								this.LabelStyle.Format, 
								series.XValueType,
								ChartElementType.AxisLabels);
						}

                        // Return axis label from data point
						return point.ReplaceKeywords(point.AxisLabel);
					}
				}
				else
				{
					// Find x value using Data point X values
					if( point.XValue == valuePosition )
					{
						// Return  label 
						return point.ReplaceKeywords(point.AxisLabel);
					}
				}
				pointIndx++;
			}
			return "";
		}

		#endregion
	}
}
