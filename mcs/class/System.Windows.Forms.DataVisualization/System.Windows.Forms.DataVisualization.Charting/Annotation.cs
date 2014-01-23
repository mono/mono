// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
// (C) Francis Fisher 2013
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
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE./

using System;
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public abstract class Annotation : ChartNamedElement
	{
		protected Annotation ()
		{
		}

		public virtual ContentAlignment Alignment { get; set; }
		public virtual bool AllowAnchorMoving { get; set; }
		public virtual bool AllowMoving { get; set; }
		public virtual bool AllowPathEditing { get; set; }
		public virtual bool AllowResizing { get; set; }
		public virtual bool AllowSelecting { get; set; }
		public virtual bool AllowTextEditing { get; set; }
		public virtual ContentAlignment AnchorAlignment { get; set; }
		public virtual DataPoint AnchorDataPoint { get; set; }
		public virtual string AnchorDataPointName { get; set; }
		public virtual double AnchorOffsetX { get; set; }
		public virtual double AnchorOffsetY { get; set; }
		public virtual double AnchorX { get; set; }
		public virtual double AnchorY { get; set; }
		public AnnotationGroup AnnotationGroup { get; private set; }
		public abstract string AnnotationType { get; } 
		public virtual Axis AxisX { get; set; }
		public virtual string AxisXName { get; set; }
		public virtual Axis AxisY { get; set; }
		public virtual string AxisYName { get; set; }
		public virtual Color BackColor { get; set; }
		public virtual GradientStyle BackGradientStyle { get; set; }
		public virtual ChartHatchStyle BackHatchStyle { get; set; }
		public virtual Color BackSecondaryColor { get; set; }
		public virtual double Bottom { get; set; }
		public virtual string ClipToChartArea { get; set; }
		public virtual Font Font { get; set; }
		public virtual Color ForeColor { get; set; }
		public virtual double Height { get; set; }
		public virtual bool IsSelected { get; set; }
		public virtual bool IsSizeAlwaysRelative { get; set; }
		public virtual Color LineColor { get; set; }
		public virtual ChartDashStyle LineDashStyle { get; set; }
		public virtual int LineWidth { get; set; }
		public override string Name { get; set; }
		public virtual double Right { get; set; }
		public virtual Color ShadowColor { get; set; }
		public virtual int ShadowOffset { get; set; }
		public AnnotationSmartLabelStyle SmartLabelStyle { get; set; }
		public virtual TextStyle TextStyle { get; set; }
		public virtual string ToolTip { get; set; }
		public virtual bool Visible { get; set; }
		public virtual double Width { get; set; }
		public virtual double X { get; set; }
		public virtual double Y { get; set; }
		public virtual string YAxisName { get; set; }

		[MonoTODO]
		public virtual void BeginPlacement ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void BringToFront ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void EndPlacement ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void ResizeToContent ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void SendToBack ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetAnchor (DataPoint dataPoint)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetAnchor (DataPoint dataPoint1, DataPoint dataPoint2)
		{
			throw new NotImplementedException ();
		}
	}
}

