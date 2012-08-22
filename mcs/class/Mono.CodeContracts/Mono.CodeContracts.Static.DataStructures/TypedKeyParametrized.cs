using System;

namespace Mono.CodeContracts.Static.DataStructures
{
	struct TypedKey<T>
	{
		private string key;

	    public TypedKey(string key)
	    {
	      this.key = key;
	    }
	}
}

