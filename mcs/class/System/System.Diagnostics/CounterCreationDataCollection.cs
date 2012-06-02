//
// System.Diagnostics.CounterCreationDataCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

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
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace System.Diagnostics {

	[Serializable]
	public class CounterCreationDataCollection : CollectionBase {

		public CounterCreationDataCollection ()
		{
		}

		public CounterCreationDataCollection (
			CounterCreationData[] value)
		{
			AddRange (value);
		}

		public CounterCreationDataCollection (
			CounterCreationDataCollection value)
		{
			AddRange (value);
		}

		public CounterCreationData this [int index] {
			get {return (CounterCreationData) InnerList[index];}
			set {InnerList[index] = value;}
		}

		public int Add (CounterCreationData value)
		{
			return InnerList.Add (value);
		}

		public void AddRange (CounterCreationData[] value)
		{
			foreach (CounterCreationData v in value)
			{
				Add (v);
			}
		}

		public void AddRange (CounterCreationDataCollection value)
		{
			foreach (CounterCreationData v in value)
			{
				Add (v);
			}
		}

		public bool Contains (CounterCreationData value)
		{
			return InnerList.Contains (value);
		}

		public void CopyTo (CounterCreationData[] array, int index)
		{
			InnerList.CopyTo (array, index);
		}

		public int IndexOf (CounterCreationData value)
		{
			return InnerList.IndexOf (value);
		}

		public void Insert (int index, CounterCreationData value)
		{
			InnerList.Insert (index, value);
		}

		protected override void OnValidate (object value)
		{
			if (!(value is CounterCreationData))
				throw new NotSupportedException (Locale.GetText(
					"You can only insert " + 
					"CounterCreationData objects into " +
					"the collection"));
		}

		public virtual void Remove (CounterCreationData value)
		{
			InnerList.Remove (value);
		}
	}
}

