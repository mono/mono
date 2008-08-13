using System.Collections.Generic;

class C
{
	class ArrayReadOnlyList<T>
	{
		T [] array;
		bool is_value_type;

		public ArrayReadOnlyList ()
		{
		}

		public T this [int index]
		{
			get
			{
				return array [index];
			}
		}

		public IEnumerator<T> GetEnumerator ()
		{
			for (int i = 0; i < array.Length; i++)
				yield return array [i];
		}
	}

	public static void Main ()
	{
	}
}