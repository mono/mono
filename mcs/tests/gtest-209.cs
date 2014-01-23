using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary3
{
	public class Dictionary1<TKey, TValue> : Dictionary<TKey, TValue>
	{ }

	public class Test
	{
		public static void Main ()
		{
			Dictionary1<Guid, String> _D = new Dictionary1<Guid, string>();
			_D.Add(Guid.NewGuid(), "foo");
		}
	}
}

