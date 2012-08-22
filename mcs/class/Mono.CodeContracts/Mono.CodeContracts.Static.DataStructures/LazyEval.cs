using System;

namespace Mono.CodeContracts.Static.DataStructures
{
	class LazyEval<T>
	{
		private readonly Func<T> func;
    	private Optional<T> value;

	    public T Value
	    {
	      get
	      {
	        if (!this.value.IsValid)
	          this.value = (Optional<T>) this.func();
	        return this.value.Value;
	      }
	    }

	    public LazyEval(Func<T> func)
	    {
	      this.func = func;
	    }
	
	    public LazyEval(LazyEval<T> l)
	    {
	      this.value = l.value;
	    }
	}
}

