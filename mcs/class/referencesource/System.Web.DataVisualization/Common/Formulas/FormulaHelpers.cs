//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=victark, alexgor, deliant
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;

#if Microsoft_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting.Formulas
#else
namespace System.Web.UI.DataVisualization.Charting.Formulas
#endif
{

    #region class FormulaHelper
    /// <summary>
    /// Formula helper is a static utility class implementing common formula related routines.
    /// </summary>
    internal static class FormulaHelper
    {
        #region Static
        /// <summary>
        /// Gets the formula info instance.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <returns>FomulaInfo instance</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal static FormulaInfo GetFormulaInfo(FinancialFormula formula)
        {
            switch (formula)
            {
                //Price indicators
                case FinancialFormula.MovingAverage:
                    return new MovingAverageFormulaInfo();
                case FinancialFormula.ExponentialMovingAverage:
                    return new ExponentialMovingAverageFormulaInfo();
                case FinancialFormula.WeightedMovingAverage:
                    return new WeightedMovingAverageFormulaInfo();
                case FinancialFormula.TriangularMovingAverage:
                    return new TriangularMovingAverageFormulaInfo();
                case FinancialFormula.TripleExponentialMovingAverage:
                    return new TripleExponentialMovingAverageFormulaInfo();
                case FinancialFormula.BollingerBands:
                    return new BollingerBandsFormulaInfo();
                case FinancialFormula.TypicalPrice:
                    return new TypicalPriceFormulaInfo();
                case FinancialFormula.WeightedClose:
                    return new WeightedCloseFormulaInfo();
                case FinancialFormula.MedianPrice:
                    return new MedianPriceFormulaInfo();
                case FinancialFormula.Envelopes:
                    return new EnvelopesFormulaInfo();
                case FinancialFormula.StandardDeviation:
                    return new StandardDeviationFormulaInfo();

                // Oscilators
                case FinancialFormula.ChaikinOscillator:
                    return new ChaikinOscillatorFormulaInfo();
                case FinancialFormula.DetrendedPriceOscillator:
                    return new DetrendedPriceOscillatorFormulaInfo();
                case FinancialFormula.VolatilityChaikins:
                    return new VolatilityChaikinsFormulaInfo();
                case FinancialFormula.VolumeOscillator:
                    return new VolumeOscillatorFormulaInfo();
                case FinancialFormula.StochasticIndicator:
                    return new StochasticIndicatorFormulaInfo();
                case FinancialFormula.WilliamsR:
                    return new WilliamsRFormulaInfo();

                // General technical indicators
                case FinancialFormula.AverageTrueRange:
                    return new AverageTrueRangeFormulaInfo();
                case FinancialFormula.EaseOfMovement:
                    return new EaseOfMovementFormulaInfo();
                case FinancialFormula.MassIndex:
                    return new MassIndexFormulaInfo();
                case FinancialFormula.Performance:
                    return new PerformanceFormulaInfo();
                case FinancialFormula.RateOfChange:
                    return new RateOfChangeFormulaInfo();
                case FinancialFormula.RelativeStrengthIndex:
                    return new RelativeStrengthIndexFormulaInfo();
                case FinancialFormula.MovingAverageConvergenceDivergence:
                    return new MovingAverageConvergenceDivergenceFormulaInfo();
                case FinancialFormula.CommodityChannelIndex:
                    return new CommodityChannelIndexFormulaInfo();

                // Forecasting
                case FinancialFormula.Forecasting:
                    return new ForecastingFormulaInfo();

                // Volume Indicators
                case FinancialFormula.MoneyFlow:
                    return new MoneyFlowFormulaInfo();
                case FinancialFormula.PriceVolumeTrend:
                    return new PriceVolumeTrendFormulaInfo();
                case FinancialFormula.OnBalanceVolume:
                    return new OnBalanceVolumeFormulaInfo();
                case FinancialFormula.NegativeVolumeIndex:
                    return new NegativeVolumeIndexFormulaInfo();
                case FinancialFormula.PositiveVolumeIndex:
                    return new PositiveVolumeIndexFormulaInfo();
                case FinancialFormula.AccumulationDistribution:
                    return new AccumulationDistributionFormulaInfo();

                default:
                    Debug.Fail(String.Format(CultureInfo.InvariantCulture, "{0} case is not defined", formula));
                    return null;
            }
        }

        /// <summary>
        /// Gets the data fields of the specified chart type.
        /// </summary>
        /// <param name="chartType">Type of the chart.</param>
        /// <returns>Data fields</returns>
        internal static IList<DataField> GetDataFields(SeriesChartType chartType)
        {
            switch (chartType)
            {
                case SeriesChartType.BoxPlot:
                    return new DataField[] { 
                        DataField.LowerWisker, DataField.UpperWisker, 
                        DataField.LowerBox, DataField.UpperBox, 
                        DataField.Average, DataField.Median };
                case SeriesChartType.Bubble:
                    return new DataField[] { 
                        DataField.Bubble, DataField.BubbleSize };
                case SeriesChartType.Candlestick:
                case SeriesChartType.Stock:
                    return new DataField[] { 
                        DataField.High, DataField.Low,
                        DataField.Open, DataField.Close };
                case SeriesChartType.ErrorBar:
                    return new DataField[] { 
                        DataField.Center, 
                        DataField.LowerError, DataField.UpperError};
                case SeriesChartType.RangeBar:
                case SeriesChartType.Range:
                case SeriesChartType.RangeColumn:
                case SeriesChartType.SplineRange:
                    return new DataField[] { 
                        DataField.Top, DataField.Bottom };
                default:
                    return new DataField[] { DataField.Y };
            }
        }

        /// <summary>
        /// Gets the default type of the chart associated with this field name.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        internal static SeriesChartType GetDefaultChartType(DataField field)
        {
            switch (field)
            {
                default:
                case DataField.Y:
                    return SeriesChartType.Line;
                case DataField.LowerWisker:
                case DataField.UpperWisker:
                case DataField.LowerBox:
                case DataField.UpperBox:
                case DataField.Average:
                case DataField.Median:
                    return SeriesChartType.BoxPlot;
                case DataField.Bubble:
                case DataField.BubbleSize:
                    return SeriesChartType.Bubble;
                case DataField.High:
                case DataField.Low:
                case DataField.Open:
                case DataField.Close:
                    return SeriesChartType.Stock;
                case DataField.Center:
                case DataField.LowerError:
                case DataField.UpperError:
                    return SeriesChartType.ErrorBar;
                case DataField.Top:
                case DataField.Bottom:
                    return SeriesChartType.Range;
            }
        }

        /// <summary>
        /// Maps formula data field to a chart type specific data field. 
        /// </summary>
        /// <param name="chartType">Type of the chart.</param>
        /// <param name="formulaField">The formula field to be mapped.</param>
        /// <returns>The series field</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static DataField? MapFormulaDataField(SeriesChartType chartType, DataField formulaField)
        {
            switch (formulaField)
            {
                case DataField.Top:
                case DataField.High:
                    switch (chartType)
                    {
                        default: return null;
                        case SeriesChartType.BoxPlot: return DataField.UpperBox;
                        case SeriesChartType.Candlestick:
                        case SeriesChartType.Stock: return DataField.High;
                        case SeriesChartType.ErrorBar: return DataField.UpperError;
                        case SeriesChartType.RangeBar:
                        case SeriesChartType.Range:
                        case SeriesChartType.RangeColumn:
                        case SeriesChartType.SplineRange: return DataField.Top;
                    }

                case DataField.Bottom:
                case DataField.Low:
                    switch (chartType)
                    {
                        default: return null;
                        case SeriesChartType.BoxPlot: return DataField.LowerBox;
                        case SeriesChartType.Candlestick:
                        case SeriesChartType.Stock: return DataField.Low;
                        case SeriesChartType.ErrorBar: return DataField.LowerError;
                        case SeriesChartType.RangeBar:
                        case SeriesChartType.Range:
                        case SeriesChartType.RangeColumn:
                        case SeriesChartType.SplineRange: return DataField.Bottom;
                    }

                case DataField.Open:
                    switch (chartType)
                    {
                        default: return null;
                        case SeriesChartType.BoxPlot: return DataField.Average;
                        case SeriesChartType.Candlestick:
                        case SeriesChartType.Stock: return DataField.Open;
                        case SeriesChartType.ErrorBar: return DataField.Center;
                        case SeriesChartType.RangeBar:
                        case SeriesChartType.Range:
                        case SeriesChartType.RangeColumn:
                        case SeriesChartType.SplineRange: return DataField.Bottom;
                    }

                case DataField.Close:
                case DataField.Y:
                    switch (chartType)
                    {
                        default: return DataField.Y;
                        case SeriesChartType.BoxPlot: return DataField.Average;
                        case SeriesChartType.Bubble: return DataField.Bubble;
                        case SeriesChartType.Candlestick:
                        case SeriesChartType.Stock: return DataField.Close;
                        case SeriesChartType.ErrorBar: return DataField.Center;
                        case SeriesChartType.RangeBar:
                        case SeriesChartType.Range:
                        case SeriesChartType.RangeColumn:
                        case SeriesChartType.SplineRange: return DataField.Top;
                    }
                default:
                    return null;
            }
        }

        #endregion
    }
    #endregion

    #region class FormulaInfo and inherited FormulaSpecific classes

    /// <summary>
    /// This a base class of the formula metainfo classes.
    /// </summary>
    internal abstract class FormulaInfo
    {
        #region Fields
        DataField[] _inputFields;
        DataField[] _outputFields;
        object[] _parameters;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the input data fields of the formula.
        /// </summary>
        /// <value>The input fields.</value>
        public DataField[] InputFields 
        { 
            get { return _inputFields; } 
        }

        /// <summary>
        /// Gets the output data fields of the formula.
        /// </summary>
        /// <value>The output fields.</value>
        public DataField[] OutputFields 
        { 
            get { return _outputFields; } 
        }

        /// <summary>
        /// Gets the parameters of the formula.
        /// </summary>
        /// <value>The parameters.</value>
        public object[] Parameters 
        { 
            get { return _parameters; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FormulaInfo"/> class.
        /// </summary>
        /// <param name="inputFields">The input data fields.</param>
        /// <param name="outputFields">The output data fields.</param>
        /// <param name="defaultParams">The default formula params.</param>
        public FormulaInfo(DataField[] inputFields, DataField[] outputFields, params object[] defaultParams)
        {
            _inputFields = inputFields;
            _outputFields = outputFields;
            _parameters = defaultParams;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Saves the formula parameters to a string.
        /// </summary>
        /// <returns>Csv string with parameters</returns>
        internal virtual string SaveParametersToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _parameters.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", _parameters[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Loads the formula parameters from string.
        /// </summary>
        /// <param name="parameters">Csv string with parameters.</param>
        internal virtual void LoadParametersFromString(string parameters)
        {
            if (String.IsNullOrEmpty(parameters))
                return;

            string[] paramStringList = parameters.Split(',');
            int paramStringIndex = 0;
            for (int i = 0; i < _parameters.Length && paramStringIndex < paramStringList.Length; i++)
            {
                string newParamValue = paramStringList[paramStringIndex++];
                if (!String.IsNullOrEmpty(newParamValue))
                {
                    _parameters[i] = ParseParameter(i, newParamValue);
                }
            }
        }

        /// <summary>
        /// Parses the formula parameter.
        /// </summary>
        /// <param name="index">The param index.</param>
        /// <param name="newParamValue">The parameter value string.</param>
        /// <returns>Parameter value.</returns>
        internal virtual object ParseParameter(int index, string newParamValue)
        {
            object param = _parameters[index];
            if (param is int)
            {
                return Convert.ToInt32(newParamValue, CultureInfo.InvariantCulture);
            }
            else if (param is bool)
            {
                return Convert.ToBoolean(newParamValue, CultureInfo.InvariantCulture);
            }
            else if (param is double)
            {
                return Convert.ToDouble(newParamValue, CultureInfo.InvariantCulture);
            }
            return null;
        }

        /// <summary>
        /// Checks the formula parameter string.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        internal virtual void CheckParameterString(string parameters)
        {
            if (String.IsNullOrEmpty(parameters))
                return;

            string[] paramStringList = parameters.Split(',');
            int paramStringIndex = 0;
            for (int i = 0; i < _parameters.Length && paramStringIndex < paramStringList.Length; i++)
            {
                string newParamValue = paramStringList[paramStringIndex++];
                if (!String.IsNullOrEmpty(newParamValue))
                {
                    try
                    {
                        ParseParameter(i, newParamValue);
                    }
                    catch (FormatException)
                    {
                        throw new ArgumentException(SR.ExceptionFormulaDataFormatInvalid(parameters));
                    }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// MovingAverage FormulaInfo
    /// </summary>
    internal class MovingAverageFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MovingAverageFormulaInfo"/> class.
        /// </summary>
        public MovingAverageFormulaInfo()
            : this(2, false)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MovingAverageFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="startFromFirst">if set to <c>true</c> [start from first].</param>
        public MovingAverageFormulaInfo(int period, bool startFromFirst)
            : base(
                new DataField[] { DataField.Y }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period, startFromFirst)
        {
        }
    }

    /// <summary>
    /// ExponentialMoving AverageFormulaInfo
    /// </summary>
    internal class ExponentialMovingAverageFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialMovingAverageFormulaInfo"/> class.
        /// </summary>
        public ExponentialMovingAverageFormulaInfo()
            : this(2, false)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialMovingAverageFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="startFromFirst">if set to <c>true</c> [start from first].</param>
        public ExponentialMovingAverageFormulaInfo(int period, bool startFromFirst)
            : base(
                new DataField[] { DataField.Y }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period, startFromFirst)
        {
        }
    }

    /// <summary>
    /// WeightedMovingAverageFormulaInfo
    /// </summary>
    internal class WeightedMovingAverageFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="WeightedMovingAverageFormulaInfo"/> class.
        /// </summary>
        public WeightedMovingAverageFormulaInfo()
            : this(2, false)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WeightedMovingAverageFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="startFromFirst">if set to <c>true</c> [start from first].</param>
        public WeightedMovingAverageFormulaInfo(int period, bool startFromFirst)
           : base(
                new DataField[] { DataField.Y }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period, startFromFirst)
        {
        }
    }

    /// <summary>
    /// TriangularMovingAverage FormulaInfo
    /// </summary>
    internal class TriangularMovingAverageFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TriangularMovingAverageFormulaInfo"/> class.
        /// </summary>
        public TriangularMovingAverageFormulaInfo()
            : this(2, false)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TriangularMovingAverageFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="startFromFirst">if set to <c>true</c> [start from first].</param>
        public TriangularMovingAverageFormulaInfo(int period, bool startFromFirst)
            : base(
                new DataField[] { DataField.Y }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period, startFromFirst)
        {
        }
    }

    /// <summary>
    /// TripleExponentialMovingAverage FormulaInfo
    /// </summary>
    internal class TripleExponentialMovingAverageFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TripleExponentialMovingAverageFormulaInfo"/> class.
        /// </summary>
        public TripleExponentialMovingAverageFormulaInfo()
            : this(12)                           //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TripleExponentialMovingAverageFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        public TripleExponentialMovingAverageFormulaInfo(int period)
            : base(
                new DataField[] { DataField.Y }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period)
        {
        }
    }

    /// <summary>
    /// BollingerBands FormulaInfo
    /// </summary>
    internal class BollingerBandsFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="BollingerBandsFormulaInfo"/> class.
        /// </summary>
        public BollingerBandsFormulaInfo()
            : this(3, 2, true)                                          //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BollingerBandsFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="deviation">The deviation.</param>
        /// <param name="startFromFirst">if set to <c>true</c> [start from first].</param>
        public BollingerBandsFormulaInfo(int period, double deviation, bool startFromFirst)
            : base(
                new DataField[] { DataField.Y },                        //Input fields
                new DataField[] { DataField.Top, DataField.Bottom },    //Output fields
                period, deviation, startFromFirst)
        {
        }
    }

    /// <summary>
    /// TypicalPrice FormulaInfo
    /// </summary>
    internal class TypicalPriceFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TypicalPriceFormulaInfo"/> class.
        /// </summary>
        public TypicalPriceFormulaInfo()
            : base(
                new DataField[] { DataField.Close, DataField.High, DataField.Low }, //Input fields
                new DataField[] { DataField.Y })                                    //Output fields
        {
        }
    }

    /// <summary>
    /// WeightedClose FormulaInfo
    /// </summary>
    internal class WeightedCloseFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="WeightedCloseFormulaInfo"/> class.
        /// </summary>
        public WeightedCloseFormulaInfo()
            : base(
                new DataField[] { DataField.Close, DataField.High, DataField.Low }, //Input fields
                new DataField[] { DataField.Y })                                    //Output fields
        {
        }
    }

    /// <summary>
    /// MedianPrice FormulaInfo
    /// </summary>
    internal class MedianPriceFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MedianPriceFormulaInfo"/> class.
        /// </summary>
        public MedianPriceFormulaInfo()
            : base(
                new DataField[] { DataField.High, DataField.Low }, //Input fields
                new DataField[] { DataField.Y })                    //Output fields
        {
        }
    }


    /// <summary>
    /// Envelopes FormulaInfo
    /// </summary>
    internal class EnvelopesFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvelopesFormulaInfo"/> class.
        /// </summary>
        public EnvelopesFormulaInfo()
            : this(2, 10, true)                                          //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvelopesFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="shiftPercentage">The shift percentage.</param>
        /// <param name="startFromFirst">if set to <c>true</c> [start from first].</param>
        public EnvelopesFormulaInfo(int period, double shiftPercentage, bool startFromFirst)
            : base(
                new DataField[] { DataField.Y },                        //Input fields
                new DataField[] { DataField.Top, DataField.Bottom },    //Output fields
                period, shiftPercentage, startFromFirst)
        {
        }
    }


    /// <summary>
    /// StandardDeviation FormulaInfo
    /// </summary>
    internal class StandardDeviationFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDeviationFormulaInfo"/> class.
        /// </summary>
        public StandardDeviationFormulaInfo()
            : this(2, false)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDeviationFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="startFromFirst">if set to <c>true</c> [start from first].</param>
        public StandardDeviationFormulaInfo(int period, bool startFromFirst)
            : base(
                new DataField[] { DataField.Y }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period, startFromFirst)
        {
        }
    }

    /// <summary>
    /// ChaikinOscillatorFormulaInfo
    /// </summary>
    internal class ChaikinOscillatorFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ChaikinOscillatorFormulaInfo"/> class.
        /// </summary>
        public ChaikinOscillatorFormulaInfo()
            : this(3, 10, false)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ChaikinOscillatorFormulaInfo"/> class.
        /// </summary>
        /// <param name="shortPeriod">The short period.</param>
        /// <param name="longPeriod">The long period.</param>
        /// <param name="startFromFirst">if set to <c>true</c> [start from first].</param>
        public ChaikinOscillatorFormulaInfo(int shortPeriod, int longPeriod, bool startFromFirst)
            : base(
                new DataField[] { DataField.High, DataField.Low, DataField.Close, DataField.Y }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                shortPeriod, longPeriod, startFromFirst)
        {
        }
    }

    /// <summary>
    /// DetrendedPriceOscillator FormulaInfo
    /// </summary>
    internal class DetrendedPriceOscillatorFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DetrendedPriceOscillatorFormulaInfo"/> class.
        /// </summary>
        public DetrendedPriceOscillatorFormulaInfo()
            : this(2, false)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DetrendedPriceOscillatorFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="startFromFirst">if set to <c>true</c> [start from first].</param>
        public DetrendedPriceOscillatorFormulaInfo(int period, bool startFromFirst)
            : base(
                new DataField[] { DataField.Y }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period, startFromFirst)
        {
        }
    }


    /// <summary>
    /// VolatilityChaikins FormulaInfo
    /// </summary>
    internal class VolatilityChaikinsFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilityChaikinsFormulaInfo"/> class.
        /// </summary>
        public VolatilityChaikinsFormulaInfo()
            : this(10, 10)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilityChaikinsFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="signalPeriod">The signal period.</param>
        public VolatilityChaikinsFormulaInfo(int period, int signalPeriod)
            : base(
                new DataField[] { DataField.High, DataField.Low }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period, signalPeriod)
        {
        }
    }

    /// <summary>
    /// VolumeOscillator FormulaInfo
    /// </summary>
    internal class VolumeOscillatorFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeOscillatorFormulaInfo"/> class.
        /// </summary>
        public VolumeOscillatorFormulaInfo()
            : this(5, 10, true)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeOscillatorFormulaInfo"/> class.
        /// </summary>
        /// <param name="shortPeriod">The short period.</param>
        /// <param name="longPeriod">The long period.</param>
        /// <param name="percentage">if set to <c>true</c> [percentage].</param>
        public VolumeOscillatorFormulaInfo(int shortPeriod, int longPeriod, bool percentage)
            : base(
                new DataField[] { DataField.Y }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                shortPeriod, longPeriod, percentage)
        {
        }
    }

    /// <summary>
    /// StochasticIndicatorFormulaInfo
    /// </summary>
    internal class StochasticIndicatorFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="StochasticIndicatorFormulaInfo"/> class.
        /// </summary>
        public StochasticIndicatorFormulaInfo()
            : this(10, 10)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="StochasticIndicatorFormulaInfo"/> class.
        /// </summary>
        /// <param name="periodD">The period D.</param>
        /// <param name="periodK">The period K.</param>
        public StochasticIndicatorFormulaInfo(int periodD, int periodK)
            : base(
                new DataField[] { DataField.High, DataField.Low, DataField.Close }, //Input fields
                new DataField[] { DataField.Y, DataField.Y }, //Output fields
                periodD, periodK)
        {
        }
    }

    /// <summary>
    /// WilliamsRFormulaInfo
    /// </summary>
    internal class WilliamsRFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="WilliamsRFormulaInfo"/> class.
        /// </summary>
        public WilliamsRFormulaInfo()
            : this(14)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WilliamsRFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        public WilliamsRFormulaInfo(int period)
            : base(
                new DataField[] { DataField.High, DataField.Low, DataField.Close }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period)
        {
        }
    }

    /// <summary>
    /// AverageTrueRange FormulaInfo
    /// </summary>
    internal class AverageTrueRangeFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AverageTrueRangeFormulaInfo"/> class.
        /// </summary>
        public AverageTrueRangeFormulaInfo()
            : this(14)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AverageTrueRangeFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        public AverageTrueRangeFormulaInfo(int period)
            : base(
                new DataField[] { DataField.High, DataField.Low, DataField.Close }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period)
        {
        }
    }

    /// <summary>
    /// EaseOfMovement FormulaInfo
    /// </summary>
    internal class EaseOfMovementFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="EaseOfMovementFormulaInfo"/> class.
        /// </summary>
        public EaseOfMovementFormulaInfo()
            : base(
                new DataField[] { DataField.High, DataField.Low, DataField.Close }, //Input fields
                new DataField[] { DataField.Y }) //Output fields
        {
        }
    }

    /// <summary>
    /// MassIndex FormulaInfo
    /// </summary>
    internal class MassIndexFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MassIndexFormulaInfo"/> class.
        /// </summary>
        public MassIndexFormulaInfo()
            : this(25, 9)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MassIndexFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="averagePeriod">The average period.</param>
        public MassIndexFormulaInfo(int period, int averagePeriod)
            : base(
                new DataField[] { DataField.High, DataField.Low }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period, averagePeriod)
        {
        }
    }

    /// <summary>
    /// Performance FormulaInfo
    /// </summary>
    internal class PerformanceFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceFormulaInfo"/> class.
        /// </summary>
        public PerformanceFormulaInfo()
            : base(
                new DataField[] { DataField.Close }, //Input fields
                new DataField[] { DataField.Y }) //Output fields
        {
        }
    }

    /// <summary>
    /// RateOfChange FormulaInfo
    /// </summary>
    internal class RateOfChangeFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RateOfChangeFormulaInfo"/> class.
        /// </summary>
        public RateOfChangeFormulaInfo()
            : this(10)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RateOfChangeFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        public RateOfChangeFormulaInfo(int period)
            : base(
                new DataField[] { DataField.Close }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period)
        {
        }
    }

    /// <summary>
    /// RelativeStrengthIndex FormulaInfo
    /// </summary>
    internal class RelativeStrengthIndexFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeStrengthIndexFormulaInfo"/> class.
        /// </summary>
        public RelativeStrengthIndexFormulaInfo()
            : this(10)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeStrengthIndexFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        public RelativeStrengthIndexFormulaInfo(int period)
            : base(
                new DataField[] { DataField.Close }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period)
        {
        }
    }

    /// <summary>
    /// MovingAverageConvergenceDivergence FormulaInfo
    /// </summary>
    internal class MovingAverageConvergenceDivergenceFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MovingAverageConvergenceDivergenceFormulaInfo"/> class.
        /// </summary>
        public MovingAverageConvergenceDivergenceFormulaInfo()
            : this(12, 26)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MovingAverageConvergenceDivergenceFormulaInfo"/> class.
        /// </summary>
        /// <param name="shortPeriod">The short period.</param>
        /// <param name="longPeriod">The long period.</param>
        public MovingAverageConvergenceDivergenceFormulaInfo(int shortPeriod, int longPeriod)
            : base(
                new DataField[] { DataField.Close }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                shortPeriod, longPeriod)
        {
        }
    }

    /// <summary>
    /// CommodityChannelIndex FormulaInfo
    /// </summary>
    internal class CommodityChannelIndexFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CommodityChannelIndexFormulaInfo"/> class.
        /// </summary>
        public CommodityChannelIndexFormulaInfo()
            : this(10)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CommodityChannelIndexFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        public CommodityChannelIndexFormulaInfo(int period)
            : base(
                new DataField[] { DataField.High, DataField.Low, DataField.Close }, //Input fields
                new DataField[] { DataField.Y }, //Output fields
                period)
        {
        }
    }

    /// <summary>
    /// Forecasting FormulaInfo
    /// </summary>
    internal class ForecastingFormulaInfo : FormulaInfo
    {
        //Fields
        string _parameters;

        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ForecastingFormulaInfo"/> class.
        /// </summary>
        public ForecastingFormulaInfo()
            : this(TimeSeriesAndForecasting.RegressionType.Polynomial, 2, 0, true, true)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ForecastingFormulaInfo"/> class.
        /// </summary>
        /// <param name="regressionType">Type of the regression.</param>
        /// <param name="polynomialDegree">The polynomial degree.</param>
        /// <param name="forecastingPeriod">The forecasting period.</param>
        /// <param name="returnApproximationError">if set to <c>true</c> [return approximation error].</param>
        /// <param name="returnForecastingError">if set to <c>true</c> [return forecasting error].</param>
        public ForecastingFormulaInfo(TimeSeriesAndForecasting.RegressionType regressionType, int polynomialDegree, int forecastingPeriod, bool returnApproximationError, bool returnForecastingError)
            : base(
                new DataField[] { DataField.Close }, //Input fields
                new DataField[] { DataField.Close, DataField.High, DataField.Low }, //Output fields
                regressionType, polynomialDegree, forecastingPeriod, returnApproximationError, returnForecastingError)
        {
        }

        //Methods
        /// <summary>
        /// Loads the formula parameters from string.
        /// </summary>
        /// <param name="parameters">Csv string with parameters.</param>
        internal override void LoadParametersFromString(string parameters)
        {
            _parameters = parameters;
        }

        /// <summary>
        /// Checks the formula parameter string.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        internal override void CheckParameterString(string parameters)
        {
            if (String.IsNullOrEmpty(parameters))
                return;

            string[] paramStringList = parameters.Split(',');
            int paramStringIndex = 1;
            //Don't check the first param
            for (int i = 2; i < Parameters.Length && paramStringIndex < paramStringList.Length; i++)
            {
                string newParamValue = paramStringList[paramStringIndex++];
                if (!String.IsNullOrEmpty(newParamValue))
                {
                    try
                    {
                        ParseParameter(i, newParamValue);
                    }
                    catch (FormatException)
                    {
                        throw new ArgumentException(SR.ExceptionFormulaDataFormatInvalid(parameters));
                    }
                }
            }
        }

        /// <summary>
        /// Saves the formula parameters to a string.
        /// </summary>
        /// <returns>Csv string with parameters</returns>
        internal override string SaveParametersToString()
        {
            if (String.IsNullOrEmpty(_parameters))
                return _parameters;
            else
                return "2,0,true,true";
        }
    }

    /// <summary>
    /// MoneyFlow FormulaInfo
    /// </summary>
    internal class MoneyFlowFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MoneyFlowFormulaInfo"/> class.
        /// </summary>
        public MoneyFlowFormulaInfo()
            : this(2)                      //Defaults
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MoneyFlowFormulaInfo"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        public MoneyFlowFormulaInfo(int period)
            : base(
                new DataField[] { DataField.High, DataField.Low, DataField.Close, DataField.Y }, //Input fields: High,Low,Close,Volume
                new DataField[] { DataField.Y }, //Output fields
                period)
        {
        }
    }

    /// <summary>
    /// PriceVolumeTrend FormulaInfo
    /// </summary>
    internal class PriceVolumeTrendFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceVolumeTrendFormulaInfo"/> class.
        /// </summary>
        public PriceVolumeTrendFormulaInfo()
            : base(
                new DataField[] { DataField.Close, DataField.Y }, //Input=Close,Volume
                new DataField[] { DataField.Y }) //Output fields
        {
        }
    }

    /// <summary>
    /// OnBalanceVolume FormulaInfo
    /// </summary>
    internal class OnBalanceVolumeFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="OnBalanceVolumeFormulaInfo"/> class.
        /// </summary>
        public OnBalanceVolumeFormulaInfo()
            : base(
                new DataField[] { DataField.Close, DataField.Y }, //Input=Close,Volume
                new DataField[] { DataField.Y }) //Output fields
        {
        }
    }

    /// <summary>
    /// NegativeVolumeIndex FormulaInfo
    /// </summary>
    internal class NegativeVolumeIndexFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="NegativeVolumeIndexFormulaInfo"/> class.
        /// </summary>
        public NegativeVolumeIndexFormulaInfo() //Note about parameters: Start value is mandatory so we don't provide the default
            : this(double.NaN)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NegativeVolumeIndexFormulaInfo"/> class.
        /// </summary>
        /// <param name="startValue">The start value.</param>
        public NegativeVolumeIndexFormulaInfo(double startValue)
            : base(
                new DataField[] { DataField.Close, DataField.Y }, //Input=Close,Volume
                new DataField[] { DataField.Y },
                startValue) //Output fields
        {
        }
    }

    /// <summary>
    /// PositiveVolumeIndex FormulaInfo
    /// </summary>
    internal class PositiveVolumeIndexFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PositiveVolumeIndexFormulaInfo"/> class.
        /// </summary>
        public PositiveVolumeIndexFormulaInfo() //Note about parameters: Start value is mandatory so we don't provide the default
            : this(double.NaN)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PositiveVolumeIndexFormulaInfo"/> class.
        /// </summary>
        /// <param name="startValue">The start value.</param>
        public PositiveVolumeIndexFormulaInfo(double startValue)
            : base(
                new DataField[] { DataField.Close, DataField.Y }, //Input=Close,Volume
                new DataField[] { DataField.Y },
                startValue) //Output fields
        {
        }
    }

    /// <summary>
    /// AccumulationDistribution FormulaInfo
    /// </summary>
    internal class AccumulationDistributionFormulaInfo : FormulaInfo
    {
        //Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AccumulationDistributionFormulaInfo"/> class.
        /// </summary>
        public AccumulationDistributionFormulaInfo() //Note about parameters: Start value is mandatory so we don't provide the default
            : base(
                new DataField[] { DataField.High, DataField.Low, DataField.Close, DataField.Y }, //Input=High, Low, Close, Volume
                new DataField[] { DataField.Y }) //Output fields
        {
        }
    }

    #endregion

    #region enum DataField
    /// <summary>
    /// Chart data fields
    /// </summary>
    internal enum DataField
    {
        X,
        Y,
        LowerWisker,
        UpperWisker,
        LowerBox,
        UpperBox,
        Average,
        Median,
        Bubble,
        BubbleSize,
        High,
        Low,
        Open,
        Close,
        Center,
        LowerError,
        UpperError,
        Top,
        Bottom
    }
    #endregion

    #region class SeriesFieldInfo
    /// <summary>
    /// SeriesFieldInfo class is a OO representation formula input/output data params ("Series1:Y2")
    /// </summary>
    internal class SeriesFieldInfo
    {
        #region Fields
        private Series _series;
        private string _seriesName;
        private DataField _dataField;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the series.
        /// </summary>
        /// <value>The series.</value>
        public Series Series 
        { 
            get { return _series; }
        }
        /// <summary>
        /// Gets the name of the series.
        /// </summary>
        /// <value>The name of the series.</value>
        public string SeriesName
        {
            get { return _series != null ? _series.Name : _seriesName; }
        }
        /// <summary>
        /// Gets the data field.
        /// </summary>
        /// <value>The data field.</value>
        public DataField DataField 
        { 
            get { return _dataField; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SeriesFieldInfo"/> class.
        /// </summary>
        /// <param name="series">The series.</param>
        /// <param name="dataField">The data field.</param>
        public SeriesFieldInfo(Series series, DataField dataField)
        {
            _series = series;
            _dataField = dataField;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SeriesFieldInfo"/> class.
        /// </summary>
        /// <param name="seriesName">Name of the series.</param>
        /// <param name="dataField">The data field.</param>
        public SeriesFieldInfo(string seriesName, DataField dataField)
        {
            _seriesName = seriesName;
            _dataField = dataField;
        }
        #endregion
    }
    #endregion

    #region class SeriesFieldList
    /// <summary>
    /// SeriesFieldInfo class is a OO representation formula input/output data params ("Series1:Y2,Series2.Y4")
    /// </summary>
    internal class SeriesFieldList : List<SeriesFieldInfo>
    {
        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
            {
                SeriesFieldInfo info = this[i];

                if (i > 0)
                    sb.Append(',');

                SeriesChartType seriesChartType = info.Series != null ?
                                                        info.Series.ChartType :
                                                        FormulaHelper.GetDefaultChartType(info.DataField);

                IList<DataField> dataFields = FormulaHelper.GetDataFields(seriesChartType);

                int dataFieldIndex = dataFields.IndexOf(info.DataField);
                if (dataFieldIndex == 0)
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0}:Y", info.SeriesName); //The string field descriptor is 1 based ;-(
                else
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0}:Y{1}", info.SeriesName, dataFieldIndex + 1); //The string field descriptor is 1 based ;-(
            }
            return sb.ToString();
        }

        //Static
        /// <summary>
        /// Parse the string defining the formula's input/output series and fields.
        /// </summary>
        /// <param name="chart">The chart.</param>
        /// <param name="seriesFields">The series fields list. The series name can be followed by the field names. For example: "Series1:Y,Series1:Y3,Series2:Close"</param>
        /// <param name="formulaFields">The formula fields list.</param>
        /// <returns></returns>
        public static SeriesFieldList FromString(Chart chart, string seriesFields, IList<DataField> formulaFields)
        {
            SeriesFieldList result = new SeriesFieldList();
            if (String.IsNullOrEmpty(seriesFields))
            {
                return result;
            }

            List<DataField> unmappedFormulaFields = new List<DataField>(formulaFields);

            //Loop through the series/field pairs
            foreach (string seriesField in seriesFields.Split(','))
            {
                //Stop processing if all the formula fields are mapped
                if (unmappedFormulaFields.Count == 0)
                    break;

                //Split a pair into a series + field
                string[] seriesFieldParts = seriesField.Split(':');
                if (seriesFieldParts.Length > 2)
                {
                    throw new ArgumentException(SR.ExceptionFormulaDataFormatInvalid(seriesField));
                }

                //Get the series and series fields
                string seriesName = seriesFieldParts[0].Trim();
                Series series = chart.Series.FindByName(seriesName);
                if (series != null)
                {
                    switch (seriesFieldParts.Length)
                    {
                        case 1: //Only series name is specified: "Series1"
                            AddSeriesFieldInfo(result, series, unmappedFormulaFields);
                            break;
                        case 2: //Series and field names are provided: "Series1:Y3"
                            AddSeriesFieldInfo(result, series, unmappedFormulaFields, seriesFieldParts[1]);
                            break;
                    }
                }
                else
                {
                    switch (seriesFieldParts.Length)
                    {
                        case 1: //Only series name is specified: "Series1"
                            AddSeriesFieldInfo(result, seriesName, unmappedFormulaFields);
                            break;
                        case 2: //Series and field names are provided: "Series1:Y3"
                            AddSeriesFieldInfo(result, seriesName, unmappedFormulaFields, seriesFieldParts[1]);
                            break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Adds the series field info.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="series">The series.</param>
        /// <param name="unmappedFormulaFields">The unmapped formula fields.</param>
        private static void AddSeriesFieldInfo(SeriesFieldList result, Series series, IList<DataField> unmappedFormulaFields)
        {
            List<DataField> seriesFields = new List<DataField>(FormulaHelper.GetDataFields(series.ChartType));

            for (int i = 0; i < unmappedFormulaFields.Count && seriesFields.Count > 0; )
            {
                DataField formulaField = unmappedFormulaFields[i];
                DataField? seriesField = null;

                // Case 1. Check if the formulaField is valid for this chart type
                if (seriesFields.Contains(formulaField))
                {
                    seriesField = formulaField;
                }
                // Case 2. Try to map the formula field to the series field
                if (seriesField == null)
                {
                    seriesField = FormulaHelper.MapFormulaDataField(series.ChartType, formulaField);
                }

                // If the seriesField is found - add it to the results
                if (seriesField != null)
                {
                    result.Add(new SeriesFieldInfo(series, (DataField)seriesField));
                    seriesFields.Remove((DataField)formulaField);
                    unmappedFormulaFields.Remove(formulaField);
                }
                else
                {
                    i++;
                }
            }
        }
        /// <summary>
        /// Adds the series field info.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="series">The series.</param>
        /// <param name="unmappedFormulaFields">The unmapped formula fields.</param>
        /// <param name="seriesFieldId">The series field id.</param>
        private static void AddSeriesFieldInfo(SeriesFieldList result, Series series, IList<DataField> unmappedFormulaFields, string seriesFieldId)
        {
            IList<DataField> seriesFields = FormulaHelper.GetDataFields(series.ChartType);

            DataField? seriesField = null;

            seriesFieldId = seriesFieldId.ToUpperInvariant().Trim();
            if (seriesFieldId == "Y")
            {
                seriesField = seriesFields[0];
            }
            else if (seriesFieldId.StartsWith("Y", StringComparison.Ordinal))
            {
                int id = 0;
                if (int.TryParse(seriesFieldId.Substring(1), out id))
                    if (id - 1 < seriesFields.Count)
                    {
                        seriesField = seriesFields[id - 1];
                    }
                    else
                    {
                        throw (new ArgumentException(SR.ExceptionFormulaYIndexInvalid, seriesFieldId));
                    }
            }
            else
            {
                seriesField = (DataField)Enum.Parse(typeof(DataField), seriesFieldId, true);
            }

            // Add the seriesField to the results
            if (seriesField != null)
            {
                result.Add(new SeriesFieldInfo(series, (DataField)seriesField));
                if (unmappedFormulaFields.Contains((DataField)seriesField))
                    unmappedFormulaFields.Remove((DataField)seriesField);
                else
                    unmappedFormulaFields.RemoveAt(0);
            }
            else
            {
                throw new ArgumentException(SR.ExceptionDataPointValueNameInvalid, seriesFieldId);
            }

        }

        /// <summary>
        /// Adds the series field info.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="seriesName">Name of the series.</param>
        /// <param name="unmappedFormulaFields">The unmapped formula fields.</param>
        private static void AddSeriesFieldInfo(SeriesFieldList result, string seriesName, IList<DataField> unmappedFormulaFields)
        {
            SeriesChartType chartType = FormulaHelper.GetDefaultChartType(unmappedFormulaFields[0]);
            List<DataField> seriesFields = new List<DataField>(FormulaHelper.GetDataFields(chartType));

            for (int i = 0; i < unmappedFormulaFields.Count && seriesFields.Count > 0; )
            {
                DataField formulaField = unmappedFormulaFields[i];
                DataField? seriesField = null;

                // Check if the formulaField is valid for this chart type
                if (seriesFields.Contains(formulaField))
                {
                    seriesField = formulaField;
                }

                // If the seriesField is found - add it to the results
                if (seriesField != null)
                {
                    result.Add(new SeriesFieldInfo(seriesName, (DataField)seriesField));
                    seriesFields.Remove((DataField)formulaField);
                    unmappedFormulaFields.Remove(formulaField);
                }
                else
                {
                    i++;
                }
            }
        }
        /// <summary>
        /// Adds the series field info.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="seriesName">Name of the series.</param>
        /// <param name="unmappedFormulaFields">The unmapped formula fields.</param>
        /// <param name="seriesFieldId">The series field id.</param>
        private static void AddSeriesFieldInfo(SeriesFieldList result, string seriesName, IList<DataField> unmappedFormulaFields, string seriesFieldId)
        {
            SeriesChartType chartType = FormulaHelper.GetDefaultChartType(unmappedFormulaFields[0]);
            IList<DataField> seriesFields = FormulaHelper.GetDataFields(chartType);

            //Find the field
            DataField? seriesField = null;

            seriesFieldId = seriesFieldId.ToUpperInvariant().Trim();

            if (seriesFieldId == "Y")
            {
                seriesField = seriesFields[0];
            }
            else if (seriesFieldId.StartsWith("Y", StringComparison.Ordinal))
            {
                int seriesFieldIndex = 0;
                if (int.TryParse(seriesFieldId.Substring(1), out seriesFieldIndex))
                    if (seriesFieldIndex < seriesFields.Count)
                    {
                        seriesField = seriesFields[seriesFieldIndex - 1];
                    }
                    else
                    {
                        throw (new ArgumentException(SR.ExceptionFormulaYIndexInvalid, seriesFieldId));
                    }
            }
            else
            {
                //Try parse the field name
                try
                {
                    seriesField = (DataField)Enum.Parse(typeof(DataField), seriesFieldId, true);
                }
                catch (ArgumentException)
                { }
            }

            if (seriesField != null)
            {
                result.Add(new SeriesFieldInfo(seriesName, (DataField)seriesField));
                unmappedFormulaFields.Remove((DataField)seriesField);
            }
            else
            {
                throw new ArgumentException(SR.ExceptionDataPointValueNameInvalid, seriesFieldId);
            }
        }
    }
    #endregion

}

