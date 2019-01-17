namespace System.Collections.Generic
{
	partial class Comparer<T>
	{
		public static Comparer<T> Default {
			get {
				throw new NotImplementedException();
			}
		}
	}

	partial class EnumComparer<T>
	{
		public override int Compare(T x, T y)
		{
			throw new NotImplementedException();
		}
	}
}
