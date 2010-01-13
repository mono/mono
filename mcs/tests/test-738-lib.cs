// Compiler options: -t:library

using System;

namespace TestNamespace
{
	public abstract class Stream : IDisposable
	{
		public void Dispose ()
		{
			Dispose(true);
		}

		protected virtual void Dispose (bool disposing)
		{
		}
	}

	public class NonClosingStream 
		: TestNamespace.Stream, IDisposable
	{
		void  IDisposable.Dispose()
		{
		}
	}
}
