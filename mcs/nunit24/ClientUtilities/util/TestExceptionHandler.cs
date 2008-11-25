// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

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
