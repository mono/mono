using System;
using System.Diagnostics;

namespace NUnit.Core
{
	/// <summary>
	/// Summary description for Logger.
	/// </summary>
	public class NTrace
	{
		private readonly static TraceSwitch Level = new TraceSwitch( "NTrace", "NUnit internal trace" ); 
		private readonly static string NL = Environment.NewLine;

		#region Error
		public static void Error( string message )
		{
			if ( Level.TraceError )
				WriteLine( message );
		}

		public static void Error( string message, string category )
		{
			if ( Level.TraceError )
				WriteLine( message, category );
		}

		public static void ErrorFormat( string message, params object[] args )
		{
			if ( Level.TraceError )
				WriteFormat( message, args );
		}

		public static void Error( string message, Exception ex )
		{
			if ( Level.TraceError )
			{
				WriteLine( message );
				WriteLine( ex.ToString() );
			}
		}
		#endregion

		#region Warning
		public static void Warning( string message )
		{
			if ( Level.TraceWarning )
				WriteLine( message );
		}

		public static void Warning( string message, string category )
		{
			if ( Level.TraceWarning )
				WriteLine( message, category );
		}

		public static void WarningFormat( string message, params object[] args )
		{
			if ( Level.TraceWarning )
				WriteFormat( message, args );
		}
		#endregion

		#region Info
		public static void Info( string message )
		{
			if ( Level.TraceInfo )
				WriteLine( message );
		}

		public static void Info( string message, string category )
		{
			if ( Level.TraceInfo )
				WriteLine( message, category );
		}

		public static void InfoFormat( string message, params object[] args )
		{
			if ( Level.TraceInfo )
				WriteFormat( message, args );
		}
		#endregion

		#region Debug
		public static void Debug( string message )
		{
			if ( Level.TraceVerbose )
				WriteLine( message );
		}

		public static void Debug( string message, string category )
		{
			if ( Level.TraceVerbose )
				WriteLine( message, category );
		}

		public static void DebugFormat( string message, params object[] args )
		{
			if ( Level.TraceVerbose )
				WriteFormat( message, args );
		}
		#endregion

		#region Helper Methods
		private static void WriteLine( string message )
		{
			Trace.WriteLine( message );
		}

		private static void WriteLine( string message, string category )
		{
			Trace.WriteLine( message, category );
		}

		private static void WriteFormat( string format, params object[] args )
		{
			string message = string.Format( format, args );
			Trace.WriteLine( message );
		}
		#endregion

		private NTrace() { }
	}
}
