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
			this.handler = handler;
			AppDomain.CurrentDomain.UnhandledException += handler;
		}

		~TestExceptionHandler()
		{
			if ( handler != null )
			{
				AppDomain.CurrentDomain.UnhandledException -= handler;
				handler = null;
			}
		}



		public void Dispose()
		{
			if ( handler != null )
			{
				AppDomain.CurrentDomain.UnhandledException -= handler;
				handler = null;
			}

			System.GC.SuppressFinalize( this );
		}
	}
}
