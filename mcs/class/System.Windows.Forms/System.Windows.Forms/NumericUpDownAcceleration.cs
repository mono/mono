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
//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)


using System;

namespace System.Windows.Forms
{
	public class NumericUpDownAcceleration
	{
		#region Fields
		private decimal increment;
		private int seconds;
		#endregion

		#region Properties
		public decimal Increment {
			get { return increment;}
			set { increment = value;}
		}

		public int Seconds {
			get { return seconds;}
			set { seconds = value;}
		}
		#endregion

		#region Constructor
		public NumericUpDownAcceleration (int seconds, decimal increment)
		{
			if (seconds < 0)
				throw new ArgumentOutOfRangeException ("Invalid seconds value. The seconds value must be equal or greater than zero.");
			if (increment < 0)
				throw new ArgumentOutOfRangeException ("Invalid increment value. The increment value must be equal or greater than zero.");
	
			this.increment = increment;
			this.seconds = seconds;
		}
		#endregion
	}
}
