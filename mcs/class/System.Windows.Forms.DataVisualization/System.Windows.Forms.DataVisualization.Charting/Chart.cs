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
using System.Drawing.Imaging;
using System.IO;
using System.ComponentModel;
using System.Data.Common;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class Chart : Control, ISupportInitialize, IDisposable
	{
		public Chart ()
		{
			BackColor = Color.White;
			ChartAreas = new ChartAreaCollection ();
			Series = new SeriesCollection ();
		}
		public AnnotationCollection Annotations { get; private set;}
		public AntiAliasingStyles AntiAliasing { get; set; }
		public override Color BackColor { get; set; }
		public GradientStyle BackGradientStyle { get; set; }
		public override Image BackgroundImage { get; set; }
		public ChartHatchStyle BackHatchStyle { get; set; }
		public string BackImage { get; set; }
		public ChartImageAlignmentStyle BackImageAlignment { get; set; }
		public Color BackImageTransparentColor { get; set; }
		public ChartImageWrapMode BackImageWrapMode { get; set; }
		public Color BackSecondaryColor { get; set; }
		public Color BorderColor { get; set; }
		public ChartDashStyle BorderDashStyle { get; set; }
		public Color BorderlineColor { get; set; }
		public ChartDashStyle BorderlineDashStyle { get; set; }
		public int BorderlineWidth { get; set; }
		public BorderSkin BorderSkin { get; set; }
		public int BorderWidth { get; set; }
		public string BuildNumber { get; private set;}
		public ChartAreaCollection ChartAreas { get; private set; }
		public DataManipulator DataManipulator { get; private set;}
		public Object DataSource { get; set; }
		protected override Size DefaultSize { get { return base.DefaultSize; } }//FIXME
		public override Color ForeColor { get; set; }
		public NamedImagesCollection Images { get; private set;}
		public bool IsSoftShadows { get; set; }
		public LegendCollection Legends { get; private set; }
		public ChartColorPalette Palette { get; set; }
		public Color[] PaletteCustomColors { get; set; }
		public PrintingManager Printing { get; private set;}
		public double RenderingDpiX { get; set; }
		public double RenderingDpiY { get; set; }
		public ChartSerializer Serializer { get; private set; }
		public SeriesCollection Series { get; private set; }
		public bool SuppressExceptions { get; set; }
		public TextAntiAliasingQuality TextAntiAliasingQuality { get; set; }
		public TitleCollection Titles { get; private set;}

		#region Protected Properties
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);

			ChartGraphics g = new ChartGraphics (e.Graphics);

			PaintElement (g, this, new ElementPosition (0, 0, 100, 100));

			foreach (var area in ChartAreas)
				PaintElement (g, area, new ElementPosition (9.299009f, 6.15f, 86.12599f, 81.1875f));

			foreach (var series in Series)
				PaintElement (g, series, new ElementPosition (9.299009f, 6.15f, 86.12599f, 81.1875f));
		}

		protected override void OnPaintBackground (PaintEventArgs pevent)
		{
			base.OnPaintBackground (pevent);
		}

		protected virtual void OnPostPaint (ChartPaintEventArgs e)
		{
		}

		protected virtual void OnPrePaint (ChartPaintEventArgs e)
		{
		}
		#endregion



		public event EventHandler AnnotationPlaced;
		public event EventHandler AnnotationPositionChanged;
		public event EventHandler<AnnotationPositionChangingEventArgs> AnnotationPositionChanging;
		public event EventHandler AnnotationSelectionChanged;
		public event EventHandler AnnotationTextChanged;
		public event EventHandler<ScrollBarEventArgs> AxisScrollBarClicked;
		public event EventHandler<ViewEventArgs> AxisViewChanged;
		public event EventHandler<ViewEventArgs> AxisViewChanging;
		public event EventHandler<CursorEventArgs> CursorPositionChanged;
		public event EventHandler<CursorEventArgs> CursorPositionChanging;
		public event EventHandler Customize;
		public event EventHandler<CustomizeLegendEventArgs> CustomizeLegend;
		public event EventHandler<FormatNumberEventArgs> FormatNumber;
		public event EventHandler<ToolTipEventArgs> GetToolTipText;
		public event EventHandler<ChartPaintEventArgs> PostPaint;
		public event EventHandler<ChartPaintEventArgs> PrePaint;
		public event EventHandler<CursorEventArgs> SelectionRangeChanged;
		public event EventHandler<CursorEventArgs> SelectionRangeChanging;


		#region Public Methods

		[MonoTODO]
		public void AlignDataPointsByAxisLabel ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void AlignDataPointsByAxisLabel (PointSortOrder sortingOrder)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void AlignDataPointsByAxisLabel(string series)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void AlignDataPointsByAxisLabel (string series, PointSortOrder sortingOrder)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ApplyPaletteColors ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void BeginInit ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DataBind ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DataBindCrossTable (System.Collections.IEnumerable dataSource, string seriesGroupByField, string xField, string yFields, string otherFields)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void DataBindCrossTable (System.Collections.IEnumerable dataSource, string seriesGroupByField, string xField, string yFields, string otherFields, PointSortOrder sortingOrder)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DataBindTable (System.Collections.IEnumerable dataSource)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void DataBindTable (System.Collections.IEnumerable dataSource, string xField)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndInit ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public HitTestResult HitTest (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public HitTestResult HitTest (int x, int y, bool ignoreTransparent)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public HitTestResult HitTest (int x, int y, ChartElementType requestedElement)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public HitTestResult[] HitTest (int x, int y, bool ignoreTransparent, params ChartElementType[] requestedElement)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void LoadTemplate (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void LoadTemplate (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetAutoValues ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SaveImage (Stream imageStream, ImageFormat format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SaveImage (Stream imageStream, ChartImageFormat format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SaveImage (string imageFileName, ImageFormat format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SaveImage (string imageFileName, ChartImageFormat format)
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region Protected Methods


		protected override void Dispose (bool disposing)
		{
		}



		#endregion

		#region Private Methods
		private void PaintElement (ChartGraphics g, object element, ElementPosition position)
		{
			ChartPaintEventArgs e = new ChartPaintEventArgs (this, element, g, position);

			OnPrePaint (e);
			OnPostPaint (e);
		}
		#endregion
	}
}
