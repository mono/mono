//
// System.Diagnostics.CounterCreationDataCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
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

		protected override void OnInsert (int index, object value)
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

