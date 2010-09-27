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
using System.ComponentModel;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class Point3D
	{
		#region Constructors
		public Point3D ()
		{
		}

		public Point3D (float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		#endregion

		#region Public Properties
		[Bindable (false)]
		[DefaultValue ("0f, 0f")]
		public PointF PointF {
			get { return new PointF (X, Y); }
			set { X = value.X; Y = value.Y; }
		}

		[Bindable (false)]
		[DefaultValue (0f)]
		public float X { get; set; }

		[Bindable (false)]
		[DefaultValue (0f)]
		public float Y { get; set; }

		[Bindable (false)]
		[DefaultValue (0f)]
		public float Z { get; set; }
		#endregion
	}
}
