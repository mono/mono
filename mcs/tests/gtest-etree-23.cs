using System;
using System.Linq.Expressions;

namespace Test
{
	public class OrderBySpecification
	{
		public OrderBySpecification (Expression<Func<object, object>> predicate)
		{
		}
	}

	public class RateOrderById : OrderBySpecification
	{
		public RateOrderById ()
			: base (x => x)
		{
		}

		public static int Main ()
		{
			new RateOrderById ();
			return 0;
		}
	}
}
