//
// Authors:
// Jonathan Pobst (monkey@jpobst.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com) 
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class Axis : ChartNamedElement, IDisposable
	{
		#region Constructors
		public Axis (ChartArea chartArea, AxisName axisTypeName)
		{
		}

		public Axis ()
		{
		}
		#endregion

		#region Public Properties
		public AxisArrowStyle ArrowStyle { get; set; }
		public virtual AxisName AxisName { get; }
		public virtual double Crossing { get; set; }
		public CustomLabelCollection CustomLabels { get; }
		public AxisEnabled Enabled { get; set; }
		public Color InterlacedColor { get; set; }
		public double Interval { get; set; }
		public IntervalAutoMode IntervalAutoMode { get; set; }
		public double IntervalOffset { get; set; }
		public DateTimeIntervalType IntervalOffsetType { get; set; }
		public DateTimeIntervalType IntervalType { get; set; }
		public bool IsInterlaced { get; set; }
		public bool IsLabelAutoFit { get; set; }
		public bool IsLogarithmic { get; set; }
		public bool IsMarginVisible { get; set; }
		public virtual bool IsMarksNextToAxis { get; set; }
		public bool IsReversed { get; set; }
		public bool IsStartedFromZero { get; set; }
		public int LabelAutoFitMaxFontSize { get; set; }
		public int LabelAutoFitMinFontSize { get; set; }
		public LabelAutoFitStyles LabelAutoFitStyle { get; set; }
		public LabelStyle LabelStyle { get; set; }
		public Color LineColor { get; set; }
		public ChartDashStyle LineDashStyle { get; set; }
		public int LineWidth { get; set; }
		public double LogarithmBase { get; set; }
		public Grid MajorGrid { get; set; }
		public TickMark MajorTickMark { get; set; }
		public double Maximum { get; set; }
		public float MaximumAutoSize { get; set; }
		public double Minimum { get; set; }
		public Grid MinorGrid { get; set; }
		public TickMark MinorTickMark { get; set; }

		public override string Name {
			get { return base.Name; }
			set { base.Name = value; }
		}

		public virtual AxisScaleBreakStyle ScaleBreakStyle { get; set; }
		public AxisScaleView ScaleView { get; set; }
		public AxisScrollBar ScrollBar { get; set; }
		public StripLinesCollection StripLines { get; }
		public TextOrientation TextOrientation { get; set; }
		public string Title { get; set; }
		public StringAlignment TitleAlignment { get; set; }
		public Font TitleFont { get; set; }
		public Color TitleForeColor { get; set; }
		public string ToolTip { get; set; }
		#endregion

		#region Public Methods
		public double GetPosition (double axisValue)
		{
			throw new NotImplementedException ();
		}

		public double PixelPositionToValue (double position)
		{
			throw new NotImplementedException ();
		}

		public double PositionToValue (double position)
		{
			throw new NotImplementedException ();
		}

		public void RoundAxisValues ()
		{
			throw new NotImplementedException ();
		}

		public double ValueToPixelPosition (double axisValue)
		{
			throw new NotImplementedException ();
		}

		public double ValueToPosition (double axisValue)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
