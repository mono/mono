namespace System.Collections.Generic
{
	partial class Comparer<T>
	{
        static volatile Comparer<T> defaultComparer;

		public static Comparer<T> Default {
			get {
                Comparer<T> comparer = defaultComparer;
                if (comparer == null) {
                    comparer = CreateComparer();
                    defaultComparer = comparer;
                }
                return comparer;
			}
		}

        static Comparer<T> CreateComparer() {
            RuntimeType t = (RuntimeType)typeof(T);

                if (typeof(IComparable<T>).IsAssignableFrom(t))
                    return (Comparer<T>)RuntimeType.CreateInstanceForAnotherGenericParameter (typeof(GenericComparer<>), t);

				// If T is a Nullable<U> where U implements IComparable<U> return a NullableComparer<U>
				if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
					RuntimeType u = (RuntimeType)t.GetGenericArguments()[0];
					if (typeof(IComparable<>).MakeGenericType(u).IsAssignableFrom(u)) {
						// FIXME:
						throw new NotImplementedException ();
					}
				}

				if (t.IsEnum)
					// FIXME:
					throw new NotImplementedException ();

				// Otherwise return an ObjectComparer<T>
				return new ObjectComparer<T>();
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
