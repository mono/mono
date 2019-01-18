namespace System.Collections.Generic
{
	partial class EqualityComparer<T>
	{
		public static EqualityComparer<T> Default {
			get {
				throw new NotImplementedException ();
			}
		}
	}

	partial class EnumEqualityComparer<T>
	{
		public override bool Equals(T x, T y)
		{
			throw new NotImplementedException ();
		}
	}
}