using System;
using System.Runtime.InteropServices;

namespace Martin.Collections.Generic {
	[Serializable]
	public abstract class EqualityComparer <T> : IEqualityComparer <T> {
		
		static EqualityComparer ()
		{
			if (typeof (IEquatable <T>).IsAssignableFrom (typeof (T)))
				_default = (EqualityComparer <T>) Activator.CreateInstance (typeof (IEquatableOfTEqualityComparer<>).MakeGenericType (typeof (T)));
			else
				_default = new DefaultComparer ();
		}
		
		
		public abstract int GetHashCode (T obj);
		public abstract bool Equals (T x, T y);
	
		static readonly EqualityComparer <T> _default;
		
		public static EqualityComparer <T> Default {
			get {
				return _default;
			}
		}
		
		[Serializable]
		class DefaultComparer : EqualityComparer<T> {
	
			public override int GetHashCode (T obj)
			{
				return obj.GetHashCode ();
			}
	
			public override bool Equals (T x, T y)
			{
				if (x == null)
					return y == null;
				
				return x.Equals (y);
			}
		}
	}
	
	[Serializable]
	class IEquatableOfTEqualityComparer <T> : EqualityComparer <T> where T : IEquatable <T> {

		public override int GetHashCode (T obj)
		{
			return obj.GetHashCode ();
		}

		public override bool Equals (T x, T y)
		{
			if (x == null)
				return y == null;
			
			return x.Equals (y);
		}
	}

	public interface IEqualityComparer<T> {
		bool Equals (T x, T y);
		int GetHashCode (T obj);
	}

	class X
	{
		public static void Main ()
		{ }
	}
}
