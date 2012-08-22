using System;
using System.Collections;
using System.Collections.Generic;

namespace Mono.CodeContracts.Static.DataStructures
{
	internal interface IReadonlySet<T> : IEnumerable<T>, IEnumerable
	{
		int Count { get; }

		bool IsEmpty { get; }

		bool IsSingleton { get; }

		bool Contains (T element);

		IMutableSet<U> ConvertAll<U> (Converter<T, U> converter);

		IMutableSet<T> Difference (IEnumerable<T> b);

		IMutableSet<T> FindAll (Predicate<T> predicate);

		void ForEach (Action<T> action);

		bool Exists (Predicate<T> predicate);

		IMutableSet<T> Intersection (IReadonlySet<T> b);

		bool IsSubset (IReadonlySet<T> s);

		T PickAnElement ();

		bool TrueForAll (Predicate<T> predicate);

		IMutableSet<T> Union (IReadonlySet<T> b);
	}
}

