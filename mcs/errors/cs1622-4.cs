// CS1622: Cannot return a value from iterators. Use the yield return statement to return a value, or yield break to end the iteration
// Line: 21

using System.Collections.Generic;

namespace McsDiff
{
	class MyObj
	{
	}
	
	class MainClass
	{
		protected static IEnumerable<MyObj> GetStuff ()
		{
			yield return null;
			
			try {
			}
			catch {
				return true;
			}
		}
	}
}
