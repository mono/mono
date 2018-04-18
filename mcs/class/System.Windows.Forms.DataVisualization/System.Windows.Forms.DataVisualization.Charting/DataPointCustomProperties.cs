//
// Authors:
// Jonathan Pobst (monkey@jpobst.com)
// Francis Fisher (frankie@terrorise.me.uk)
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
	public class DataPointCustomProperties : ChartNamedElement
	{
		public virtual string AxisLabel { get; set; }
		public GradientStyle BackGradientStyle { get; set; }
		public ChartHatchStyle BackHatchStyle { get; set; }
		public string BackImage { get; set; }
		public ChartImageAlignmentStyle BackImageAlignment { get; set; }
		public Color BackImageTransparentColor { get; set; }
		public ChartImageWrapMode BackImageWrapMode { get; set; }
		public Color BackSecondaryColor { get; set; }
		public Color BorderColor { get; set; }
		public ChartDashStyle BorderDashStyle { get; set; }
		public int BorderWidth { get; set; }
		public Color Color { get; set; }
		public string CustomProperties { get; set; }
		public CustomProperties CustomPropertiesExtended { get; set; }
		public Font Font { get; set; }
		public bool IsValueShownAsLabel { get; set; }
		public bool IsVisibleInLegend { get; set; }
		
		[MonoTODO]
		public string this[int idx] {
			get { 
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string this[string name] {
			get { 
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public virtual string Label { get; set; }
		public int LabelAngle { get; set; }
		public Color LabelBackColor { get; set; }
		public Color LabelBorderColor { get; set; }
		public ChartDashStyle LabelBorderDashStyle { get; set; }
		public int LabelBorderWidth { get; set; }
		public Color LabelForeColor { get; set; }
		public string LabelFormat { get; set; }
		public string LabelToolTip { get; set; }
		public string LegendText { get; set; }
		public string LegendToolTip { get; set; }
		public Color MarkerBorderColor { get; set; }
		public int MarkerBorderWidth { get; set; }
		public Color MarkerColor { get; set; }
		public string MarkerImage { get; set; }
		public Color MarkerImageTransparentColor { get; set; }
		public int MarkerSize { get; set; }
		public MarkerStyle MarkerStyle { get; set; }
		public string ToolTip { get; set; }

		#region Public methods
		[MonoTODO]
		public virtual void DeleteCustomProperty (string name)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual string GetCustomProperty(string name)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool IsCustomPropertySet(string name)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ResetIsValueShownAsLabel ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ResetIsVisibleInLegend ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void SetCustomProperty (string name,string propertyValue)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void SetDefault (bool clearAll)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
