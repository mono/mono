using System;

namespace System.Linq
{
	static class Error
	{
		public static ArgumentNullException ArgumentNull (string parameter)
		{
			return new ArgumentNullException (parameter);
		}

		public static ArgumentOutOfRangeException ArgumentOutOfRange (string parameter)
		{
			return new ArgumentOutOfRangeException (parameter);
		}

		public static ArgumentException ArgumentNotValid (string parameter)
		{
			return new ArgumentException (parameter);
		}

		public static NotSupportedException NotSupported ()
		{
			return new NotSupportedException ();
		}

		public static InvalidOperationException NoElements ()
		{
			return new InvalidOperationException (Strings.NoElements);
		}

		public static InvalidOperationException NoMatch ()
		{
			return new InvalidOperationException (Strings.NoMatch);
		}

		public static InvalidOperationException MoreThanOneElement ()
		{
			return new InvalidOperationException (Strings.MoreThanOneElement);
		}

		public static InvalidOperationException MoreThanOneMatch ()
		{
			return new InvalidOperationException (Strings.MoreThanOneMatch);
		}

		public static ArgumentException ArgumentNotIEnumerableGeneric (object message)
		{
			return new ArgumentException (String.Format ("{0} is not IEnumerable<>", message));
		}

		public static InvalidOperationException NoMethodOnTypeMatchingArguments (object p0, object p1)
		{
			return new InvalidOperationException (Strings.NoMethodOnTypeMatchingArguments (p0, p1));
		}

		public static InvalidOperationException NoMethodOnType (object p0, object p1)
		{
			return new InvalidOperationException (Strings.NoMethodOnType (p0, p1));
		}
	}
}