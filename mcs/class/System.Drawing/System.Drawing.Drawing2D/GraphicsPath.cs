//
// System.Drawing.Drawing2D.GraphicsPath.cs
//
// Authors:
//
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Ximian, Inc
//
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D
{

public sealed class GraphicsPath : MarshalByRefObject, ICloneable, IDisposable {

	public GraphicsPath ()
	{
	}
	
	public object Clone ()
	{
		throw new NotImplementedException ();
	}

	public void Dispose ()
	{
		Dispose (true);
		System.GC.SuppressFinalize (this);
	}

	~GraphicsPath ()
	{
		Dispose (false);
	}
	
	void Dispose (bool disposing)
	{
		
	}

	//
	// AddArc
	//
	public void AddArc (Rectangle rect, float start_angle, float sweep_angle)
	{
	}

	public void AddArc (RectangleF rect, float start_angle, float sweep_angle)
	{
	}

	public void AddArc (int x, int y, int width, int height, float start_angle, float sweep_angle)
	{
	}

	public void AddArc (float x, float y, float width, float height, float start_angle, float sweep_angle)
	{
	}

	//
	// AddLine
	//
	public void AddLine (Point a, Point b)
	{
	}

	public void AddLine (PointF a, PointF b)
	{
	}

	public void AddLine (int x1, int y1, int x2, int y2)
	{
	}

	public void AddLine (float x1, float y1, float x2, float y2)
	{
	}

	
	
}

}
