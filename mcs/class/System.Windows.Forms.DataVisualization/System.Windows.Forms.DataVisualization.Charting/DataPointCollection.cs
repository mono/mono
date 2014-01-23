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

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class DataPointCollection : ChartElementCollection<DataPoint>
	{
		[MonoTODO]
		public DataPoint Add (params double[] y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int AddXY (double xValue, double yValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int AddXY (Object xValue, params Object[] yValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int AddY (double yValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int AddY (params Object[] yValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void ClearItems ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DataBind (System.Collections.IEnumerable dataSource, string xField, string yFields, string otherFields)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DataBindXY (System.Collections.IEnumerable xValue, params System.Collections.IEnumerable[] yValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DataBindXY (System.Collections.IEnumerable xValue, string xField, System.Collections.IEnumerable yValue, string yFields)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DataBindY (params System.Collections.IEnumerable[] yValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DataBindY (System.Collections.IEnumerable yValue, string yFields)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public System.Collections.Generic.IEnumerable<DataPoint> FindAllByValue (double valueToFind)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public System.Collections.Generic.IEnumerable<DataPoint> FindAllByValue (double valueToFind, string useValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public System.Collections.Generic.IEnumerable<DataPoint> FindAllByValue (double valueToFind, string useValue, int startIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataPoint FindByValue (double valueToFind)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataPoint FindByValue (double valueToFind, string useValue)
		{
			return FindByValue( valueToFind, useValue, 0);
		}

		[MonoTODO]
		public DataPoint FindByValue (double valueToFind, string useValue, int startIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataPoint FindMaxByValue () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataPoint FindMaxByValue (string useValue)
		{
			return FindMaxByValue (useValue, 0);
		}

		[MonoTODO]
		public DataPoint FindMaxByValue (string useValue, int startIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataPoint FindMinByValue ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataPoint FindMinByValue (string useValue)
		{
			return FindMinByValue (useValue, 0);
		}

		[MonoTODO]
		public DataPoint FindMinByValue (string useValue, int startIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertXY (int index, Object xValue, params Object[] yValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertY (int index, params Object[] yValue)
		{
			throw new NotImplementedException ();
		}
	}
}
