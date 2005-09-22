#if NET_2_0
using System;
using System.Collections.ObjectModel;

namespace System.Runtime.Serialization
{
	public sealed class KnownTypeCollection : Collection<Type>
	{
		public KnownTypeCollection ()
		{
		}

		public void ClearItems ()
		{
			base.Clear ();
		}

		public void InsertItem (int index, Type type)
		{
			base.Insert (index, type);
		}

		public void RemoveItem (int index)
		{
			base.RemoveAt (index);
		}

		public void SetItem (int index, Type type)
		{
			base [index] = type;
		}
	}
}
#endif
