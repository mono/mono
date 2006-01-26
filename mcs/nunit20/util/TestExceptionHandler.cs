using System;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for UnhandledExceptionCatcher.
	/// </summary>
	public class TestExceptionHandler : IDisposable
	{
		private UnhandledExceptionEventHandler handler;

		public TestExceptionHandler( UnhandledExceptionEventHandler handler )
		{
#if !TARGET_JVM
			this.handler = handler;
			AppDomain.CurrentDomain.UnhandledException += handler;
#endif
		}

		~TestExceptionHandler()
		{
#if  !TARGET_JVM
			if ( handler != null )
			{
				AppDomain.CurrentDomain.UnhandledException -= handler;
				handler = null;
			}
#endif
		}



		public void Dispose()
		{
#if !TARGET_JVM
			if ( handler != null )
			{
				AppDomain.CurrentDomain.UnhandledException -= handler;
				handler = null;
			}

			System.GC.SuppressFinalize( this );
#endif
		}
	}
}
