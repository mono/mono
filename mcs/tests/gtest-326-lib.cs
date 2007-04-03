// Compiler options: /t:library
using System;
using SCG = System.Collections.Generic;

namespace C5
{
	public abstract class EnumerableBase<T> : SCG.IEnumerable<T>
	{
		public abstract SCG.IEnumerator<T> GetEnumerator ();

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class ArrayBase<T> : EnumerableBase<T>
	{
		public override SCG.IEnumerator<T> GetEnumerator ()
		{
			yield break;
		}
	}

	public class ArrayList<T> : ArrayBase<T>
	{
		public override SCG.IEnumerator<T> GetEnumerator ()
		{
			return base.GetEnumerator ();
		}
	}
}
