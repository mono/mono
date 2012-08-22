using System;
using System.Collections;
using System.Collections.Generic;

namespace Mono.CodeContracts.Static.DataStructures
{
	internal interface IMutableSet<T> : IReadonlySet<T>, IEnumerable<T>, IEnumerable
	{
		bool Add (T element);
			
		void AddRange (IEnumerable<T> range);
	}
}

