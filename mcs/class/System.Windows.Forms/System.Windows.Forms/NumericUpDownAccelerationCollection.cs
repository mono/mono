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
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

namespace System.Windows.Forms
{
	[ListBindable (false)]
	public class NumericUpDownAccelerationCollection : MarshalByRefObject, ICollection<NumericUpDownAcceleration>, 
		IEnumerable<NumericUpDownAcceleration>, IEnumerable
	{
		#region Fields
		private List<NumericUpDownAcceleration> items;
		#endregion

		#region Properties
		public int Count {
			get { return items.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public NumericUpDownAcceleration this[int index] {
			get { return items[index]; }
		}
		#endregion

		#region Constructor
		public NumericUpDownAccelerationCollection ()
		{
			items = new List<NumericUpDownAcceleration> ();
		}
		#endregion

		#region Public Methods
		public void Add (NumericUpDownAcceleration acceleration)
		{
			if (acceleration == null)
				throw new ArgumentNullException ("Acceleration cannot be null");

			int i = 0;
			for (; i < items.Count; i++) {
				if (acceleration.Seconds < items[i].Seconds)
					break;
			}
			items.Insert (i, acceleration);
		}

		public void AddRange (params NumericUpDownAcceleration[] accelerations)
		{
			for (int i = 0; i < accelerations.Length; i++)
				Add (accelerations [i]);
		}

		public void Clear ()
		{
			items.Clear ();
		}

		public bool Contains (NumericUpDownAcceleration acceleration)
		{
			return items.Contains (acceleration);
		}

		public void CopyTo (NumericUpDownAcceleration[] array, int index)
		{
			items.CopyTo (array, index);
		}

		public bool Remove (NumericUpDownAcceleration acceleration)
		{
			return items.Remove (acceleration);
		}

		IEnumerator<NumericUpDownAcceleration> IEnumerable<NumericUpDownAcceleration>.GetEnumerator ()
		{
			return items.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return items.GetEnumerator ();
		}
		#endregion
	}
}
