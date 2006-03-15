// cs0553-2.cs: User-defined conversion `plj.aClass.implicit operator plj.aClass(object)' cannot convert to or from base class
// Line: 10

using System;

namespace plj
{
	public abstract class aClass
	{
		public static implicit operator aClass(object o)
		{ 
			return null;
		}
	}
}