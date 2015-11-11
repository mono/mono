using System.Threading;
using StringMaker = System.Security.Util.Tokenizer.StringMaker;

namespace System
{
	static class SharedStatics
	{
		static StringMaker shared;
		static public StringMaker GetSharedStringMaker ()
		{
			if (shared == null)
				Interlocked.CompareExchange (ref shared, new StringMaker (), null);

			return shared;
		}

		static public void ReleaseSharedStringMaker (ref StringMaker maker)
		{

		}
	}
}