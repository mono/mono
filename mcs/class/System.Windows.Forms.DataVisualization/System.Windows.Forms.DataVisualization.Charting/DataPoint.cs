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
using System.Linq;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class DataPoint : DataPointCustomProperties
	{
		#region Constructors
		public DataPoint ()
		{
		}

		public DataPoint (double xValue, double yValue)
		{
			XValue = xValue;
			YValues = new double[] { yValue };
		}

		public DataPoint (double xValue, double[] yValues)
		{
			XValue = xValue;
			YValues = yValues;
		}

		[MonoTODO ()]
		public DataPoint (Series series)
		{
		}

		[MonoTODO ()]
		public DataPoint(double xValue,	string yValues)
		{
		}

		#endregion

		#region Public Properties
		public bool IsEmpty { get; set; }
		public override string Name { get; set; }
		public double XValue { get; set; }
		public double[] YValues { get; set; }
		#endregion

		#region Public Methods
		public DataPoint Clone ()
		{
			DataPoint clone = new DataPoint (XValue, YValues);
			clone.IsEmpty = IsEmpty;
			clone.Name = Name;

			return clone;
		}

		public double GetValueByName (string valueName)
		{
			if (valueName == null)
				throw new ArgumentNullException ("valueName");
			
			valueName = valueName.ToLowerInvariant ();

			if (valueName == "x")
				return XValue;

			if (valueName.StartsWith ("y")) {
				if (valueName.Length == 1)
					return YValues[0];

				int index = 0;

				if (int.TryParse (valueName.Substring (1), out index)) {
					if (index > YValues.Length)
						throw new ArgumentException ("Y index greater than number of YValues");
					if (index == 0)
						throw new ArgumentException ("Y index must be greater than zero");

					return YValues[index - 1];
				}
			}

			throw new ArgumentException ("valueName");
		}

		public void SetValueXY (object xValue, params object[] yValue)
		{
			XValue = (double)xValue;
			YValues = yValue.Cast<double> ().ToArray ();
		}

		public void SetValueY (params object[] yValue)
		{
			YValues = yValue.Cast<double> ().ToArray ();
		}
		#endregion
	}
}
